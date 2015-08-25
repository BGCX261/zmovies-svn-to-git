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
	public class BasicIMDBPosterSearch
	{
		public event Action<string> AddEntry;

		public readonly BasicWebCrawler Crawler;

		public BasicIMDBPosterSearch()
		{
			this.Crawler =
				new Library.BasicWebCrawler("www.imdb.com", 80)
				{
					CoralEnabled = true
				};



			var DefaultImage = new { Source = "", Alt = "", Title = "" };

			var ParseImage = DefaultImage.ToAnonymousConstructor(
				(string element) =>
				{
					var Source = "";
					var Alt = "";
					var Title = "";

					element.
						ParseAttribute("src", value => Source = value).
						ParseAttribute("alt", value => Alt = value).
						ParseAttribute("title", value => Title = value).
						ParseContent(null).
						Parse();

					return new { Source, Alt, Title };
				}
			);


			string location = null;

			this.Crawler.AllHeadersSent +=
				() =>
				{
					location = null;
				};

			this.Crawler.LocationReceived +=
				value =>
				{
					location = value;
				};

			this.Crawler.DataReceived +=
				document =>
				{
					if (!string.IsNullOrEmpty(location))
					{
						var u = new Uri(location);

						this.Crawler.Crawl(u.PathAndQuery);

						return;
					}

					var poster_tag = "<table id=\"principal\">";
					var poster_i = document.IndexOf(poster_tag);
					var poster_close_tag = "</table>";
					var poster_close_i = document.IndexOf(poster_close_tag, poster_i);

					var poster = ParseImage( 
						BasicElementParser.GetContent(
							document.Substring(poster_i, poster_close_i + poster_close_tag.Length - poster_i)
						, "td")
					);

					if (this.AddEntry != null)
						this.AddEntry(poster.Source);
				};

		}

		public void Search(string Path)
		{
			this.Crawler.Crawl(Path);
		}

		public static void Search(string Path, Action<string> AddEntry)
		{
			var t = new BasicIMDBPosterSearch();

			t.AddEntry += AddEntry;

			t.Search(Path);

		}
	}
}
