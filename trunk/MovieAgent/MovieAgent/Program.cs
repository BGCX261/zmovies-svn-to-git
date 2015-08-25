using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ScriptCoreLib.CSharp.Avalon.Extensions;
using ScriptCoreLib.Shared.Lambda;
using MovieAgent.Client.Avalon;
using System.Diagnostics;
using System.Threading;
using MovieAgent.Shared;

namespace MovieAgent
{
	class Program
	{
		

		[STAThread]
		static public void Main(string[] args)
		{

			Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "web");

			//var Queries = new Queue<string>();

			//Queries.Enqueue("console");

			var Fork = default(Action<string>);

			Action<string> AddFork =
				Query =>
				{
					ThreadPool.QueueUserWorkItem(
						delegate
						{
							Fork(Query);
						}
					);
					//new Thread(
					//        delegate ()
					//        {
					//            //lock (Fork)
					//            Fork(Query);
					//        }
					//    )
					// { Name = Query }.Start();
				};

			Fork =
				Query =>
				{
					//var Query = Queries.Dequeue();
					//Console.ForegroundColor = ConsoleColor.Yellow;
					//Console.WriteLine("?" + Query);
					//Console.ForegroundColor = ConsoleColor.Gray;
					var w = new Stopwatch();
					w.Start();
					Server.Application.Application_Entrypoint(Query,
						k =>
						{
							//Console.ForegroundColor = ConsoleColor.Cyan;
							//Console.WriteLine(k);
							//Console.ForegroundColor = ConsoleColor.Gray;

							AddFork(k);

						}
					);

					w.Stop();
					//Console.ForegroundColor = ConsoleColor.Yellow;
					//Console.WriteLine(Query + " " + w.Elapsed);
					//Console.ForegroundColor = ConsoleColor.Gray;

				};

			Console.Title += ("press any key");

			//AddFork("Task6_MediaCollector?html");
			AddFork("console");
			//AddFork("Task6_MediaCollector?timestamp");
			Console.ReadKey(true);

			//var w = new AvalonCanvas().ToWindow();

			//w.ShowDialog();
		}
	}
}

