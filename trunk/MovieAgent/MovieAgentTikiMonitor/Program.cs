using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieAgent.Server.Library;
using System.Threading;

namespace MovieAgentTikiMonitor
{
	class Program
	{
		static void Main(string[] args)
		{
			var hosts = new[] { "zmovies.tk"/*, "zmoviez.tk" */};

			while (true)
			{
				foreach (var h in hosts)
				{

					var c = new BasicWebCrawler(h, 80);

					c.DataReceived +=
						document =>
						{
							var trigger = "<frame src=\"";
							var i = document.IndexOf(trigger);
							var j = document.IndexOf("\"", i + trigger.Length);

							var data = document.Substring(i + trigger.Length, j - i - trigger.Length);

							var gmoduleprefix = "http://www.gmodules.com/ig/ifr?url=";

							if (data.StartsWith(gmoduleprefix))
							{
								var module = data.Substring(gmoduleprefix.Length);

								if (module.StartsWith("http://zproxy.planet.ee"))
									Console.ForegroundColor = ConsoleColor.Yellow;
								else
									Console.ForegroundColor = ConsoleColor.Green;

								Console.WriteLine(DateTime.Now.ToString() + " " + h + " : " + module);
							}
							else
							{
								Console.ForegroundColor = ConsoleColor.Red;
								Console.WriteLine(DateTime.Now.ToString() + " " + h + " : " + data);
							}

						};

					c.Crawl("/");
				}

				Thread.Sleep(15000);
			}
		}
	}
}
