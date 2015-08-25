using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace MovieAgentOtterExperience.Otter
{
	class OtterApplication
	{
		public readonly string Output;
		public readonly string[] Kept;
		public readonly Process Process;

		public OtterApplication(string e)
		{
			// http://www.codeguru.com/forum/printthread.php?t=229520

			var p = Process.Start(
				new ProcessStartInfo(@"C:\util\Otter33-Win32\bin\otter.exe")
				{
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,

				}
				
			);

			p.EnableRaisingEvents = true;

			this.Process = p;


			var w = new StringBuilder();

			#region StandardOutputReader

			// you have a more general problem, which is the possibility that the standard output
			// will wind up blocked because you haven't been reading from standard error.
			var StandardOutputReader = new Thread(
				delegate()
				{
					while (!p.StandardOutput.EndOfStream)
					{
						var x = p.StandardOutput.ReadLine();
						w.AppendLine(x);
					}
				}
			);
			StandardOutputReader.Start();
			#endregion

			#region StandardErrorReader

			// you have a more general problem, which is the possibility that the standard output
			// will wind up blocked because you haven't been reading from standard error.
			var StandardErrorReader = new Thread(
				delegate()
				{
					while (!p.StandardError.EndOfStream)
					{
						var x = p.StandardError.ReadLine();
					}
				}
			);
			StandardErrorReader.Start();
			#endregion


			p.StandardInput.Write(e);

			p.StandardInput.Flush();
			p.StandardInput.Close();
			p.StandardInput.Dispose();

			StandardOutputReader.Join();

			Output = w.ToString();
			

			var lines = Output
				.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			this.Kept = lines
				.SkipWhile(k => !k.Contains("= start of search ="))
				.TakeWhile(k => !k.Contains("= end of search ="))
				.Where(k => k.StartsWith("** KEPT"))
				.ToArray();
		}


		private static void OtterTesting()
		{

			var otter = new OtterApplication(
@"% this is the way comments start
set(hyper_res).  % another alternative is set(binary_res)
set(factor).   % probably comes automatically
set(print_kept).  % this will print out ALL derived and kept stuff

formula_list(sos).  % use list(sos) if not formula syntax

rdfx1('xx 1',job,cleaner).
rdfx1('xx 1',phone,123).
rdfx1('xx 1',blog,'http://zproxy.wordpress.com').

rdfx1(s2,job,guard).
rdfx1(s2,phone,345).
rdfx1(s2,blog,'http://zproxy.wordpress.com').

rdfx1(s3,title,breakingnews).
rdfx1(s3,link,www_epl_ee).


% following three rows are not formula syntax: x,y,z,u,v,w are vars
% -rdf(x,job,z) | 
% -rdf(x,phone,u) |
% rdf(x,type,employee).
 
% to be an employee one must have a job and a phone
%
% this is formula syntax
 all x all z all u (
    rdfx1(x,job,z) &
    rdfx1(x,phone,u)
    ->
    rdfx1(x,type,employee) ).
    
 all x all z all u (
    rdfx1(x,blog,z)
    ->
    rdfx1(x,type,blogger) ).

 all x all z all u (
    rdfx1(x,type,employee) &
    rdfx1(x,type,blogger)
    ->
    rdfx1(x,type,smartguy) ).
 
% and there is no query! try giving -rdf(s1,type,employee) as query.

% useful for query: say that s1 is not s2
% s1!=s2.
% query example: look for all guys not s2
% all x (rdf(x,type,employee) &
%       x!=s2
%       ->
%       $answer(x))."
			);


			Console.WriteLine(@"newly derived facts:");

			otter.ToConsole();
		}

		public class Tuple1
		{
			public string Name;

			public string Value;
		}

		public IEnumerable<Tuple1> KeptFactsDictionary
		{
			get
			{

				foreach (var k in KeptFacts)
				{
					var i = k.IndexOf("(");
					var j = k.IndexOf(")");

					yield return new Tuple1 { Name = k.Substring(0, i), Value = k.Substring(i + 2, j - i - 3) };
				}

			}
		}

		public IEnumerable<string> KeptFacts
		{
			get
			{
				foreach (var k in this.Kept)
				{
					var i = k.IndexOf("]");
					yield return (k.Substring(i + 2));
				}
			}
		}

		public void ToConsole()
		{
			foreach (var k in this.KeptFacts)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(k);
				Console.WriteLine();
			}
		}

		public void ToConsole(Func<string, bool> filter)
		{
			foreach (var k in this.KeptFactsDictionary.Where(kk => filter(kk.Name)))
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(k.Value);
				Console.WriteLine();
			}
		}

	}
}
