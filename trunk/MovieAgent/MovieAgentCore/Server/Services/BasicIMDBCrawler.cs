using System;
using System.Collections.Generic;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;
using System.IO;
using ScriptCoreLib.PHP;
using MovieAgent.Shared;

namespace MovieAgent.Server.Services
{
	[Script]
	public class BasicIMDBCrawler
	{
		// http://www.imdb.com/title/tt0479952/
		// http://cybernetnews.com/2008/02/23/cybernotes-three-imdbcom-alternatives/

		[Script]
		public class Entry
		{
			public string Title;
			public string Year;

			public string MediumPosterTitle;

			public string MediumPosterImageProvider;
			public string MediumPosterImage;
			public string MediumPosterImagePage;

			public string UserRating;

			public string[] Genres;

			public string Tagline;

			public string Runtime;


			public Uri MediumPosterImageCoralCache
			{
				get
				{
					if (MediumPosterImage == null)
						return null;

					return new Uri(MediumPosterImage).ToCoralCache();
				}
			}

			public string SmartTitle
			{
				get
				{
					return Title + " (" + Year + ") " + UserRating + " | " + Runtime + " | " + Tagline;
				}
			}
		}

		public event Action<Entry> AddEntry;

		public readonly BasicWebCrawler Crawler;

		public BasicIMDBCrawler()
		{
			this.Crawler =
				new Library.BasicWebCrawler("www.imdb.com", 80)
				{
					//CoralEnabled = true
				};

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

			this.Crawler.DataReceived +=
				document =>
				{
					var entry = new Entry();

					var title = BasicElementParser.GetContent(document, "title");
					var title_i = title.IndexOf("(");

					entry.Title = title.Substring(0, title_i).Trim();

					// remove qoutes from the title
					entry.Title = entry.Title.Replace("&#34;", "");


					entry.Year = title.Substring(title_i + 1, title.IndexOf(")", title_i + 1) - (title_i + 1));


					var poster_i = document.IndexOf("name=\"poster\"");

					// no poster - the poster may be found on other services
					if (poster_i < 0)
					{
					}
					else
					{


						var poster_j = document.Substring(0, poster_i).LastIndexOf("<a");
						var poster_q = document.IndexOf("</a>", poster_i);

						var poster = ParseLink(document.Substring(poster_j, poster_q - poster_j + 4));
						var poster_image = ParseImage(poster.Text);

						entry.MediumPosterImageProvider = "imdb";
						entry.MediumPosterImage = poster_image.Source;
						entry.MediumPosterImagePage = poster.Link;
						entry.MediumPosterTitle = poster.Title;
					}


					#region UserRating
					var meta_tag = "<div class=\"meta\">";
					var meta_i = document.IndexOf(meta_tag);

					if (meta_i < 0)
						entry.UserRating = "";
					else
					{
						var meta = document.Substring(meta_i + meta_tag.Length, document.IndexOf("</div>", meta_i) - meta_i - meta_tag.Length);

						entry.UserRating = BasicElementParser.GetContent(meta, "b");
					}
					#endregion

					#region Genres
					var genre_tag = "<h5>Genre:</h5>";
					var genre_i = document.IndexOf(genre_tag);
					var genres = new List<string>();

					if (genre_i < 0)
					{
					}
					else
					{
						var genre = document.Substring(genre_i + genre_tag.Length, document.IndexOf("</div>", genre_i) - genre_i - genre_tag.Length);

						BasicElementParser.Parse(genre, "a",
							(text, index) =>
							{
								if (text == "more")
									return;

								genres.Add(text);
							}
						);
					}

					entry.Genres = genres.ToArray();
					#endregion

					#region Runtime
					var runtime_tag = "<h5>Runtime:</h5>";
					if (genre_i < 0)
						genre_i = 0;

					var runtime_i = document.IndexOf(runtime_tag, genre_i);

					if (runtime_i < 0)
						entry.Runtime = "";
					else
					{
						var runtime = document.Substring(runtime_i + runtime_tag.Length, document.IndexOf("</div>", runtime_i) - runtime_i - runtime_tag.Length);

						entry.Runtime = runtime.Trim();
					}

					#endregion

					#region Tagline
					var Tagline_tag = "<h5>Tagline:</h5>";
					var Tagline_i = document.IndexOf(Tagline_tag, genre_i);

					if (Tagline_i < 0)
						entry.Tagline = "";
					else
					{
						var Tagline = document.Substring(Tagline_i + Tagline_tag.Length, document.IndexOf("<", Tagline_i + Tagline_tag.Length) - Tagline_i - Tagline_tag.Length);

						entry.Tagline = Tagline.Trim();
					}
					#endregion

					if (AddEntry != null)
						AddEntry(entry);
				};
		}


