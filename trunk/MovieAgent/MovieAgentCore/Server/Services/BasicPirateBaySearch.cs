using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;
using System.IO;
using MovieAgent.Shared;

namespace MovieAgent.Server.Services
{
	[Script]
	public class BasicPirateBaySearch
	{
		public readonly BasicWebCrawler Crawler;

		public BasicPirateBaySearch()
		{
			this.Crawler = new BasicWebCrawler("thepiratebay.org", 80);

			this.Crawler.DataReceived +=
				document =>
				{
					var results = document.IndexOf("<table id=\"searchResult\">");
					var headend = document.IndexOf("</thead>", results);
					var results_end = document.IndexOf("</table>", headend);

					int entryindex = -1;

					Action<Action<Entry, int>> ForEachEntry =
						AddEntry =>
						{
							#region ScanSingleResultOrReturn
							Func<int, int> ScanSingleResultOrReturn =
								offset =>
								{

									var itemstart = document.IndexOf("<tr>", offset);

									if (itemstart < 0)
										return offset;

									if (itemstart > results_end)
										return offset;

									var itemend = document.IndexOf("</tr>", itemstart);

									if (itemend < 0)
										return offset;

									if (itemend > results_end)
										return offset;

									var itemdata = document.Substring(itemstart, itemend - itemstart);



									//<tr>
									//<td class="vertTh"><a href="/browse/205" title="More from this category">Video &gt; TV shows</a></td>
									//<td><a href="/torrent/4727946/Heroes.S03E16.HDTV.XviD-XOR.avi" class="detLink" title="Details for Heroes.S03E16.HDTV.XviD-XOR.avi">Heroes.S03E16.HDTV.XviD-XOR.avi</a></td>
									//<td>Today&nbsp;04:55</td>
									//<td><a href="http://torrents.thepiratebay.org/4727946/Heroes.S03E16.HDTV.XviD-XOR.avi.4727946.TPB.torrent" title="Download this torrent"><img src="http://static.thepiratebay.org/img/dl.gif" class="dl" alt="Download" /></a><img src="http://static.thepiratebay.org/img/icon_comment.gif" alt="This torrent has 22 comments." title="This torrent has 22 comments." /><img src="http://static.thepiratebay.org/img/vip.gif" alt="VIP" title="VIP" style="width:11px;" /></td>
									//<td align="right">348.97&nbsp;MiB</td>
									//<td align="right">47773</td>
									//<td align="right">60267</td>

									//Console.WriteLine("<h1>Most Popular video</h1>");
									//Console.WriteLine("<table>");

									// type, name, uploaded, links, size, se, le

									var Fields = new BasicPirateBaySearch.Entry();

									Action<string> SetField = null;

									SetField = Type =>
									SetField = Name =>
									SetField = Time =>
									SetField = Links =>
									SetField = Size =>
									SetField = Seeders =>
									SetField = Leechers =>
									{

										Fields = new BasicPirateBaySearch.Entry
										{
											Type = Type,
											Name = Name,
											Time = Time,
											Links = Links,
											Size = Size,
											Seeders = Seeders,
											Leechers = Leechers
										};

										SetField = delegate { };
									};


									var ep = new BasicElementParser();

									ep.AddContent +=
										(value, index) =>
										{
											//Console.WriteLine("AddContent start #" + index);
											SetField(value);
											//Console.WriteLine("AddContent stop #" + index);
										};

									ep.Parse(itemdata, "td");

									entryindex++;

									if (AddEntry != null)
										AddEntry(Fields, entryindex);



									return itemend + 5;
								};
							#endregion


							ScanSingleResultOrReturn.ToChainedFunc((x, y) => y > x)(headend);
						};

					if (this.Loaded != null)
						this.Loaded(ForEachEntry);

				};
		}

		[Script]
		public class Entry
		{
			public string Type;
			public string Name;
			public string Time;
			public string Links;
			public string Size;
			public string Seeders;
			public string Leechers;
		}


