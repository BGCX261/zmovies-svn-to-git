using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Shared;
using MovieAgent.Server.Library;

namespace MovieAgent.Server.Services
{
	[Script]
	public class BasicTinyArrowsCrawler
	{
		public static void Spawn(string url, Action<string> handler)
		{
			var Hosts = new[]
			{
				"xn--hgi.ws",
				"xn--ogi.ws",
				"xn--vgi.ws",
				"xn--3fi.ws",
				"xn--egi.ws",
				"xn--9gi.ws",
				"xn--5gi.ws",
				"xn--1ci.ws",
				"xn--odi.ws",
				"xn--rei.ws",
				"xn--cwg.ws",
				"ta.gd",
			};

			
			var Host = Hosts[url.XorBytes() % Hosts.Length];

			var c = new BasicWebCrawler("tinyarro.ws", 80);

			c.Buffer = new byte[100];

			c.BinaryDataReceived +=
				data =>
				{
					var Target = "http://";

					int i = Target.Length;

					for (; i < data.Length; i++)
					{
						if (data[i] == '/')
						{
							i++;
							break;
						}
					}

					Target += Host + "/";

					for (; i < data.Length; i++)
					{
						Target += "%" + data[i].ToHexString();
					}

					handler(Target);
				};



			c.Crawl("/api-create.php?host=" + Host + "&url=" + url);

		}
	}
}