		public void Search(string Key)
		{
			this.Crawler.Crawl("/title/" + Key + "/");
		}

		public static void Search(string Key, Action<Entry> AddEntry)
		{
			var t = new BasicIMDBCrawler();

			t.AddEntry += AddEntry;

			t.Search(Key);

		}

		public static void Search(string Key, Action<Entry, BasicTinEyeSearch.Entry, BasicPirateBayImage.Entry> AddEntry)
		{
			Search(Key, null, AddEntry);
		}

		public static void Search(string Key, MemoryDirectory memory, Action<Entry, BasicTinEyeSearch.Entry, BasicPirateBayImage.Entry> AddEntry)
		{
			Search(Key,
				imdb =>
				{
					var _tineye = default(BasicTinEyeSearch.Entry);
					var _bayimg = default(BasicPirateBayImage.Entry);

					BasicTinEyeSearch.Search(imdb.MediumPosterImage,
						tineye =>
						{
							_tineye = tineye;

							var tineye_memory = default(DirectoryInfo);
							var bayimg_memory = default(DirectoryInfo);

							if (memory != null)
							{
								tineye_memory = memory[tineye.Hash];
								bayimg_memory = tineye_memory.FirstDirectoryOrDefault();

								if (bayimg_memory != null)
									_bayimg = new BasicPirateBayImage.Entry(bayimg_memory.Name);
							}

							if (_bayimg == null)
							{
								Native.API.set_time_limit(5);

								BasicPirateBayImage.Clone(tineye.QueryLink.ToUri(),
									bayimg =>
									{
										_bayimg = bayimg;

										if (tineye_memory != null)
										{
											bayimg_memory = tineye_memory.CreateSubdirectory(bayimg.Key);
										}
									}
								);
							}
						}
					);

					AddEntry(imdb, _tineye, _bayimg);
				}
			);

		}

		public static string ToLink(string Key)
		{
			return "http://www.imdb.com/title/" + Key + "/";
		}
	}

	[Script]
	public static class BasicIMDBCrawlerExtensions
	{
		public static void ToFile(this BasicIMDBCrawler.Entry e, FileInfo f)
		{
			using (var s = new StreamWriter(f.OpenWrite()))
			{
				ToFile(e, s);
			}
		}

		public static void ToFile(this BasicIMDBCrawler.Entry e, StreamWriter s)
		{
			s.WriteLines(
							string.Join("|", e.Genres),
							e.MediumPosterImage,
							e.MediumPosterImagePage,
							e.Runtime,
							e.Tagline,
							e.MediumPosterTitle,
							e.UserRating
						);
		}

		public static BasicIMDBCrawler.Entry FromFile(this BasicIMDBCrawler.Entry e, FileInfo f)
		{
			using (var s = new StreamReader(f.OpenRead()))
			{
				FromFile(e, s);
			}

			return e;
		}

		public static void FromFile(this BasicIMDBCrawler.Entry e, StreamReader s)
		{
			e.Genres = s.ReadLine().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			e.MediumPosterImage = s.ReadLine();
			e.MediumPosterImagePage = s.ReadLine();
			e.Runtime = s.ReadLine();
			e.Tagline = s.ReadLine();
			e.MediumPosterTitle = s.ReadLine();
			e.UserRating = s.ReadLine();
		}

	}
}
