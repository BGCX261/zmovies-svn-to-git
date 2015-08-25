using ScriptCoreLib;
using ScriptCoreLib.Shared;

using ScriptCoreLib.PHP;
using System;
using System.Text;
using System.IO;
using MovieAgent.Client.Avalon;
using MovieAgent.Client.Java;
using MovieAgent.Shared;
using System.Threading;
using MovieAgent.web.tasks;
using MovieAgent.Server.Library;
using System.Diagnostics;
using System.Collections.Generic;
using MovieAgent.Server.Services;

namespace MovieAgent.Server
{
	partial class Application
	{
		private static void DemoMPDB()
		{
			BasicMPDBCrawler.SearchSingle("go fast", "2008",
				e =>
				{
					e.GetPoster(
						p =>
						{
							// cached by tinysrc
							// cached by coral cache
							// redirected by tinyurl...

							var t = p.ToUri().ToCoralCache();

							BasicTinyURLCrawler.Search(t.ToString(),
								ey =>
								{
									BasicTinyURLCrawler.Search("http://i.tinysrc.mobi.nyud.net/" + ey.Alias,
										ex =>
										{
											var protected_poster = ex.Alias;

											var text = new
											{
												e.Title,
												e.Year,
												Link = e.Link.ToLink(),

												Poster = protected_poster.ToImage()
											};

											Console.WriteLine(
												text.ToString()
											);
										}
									);
								}
							);
						}
					);
				}
			);
		}


		private static void DemoOMDB()
		{
			BasicOMDBCrawler.SearchSingle("Being Erica", "2008",
				 e =>
				 {
					 e.GetPoster(
						 p =>
						 {
							 // cached by tinysrc
							 // cached by coral cache
							 // redirected by tinyurl...

							 BasicTinyURLCrawler.Search("http://i.tinysrc.mobi.nyud.net/" + p,
								 ex =>
								 {
									 var protected_poster = ex.Alias;

									 var text = new
									 {
										 e.Title,
										 e.Year,
										 Link = e.Link.ToLink(),

										 Poster = protected_poster.ToImage()
									 };

									 Console.WriteLine(
										 text.ToString()
									 );
								 }
							 );
						 }
					 );
				 }
			 );
		}

		private static void DemoGooTube()
		{
			BasicGooTubeCrawler.Search("http://www.youtube.com/watch?v=yXqbmRLlhtE",
						flv =>
						{
							"flv".ToLink(flv).ToConsole();
						}
					);
		}


		private static void DemoCreateTinyArrowsToLetMeGoogleThatForYou()
		{
			BasicTinyArrowsCrawler.Spawn("http://lmgtfy.com/?q=tinyarrows&l=1",
				k =>
				{
					BasicTinyURLCrawler.Search(k,
						e =>
						{
							e.Alias.ToLink().ToConsole();
						}
					);
				}
			);
		}

		private static void DemoGetPosterViaTinEyeAndStreamItViaPirateBayImage(string MovieTitle, string Year, MemoryDirectory memory)
		{
			Console.WriteLine();
			Console.WriteLine(MovieTitle);


			BasicIMDBAliasSearch.SearchSingle(MovieTitle, Year,
				 e =>
				 {
					 BasicIMDBCrawler.Search(e.Key, memory,
						 (imdb, tineye, bayimg) =>
						 {

							 new IHTMLImage
							 {
								 src = bayimg.Image.ToString(),
								 title = imdb.SmartTitle
							 }.ToString().ToLink(
								BasicIMDBCrawler.ToLink(e.Key)
							 ).ToConsole();
						 }
					 );
				 }
			 );
		}

		private static void DemoGetPosterViaTinEyeAndStreamIt(string MovieTitle)
		{
			Console.WriteLine();
			Console.WriteLine(MovieTitle);

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

			BasicIMDBAliasSearch.Search(MovieTitle,
				 (e, i) =>
				 {
					 Console.WriteLine("BasicIMDBAliasSearch");
					 if (i > 0)
						 return;

					 BasicIMDBCrawler.Search(e.Key,
						 imdb =>
						 {
							 Console.WriteLine("BasicIMDBCrawler");
							 BasicTinEyeSearch.Search(imdb.MediumPosterImage,
								 tineye =>
								 {
									 Console.WriteLine("BasicTinEyeSearch");
									 // yay, we have the thumbnail, get it

									 #region downloader
									 var downloader = new BasicWebCrawler(tineye.QueryLink.ToUri().Host, 80);
									 var uploader = new BasicWebCrawler("bayimg.com", 80)
									 {
										 Method = "POST"
									 };

									 // http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
									 var boundary = "---------------------------" + int.MaxValue.Random();

									 #region StreamWriter
									 Action<StreamWriter, Stream, int> StreamWriter =
										  (stream, source, sourcelength) =>
										  {
											  stream.AutoFlush = true;

											  stream.WriteLine("--" + boundary);
											  stream.WriteLine("Content-Disposition: form-data; name=\"file\"; filename=\"" + tineye.Hash + "\"");
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

									 downloader.ContentLengthReceived +=
										 ContentLength =>
										 {
											 var value = int.Parse(ContentLength);

											 uploader.HeaderWriter +=
												stream =>
												{
													stream.WriteLine("Content-Type: multipart/form-data; boundary=" + boundary);

													using (var v = new StreamWriter(new VoidStream()))
													{

														StreamWriter(v, null, value);

														stream.WriteLine("Content-Length: " +
															v.BaseStream.Position
														);
													}
												};
										 };

									 downloader.StreamReader +=
										 source =>
										 {
											 uploader.StreamWriter +=
												 stream => StreamWriter(stream, source, 0);

											 uploader.Crawl("/upload");
										 };

									 uploader.DataReceived +=
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

											var ImageLink = "http://bayimg.com/image" + Link.Link.ToLower() + ".jpg";
											var ImageHTML = ImageLink.ToImage();

											var ThumbnailImageLink = "http://bayimg.com/thumb" + Link.Link.ToLower() + ".jpg";
											var ThumbnailImageHTML = ThumbnailImageLink.ToImage();

											new IHTMLImage { src = ImageLink, title = imdb.SmartTitle }.ToString().ToConsole();

											//Console.WriteLine(ImageHTML);
											//Console.WriteLine(ThumbnailImageHTML);
										};

									 downloader.Crawl(tineye.QueryLink.ToUri().PathAndQuery);

									 #endregion







								 }
							 );
						 }
					 );
				 }
			 );
		}


