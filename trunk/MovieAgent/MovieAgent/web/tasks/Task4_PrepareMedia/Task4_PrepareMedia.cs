using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;
using System.Threading;
using MovieAgent.Server.Services;
using MovieAgent.web.tasks.Task5_CloneMedia;
using System.IO;
using MovieAgent.Shared;

namespace MovieAgent.web.tasks.Task4_PrepareMedia
{
	[Script]
	public class Task4_PrepareMedia : WorkTask
	{

		public Task4_PrepareMedia(MyNamedTasks Tasks)
			: base(Tasks, "Task4_PrepareMedia")
		{
			var NamedTasks = Tasks;
			this.Description = @"
We now should have the torrent, name, seeders, leechers. We must now get the poster, trailer, rating, tagline and hash.
Uses memory to store tineye poster hash.
			";

			this.YieldWork =
				(Task, Input) =>
				{

					var c = Input.ToFieldBuilder();

					FileMappedField
						TorrentName = c,
						TorrentLink = c,
						TorrentSize = c,
						TorrentComments = c,
						IMDBKey = c,
						IMDBTitle = c,
						IMDBYear = c,
						IMDBRaiting = c,
						IMDBRuntime = c,
						IMDBGenre = c,
						IMDBTagline = c,
						OMDBSearched = c,
						MPDBSearched = c,
						PosterLink = c,
						TinEyeHash = c,
						BayImageLink = c,
						BayImageTinyLink = c,
						//BayImageTinySourceLink = c,
						YouTubeKey = c,
						TorrentLinkTinyLink = c,
						TaskComplete = c;

					c.FromFile();

					Input.Delete();

					//AppendLog("in " + TorrentName.Value);

					#region IMDBKey
					c[IMDBKey] = delegate
					{
						var TorrentSmartName = new BasicFileNameParser(TorrentName.Value);

						AppendLog("looking for imdb key");

						BasicIMDBAliasSearch.SearchSingle(TorrentSmartName.Title, TorrentSmartName.Year,
							imdb =>
							{
								AppendLog("imdb key found");
								IMDBKey.Value = imdb.Key;
							}
						);
					};
					#endregion

					#region IMDBTitle
					c[IMDBTitle] = delegate
					{
						AppendLog("looking for imdb for details");

						BasicIMDBCrawler.Search(IMDBKey.Value,
							imdb =>
							{
								AppendLog("imdb details found");

								IMDBTitle.Value = imdb.Title;
								IMDBYear.Value = imdb.Year;
								IMDBRaiting.Value = imdb.UserRating;
								IMDBRuntime.Value = imdb.Runtime;
								IMDBTagline.Value = imdb.Tagline;
								IMDBGenre.Value = string.Join("|", imdb.Genres);

								PosterLink.Value = imdb.MediumPosterImage;
							}
						);
					};
					#endregion

					#region PosterLink
					c[PosterLink] = delegate
					{
						AppendLog("looking for posters...");

						c[OMDBSearched] = delegate
						{
							AppendLog("looking for posters... omdb");
							OMDBSearched.Value = "true";

							BasicOMDBCrawler.SearchSingle(IMDBTitle.Value, IMDBYear.Value,
								omdb =>
								{
									omdb.GetPoster(
										poster =>
										{
											AppendLog("looking for posters... omdb... found!");
											PosterLink.Value = poster;
										}
									);
								}
							);
						};

						c[MPDBSearched] = delegate
						{
							AppendLog("looking for posters... mpdb");

							BasicMPDBCrawler.SearchSingle(IMDBTitle.Value, IMDBYear.Value,
								mpdb =>
								{
									mpdb.GetPoster(
										poster =>
										{
											AppendLog("looking for posters... mpdb... found!");
											PosterLink.Value = poster;
										}
									);
								}
							);
						};
					};
					#endregion

					#region TinEyeHash
					c[TinEyeHash] = delegate
					{
						AppendLog("looking for tineye hash...");
						BasicTinEyeSearch.Search(PosterLink.Value,
							tineye =>
							{
								AppendLog("looking for tineye hash... found");
								TinEyeHash.Value = tineye.Hash;

								var Memory = this.Memory[TinEyeHash.Value].FirstDirectoryOrDefault();

								if (Memory != null)
								{
									BayImageLink.Value = "_";
									//BayImageTinyLink.Value = "_";
									BayImageTinyLink.Value = "http://tinyurl.com/" + Memory.Name;
								}
							}
						);
					};
					#endregion

					#region BayImageLink
					c[BayImageLink] = delegate
					{
						AppendLog("updating bay image...");

						var tineye = new BasicTinEyeSearch.Entry { Hash = TinEyeHash.Value };

						BasicPirateBayImage.Clone(tineye.QueryLink.ToUri(),
							bayimg =>
							{
								AppendLog("updating bay image... done");
								BayImageLink.Value = bayimg.Image.ToString();
							}
						);

					};
					#endregion

					#region BayImageTinyLink
					c[BayImageTinyLink] = delegate
					{
						AppendLog("looking for tinyurl...");
						BasicTinyURLCrawler.Search(BayImageLink.Value,
							tinyurl =>
							{
								AppendLog("looking for tinyurl... found");
								BayImageTinyLink.Value = tinyurl.Alias;
							}
						);
					};
					#endregion

					//#region BayImageTinySourceLink
					//c[BayImageTinySourceLink] = delegate
					//{
					//    BasicTinyURLCrawler.Search("http://i.tinysrc.mobi/" + BayImageTinyLink.Value,
					//        tinyurl =>
					//        {
					//            BayImageTinySourceLink.Value = tinyurl.Alias;

					//            this.Memory[TinEyeHash.Value].CreateSubdirectory(tinyurl.AliasKey);
					//        }
					//    );
					//};
					//#endregion

					#region YouTubeKey
					c[YouTubeKey] = delegate
					{
						var Query = IMDBTitle.Value;
						var TorrentSmartName = new BasicFileNameParser(TorrentName.Value);

						var SeasonAndEpisode = TorrentSmartName.SeasonAndEpisode;

						if (!string.IsNullOrEmpty(SeasonAndEpisode))
							Query += " " + SeasonAndEpisode;
						else
							Query += " trailer";


						BasicGoogleVideoCrawler.Search(Query,
							(key, src) =>
							{
								YouTubeKey.Value = key;
							}
						);
					};
					#endregion

					#region TorrentLinkTinyLink
					c[TorrentLinkTinyLink] = delegate
					{
						BasicTinyURLCrawler.Search(TorrentLink.Value,
							torrent =>
							{
								TorrentLinkTinyLink.Value = torrent.Alias;
							}
						);
					};
					#endregion

					c[TaskComplete] = delegate
					{
						var TorrentSmartName = new BasicFileNameParser(TorrentName.Value);

						var YouTubeLink = "http://www.youtube.com/v/" + YouTubeKey + @"&hl=en&fs=1";
						var YouTubeImage = "http://img.youtube.com/vi/" + YouTubeKey + @"/0.jpg";

						var SmartTitle = "";

						if (!string.IsNullOrEmpty(TorrentSmartName.SeasonAndEpisode))
							SmartTitle = "TV show: " + IMDBTitle.Value + " " + TorrentSmartName.SeasonAndEpisode + " " + IMDBYear.Value;
						else
							SmartTitle = "Movie: " + IMDBTitle.Value + " " + IMDBYear.Value;

						var NextWork = NamedTasks.Task6_MediaCollector.AddWork(5, Input.Name);

						using (var w = new StreamWriter(NextWork.OpenWrite()))
						{
							w.WriteLine("<item>");
							w.WriteLine("<title>" + SmartTitle + "</title>");
							w.WriteLine("<link>" + TorrentLinkTinyLink.Value + "</link>");

							#region description
							w.WriteLine("<description><![CDATA[");

							w.WriteLine(@"
	<object width='640' height='385'>
		<param name='movie' " + YouTubeLink.ToAttributeString("value") + @"></param>
		<param name='allowFullScreen' value='true'></param>
		<param name='allowscriptaccess' value='always'></param>
		<embed " + YouTubeLink.ToAttributeString("src") + @" type='application/x-shockwave-flash' allowscriptaccess='always' allowfullscreen='true' width='640' height='385'></embed>
	</object>
							");

							w.WriteLine(
								new IHTMLAnchor
								{
									Style = new IStyle { @float = "left" },
									URL = YouTubeLink,
									innerHTML = new IHTMLImage
									{
										alt = YouTubeKey.Value,
										align = "left",
										src = YouTubeImage
									}
								}.ToString()
							);

							w.WriteLine(
								new IHTMLAnchor
								{
									URL = BasicIMDBCrawler.ToLink(IMDBKey.Value),
									Title = SmartTitle,
									innerHTML = new IHTMLImage
									{
										align = "right",
										src = BayImageTinyLink.Value,
									}
								}.ToString()
							);

							w.WriteLine(
								"<h2>" + SmartTitle.ToLink("http://piratebay.org" + TorrentComments.Value) + "</h2>"
							);

							w.WriteLine(
								new IHTMLElement { innerHTML = IMDBRaiting.Value, title = "raiting" }.ToString()
							);

							w.WriteLine(
								new IHTMLElement { innerHTML = (IMDBRuntime.Value + ", " + TorrentSize.Value.Replace("&nbsp;", " ")), title = "runtime" }.ToString()
							);

							w.WriteLine(
								new IHTMLElement { innerHTML = IMDBTagline.Value, title = "tagline" }.ToString()
							);

							w.WriteLine(
								new IHTMLElement { innerHTML = IMDBGenre.Value.Replace("|", ", "), title = "genres" }.ToString()
							);

							w.WriteLine(
								new IHTMLElement { innerHTML = TorrentSmartName.SeasonAndEpisode, title = "episode" }.ToString()
							);

						


							w.WriteLine(
								new IHTMLAnchor
								{
									URL = TorrentLinkTinyLink.Value,
									Title = SmartTitle,
									innerHTML = new IHTMLImage
									{
										src = "http://static.thepiratebay.org/img/dl.gif"
									}.ToString() + " " + TorrentName.Value
								}.ToString()
							);


							w.WriteLine(" ]]></description>");
							#endregion

							#region category
							if (string.IsNullOrEmpty(TorrentSmartName.SeasonAndEpisode))
								w.WriteLine("<category>Movies</category>");
							else
								w.WriteLine("<category>TV shows</category>");

							var Genres = IMDBGenre.Value.Split(new[] { '|' });

							foreach (var g in Genres)
							{
								w.WriteLine("<category>" + g + "</category>");
							}
							#endregion

							w.WriteLine("<media:thumbnail url='" + BayImageTinyLink.Value + "' />");
							w.WriteLine("<media:content " + YouTubeLink.ToAttributeString("url") + @" type='application/x-shockwave-flash' />");
							w.WriteLine("<media:description type='plain'>" + SmartTitle + " | " + IMDBRaiting.Value + " | " + IMDBTagline.Value + " | " + IMDBGenre.Value + "</media:description>");

							w.WriteLine("</item>");
						}
					};

					return c.ToFileWhenDirty;
				};
		}



		//public override IHTMLOrderedList VisualizedWorkItems(FileInfo[] a)
		//{
		//    var o = new IHTMLOrderedList();

		//    foreach (var f in a)
		//    {
		//        var k = new BasicPirateBaySearch.SearchEntry().FromFile(f);

		//        var WorkItem = new IHTMLAnchor
		//        {
		//            URL = f.FullName.ToRelativePath(),
		//            innerHTML = k.Name
		//        }.ToString() + " - <b>" + k.SmartName.ToString() + "</b>";

		//        o.innerHTML += (IHTMLListItem)WorkItem;
		//    }

		//    return o;
		//}
	}
}
