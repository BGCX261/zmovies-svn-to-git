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
	public class BasicIMDBAliasSearch
	{
		[Script]
		public class AlsoKnownAs
		{
			public string Text;

			public AlsoKnownAs Alias;
		}

		[Script]
		public class Entry
		{
			// for tv shows
			// http://www.imdb.com/title/tt0411008/episodes

			public string OptionalReleaseDate;
			public AlsoKnownAs OptionalAlias;
			public string OptionalTitle;

			/// <summary>
			/// There might be no image when there was a redirect,
			/// or there is simply no image.
			/// </summary>
			public string OptionalImage;


			/// <summary>
			/// The link property is the only property guarantined to be assigned
			/// </summary>
			public string Link { get; private set; }

			public string Key
			{
				get
				{
					var Link = new Uri(this.Link);
					var Segments = Link.Segments;
					var Key = Segments[2];
					Key = Key.Substring(0, Key.Length - 1);

					return Key;
				}
			}
			public Entry(string Link)
			{
				this.Link = Link;
			}

		}

		public event Action<Entry, int> AddEntry;

		public readonly BasicWebCrawler Crawler;

		public readonly string Host = "www.imdb.com";

		public BasicIMDBAliasSearch()
		{
			var c = new BasicWebCrawler(Host, 80)
			{
				// doesnt seem to respond at 2009.03.18
				//CoralEnabled = true
			};

			this.Crawler = c;

			var DefaultLink = new { Link = "", Title = "", Text = "" };
			var DefaultImage = new { Source = "", Alt = "", Title = "", width = "", height = "" };

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
					var width = "";
					var height = "";

					element.
						ParseAttribute("src", value => Source = value).
						ParseAttribute("alt", value => Alt = value).
						ParseAttribute("title", value => Title = value).
						ParseAttribute("width", value => width = value).
						ParseAttribute("height", value => height = value).
						ParseContent(null).
						Parse();

					return new { Source, Alt, Title, width, height };
				}
			);

			var EntryIndex = -1;

			#region AddItem
			Action<string, string> AddItem =
				(ImageElement, Content) =>
				{
					var ImageSource = "";

					if (ImageElement.StartsWith("<a"))
					{
						var ImageLink = ParseLink(ImageElement);
						var Image = ParseImage(ImageLink.Text);

						ImageSource = Image.Source;
					}

					/*
					 * <img src="/images/b.gif" width="1" height="6"><br>
					 * <a href="/title/tt0397892/" onclick="(new Image()).src='/rg/find-title-1/title_popular/images/b.gif?link=/title/tt0397892/';">Bolt</a> (2008)    <br>
					 * &#160;aka <em>"Bolt - Pes pro kazd&#253; pr&#237;pad"</em> - Czech Republic<br>
					 * &#160;aka <em>"Bolt - Un perro fuera de serie 3D"</em> - Chile<br>
					 * &#160;aka <em>"Bolt - Superc&#227;o"</em> - Brazil<br>
					 * &#160;aka <em>"Bolt - Un perro fuera de serie"</em> - Argentina, Mexico<br>
					 * &#160;aka <em>"Bolt - Ein Hund f&#252;r alle F&#228;lle"</em> - Germany 
					 */

					var ContentLink_start = Content.IndexOf("<a");
					var ContentLink_end = Content.IndexOf("</a>");
					var ContentLink = ParseLink(Content.Substring(ContentLink_start, ContentLink_end - ContentLink_start + 4));

					var Details = Content.Substring(ContentLink_end + 4);

					var ReleaseDate = "";
					var Alias = default(AlsoKnownAs);

					Details.Split("<br>",
						(text, index) =>
						{
							if (index == 0)
							{
								ReleaseDate = text;
								return;
							}

							Alias = new AlsoKnownAs
							{
								Text = text,
								Alias = Alias
							};
						}
					);

					EntryIndex++;

					if (this.AddEntry != null)
					{
						this.AddEntry(
							new Entry("http://" + Host + ContentLink.Link)
							{
								OptionalAlias = Alias,
								OptionalReleaseDate = ReleaseDate,
								OptionalTitle = ContentLink.Text,
								OptionalImage = ImageSource
							},
							EntryIndex
						);
					}
				};
			#endregion

			// http://www.imdb.com/find?s=tt;site=aka;q=The%20Dark%20Knight

			//const string Header_Location = "Location: ";

			string Redirect = null;

			// Location: http://www.imdb.com/title/tt1129442/
			c.LocationReceived +=
				href =>
				{
					//Console.WriteLine("LocationReceived.");

					Redirect = href;
				};


			c.DataReceivedWithTimeSpan +=
				(document, elapsed) =>
				{
					//Console.WriteLine("DataReceivedWithTimeSpan.");

					#region redirect
					if (!string.IsNullOrEmpty(Redirect))
					{
						EntryIndex++;
						if (this.AddEntry != null)
						{
							this.AddEntry(
								new Entry(Redirect)
								{
								},
								EntryIndex
							);

						}

						return;
					}
					#endregion

					var approx_section = document.IndexOf("<b>Titles (Approx Matches)</b>");
					var exact_section = document.IndexOf("<b>Titles (Exact Matches)</b>");
					var popular_section = document.IndexOf("<b>Popular Titles</b>");

					var first_section = popular_section;

					if (first_section < 0)
						first_section = exact_section;

					if (first_section < 0)
						first_section = approx_section;


					if (first_section < 0)
						return;

					var section_start = document.IndexOf("<table>", first_section);
					var section_end = document.IndexOf("</table>", section_start);
					var section = document.Substring(section_start, section_end - section_start + 8);

					BasicElementParser.Parse(section, "tr",
						(tr, tr_index) =>
						{
							/*
 <td valign="top">
<a href="/title/tt0397892/" onClick="(new Image()).src='/rg/find-tiny-photo-1/title_popular/images/b.gif?link=/title/tt0397892/';"><img src="http://ia.media-imdb.com/images/M/MV5BNDQyNDE5NjQ1N15BMl5BanBnXkFtZTcwMDExMTAwMg@@._V1._SY30_SX23_.jpg" width="23" height="32" border="0"></a>&nbsp;</td>
<td align="right" valign="top"><img src="/images/b.gif" width="1" height="6"><br>1.</td>
<td valign="top"><img src="/images/b.gif" width="1" height="6"><br><a href="/title/tt0397892/" onclick="(new Image()).src='/rg/find-title-1/title_popular/images/b.gif?link=/title/tt0397892/';">Bolt</a> (2008)    <br>&#160;aka <em>"Bolt - Pes pro kazd&#253; pr&#237;pad"</em> - Czech Republic<br>&#160;aka <em>"Bolt - Un perro fuera de serie 3D"</em> - Chile<br>&#160;aka <em>"Bolt - Superc&#227;o"</em> - Brazil<br>&#160;aka <em>"Bolt - Un perro fuera de serie"</em> - Argentina, Mexico<br>&#160;aka <em>"Bolt - Ein Hund f&#252;r alle F&#228;lle"</em> - Germany </td>
							 * 
							 */

							var Image = "";
							var Content = "";

							BasicElementParser.Parse(tr, "td",
								(td, td_index) =>
								{
									if (td_index == 0)
										Image = td;

									if (td_index == 2)
										Content = td;
								}
							);

							AddItem(Image, Content);
						}
					);



				};

			//c.Crawl("/find?s=tt;site=aka;q=" + "The Dark Knight".URLEncode());
			//c.Crawl("/find?s=tt;site=aka;q=" + "Bolt".URLEncode());

		}

		public void Search(string Title)
		{
			this.Crawler.Crawl("/find?s=tt;site=aka;q=" + Title.URLEncode());
		}

		public static void Search(string Title, Action<Entry, int> AddEntry)
		{
			var c = new BasicIMDBAliasSearch();

			c.AddEntry += AddEntry;

			c.Search(Title);
		}

		public static void SearchSingle(string Title, string Year, Action<Entry> AddEntry)
		{
			var c = new BasicIMDBAliasSearch();

			var x = default(Entry);

			c.AddEntry +=
				(e, i) =>
				{
					if (x == null)
					{
						x = e;
					}
					else
					{
						if (!string.IsNullOrEmpty(Year))
							if (!string.IsNullOrEmpty(e.OptionalReleaseDate))
								if (e.OptionalReleaseDate.Contains(Year))
									x = e;
					}

				};

			c.Search(Title);

			if (x != null)
				AddEntry(x);
		}
	}
}