		public event ForEachCallback<Entry> Loaded;

		[Script]
		public class SearchEntry
		{
			public string Size;
			public string Seeders;
			public string Leechers;

			public string Name;
			public string TorrentLink;
			public string CommentText;
			public string Link;

			public string AbsoluteLink
			{
				get
				{
					return "http://piratebay.org" + Link;
				}
			}

			public BasicFileNameParser SmartName
			{
				get
				{
					return new BasicFileNameParser(Name);
				}
			}


			public string Hash
			{
				get
				{
					return this.Name.ToMD5Bytes().ToHexString();
				}
			}

		}

		public static void Search(Func<SearchEntry, bool> deferfilter, Action<SearchEntry, bool> handler)
		{
			Search(
				(entry, defer) =>
				{
					if (deferfilter(entry))
						defer(e => handler(e, true));
					else
						handler(entry, false);
				}
			);
		}

		public static void Search(Action<SearchEntry, Action<Action<SearchEntry>>> Handler)
		{
			var a = new List<Action>();

			Search(
				e =>
				{
					Handler(e,
						h =>
						{
							a.Add(() => h(e));
						}
					);
				}
			);

			foreach (var i in a)
			{
				i();
			}

			a.Clear();
		}

		public static void Search(Action<SearchEntry> Handler)
		{
			var DefaultLink = new { Link = "", Title = "", Text = "" };
			var DefaultImage = new { Source = "", Alt = "", Title = "" };

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





			var search = new BasicPirateBaySearch();


			search.Loaded +=
				ForEachEntry =>
				{

					ForEachEntry(
						(entry, entryindex) =>
						{
							var Type = ParseLink(entry.Type);
							var Name = ParseLink(entry.Name);
							var TorrentLink = DefaultLink;
							var Comment = DefaultImage;

							entry.Links.ParseElements(
								(tag, index, element) =>
								{
									if (tag == "a")
									{
										TorrentLink = ParseLink(element);
									}

									if (tag == "img")
									{
										var img = ParseImage(element);

										if (img.Title.Contains("comment"))
											Comment = img;

									}
								}
							);

							Handler(
								new SearchEntry
								{
									CommentText = Comment.Title,
									Size = entry.Size,
									Seeders = entry.Seeders,
									Leechers = entry.Leechers,
									Name = Name.Text,
									Link = Name.Link,
									TorrentLink = TorrentLink.Link
								}
							);


						}
					);

				};

			search.Crawler.Crawl("/top/200");
		}
	}


	[Script]
	public delegate void ForEachCallback<T>(Action<Action<T, int>> ForEach);

	[Script]
	public static class BasicPirateBaySearchExtensions
	{
		public static void ToFile(this BasicPirateBaySearch.SearchEntry e, FileInfo f)
		{
			using (var s = new StreamWriter(f.OpenWrite()))
			{
				ToFile(e, s);
			}
		}

		public static void ToFile(this BasicPirateBaySearch.SearchEntry e, StreamWriter s)
		{
			s.WriteLines(
							e.CommentText,
							e.Leechers,
							e.Link,
							e.Name,
							e.Seeders,
							e.Size,
							e.TorrentLink
						);
		}

		public static BasicPirateBaySearch.SearchEntry FromFile(this BasicPirateBaySearch.SearchEntry e, FileInfo f)
		{
			using (var s = new StreamReader(f.OpenRead()))
			{
				FromFile(e, s);
			}

			return e;
		}

		public static void FromFile(this BasicPirateBaySearch.SearchEntry e, StreamReader s)
		{
			e.CommentText = s.ReadLine();
			e.Leechers = s.ReadLine();
			e.Link = s.ReadLine();
			e.Name = s.ReadLine();
			e.Seeders = s.ReadLine();
			e.Size = s.ReadLine();
			e.TorrentLink = s.ReadLine();
		}

	}
}
