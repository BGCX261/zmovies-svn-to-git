using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;

namespace MovieAgent.Server.Services
{
	[Script]
	public class BasicGooTubeCrawler
	{
		public static void Search(string url, Action<string> handler)
		{
			var c = new BasicWebCrawler("kej.tw", 80);

			c.DataReceived +=
				document =>
				{
					var trigger_tag = "<textarea id=\"outputfield\">";
					var trigger_i = document.IndexOf(trigger_tag);

					var trigger_end_tag = "</textarea>";
					var trigger_end_i = document.IndexOf(trigger_end_tag, trigger_i + trigger_tag.Length);

					var data = document.Substring(trigger_i + trigger_tag.Length, trigger_end_i - trigger_i - trigger_tag.Length);

					handler(data);
				};

			c.Crawl("/flvretriever/?videoUrl=" + url);

		}
	}
}
