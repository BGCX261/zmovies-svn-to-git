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
	public class BasicOMDBCrawler
	{
		// http://www.omdb.si/index.php/ofilm/?i=432936
		// Being Erica 

		[Script]
		public class AliasEntry
		{
			public string Title;
			public string Year;

			// comma separated
			public string[] Genres;

			public string Duration;

			public string Link;

			public void GetPoster(Action<string> handler)
			{
				var uri = Link.ToUri();

				var c = new BasicWebCrawler(uri.Host, 80);

				c.DataReceived +=
					document =>
					{
						var prefix = "http://static.omdb.si/posters/active/";

						var trigger = "<img src=\"" + prefix;

						var trigger_i = document.IndexOf(trigger);

						if (trigger_i < 0)
							return;

						var end_i = document.IndexOf("\"", trigger_i + trigger.Length);

						var data = prefix + document.Substring(trigger_i + trigger.Length, end_i - (trigger_i + trigger.Length));

						handler(data);
					};

				c.Crawl(uri.PathAndQuery);
			}
		}

		public static void SearchSingle(string stitle, string Year, Action<AliasEntry> handler)
		{
			var x = default(AliasEntry);

			Search(stitle,
				e =>
				{
					if (x == null)
					{
						x = e;
					}
					else
					{
						if (!string.IsNullOrEmpty(Year))
							if (!string.IsNullOrEmpty(e.Year))
								if (e.Year.Contains(Year))
									x = e;
					}
				}
			);

			if (x != null)
				handler(x);
		}

		public static void Search(string stitle, Action<AliasEntry> handler)
		{
			var c = new BasicWebCrawler("www.omdb.si", 80);

			var DefaultLink = new { Link = "", Title = "", Text = "" };

			var ParseLink = DefaultLink.ToAnonymousConstructor(
				(string element) =>
				{
					var Link = "";
					var Title = "";
					var Text = "";

					element.
						ParseAttribute("href", value => Link = value).
						ParseAttribute("title", value => Title = value).
						ParseContent(value => Text = value).
						Parse();

					return new { Link, Title, Text };
				}
			);

			c.DataReceived +=
				document =>
				{

					var trigger_tag = "<table width=\"100%\" class=\"fW\">";
					var trigger_end_tag = "</table>";

					Func<int, int> scan =
						offset =>
						{
							var trigger_i = document.IndexOf(trigger_tag, offset);

							if (trigger_i < 0)
								return offset;

							var tirgger_end_i = document.IndexOf(trigger_end_tag, trigger_i);

							if (tirgger_end_i < 0)
								return offset;

							var data = document.Substring(trigger_i + trigger_tag.Length, tirgger_end_i - trigger_i - trigger_tag.Length);

							/*
<tr>
	<td class="bTl"><img alt="" src="/images/default/Ogrodje0.gif" width="10" height="10" /></td>
	<td class="bT"></td><td class="bTr"></td></tr>
<tr>
	<td class="bL"></td>
	<td class="bM">
		<div align="left">
			<table width="100%"  border="0" cellspacing="0" cellpadding="0">
				<tr>
					<td width="5" rowspan="2"></td>
					<td width="444" align="left">
						<div align="left"><a href="/index.php/ofilm/?i=401737">Lost <b>(2004)</b></a></div>
					</td>
					<td align="right">
						<span class="forumozadje3"><b>8.9</b> 
							 <img src='/images/default/zvezdice/yellow.gif' class='slikevvrsti' /><img src='/images/default/zvezdice/yellow.gif' class='slikevvrsti' /><img src='/images/default/zvezdice/yellow.gif' class='slikevvrsti' /><img src='/images/default/zvezdice/yellow.gif' class='slikevvrsti' /><img src='/images/default/zvezdice/yellow.gif' class='slikevvrsti' /><img src='/images/default/zvezdice/yellow.gif' class='slikevvrsti' /><img src='/images/default/zvezdice/yellow.gif' class='slikevvrsti' /><img src='/images/default/zvezdice/yellow.gif' class='slikevvrsti' /><img src='/images/default/zvezdice/d9.gif' class='slikevvrsti' /><img src='/images/default/zvezdice/yellowEmpty.gif' class='slikevvrsti' /></span>
					</td></tr>
				<tr>
					<td align="left">
						<span class="oddelki_forum_mala">Genre: <b>Drama, Adventure, Mystery, Thriller</b> Duration: <b>45 min</b></span>
					</td>
					<td align="right"><span class="oddelki_forum_mala">(174 votes)</span></td></tr>
							 */

							var title_tag = "<div align=\"left\">";
							var title_end_tag = "</div>";

							var title_i = data.IndexOf(title_tag);

							if (title_i < 0)
								return offset;

							title_i = data.IndexOf(title_tag, title_i + title_tag.Length);

							var title_end_i = data.IndexOf(title_end_tag, title_i);

							//  Lost <b>(2004)</b>
							var title = ParseLink(data.Substring(title_i + title_tag.Length, title_end_i - title_i - title_tag.Length));

							var genre_tag = "<span class=\"oddelki_forum_mala\">";
							var genre_i = data.IndexOf(genre_tag, title_end_i);
							var genre_end_tag = "</span>";
							var genre_end_i = data.IndexOf(genre_end_tag, genre_i);
							// Genre: <b>Drama, Adventure, Mystery, Thriller</b> Duration: <b>45 min</b>
							var genre = data.Substring(genre_i + genre_tag.Length, genre_end_i - genre_i - genre_tag.Length);

							var e = new AliasEntry
							{
								Genres = genre.Substring("Genre: <b>", "</b>").Split(new[] { ',' }).Trim(),
								Duration = genre.Substring("Duration: <b>", "</b>"),
								Link = "http://www.omdb.si" + title.Link,
								Title = title.Text.Substring(0, title.Text.IndexOf("<")),
								Year = title.Text.Substring("<b>", "</b>")
							};

							handler(e);

							return tirgger_end_i + trigger_end_tag.Length;
						};

					var start_tag = "<td align=\"right\" class=\"bM\">";
					var start_offset = document.IndexOf(start_tag);
					start_offset = document.IndexOf(start_tag, start_offset);

					scan.ToChainedFunc((x, y) => y > x)(start_offset);
				};

			// we will only look at the first result page
			c.Crawl("/index.php/odefault/search?sK=" + stitle.URLEncode());
		}


	
	}
}
