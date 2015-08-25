using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;

namespace MovieAgent.Server.Services
{
	[Script]
	public class BasicTinEyeSearch
	{
		public readonly BasicWebCrawler Crawler;

		[Script]
		public class Entry
		{
			public string Hash;

			public string QueryLink
			{
				get
				{
					return "http://tineye.com/query/" + Hash;
				}
			}

			public string SearchLink
			{
				get
				{
					return "http://tineye.com/search/" + Hash;
				}
			}

		}

		public event Action<Entry> AddEntry;

		public BasicTinEyeSearch()
		{


			this.Crawler =
				new Library.BasicWebCrawler("tineye.com", 80)
				{
					//CoralEnabled = true,
					Method = "HEAD"
				};

			this.Crawler.LocationReceived +=
				value =>
				{
					var tag = "/search/";
					var i = value.LastIndexOf(tag);

					if (i < 0)
						return;

					var hash = value.Substring(i + tag.Length);

					var n = new Entry
					{
						Hash = hash,

					};

					if (AddEntry != null)
						AddEntry(n);
				};
		}

		public void Search(string ImageLink)
		{
			this.Crawler.Crawl("/search/?url=" + ImageLink);
		}


		public static void Search(string ImageLink, Action<Entry> AddEntry)
		{
			var t = new BasicTinEyeSearch();

			t.AddEntry += AddEntry;

			t.Search(ImageLink);

		}

		public static byte[] ToBytes(Uri uri)
		{
			var x = default(byte[]);
			BasicTinEyeSearch.Search(uri.ToString(),
				e =>
				{
					x = BasicWebCrawler.ToBytes(new Uri(e.QueryLink));

				}
			);
			return x;


		}
	}
}