		private static void DemoGetPosterViaTinEye(string MovieTitle)
		{
			Console.WriteLine();
			Console.WriteLine(MovieTitle);

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

			BasicIMDBAliasSearch.Search(MovieTitle,
				 (e, i) =>
				 {
					 Console.WriteLine("BasicIMDBAliasSearch");
					 if (i > 0)
						 return;

					 BasicIMDBCrawler.Search(e.Key,
						 imdb =>
						 {
							 Console.WriteLine("BasicIMDBCrawler");
							 BasicTinEyeSearch.Search(imdb.MediumPosterImage,
								 tineye =>
								 {
									 Console.WriteLine("BasicTinEyeSearch");
									 // yay, we have the thumbnail, get it

									 #region downloader
									 var downloader = new BasicWebCrawler(tineye.QueryLink.ToUri().Host, 80);

									 downloader.ContentLengthReceived +=
										 ContentLength =>
										 {
											 var f = new FileInfo(tineye.Hash);
											 using (var s = f.OpenWrite())
											 {
												 s.SetLength(int.Parse(ContentLength));
											 }
										 };

									 downloader.StreamReader +=
										 stream =>
										 {
											 var buffer = new byte[0x1000];
											 var offset = 0;
											 var size = stream.Read(buffer, 0, buffer.Length);

											 var f = new FileInfo(tineye.Hash);
											 using (var s = f.OpenWrite())
												 while (size > 0)
												 {
													 s.Seek(offset, SeekOrigin.Begin);
													 s.Write(buffer, 0, size);
													 offset += size;
													 size = stream.Read(buffer, 0, buffer.Length);
												 }

										 };

									 downloader.Crawl(tineye.QueryLink.ToUri().PathAndQuery);

									 #endregion

									 if (!File.Exists(tineye.Hash))
										 return;

									 tineye.Hash.ToImage().ToConsole();

									 var uploader = new BasicWebCrawler("bayimg.com", 80)
									 {
										 Method = "POST"
									 };

									 // http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
									 var boundary = "---------------------------" + int.MaxValue.Random();

									 Action<StreamWriter> StreamWriter =
										  stream =>
										  {
											  stream.AutoFlush = true;

											  stream.WriteLine("--" + boundary);
											  stream.WriteLine("Content-Disposition: form-data; name=\"file\"; filename=\"" + tineye.Hash + "\"");
											  stream.WriteLine("Content-Type: application/octet-stream");
											  stream.WriteLine();

											  var f = new FileInfo(tineye.Hash);
											  using (var s = f.OpenRead())
											  {
												  var buffer = new byte[0x1000];
												  var offset = 0;
												  var size = s.Read(buffer, 0, buffer.Length);

												  while (size > 0)
												  {
													  stream.BaseStream.Write(buffer, 0, size);
													  offset += size;
													  size = s.Read(buffer, 0, buffer.Length);
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

									 uploader.HeaderWriter +=
										 stream =>
										 {
											 stream.WriteLine("Content-Type: multipart/form-data; boundary=" + boundary);

											 using (var v = new StreamWriter(new VoidStream()))
											 {

												 StreamWriter(v);

												 stream.WriteLine("Content-Length: " +
													 v.BaseStream.Position
												 );
											 }
										 };

									 uploader.StreamWriter += StreamWriter;

									 uploader.HeaderReceived +=
										 header =>
										 {
											 //Console.WriteLine(header);
										 };

									 uploader.DataReceived +=
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

											 var ImageLink = "http://bayimg.com/image" + Link.Link.ToLower() + ".jpg";
											 var ImageHTML = ImageLink.ToImage();

											 var ThumbnailImageLink = "http://bayimg.com/thumb" + Link.Link.ToLower() + ".jpg";
											 var ThumbnailImageHTML = ThumbnailImageLink.ToImage();

											 Console.WriteLine(ImageHTML);
											 Console.WriteLine(ThumbnailImageHTML);
										 };

									 uploader.Crawl("/upload");
								 }
							 );
						 }
					 );
				 }
			 );
		}



		private static void ShowPirateBayPosters()
		{
			Console.WriteLine("<style>");
			Console.WriteLine("img { border: 0; }");
			//Console.WriteLine("ol { -moz-column-count: 2; }");
			Console.WriteLine(@"

body{
	text-align: center;
	font-family:Verdana, Arial, Helvetica, sans-serif;
	font-size:.7em;
	margin: 10px;
	color: #fff;
	background: #000;
	min-width: 520px;
	overflow: hidden;
}


			
			");
			Console.WriteLine("</style>");

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

			Action<string, BasicIMDBAliasSearch.Entry> SearchPoster =
				 (Title, e) =>
				 {
					 //Console.WriteLine(e.Link);
					 Native.API.set_time_limit(20);


					 BasicIMDBCrawler.Search(e.Key,
						 k =>
						 {
							 IHTMLImage Image = k.MediumPosterImageCoralCache.OriginalString;

							 //new IHTMLAnchor
							 //{
							 //    Title = k.SmartTitle,
							 //    URL = Link.ToCoralCache().WithoutQuery().OriginalString,
							 //    Content = Image.ToString()
							 //}.ToString().ToConsole();

							 Native.API.set_time_limit(20);


							 BasicIMDBPosterSearch.Search(
									 k.MediumPosterImagePage,
									 LargePosterImage =>
									 {

										 var ur = new Uri(LargePosterImage);

										 IHTMLImage LargeImage = ur.ToCoralCache().OriginalString;

										 LargeImage.ToString().ToConsole();
									 }
								 );
						 }
					 );




				 };


			var search = new BasicPirateBaySearch();


			search.Loaded +=
				ForEachEntry =>
				{


					//Console.WriteLine("<hr />");

					//var logo = "http://static.thepiratebay.org/img/tpblogo_sm_ny.gif";

					//Console.WriteLine(logo.ToImage().ToLink("http://tineye.com/search?url=" + logo));

					// http://code.google.com/apis/youtube/chromeless_example_1.html
					//Console.WriteLine("<embed wmode='transparent' id='tv' src='http://www.youtube.com/apiplayer?enablejsapi=1&playerapiid=tv' allowScriptAccess='always' width='400' height='300' />");

					//Console.WriteLine("<h2>Top Movies</h2>");

					//Console.WriteLine("<ol>");

					ForEachEntry(
						(entry, entryindex) =>
						{
							Native.API.set_time_limit(20);
							//if (entryindex > 2)
							//    return;

							var Type = ParseLink(entry.Type);
							var Name = ParseLink(entry.Name);

							var SmartName = new BasicFileNameParser(Name.Text);

							var MovieInfo = default(BasicIMDBAliasSearch.Entry);


							BasicIMDBAliasSearch.Search(SmartName.Title,
								(e, index) =>
								{
									if (MovieInfo == null)
										MovieInfo = e;

								}
							);

							var c = new BasicGoogleVideoCrawler();

							var Video = "";
							var VideoSource = "";

							c.VideoSourceFound +=
								(video, src) =>
								{
									Video = video;
									VideoSource = src;


								};

							Native.API.set_time_limit(16);

							//Thread.Sleep(1500);

							c.Search(SmartName.Title + " trailer");

							//Console.WriteLine("<li>");

							//new IHTMLButton
							//{
							//    //onclick = "if (getElementById(\"tv\").getPlayerState() != 1) getElementById(\"tv\").loadVideoById(\"" + Video + "\")",
							//    onclick = "getElementById(\"tv\").loadVideoById(\"" + Video + "\")",
							//    Content = "View Trailer"
							//}.ToString().ToConsole();

							//Console.WriteLine("<span style='background: white; color: black;'>");


							if (MovieInfo != null)
							{
								SearchPoster(SmartName.Title, MovieInfo);

								//if (MovieInfo.Image != null)
								//    Console.WriteLine(MovieInfo.Image.ToImage().ToLink(MovieInfo.Link));

								//Console.WriteLine("<b>" + SmartName.Title.ToLink(MovieInfo.Link) + "</b>");
							}
							else
							{
								//Console.WriteLine("<b>" + SmartName.Title.ToLink(k => "http://www.imdb.com/find?s=tt;site=aka;q=" + k) + "</b>");
							}

							//if (!string.IsNullOrEmpty(SmartName.Season))
							//{
							//    Console.WriteLine(" | Season <i>" + SmartName.Season + "</i>");
							//}

							//if (!string.IsNullOrEmpty(SmartName.Episode))
							//{
							//    Console.WriteLine(" | Episode <i>" + SmartName.Episode + "</i>");
							//}


							//if (!string.IsNullOrEmpty(SmartName.SubTitle))
							//{
							//    Console.WriteLine(" | <b>" + SmartName.SubTitle + "</b>");
							//}

							//if (!string.IsNullOrEmpty(SmartName.Year))
							//{
							//    Console.WriteLine(" | <i>" + SmartName.Year + "</i>");
							//}



							//Console.WriteLine(" | ");
							//Console.WriteLine("<b>");
							//Console.WriteLine("trailer".ToLink(VideoSource, Video));
							//Console.WriteLine("</b>");





							//Console.WriteLine("<br />");

							//Console.WriteLine("<small>");
							//Console.WriteLine(SmartName.ColoredText.ToString().ToLink("http://thepiratebay.org" + Name.Link) + "<br />");


							//Console.WriteLine(Type.Text.ToLink("http://thepiratebay.org" + Type.Link));


							//entry.Links.ParseElements(
							//    (tag, index, element) =>
							//    {
							//        if (tag == "a")
							//        {
							//            var a = ParseLink(element);

							//            Console.WriteLine(" | " + "torrent".ToLink(a.Link));
							//        }

							//        if (tag == "img")
							//        {
							//            var img = ParseImage(element);

							//            if (img.Title.Contains("comment"))
							//            {
							//                Console.WriteLine(" | " + img.Title.ToLink("http://thepiratebay.org" + Name.Link));
							//            }
							//            else
							//            {
							//                Console.WriteLine(" | " + img.Title);
							//            }
							//        }
							//    }
							//);

							//Console.WriteLine(" | " + entry.Size);
							//Console.WriteLine(" | " + entry.Seeders);
							//Console.WriteLine(" | " + entry.Leechers + "<br />");


							//Console.WriteLine("</small>");

							//Console.WriteLine("</span>");

							////Console.WriteLine("</div>");
							//Console.WriteLine("</li>");


						}
					);

					//Console.WriteLine("</ol>");
				};

			search.Crawler.Crawl("/top/200");
		}

		private static void DemoFindMovieAlias()
		{
			DemoFindMovieAlias("Go fast", "2008");
			DemoFindMovieAlias("Being Erica", "2008");
			DemoFindMovieAlias("Shrek", "2004");
		}

		private static void DemoFindMovieAlias(string title, string year)
		{
			Native.API.set_time_limit(20);
			BasicIMDBAliasSearch.SearchSingle(
				title, year,
				e =>
				{

					BasicIMDBCrawler.Search(e.Key,
						x =>
						{

							if (string.IsNullOrEmpty(x.MediumPosterImage))
								BasicMPDBCrawler.SearchSingle(x.Title, x.Year,
									mpdb => mpdb.GetPoster(value =>
										{

											x.MediumPosterImage = value;
											x.MediumPosterImageProvider = "mpdb";
										}
									)
								);

							if (string.IsNullOrEmpty(x.MediumPosterImage))
								BasicOMDBCrawler.SearchSingle(x.Title, x.Year,
									omdb => omdb.GetPoster(value =>
										{
											x.MediumPosterImage = value;
											x.MediumPosterImageProvider = "omdb";
										}
									)
								);




							#region show poster
							if (!string.IsNullOrEmpty(x.MediumPosterImage))
							{
								BasicTinEyeSearch.Search(x.MediumPosterImage,
									tineye =>
										BasicPirateBayImage.Clone(tineye.QueryLink.ToUri(),
											bayimg =>
											{
												BasicTinyURLCrawler.Search("http://i.tinysrc.mobi/" + bayimg.Image.ToString(),
													ex =>
													{
														var protected_poster = ex.Alias;

														new IHTMLImage
														{
															src = protected_poster,
															title = x.SmartTitle + " | " + x.MediumPosterImageProvider
														}.ToString().ToConsole();
													}
												);
											}
										)
								);

								

							}
							#endregion

						}
					);
				}
			);
		}


		private static void DemoFindPosters()
		{
			Action<string> Search =
				 Title =>
				 {
					 Console.WriteLine(Title);

					 BasicIMDBAliasSearch.Search(
						 //"The Dark Knight",
						 //"Transporter 3",
						 Title,
						 (e, index) =>
						 {
							 if (index > 0)
								 return;


							 //Console.WriteLine(e.Link);
							 Native.API.set_time_limit(20);

							 var Link = new Uri(e.Link);
							 var Segments = Link.Segments;
							 var Key = Segments[2];

							 Key = Key.Substring(0, Key.Length - 1);

							 Console.WriteLine(Key);

							 //Console.WriteLine(Key);

							 BasicIMDBCrawler.Search(Key,
								 k =>
								 {
									 IHTMLImage MediumImage = new Uri(k.MediumPosterImage).ToCoralCache().OriginalString;

									 new IHTMLAnchor
									 {
										 Title = k.SmartTitle,
										 URL = Link.ToCoralCache().WithoutQuery().OriginalString,
										 innerHTML = MediumImage.ToString()
									 }.ToString().ToConsole();

									 BasicIMDBPosterSearch.Search(
										 k.MediumPosterImagePage,
										 LargePosterImage =>
										 {

											 var ur = new Uri(LargePosterImage);

											 IHTMLImage LargeImage = ur.ToCoralCache().OriginalString;

											 new IHTMLAnchor
											 {
												 Title = k.SmartTitle,
												 URL = Link.ToCoralCache().WithoutQuery().OriginalString,
												 innerHTML = LargeImage.ToString()
											 }.ToString().ToConsole();
										 }
									 );


								 }
							 );




						 }
					 );
				 };


			Search("Transporter 3");
			Search("The Dark Knight");
			Search("Watchmen");
		}



		private static void DemoPosters()
		{
			BasicIMDBCrawler.Search("tt0479952",
						 e =>
						 {
							 Console.WriteLine(
								 new
								 {
									 Title = e.MediumPosterTitle,
									 MediumPosterImage = (IHTMLImage)e.MediumPosterImage,
									 e.Runtime
								 }
							 );
						 }
					 );

			BasicIMDBCrawler.Search("tt0409459",
				e =>
				{
					Console.WriteLine(
						new
						{
							Title = e.MediumPosterTitle,
							MediumPosterImage = (IHTMLImage)e.MediumPosterImage,
							e.Runtime
						}
					);
				}
			);
		}



		private static void ShowExampleDotCom()
		{
			var crawler = new BasicWebCrawler("example.com", 80);

			var headers = 0;

			crawler.HeaderReceived += delegate { headers++; };

			crawler.DataReceived +=
				document =>
				{
					document = document.Replace(
						"reached this web page",
						"<b>received " + headers + " HTTP header(s)</b> and you have reached this web page"
					);

					document = document.Replace(
						"are reserved for use in documentation and",
						"are reserved for use in documentation <b>including examples</b> and"
					);

					Console.Write(document);
				};




			crawler.Crawl("/");
		}


		private static void ShowGoogleVideo()
		{
			var c = new BasicGoogleVideoCrawler();

			c.VideoSourceFound +=
				(video, src) =>
				{
					Console.WriteLine(src.ToLink(src));

				};

			c.Search("terminator+trailer");


		}

		private static void DemoTinEye()
		{
			BasicTinEyeSearch.Search(
				//"http://ia.media-imdb.com/images/M/MV5BMTk1Nzg0MDIxM15BMl5BanBnXkFtZTcwMjk5ODkyMg@@._V1._SX95_SY140_.jpg",
					"http://ia.media-imdb.com/images/M/MV5BMzA2Nzg1MjI2NV5BMl5BanBnXkFtZTcwNzA3NTEzMg@@._V1._SX600_SY400_.jpg",
					entry =>
					{
						Console.WriteLine(entry.Hash);
					}
				);
		}


		private static void DemoList()
		{
			var a = new List<string>
			{
				"hello",
				"dude",
				"world",
			};

			a.ForEach(
				k =>
				{
					Console.WriteLine(k);
				}
			);

			Console.WriteLine("<hr />");

			Console.WriteLine("i: " + a.IndexOf("dude"));

			a.Remove("dude");

			a.ForEach(
				k =>
				{
					Console.WriteLine(k);
				}
			);
		}


		private static void DownloadGame()
		{
			var ContentLength = -1;

			var c = new BasicWebCrawler("games.mochiads.com", 80);

			// coral does not support ranges?
			//c.CoralEnabled = true;
			c.Method = "HEAD";

			//c.AllHeadersReceived +=
			//    delegate
			//    {
			//        Console.WriteLine("<!-- AllHeadersReceived -->");
			//        Console.WriteLine("<hr />");
			//        Console.WriteLine();

			//    };

			c.HeaderReceived +=
				header =>
				{
					//Console.WriteLine("header: " + header);

					//Console.WriteLine(header);

					header.WhenStartsWith("Content-Length:",
						value =>
						{

							if (ContentLength < 0)
								ContentLength = int.Parse(value.Trim());
						}
					);
				};

			c.Crawl("/c/g/bubble-pop_v3/bubbles.swf");

			Console.WriteLine("Content-Length: " + ContentLength);
			Console.WriteLine();

			if (ContentLength > 0)
			{
				var Games = new DirectoryInfo("games");

				if (!Games.Exists)
				{
					Console.WriteLine("games folder does not exist yet!");

					return;
				}

				var LocalTarget = Games.ToFile("bubbels.swf");

				using (var f = LocalTarget.OpenWrite())
				{
					f.SetLength(ContentLength);
					//f.Seek(c.Range.From, SeekOrigin.Begin);
					//f.Write(bytes, 0, bytes.Length);
				}

				//var Chunck = 0x10;
				var Chunck = 0x7FFF;
				DownloadByChunks(ContentLength, LocalTarget.FullName, Chunck);

				//for (int i = 0x10; i < 0x1000; i += 0x10)
				//{
				//    DownloadByChunks(ContentLength, LocalTarget, i);
				//}
				//Fetch(Chunck * 2);




				"Play game".ToLink(LocalTarget.FullName.ToRelativePath()).ToConsole();

				//Console.WriteLine("bubbels.swf".ToLink());
			}

		}

		private static void DownloadByChunks(int ContentLength, string LocalTarget, int Chunck)
		{
			var cc = new BasicWebCrawler("games.mochiads.com", 80);

			cc.Method = "GET";

			cc.HeaderReceived +=
				header =>
				{
					//Console.WriteLine("header: " + header);
					if (header.StartsWith("Content-Range:"))
						Console.WriteLine(header);

				};

			cc.BinaryDataReceivedWithTimeSpan +=
				(bytes, elapsed) =>
				{
					//foreach (var v in bytes)
					//{
					//    Console.WriteLine("" + v);
					//}

					// SmartStreamReader: { offset = , count = 4096 } SmartStreamReader: { o = 1460, c = 2636, r = 1460 } SmartStreamReader: { r = 2920 } bytes 2920

					// bytes 16384 elapsed 78.08

					// bytes 32768 elapsed 737.3184
					// bytes 32768 elapsed 208.0512
					// bytes 32768 elapsed 447.8336
					// bytes 32768 elapsed 254.7712

					// bytes 32768 elapsed 434.7776
					// bytes 32768 elapsed 116.9408

					Console.WriteLine("bytes " + bytes.Length + " elapsed " + elapsed.TotalMilliseconds);

					//// http://ee.php.net/manual/en/function.fread.php
					////File.WriteAllBytes("bubbels.swf", bytes);

					Native.API.set_time_limit(9);
					//Thread.Sleep(5000);

					//var Target = LocalTarget + "_" + Chunck + "_" + cc.Range.From;


					using (var f = File.OpenWrite(LocalTarget))
					{
						f.Seek(cc.Range.From, SeekOrigin.Begin);

						//Console.WriteLine("Position: " + f.Position);

						f.Write(bytes, 0, bytes.Length);
					}

				};

			Console.WriteLine("allocating buffer: " + Chunck);

			cc.Buffer = new byte[Chunck];

			Action<int> Fetch =
				offset =>
				{
					//Console.WriteLine("offset: " + offset);

					cc.Range = new BasicWebCrawler.RangeHeader
					{
						From = offset,
						MaxCount = ContentLength,
						Count = Chunck
					};

					//Console.WriteLine("before Crawl");
					cc.Crawl("/c/g/bubble-pop_v3/bubbles.swf");
					//Console.WriteLine("after Crawl");
				};

			//cc.Diagnostics +=
			//    text => Console.WriteLine("<!-- " + text + " -->");

			//Fetch(Chunck * 0);

			for (int i = 0; i < ContentLength; i += Chunck)
			{
				Fetch(i);
			}

		}


		private static void ShowTinyURL()
		{
			BasicTinyURLCrawler.Search("http://imdb.com",
				 entry =>
				 {
					 Console.WriteLine(
						 new
						 {
							 Alias = entry.Alias.ToLink(),
							 entry.URL
						 }
					 );


				 }
			 );
		}

		private static void ShowTitleSearch()
		{
			// The%20Dark%20Knight
			// Transporter 3
			BasicIMDBAliasSearch.Search(
				//"The Dark Knight",
				"Transporter 3",
				(e, index) =>
				{
					if (e.OptionalImage != null)
						Console.WriteLine(e.OptionalImage.ToImage().ToLink(e.Link));

					Console.WriteLine((e.OptionalTitle + " | " + e.OptionalReleaseDate).ToLink(e.Link));
					Console.WriteLine("<br />");


				}
			);
		}



		private static void ShowPirateBayWithVideo()
		{
			Console.WriteLine("<style>");
			Console.WriteLine("img { border: 0; }");
			//Console.WriteLine("ol { -moz-column-count: 2; }");
			Console.WriteLine(@"
embed {
width: 100%;
height: 100%;
position: absolute; 
left: 0;
top: 0;
z-index: 0;
}

ol
{
display: block;
width: 100%;
height: 100%;
position: absolute; 
left: 0;
top: 0;
z-index: 1;

overflow: scroll;
}

li
{
}

body{
	text-align: center;
	font-family:Verdana, Arial, Helvetica, sans-serif;
	font-size:.7em;
	margin: 10px;
	color: #fff;
	background: #000;
	min-width: 520px;
	overflow: hidden;
}


a{
	color: #009;
	text-decoration: none;
	border-bottom: 1px dotted #4040D9;
}
a:hover{
	text-decoration: none;
	border-bottom: 1px solid #009;
}
		li { 

text-align: left;
margin: 1em;}	
			
			");
			Console.WriteLine("</style>");

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

			Action<string, BasicIMDBAliasSearch.Entry> SearchPoster =
				 (Title, e) =>
				 {
					 //Console.WriteLine(e.Link);
					 Native.API.set_time_limit(20);

					 var Link = new Uri(e.Link);
					 var Segments = Link.Segments;
					 var Key = Segments[2];

					 Key = Key.Substring(0, Key.Length - 1);

					 //Console.WriteLine(Key);

					 BasicIMDBCrawler.Search(Key,
						 k =>
						 {
							 IHTMLImage Image = k.MediumPosterImageCoralCache.OriginalString;

							 new IHTMLAnchor
							 {
								 Title = k.SmartTitle,
								 URL = Link.ToCoralCache().WithoutQuery().OriginalString,
								 innerHTML = Image.ToString()
							 }.ToString().ToConsole();

							 Native.API.set_time_limit(20);


							 BasicIMDBPosterSearch.Search(
									 k.MediumPosterImagePage,
									 LargePosterImage =>
									 {

										 var ur = new Uri(LargePosterImage);

										 IHTMLImage LargeImage = ur.ToCoralCache().OriginalString;

										 new IHTMLAnchor
										 {
											 Title = k.SmartTitle,
											 URL = Link.ToCoralCache().WithoutQuery().OriginalString,
											 innerHTML = LargeImage.ToString()
										 }.ToString().ToConsole();
									 }
								 );
						 }
					 );




				 };


			var search = new BasicPirateBaySearch();


			search.Loaded +=
				ForEachEntry =>
				{


					//Console.WriteLine("<hr />");

					//var logo = "http://static.thepiratebay.org/img/tpblogo_sm_ny.gif";

					//Console.WriteLine(logo.ToImage().ToLink("http://tineye.com/search?url=" + logo));

					// http://code.google.com/apis/youtube/chromeless_example_1.html
					Console.WriteLine("<embed wmode='transparent' id='tv' src='http://www.youtube.com/apiplayer?enablejsapi=1&playerapiid=tv' allowScriptAccess='always' width='400' height='300' />");

					//Console.WriteLine("<h2>Top Movies</h2>");

					Console.WriteLine("<ol>");

					ForEachEntry(
						(entry, entryindex) =>
						{
							Native.API.set_time_limit(20);
							//if (entryindex > 2)
							//    return;

							var Type = ParseLink(entry.Type);
							var Name = ParseLink(entry.Name);

							var SmartName = new BasicFileNameParser(Name.Text);

							var MovieInfo = default(BasicIMDBAliasSearch.Entry);


							BasicIMDBAliasSearch.Search(SmartName.Title,
								(e, index) =>
								{
									if (MovieInfo == null)
										MovieInfo = e;

								}
							);

							var c = new BasicGoogleVideoCrawler();

							var Video = "";
							var VideoSource = "";

							c.VideoSourceFound +=
								(video, src) =>
								{
									Video = video;
									VideoSource = src;


								};

							Native.API.set_time_limit(16);

							//Thread.Sleep(1500);

							c.Search(SmartName.Title + " trailer");

							Console.WriteLine("<li>");

							new IHTMLButton
							{
								//onclick = "if (getElementById(\"tv\").getPlayerState() != 1) getElementById(\"tv\").loadVideoById(\"" + Video + "\")",
								onclick = "getElementById(\"tv\").loadVideoById(\"" + Video + "\")",
								innerHTML = "View Trailer"
							}.ToString().ToConsole();

							Console.WriteLine("<span style='background: white; color: black;'>");


							if (MovieInfo != null)
							{
								SearchPoster(SmartName.Title, MovieInfo);

								//if (MovieInfo.Image != null)
								//    Console.WriteLine(MovieInfo.Image.ToImage().ToLink(MovieInfo.Link));

								Console.WriteLine("<b>" + SmartName.Title.ToLink(MovieInfo.Link) + "</b>");
							}
							else
							{
								Console.WriteLine("<b>" + SmartName.Title.ToLink(k => "http://www.imdb.com/find?s=tt;site=aka;q=" + k) + "</b>");
							}

							if (!string.IsNullOrEmpty(SmartName.Season))
							{
								Console.WriteLine(" | Season <i>" + SmartName.Season + "</i>");
							}

							if (!string.IsNullOrEmpty(SmartName.Episode))
							{
								Console.WriteLine(" | Episode <i>" + SmartName.Episode + "</i>");
							}


							if (!string.IsNullOrEmpty(SmartName.SubTitle))
							{
								Console.WriteLine(" | <b>" + SmartName.SubTitle + "</b>");
							}

							if (!string.IsNullOrEmpty(SmartName.Year))
							{
								Console.WriteLine(" | <i>" + SmartName.Year + "</i>");
							}



							Console.WriteLine(" | ");
							Console.WriteLine("<b>");
							Console.WriteLine("trailer".ToLink(VideoSource, Video));
							Console.WriteLine("</b>");





							Console.WriteLine("<br />");

							Console.WriteLine("<small>");
							Console.WriteLine(SmartName.ColoredText.ToString().ToLink("http://thepiratebay.org" + Name.Link) + "<br />");


							Console.WriteLine(Type.Text.ToLink("http://thepiratebay.org" + Type.Link));


							entry.Links.ParseElements(
								(tag, index, element) =>
								{
									if (tag == "a")
									{
										var a = ParseLink(element);

										Console.WriteLine(" | " + "torrent".ToLink(a.Link));
									}

									if (tag == "img")
									{
										var img = ParseImage(element);

										if (img.Title.Contains("comment"))
										{
											Console.WriteLine(" | " + img.Title.ToLink("http://thepiratebay.org" + Name.Link));
										}
										else
										{
											Console.WriteLine(" | " + img.Title);
										}
									}
								}
							);

							Console.WriteLine(" | " + entry.Size);
							Console.WriteLine(" | " + entry.Seeders);
							Console.WriteLine(" | " + entry.Leechers + "<br />");


							Console.WriteLine("</small>");

							Console.WriteLine("</span>");

							//Console.WriteLine("</div>");
							Console.WriteLine("</li>");


						}
					);

					Console.WriteLine("</ol>");
				};

			search.Crawler.Crawl("/top/200");
		}


		private static void ShowPirateBayWithoutVideo()
		{
			Console.WriteLine("<style>");
			Console.WriteLine("img { border: 0; }");
			Console.WriteLine("ol { -moz-column-count: 2; }");
			Console.WriteLine(@"
embed {
width: 100%;
height: 100%;
position: absolute; 
left: 0;
top: 0;
z-index: 0;
}

ol
{
display: block;
width: 100%;
height: 100%;
position: absolute; 
left: 0;
top: 0;
z-index: 1;

overflow: scroll;
}

li
{
}

body{
	text-align: center;
	font-family:Verdana, Arial, Helvetica, sans-serif;
	font-size:.7em;
	margin: 10px;
	min-width: 520px;
	overflow: hidden;
}


a{
	color: #009;
	text-decoration: none;
	border-bottom: 1px dotted #4040D9;
}
a:hover{
	text-decoration: none;
	border-bottom: 1px solid #009;
}
		li { 

text-align: left;
margin: 1em;}	
			
			");
			Console.WriteLine("</style>");

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

			Action<string, BasicIMDBAliasSearch.Entry> SearchPoster =
				 (Title, e) =>
				 {
					 //Console.WriteLine(e.Link);
					 Native.API.set_time_limit(20);

					 var Link = new Uri(e.Link);
					 var Segments = Link.Segments;
					 var Key = Segments[2];

					 Key = Key.Substring(0, Key.Length - 1);

					 //Console.WriteLine(Key);

					 BasicIMDBCrawler.Search(Key,
						 k =>
						 {
							 IHTMLImage Image = k.MediumPosterImageCoralCache.OriginalString;

							 new IHTMLAnchor
							 {
								 Title = k.SmartTitle,
								 URL = Link.ToCoralCache().WithoutQuery().OriginalString,
								 innerHTML = Image.ToString()
							 }.ToString().ToConsole();

							 Native.API.set_time_limit(20);


							 BasicIMDBPosterSearch.Search(
									 k.MediumPosterImagePage,
									 LargePosterImage =>
									 {

										 var ur = new Uri(LargePosterImage);

										 IHTMLImage LargeImage = ur.ToCoralCache().OriginalString;

										 new IHTMLAnchor
										 {
											 Title = k.SmartTitle,
											 URL = Link.ToCoralCache().WithoutQuery().OriginalString,
											 innerHTML = LargeImage.ToString()
										 }.ToString().ToConsole();
									 }
								 );
						 }
					 );




				 };


			var search = new BasicPirateBaySearch();



			search.Loaded +=
				ForEachEntry =>
				{


					//Console.WriteLine("<hr />");

					//var logo = "http://static.thepiratebay.org/img/tpblogo_sm_ny.gif";

					//Console.WriteLine(logo.ToImage().ToLink("http://tineye.com/search?url=" + logo));

					// http://code.google.com/apis/youtube/chromeless_example_1.html
					//Console.WriteLine("<embed wmode='transparent' id='tv' src='http://www.youtube.com/apiplayer?enablejsapi=1&playerapiid=tv' allowScriptAccess='always' width='400' height='300' />");

					//Console.WriteLine("<h2>Top Movies</h2>");

					Console.WriteLine("<ol>");

					ForEachEntry(
						(entry, entryindex) =>
						{
							Native.API.set_time_limit(20);
							//if (entryindex > 2)
							//    return;

							var Type = ParseLink(entry.Type);
							var Name = ParseLink(entry.Name);

							var SmartName = new BasicFileNameParser(Name.Text);

							var MovieInfo = default(BasicIMDBAliasSearch.Entry);


							BasicIMDBAliasSearch.Search(SmartName.Title,
								(e, index) =>
								{
									if (MovieInfo == null)
										MovieInfo = e;

								}
							);

							var c = new BasicGoogleVideoCrawler();

							var Video = "";
							var VideoSource = "";

							c.VideoSourceFound +=
								(video, src) =>
								{
									Video = video;
									VideoSource = src;


								};

							Native.API.set_time_limit(16);

							//Thread.Sleep(1500);

							c.Search(SmartName.Title + " trailer");

							Console.WriteLine("<li>");

							//new IHTMLButton
							//{
							//    //onclick = "if (getElementById(\"tv\").getPlayerState() != 1) getElementById(\"tv\").loadVideoById(\"" + Video + "\")",
							//    onclick = "getElementById(\"tv\").loadVideoById(\"" + Video + "\")",
							//    Content = "View Trailer"
							//}.ToString().ToConsole();

							Console.WriteLine("<span style='background: white; color: black;'>");


							if (MovieInfo != null)
							{
								//SearchPoster(SmartName.Title, MovieInfo);

								if (MovieInfo.OptionalImage != null)
									Console.WriteLine(MovieInfo.OptionalImage.ToImage().ToLink(MovieInfo.Link));

								Console.WriteLine("<b>" + SmartName.Title.ToLink(MovieInfo.Link) + "</b>");
							}
							else
							{
								Console.WriteLine("<b>" + SmartName.Title.ToLink(k => "http://www.imdb.com/find?s=tt;site=aka;q=" + k) + "</b>");
							}

							if (!string.IsNullOrEmpty(SmartName.Season))
							{
								Console.WriteLine(" | Season <i>" + SmartName.Season + "</i>");
							}

							if (!string.IsNullOrEmpty(SmartName.Episode))
							{
								Console.WriteLine(" | Episode <i>" + SmartName.Episode + "</i>");
							}


							if (!string.IsNullOrEmpty(SmartName.SubTitle))
							{
								Console.WriteLine(" | <b>" + SmartName.SubTitle + "</b>");
							}

							if (!string.IsNullOrEmpty(SmartName.Year))
							{
								Console.WriteLine(" | <i>" + SmartName.Year + "</i>");
							}



							Console.WriteLine(" | ");
							Console.WriteLine("<b>");
							Console.WriteLine("trailer".ToLink(VideoSource, Video));
							Console.WriteLine("</b>");





							Console.WriteLine("<br />");

							Console.WriteLine("<small>");
							Console.WriteLine(SmartName.ColoredText.ToString().ToLink("http://thepiratebay.org" + Name.Link) + "<br />");


							Console.WriteLine(Type.Text.ToLink("http://thepiratebay.org" + Type.Link));


							entry.Links.ParseElements(
								(tag, index, element) =>
								{
									if (tag == "a")
									{
										var a = ParseLink(element);

										Console.WriteLine(" | " + "torrent".ToLink(a.Link));
									}

									if (tag == "img")
									{
										var img = ParseImage(element);

										if (img.Title.Contains("comment"))
										{
											Console.WriteLine(" | " + img.Title.ToLink("http://thepiratebay.org" + Name.Link));
										}
										else
										{
											Console.WriteLine(" | " + img.Title);
										}
									}
								}
							);

							Console.WriteLine(" | " + entry.Size);
							Console.WriteLine(" | " + entry.Seeders);
							Console.WriteLine(" | " + entry.Leechers + "<br />");


							Console.WriteLine("</small>");

							Console.WriteLine("</span>");

							//Console.WriteLine("</div>");
							Console.WriteLine("</li>");


						}
					);

					Console.WriteLine("</ol>");
				};

			search.Crawler.Crawl("/top/200");
		}

		private static void ShowPirateBayFast()
		{
			Console.WriteLine("<ol>");

			BasicPirateBaySearch.Search(
				entry =>
				{
					var SmartName = entry.SmartName;

					Console.WriteLine("<li>");
					Console.WriteLine("<span style='background: white; color: black;'>");
					Console.WriteLine("<b>" + SmartName.Title.ToLink("http://www.imdb.com/find?s=tt;site=aka;q=" + SmartName.Title) + "</b>");

					#region SmartName
					if (!string.IsNullOrEmpty(SmartName.Season))
					{
						Console.WriteLine(" | Season <i>" + SmartName.Season + "</i>");
					}

					if (!string.IsNullOrEmpty(SmartName.Episode))
					{
						Console.WriteLine(" | Episode <i>" + SmartName.Episode + "</i>");
					}

					if (!string.IsNullOrEmpty(SmartName.SubTitle))
					{
						Console.WriteLine(" | <b>" + SmartName.SubTitle + "</b>");
					}

					if (!string.IsNullOrEmpty(SmartName.Year))
					{
						Console.WriteLine(" | <i>" + SmartName.Year + "</i>");
					}
					#endregion

					Console.WriteLine("<br />");
					Console.WriteLine("<small>");
					Console.WriteLine(SmartName.ColoredText.ToString().ToLink("http://thepiratebay.org" + entry.Link, entry.Name.ToMD5Bytes().ToHexString()) + "<br />");
					Console.WriteLine(" | " + "torrent".ToLink(entry.TorrentLink));
					Console.WriteLine(" | " + entry.CommentText.ToLink("http://thepiratebay.org" + entry.Link));
					Console.WriteLine(" | " + entry.Size);
					Console.WriteLine(" | " + entry.Seeders);
					Console.WriteLine(" | " + entry.Leechers);
					Console.WriteLine("<br />");
					Console.WriteLine("</small>");
					Console.WriteLine("</span>");
					Console.WriteLine("</li>");
				}
			);

			Console.WriteLine("</ol>");

		}

		private static void ShowPirateBayFastWithMemory()
		{
			var memory = new MemoryDirectory(new DirectoryInfo("memory"));

			Console.WriteLine("memory: " + memory.Count);

			Console.WriteLine("<ol>");



			BasicPirateBaySearch.Search(
				k => memory.Contains(k.Hash),
				(entry, deferred) =>
				{
					var hash = entry.Name.ToMD5Bytes().ToHexString();

					if (memory.Contains(hash))
						Console.WriteLine("<li style='color:gray; font-size: small;'>");
					else
					{
						memory.Add(hash);
						Console.WriteLine("<li>");
					}

					Console.WriteLine(entry.Name.ToLink("http://thepiratebay.org" + entry.Link, hash));
					Console.WriteLine(" | " + "torrent".ToLink(entry.TorrentLink));
					Console.WriteLine(" | " + entry.CommentText);
					Console.WriteLine(" | " + entry.Size);
					Console.WriteLine(" | " + entry.Seeders);
					Console.WriteLine(" | " + entry.Leechers);
					Console.WriteLine("</li>");
				}
			);

			Console.WriteLine("</ol>");

			Console.WriteLine("memory: " + memory.Count);

		}
	}
}
