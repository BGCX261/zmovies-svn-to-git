using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib.Tools;
using MovieAgent.Server.Library;
using MovieAgentOtterExperience.GFeed;
using System.IO;
using System.Diagnostics;
using MovieAgentOtterExperience.Otter;
using MovieAgent.Server.Services;

namespace MovieAgentOtterExperience
{
	class Program
	{
		// http://diveintomark.org/archives/2004/02/04/incompatible-rss
		// http://base.google.com/support/bin/answer.py?hl=en&answer=58085

		// C:\util\Otter33-Win32>

		// more examples:
		// http://www.comp.leeds.ac.uk/krr/handouts/otter_exercise.pdf
		// http://mally.stanford.edu/Papers/computation.pdf

		// http://www.cs.unm.edu/~mccune/otter/otter-examples/auto/index.html
		// http://www.cs.unm.edu/~mccune/otter/download.html
		// http://www.cs.unm.edu/~mccune/prover9/download/
		// http://www.cs.unm.edu/~mccune/prover9/download/LADR1007B-win.zip

		// http://download.dojotoolkit.org/release-1.0.2/dojo-release-1.0.2/dojox/gfx/demos/beautify.html
		// http://lambda.ee/index.php/Details_of_the_second_lab:_rule_system
		//Concretely, you will have:
		//    * Look at your rdfa pages and invent/create sensible small rule files in the 
		//		Otter syntax for deriving new data from physically present rdfa data.
		//          o It may happen that the rdfa pages from the first lab do not 
		//				contain really suitable/interesting data for this. In that 
		//				case, create new pages.
		//          o The rdfa pages should indicate (by having a corresponding link 
		//				in the rdfa data) which rule set should be used. You should 
		//				have at least two different rdfa pages and two different 
		//				rulesets, one for each page.
		//					-> Movies
		//					-> TV Shows
		//          o The rule file should be such that no contradiction arises and 
		//				the set of derived facts is still maintainable (not millions 
		//				of new facts). 
		//    * Write a program which takes your first lab output, finds an indicated 
		//		rule file and builds a new Otter syntax file containing both rules 
		//		and the data obtained from the rdfa page.
		//    * Then let the same program call Otter with the newly built file and send 
		//		the result to a result file.
		//    * Write a program which takes the output file, parses out the derived 
		//		facts (fact has just one atom and no variables) and adds these to 
		//		the original rdfa-parsed dataset.
		//    * Finally print out the extended dataset: the original facts and newly 
		//		derived facts from this dataset.
		//    * Create an addition to the program which answers the posed 
		//		query: true/false or finds a concrete necessary value (or values). 
		//		Use otter atom like person(X) or forall person(X) or 
		//		similar (choose a suitable one yourself) as a query input. 
		//You may write just one program for all these tasks or several different. In the latter case you will also have to write a "master" program (as a shell script or bat file, for example) calling these subprograms.
		//The final resulting program should take a page url or file name as a single input and produce a list of all found and derived facts as output. 

