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
	public class BasicPirateBayImage
	{
		[Script]
		public class Entry
		{
			public readonly string Key;

			public Entry(string Key)
			{
				this.Key = Key;

				this.Image = ("http://bayimg.com/image/" + Key + ".jpg").ToUri();
				this.Thumbnail = ("http://bayimg.com/thumb/" + Key + ".jpg").ToUri();
			}

			public readonly Uri Image;
			public readonly Uri Thumbnail;
		}

		public event Action<Entry> AddEntry;

		readonly BasicWebCrawler CrawlerDownloader;
		readonly BasicWebCrawler CrawlerUploader;

		public BasicPirateBayImage()
		{
			this.CrawlerDownloader =
				new Library.BasicWebCrawler("", 80)
				{
					//CoralEnabled = true
				};

			this.CrawlerUploader =
				new Library.BasicWebCrawler("bayimg.com", 80)
				{
					Method = "POST"
				};

			#region parser
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
			#endregion

			// http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
			var boundary = "---------------------------" + int.MaxValue.Random();
			var current_filename = "_" + int.MaxValue.Random();

			#region StreamWriter
			Action<StreamWriter, Stream, int, string> StreamWriter =
				 (stream, source, sourcelength, filename) =>
				 {
					 stream.AutoFlush = true;

					 stream.WriteLine("--" + boundary);
					 stream.WriteLine("Content-Disposition: form-data; name=\"file\"; filename=\"" + filename + "\"");
					 stream.WriteLine("Content-Type: application/octet-stream");
					 stream.WriteLine();

					 if (source == null)
					 {
						 stream.BaseStream.Position += sourcelength;
					 }
					 else
					 {
						 var buffer = new byte[0x1000];
						 var offset = 0;
						 var size = source.Read(buffer, 0, buffer.Length);

						 while (size > 0)
						 {
							 //Console.WriteLine(new { offset, size });

							 stream.BaseStream.Write(buffer, 0, size);
							 offset += size;
							 size = source.Read(buffer, 0, buffer.Length);
						 }
					 }

					 stream.WriteLine();

					 stream.WriteLine("--" + boundary);
					 stream.WriteLine("Content-Disposition: form-data; name=\"code\"");
					 stream.WriteLine();
					 stream.WriteLine("tpb");

					 stream.WriteLine("--" + boundary);
					 stream.WriteLine("Content-Disposition: form-data; name=\"tags\"");
					 stream.WriteLine();
					 stream.WriteLine("");

					 stream.WriteLine("--" + boundary + "--");
				 };
			#endregion

			this.CrawlerDownloader.ContentLengthReceived +=
				ContentLength =>
				{
					current_filename = "_" + int.MaxValue.Random();

					var value = int.Parse(ContentLength);

					this.CrawlerUploader.HeaderWriter +=
					   stream =>
					   {
						   stream.WriteLine("Content-Type: multipart/form-data; boundary=" + boundary);

						   using (var v = new StreamWriter(new VoidStream()))
						   {

							   StreamWriter(v, null, value, current_filename);

							   stream.WriteLine("Content-Length: " +
								   v.BaseStream.Position
							   );
						   }
					   };
				};

			this.CrawlerDownloader.StreamReader +=
				source =>
				{
					this.CrawlerUploader.StreamWriter +=
						stream => StreamWriter(stream, source, 0, current_filename);

					this.CrawlerUploader.Crawl("/upload");
				};

			this.CrawlerUploader.DataReceived +=
			   document =>
			   {
				   var result_tag = "<div id=\"extra2\">";
				   var result_i = document.IndexOf(result_tag);
				   var result_end_tag = "<br/>";
				   var result_end_i = document.IndexOf(result_end_tag, result_i);

				   var data = document.Substring(result_i + result_tag.Length, result_end_i - (result_i + result_tag.Length)).Trim();

				   // http://bayimg.com/image/eaofgaabg.jpg

				   var Link = ParseLink(data);
				   var ThumbnailImage = ParseImage(Link.Text);


				   if (this.AddEntry != null)
					   this.AddEntry(new Entry(Link.Link.Substring(1).ToLower()));
				   //new IHTMLImage { Source = ImageLink, Title = imdb.SmartTitle }.ToString().ToConsole();

				   //Console.WriteLine(ImageHTML);
				   //Console.WriteLine(ThumbnailImageHTML);
			   };
		}

		public void Clone(Uri source)
		{
			this.CrawlerDownloader.Crawl(source);
		}

		public static void Clone(Uri source, Action<Entry> AddEntry)
		{
			var t = new BasicPirateBayImage();

			t.AddEntry += AddEntry;

			t.Clone(source);

		}
	}
}
