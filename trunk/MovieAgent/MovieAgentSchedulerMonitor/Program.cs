using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieAgent.Server.Library;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace MovieAgentSchedulerMonitor
{
	class Program
	{
		// http://msdn.microsoft.com/en-us/library/aa373347(VS.85).aspx



		static void Main(string[] args)
		{


			var u = new Uri("http://zproxy.planet.ee/zmovies/server/tasks/Scheduler/Counter");

			var x = 0;

			var skip = 5;

			while (true)
			{
				var c = new BasicWebCrawler(u.Host, 80);

				c.DataReceived +=
					document =>
					{
						var n = int.Parse(document);

						if (x > 0)
						{
							if (x < n)
							{
								if (x < (n - skip))
									Console.ForegroundColor = ConsoleColor.Yellow;
								else
									Console.ForegroundColor = ConsoleColor.Green;
							}
							else
								Console.ForegroundColor = ConsoleColor.Red;

							Console.WriteLine(n);
						}

						x = n;
					};

				c.Crawl(u.PathAndQuery);

				Thread.Sleep(5500 * skip);
			}
		}
	}
}
