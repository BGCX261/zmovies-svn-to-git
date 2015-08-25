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
	public class BasicMPDBCrawler
	{
		// http://www.movieposterdb.com/movie/1179855/Go-Fast.html

		[Script]
		public class AliasEntry
		{
			public string Link;
			public string Title;
			public string Year;

			public void GetPoster(Action<string> handler)
			{
				var uri = Link.ToUri();

				var c = new BasicWebCrawler(uri.Host, 80);

				c.DataReceived +=
					document =>
					{
						var prefix = "http://www.movieposterdb.com/posters/";

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

		public static void Search(string title, Action<AliasEntry> handler)
		{
			var t = new Uri("http://www.movieposterdb.com/browse/search?search_type=movies&title=");
			var c = new BasicWebCrawler(t.Host, 80);

			var DefaultLink = new { Link = "", Title = "", Text = "" };
			var DefaultSpan = new { Text = "", Title = "" };

			var ParseSpan = DefaultSpan.ToAnonymousConstructor(
				(string element) =>
				{
					var Text = "";
					var Title = "";

					element.
						ParseAttribute("title", value => Title = value).
						ParseContent(value => Text = value).
						Parse("span");

					return new { Text, Title };
				}
			);

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
						Parse("a");

					return new { Link, Title, Text };
				}
			);

			c.DataReceived +=
				document =>
				{
					var trigger = "Movies</h3>";

					var trigger_i = document.IndexOf(trigger);

					var data = BasicElementParser.GetContent(document.Substring(trigger_i), "table");

					BasicElementParser.Parse(data, "tr",
						(element, index) =>
						{

							/*
	<td valign="middle" style="font-size: 0pt; border-bottom: 1px solid #D2D2D2; height: 54px; width: 44px;">
			<img src="http://www.movieposterdb.com/posters/08_09/2008/1179855/m_1179855_4fb9999f.jpg" style="margin-right: 8px; padding: 2px; border: 1px solid #D2D2D2; float: left;" />
	</td>
	<td valign="middle" style="border-bottom: 1px solid #D2D2D2; width: 60%;">
			<b><a class="bbg" href="http://www.movieposterdb.com/movie/1179855/Go-Fast.html">Go Fast</a><br /><span style="color: #8C8C8C;">2008</span></b>
	</td>
	<td style="border-bottom: 1px solid #D2D2D2; font-size: 8pt; color: #808080;">

	</td>
							 */

							BasicElementParser.Parse(element, "td",
								(tdelement, tdindex) =>
								{
									if (tdindex == 1)
									{
										// <b><a class="bbg" href="http://www.movieposterdb.com/movie/1179855/Go-Fast.html">Go Fast</a><br /><span style="color: #8C8C8C;">2008</span></b>
										var _title = ParseLink(tdelement);
										var _year = ParseSpan(tdelement);

										handler(
											new AliasEntry
											{
												Link = _title.Link,
												Title = _title.Text,
												Year = _year.Text
											}
										);
									}
								}
							);
						}
					);
				};

			c.Crawl(t.PathAndQuery + title.URLEncode());

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

	}
}