		static void Main(string[] args)
		{
			var limit = 250;

			Console.WriteLine("loading latest movies...");

			Document doc =
				// offline
				new FileInfo(@"Data\test.txt");
			// online
				//new Uri("http://www.google.com/uds/Gfeeds?scoring=h&context=0&num=250&hl=en&output=json&v=1.0&nocache=0&q=http://feeds2.feedburner.com/zmovies");


			Console.WriteLine(doc.responseData.feed.entries.Length + " feed items found!");
			Console.WriteLine();

			IDX5711.DecisionEngine.Invoke(doc);


			var query = from k in doc.responseData.feed.entries
						let t = new TitleParser(k.title)
						select new { k, t };


			var rulesets = new
			{
				TV_show = new StringBuilder(),
				Movie = new StringBuilder(),
			};

			Func<string, StringBuilder> rulesets_AppendLine = null;
			rulesets_AppendLine += rulesets.Movie.AppendLine;
			rulesets_AppendLine += rulesets.TV_show.AppendLine;

			rulesets_AppendLine(@"
set(hyper_res).
set(factor).
set(print_kept).
formula_list(sos).
			");




			#region ruleset: TV show

			rulesets.TV_show.AppendLine(@"
%sensible small rule files
	all x all z all u (
		facts(x,season,u) & $EQ(u, 1)
		->
		pilot(x) 
	).

	all x all z all u (
		facts(x,episode,u) & $GT(u, 20)
		->
		probablyendofseason(x) 
	).

");

			foreach (var entry in query.Where(k => k.t.Type == "TV show").Take(limit))
			{
				var p = new BasicFileNameParser(entry.t.Title);

				rulesets.TV_show.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', year, " + entry.t.Year + ").");
				rulesets.TV_show.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', season, " + int.Parse(p.Season) + ").");
				rulesets.TV_show.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', episode, " + int.Parse(p.Episode) + ").");


				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("# " + entry.t.Title);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("; " + entry.t.Year);

				Console.ForegroundColor = ConsoleColor.Cyan;
				foreach (var category in entry.k.categories)
				{
					rulesets.TV_show.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', category, " + category.GetHashCode() + "). %" + category);

					Console.Write("; " + category);
				}
				Console.WriteLine();
			}
			#endregion



			var o_TV_show = new OtterApplication(rulesets.TV_show.ToString());

			o_TV_show.ToConsole();

			File.WriteAllText(@"Data\TV_show.in", rulesets.TV_show.ToString());
			File.WriteAllText(@"Data\TV_show.out", o_TV_show.Output);


			Console.WriteLine();

			#region ruleset: Movie


			rulesets.Movie.AppendLine(@"

%sensible small rule files
all x all z all u (
    facts(x,year,u) & $LT(u, 2009)
    ->
    oldmedia(x)
).

all x all z all u (
	oldmedia(x) &
    facts(x,category,u) & $EQ(u, " + "Comedy".GetHashCode() + @")
    ->
    oldcomedy(x)
).
			"
			);

			foreach (var entry in query.Where(k => k.t.Type == "Movie").Take(limit))
			{
				rulesets.Movie.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', year, " + entry.t.Year + ").");

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("# " + entry.t.Title);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("; " + entry.t.Year);

				Console.ForegroundColor = ConsoleColor.Cyan;
				foreach (var category in entry.k.categories)
				{
					rulesets.Movie.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', category, " + category.GetHashCode() + "). %" + category);

					Console.Write("; " + category);
				}
				Console.WriteLine();
			}
			#endregion

			Console.WriteLine();



			var o_Movie = new OtterApplication(rulesets.Movie.ToString());

			o_Movie.ToConsole();

			File.WriteAllText(@"Data\Movie.in", rulesets.Movie.ToString());
			File.WriteAllText(@"Data\Movie.out", o_Movie.Output);

			using (var w = new StreamWriter(new FileInfo(@"Data\extended.xml").OpenWrite()))
			{
				w.WriteLine(@"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<rss version='2.0' xmlns:media='http://search.yahoo.com/mrss' xmlns:atom='http://www.w3.org/2005/Atom'>
  <channel>
    <title>zmovies</title>
    <description>zmovies can haz entertainment</description>
    <link>http://zproxy.planet.ee/zmovies</link>");

				#region TV show
				foreach (var k in query.Where(k => k.t.Type == "TV show").Take(limit))
				{
					w.WriteLine("<item>");
					w.WriteLine("<title>" + k.k.title + "</title>");
					w.WriteLine("<link>" + k.k.link + "</link>");

					w.WriteLine("<description><![CDATA[");
					w.WriteLine(k.k.content);

					w.WriteLine(" ]]></description>");


					foreach (var g in k.k.categories)
					{
						w.WriteLine("<category>" + g + "</category>");
					}

					foreach (var f in o_TV_show.KeptFactsDictionary.Where(kk => kk.Value == k.t.Title.Replace("'", @"-")))
					{
						w.WriteLine("<category>" + f.Name + "</category>");
					}

					w.WriteLine("</item>");
				}
				#endregion

				#region Movie
				foreach (var k in query.Where(k => k.t.Type == "Movie").Take(limit))
				{
					w.WriteLine("<item>");
					w.WriteLine("<title>" + k.k.title + "</title>");
					w.WriteLine("<link>" + k.k.link + "</link>");

					w.WriteLine("<description><![CDATA[");
					w.WriteLine(k.k.content);

					w.WriteLine(" ]]></description>");


					foreach (var g in k.k.categories)
					{
						w.WriteLine("<category>" + g + "</category>");
					}

					foreach (var f in o_Movie.KeptFactsDictionary.Where(kk => kk.Value == k.t.Title.Replace("'", @"-")))
					{
						w.WriteLine("<category>" + f.Name + "</category>");
					}

					w.WriteLine("</item>");
				}
				#endregion

				w.WriteLine(@"
	  </channel>
</rss>");
			}
		}

	}


}
