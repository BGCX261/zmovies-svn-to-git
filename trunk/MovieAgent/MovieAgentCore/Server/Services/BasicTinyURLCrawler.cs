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
	public class BasicTinyURLCrawler
	{
		// http://tinyurl.com/create.php?url=thepiratebay.org
		public readonly BasicWebCrawler Crawler;

		[Script]
		public class Entry
		{
			public string Alias;

			public string AliasKey
			{
				get
				{
					return Alias.ToUri().PathAndQuery.Substring(1);
				}
			}

			public string URL;


		}


		public event Action<Entry> AddEntry;

		public bool APIMode = true;

		public BasicTinyURLCrawler()
		{
			this.Crawler =
				new Library.BasicWebCrawler("tinyurl.com", 80)
				{
					//CoralEnabled = true
				};

			this.Crawler.DataReceived +=
				document =>
				{
					var entry = new Entry();

					if (APIMode)
					{
						entry.Alias = document;
					}
					else
					{
						var trigger = "<h1>TinyURL was created!</h1>";

						var trigger_i = document.IndexOf(trigger);

						if (trigger_i < 0)
							return;

						// we are still in the business...

						//<h1>TinyURL was created!</h1>
						//<p>The following URL:
						//<blockquote><b>http://thepiratebay.org<br />
						//</b></blockquote>
						//has a length of 23 characters and resulted in the following TinyURL which has a length of 24 characters:
						//<blockquote><b>http://tinyurl.com/5umsn</b><br><small>[<a href="http://tinyurl.com/5umsn" target="_blank">Open in new window</a>]</small></blockquote>
						//Or, give your recipients confidence with a preview TinyURL:
						//<blockquote><b>http://preview.tinyurl.com/5umsn</b><br><small>[<a href="http://preview.tinyurl.com/5umsn" target="_blank">Open in new window</a>]</small>

						//</blockquote>
						//</p>

						var start_tag = "<p>";
						var start_i = document.IndexOf(start_tag, trigger_i);

						if (start_i < 0)
							return;

						var end_tag = "</p>";
						var end_i = document.IndexOf(end_tag, start_i);

						var data = document.Substring(start_i + start_tag.Length, end_i - start_i + start_tag.Length);



						BasicElementParser.Parse(data, "blockquote",
							(value, index) =>
							{
								if (index == 0)
								{
									entry.URL = BasicElementParser.GetContent(value, "b");

									var br_tag = "<br />";
									var br_i = entry.URL.IndexOf(br_tag);

									if (br_i >= 0)
										entry.URL = entry.URL.Substring(0, br_i);


									return;
								}

								if (index == 1)
								{
									entry.Alias = BasicElementParser.GetContent(value, "b");

									return;
								}
							}
						);
					}

					if (this.AddEntry != null)
						this.AddEntry(entry);
				};
		}

		public void Search(string URL)
		{
			if (APIMode)
				this.Crawler.Crawl("/api-create.php?url=" + URL);
			else
				this.Crawler.Crawl("/create.php?url=" + URL);
		}

		public static void Search(string URL, Action<Entry> AddEntry)
		{
			var t = new BasicTinyURLCrawler
			{
			};

			t.AddEntry +=
				entry =>
				{
					entry.URL = URL;

					if (AddEntry != null)
						AddEntry(entry);
				};

			t.Search(URL);

		}
	}
}
