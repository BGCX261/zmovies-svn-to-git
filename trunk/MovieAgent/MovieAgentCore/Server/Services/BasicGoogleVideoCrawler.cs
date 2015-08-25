using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;
using MovieAgent.Shared;

namespace MovieAgent.Server.Services
{
	[Script]
	public class BasicGoogleVideoCrawler
	{
		public event Action<string, string> VideoSourceFound;

		public readonly BasicWebCrawler Crawler;

		public BasicGoogleVideoCrawler()
		{
			var c = new BasicWebCrawler("video.google.com", 80);

			this.Crawler = c;

			//<div class="embed_html" style="display: none">
			// &lt;object id=&quot;object_player_1&quot; classid=&quot;clsid:D27CDB6E-AE6D-11cf-96B8-444553540000&quot; codebase=&quot;http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,0,0&quot; width=&quot;100%&quot; height=&quot;100%&quot;&gt;&lt;param name=&quot;movie&quot; value=&quot;http://www.youtube.com/v/aDWPsoKQoOs&amp;fs=1&amp;hl=en&amp;enablejsapi=1&amp;playerapiid=object_player_1&quot;/&gt;&lt;param name=&quot;allowFullScreen&quot; value=&quot;true&quot;/&gt;&lt;param name=&quot;allowScriptAccess&quot; value=&quot;always&quot;/&gt;
			// &lt;embed 
			//		id=&quot;embed_player_1&quot; 
			//		width=&quot;100%&quot; 
			//		height=&quot;100%&quot; 
			//		bgcolor=&quot;#000000&quot; 
			//		type=&quot;application/x-shockwave-flash&quot; 
			//		pluginspage=&quot;http://www.macromedia.com/go/getflashplayer&quot; 
			//		allowScriptAccess=&quot;always&quot; 
			//		allowFullScreen=&quot;true&quot; 
			//		src=&quot;http://www.youtube.com/v/aDWPsoKQoOs&amp;fs=1&amp;hl=en&amp;enablejsapi=1&amp;playerapiid=embed_player_1&quot;/&gt;
			// &lt;/object&gt;
			//</div>

			// <embed id="embed_player_1" width="100%" height="100%" bgcolor="#000000" type="application/x-shockwave-flash" pluginspage="http://www.macromedia.com/go/getflashplayer" allowScriptAccess="always" allowFullScreen="true" src="http://www.youtube.com/v/aDWPsoKQoOs&fs=1&hl=en&enablejsapi=1&playerapiid=embed_player_1"/>

			var ParseEmbed = new
			{
				id = "",
				width = "",
				height = "",
				bgcolor = "",
				type = "",
				pluginspage = "",
				allowScriptAccess = "",
				allowFullScreen = "",
				src = "",
			}.ToAnonymousConstructor(
				(string element) =>
				{
					string id = "",
						width = "",
						height = "",
						bgcolor = "",
						type = "",
						pluginspage = "",
						allowScriptAccess = "",
						allowFullScreen = "",
						src = "";

					element.
						ParseAttribute("id", value => id = value).
						ParseAttribute("width", value => width = value).
						ParseAttribute("height", value => height = value).
						ParseAttribute("bgcolor", value => bgcolor = value).
						ParseAttribute("type", value => type = value).
						ParseAttribute("pluginspage", value => pluginspage = value).
						ParseAttribute("allowScriptAccess", value => allowScriptAccess = value).
						ParseAttribute("allowFullScreen", value => allowFullScreen = value).
						ParseAttribute("src", value => src = value).
						ParseContent(null).
						Parse();

					return new
					{
						id,
						width,
						height,
						bgcolor,
						type,
						pluginspage,
						allowScriptAccess,
						allowFullScreen,
						src
					};
				}
			);

			c.DataReceived +=
				document =>
				{

					var embed_start = document.IndexOf("&lt;embed");

					if (embed_start < 0)
						return;

					var embed_end = document.IndexOf("/&gt;", embed_start);
					var embed_content = document.
						Substring(embed_start, embed_end - embed_start + 5).
						Replace("&quot;", "\"").
						Replace("&amp;", "&").
						Replace("&lt;", "<").
						Replace("&gt;", ">");

					var embed = ParseEmbed(embed_content);

					if (string.IsNullOrEmpty(embed.src))
						return;

					var video_start = embed.src.IndexOf("v/");

					var video_end = embed.src.IndexOf("&", video_start);
					var video = embed.src.Substring(video_start + 2, video_end - video_start - 2);

					if (this.VideoSourceFound != null)
						this.VideoSourceFound(video, embed.src);
				};
		}

		public void Search(string title)
		{
			// http://en.wikipedia.org/wiki/Percent-encoding

			var e = title.URLEncode();

			this.Crawler.Crawl("/videosearch?q=" + e + "+site%3Ayoutube.com&emb=on#");
		}

		public static void Search(string title, Action<string, string> VideoSourceFound)
		{
			var c = new BasicGoogleVideoCrawler();

			c.VideoSourceFound += VideoSourceFound;

			c.Search(title);
		}
	}
}
