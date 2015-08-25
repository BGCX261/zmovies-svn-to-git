using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Shared;

namespace MovieAgentGadget.Data
{
	[Script]
	public sealed class MovieItem
	{
		public int FeedIndex;
		public int FeedCapacity;

		public string YouTubeKey = "";
		public string IMDBLink = "";
		public string PosterLink = "";
		public string TorrentCommentLink = "";
		public string TorrentLink = "";
		public string TorrentName = "";
		public string SmartTitle = "";
		public string IMDBRaiting = "";
		public string IMDBRuntime = "";
		public string IMDBTagline = "";
		public string IMDBGenres = "";
		public string Episode = "";

		public double Raiting
		{
			get
			{
				var value = 1.0;

				var x = IMDBRaiting.IndexOf("/");
				if (x < 0)
					return value;

				var c = int.Parse(IMDBRaiting.Substring(0, x));
				var m = int.Parse(IMDBRaiting.Substring(x + 1));

				return c / m;
			}
		}

		public string SmartTitleWithoutQuotes
		{
			get
			{
				return SmartTitle.Replace("&quot;", "");
			}
		}

		public string ToDetails()
		{
			var w = new StringBuilder();

			w.Append(
				("<img " + this.PosterLink.ToAttributeString("src")
				+ " " + "0".ToAttributeString("border")
				+ " " + "right".ToAttributeString("align")
				+ " />").ToLink(this.IMDBLink)
			);

			w.Append(
				("<h3>" + this.SmartTitle + "</h3>").ToLink(TorrentCommentLink)
			);

			Action<string, string> AddDetail =
				(key, value) =>
				{
					w.Append(
						("<div " + key.ToAttributeString("title") + "><small>" + value + "</small></div>")
					);
				};

			AddDetail("raiting", this.IMDBRaiting);
			AddDetail("runtime", this.IMDBRuntime);
			AddDetail("tagline", this.IMDBTagline);
			AddDetail("genres", this.IMDBGenres);
			AddDetail("episode", this.Episode);

			w.Append(
				(("<img " + "http://static.thepiratebay.org/img/dl.gif".ToAttributeString("src")
				+ " " + "0".ToAttributeString("border")
				+ " />") + " " + this.TorrentName).ToLink(this.TorrentLink)
			);

			return w.ToString();
		}
	}

	[Script]
	public static class MovieItemExtension
	{
		public static void ParseMovieItem(this string data, Action<MovieItem> handler)
		{
			var n = new MovieItem();

			var DefaultLink = new { Link = "", Title = "", Text = "" };
			var DefaultImage = new { Source = "", Alt = "", Title = "" };
			var DefaultHeader = new { Title = "", Text = "" };

			var ParseHeader = DefaultHeader.ToAnonymousConstructor(
				(string element) =>
				{
					var Title = "";
					var Text = "";

					element.
						ParseAttribute("title", value => Title = value).
						ParseContent(value => Text = value).
						Parse();

					return new { Title, Text };
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
						Parse("img");

					return new { Source, Alt, Title };
				}
			);



			data.ParseElements(
				(tag, index, element) =>
				{
					if (tag == "h2")
					{
						var h = ParseHeader(element);
						var a = ParseLink(h.Text);

						if (a != null)
						{
							n.TorrentCommentLink = a.Link;
							n.SmartTitle = a.Text;
						}
					}
					else if (tag == "div")
					{
						var h = ParseHeader(element);

						if (h.Title == "raiting") n.IMDBRaiting = h.Text;
						else if (h.Title == "runtime") n.IMDBRuntime = h.Text;
						else if (h.Title == "tagline") n.IMDBTagline = h.Text;
						else if (h.Title == "genres") n.IMDBGenres = h.Text;
						else if (h.Title == "episode") n.Episode = h.Text;
					}
					else if (tag == "a")
					{
						var a = ParseLink(element);
						var img = ParseImage(a.Text);

						if (img != null)
						{
							if (a.Link.StartsWith("http://www.youtube.com"))
							{
								n.YouTubeKey = img.Alt;
							}
							else if (a.Link.StartsWith("http://www.imdb.com"))
							{
								n.IMDBLink = a.Link;
								n.PosterLink = img.Source;
							}
							else if (a.Link.StartsWith("http://tinyurl.com"))
							{
								n.TorrentLink = a.Link;
								n.TorrentName = a.Text.Substring(a.Text.IndexOf(">") + 1).Trim();
							}
						}
					}
				}


			);

			handler(n);
		}
	}
}
