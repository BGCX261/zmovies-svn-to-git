using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using MovieAgentGadget.ActionScript.Library;
using MovieAgentGadget.Data;
using MovieAgentGadget.Promotion;
using MovieAgentGadget.Shared;
using ScriptCoreLib;
using ScriptCoreLib.ActionScript;
using ScriptCoreLib.ActionScript.DOM;
using ScriptCoreLib.ActionScript.DOM.Extensions;
using ScriptCoreLib.ActionScript.DOM.HTML;
using ScriptCoreLib.ActionScript.Extensions;
using ScriptCoreLib.Shared.Lambda;
using MovieAgent.Shared;

namespace MovieAgentGadget.ActionScript
{

	partial class MovieAgentGadget
	{
		[Script]
		public class MovieItemWithPoster
		{
			public MovieItem Movie;
			public IHTMLImage Poster;
		}

		public readonly List<MovieItemWithPoster> KnownMovies = new List<MovieItemWithPoster>();
		public Func<MovieItem, bool> KnownMoviesFilter;

		public void Initialize(ExternalContext Context)
		{
			KnownMoviesFilter = e => true;

			//Context.Document.title = "2 token: " + Context.PrivateKey;

			// wtf? we cannot modify body without resetting our flash
			// http://dojotoolkit.org/forum/dojo-core-dojo-0-9/dojo-core-support/firefox-reloads-flash-when-portion-page-refreshed-using-a
			//Context.Document.body.style.overflow = "hidden";
			//Context.Document.body.style.backgroundColor = "black";
			//Context.Document.body.style.color = "white";

			#region ContainerForPosters
			var ContainerForPosters = new IHTMLDiv().AttachTo(Context);

			ContainerForPosters.style.position = "absolute";
			ContainerForPosters.style.width = "100%";
			ContainerForPosters.style.height = "100%";
			//ContainerForPosters.style.background = "black url('http://www.stripegenerator.com/generators/generate_stripes.php?fore=000000&h=30&w=6&p=7&back1=333333&back2=ff0000&gt=0&d=0&shadow=0&')";
			ContainerForPosters.style.backgroundColor = "black";
			ContainerForPosters.style.color = "white";
			ContainerForPosters.style.textAlign = "center";
			ContainerForPosters.style.overflow = "auto";
			#endregion

			var PostersGroup = new IHTMLDiv().AttachTo(ContainerForPosters);
			var FooterGroup = new IHTMLDiv().AttachTo(ContainerForPosters);

			var ButtonsOnRightEdge = new IHTMLSpan().AttachTo(Context);

			ButtonsOnRightEdge.style.Apply(
				s =>
				{
					s.position = "absolute";
					s.marginLeft = "-8em";
					s.marginTop = "-2em";
					s.top = "100%";
					s.left = "100%";
				}
			);

			ButtonsOnRightEdge.innerHTML =
				Info.GoogleGadget.AddImage.ToImageMiddle().ToLink(Info.GoogleGadget.AddToYourWebPageLink) + "&nbsp;" +
				Info.RSSImage.ToImageMiddle().ToLink(Info.BuzzBlogPost);

			var Toolbar = new IHTMLDiv
			{
				innerHTML = ""
			}.AttachTo(Context);


			Toolbar.style.Apply(
				s =>
				{
					s.position = "absolute";
					s.marginTop = "-2em";
					s.top = "100%";
					s.left = "1em";
				}
			);

			#region ToggleScrollbar
			var ToggleScrollbar = new IHTMLSpan { innerHTML = "&raquo; toggle scrollbar" }.AttachTo(Toolbar);

			ToggleScrollbar.style.Apply(
				s =>
				{
					s.border = "1px dotted white";
					s.cursor = "pointer";
					s.backgroundColor = "black";
					s.color = "white";
					s.marginLeft = "1em";
				}
			);

			var ToggleScrollbarCounter = 0;
			ToggleScrollbar.onclick +=
				delegate
				{
					ToggleScrollbarCounter++;

					if (ToggleScrollbarCounter % 2 == 0)
						ContainerForPosters.style.overflow = "auto";
					else
						ContainerForPosters.style.overflow = "hidden";
				};
			#endregion



			#region Shadow
			var Shadow = new IHTMLDiv().AttachTo(Context);

			Shadow.style.position = "absolute";
			Shadow.style.left = "0px";
			Shadow.style.top = "0px";
			Shadow.style.width = "100%";
			Shadow.style.height = "100%";



			// http://www.stripegenerator.com/generators/generate_stripes.php?fore=000000&h=30&w=1&p=18&back1=171313&back2=ff0000&gt=0&d=0&shadow=5&


			Shadow.style.overflow = "hidden";
			// chrome does not support opacity?
			#endregion

			Func<string, int, double, IHTMLDiv> AddContainerShadow =
				(color, margin, opacity) =>
				{
					var ContainerShadow = new IHTMLDiv().AttachTo(Shadow);

					ContainerShadow.style.position = "absolute";
					ContainerShadow.style.left = "50%";
					ContainerShadow.style.top = "50%";
					ContainerShadow.style.marginLeft = (-400 - margin) + "px";
					ContainerShadow.style.marginTop = (-300 - margin) + "px";
					ContainerShadow.style.backgroundColor = color;
					ContainerShadow.style.width = (800 + margin * 2) + "px";
					ContainerShadow.style.height = (600 + margin * 2) + "px";

					Action<double> ContainerShadow_set_opacity =
						value =>
						{
							ContainerShadow.style.opacity = "" + value;
							ContainerShadow.style.filter = "Alpha(Opacity=" + Convert.ToInt32(value * 100) + ")";
						};

					if (opacity < 1.0)
						ContainerShadow_set_opacity(opacity);

					return ContainerShadow;
				};

			AddContainerShadow("#000000", 800, 0.5);
			for (int i = 0; i < 8; i++)
			{
				AddContainerShadow("#000000", i * 4, 0.4);
			}

			var SuggestionDialog = InitializeSuggestMovie(Context, KnownMovies, Toolbar, Shadow);

			#region ContainerForVideoPlayer
			var ContainerForVideoPlayer = new IHTMLDiv().AttachTo(Context);

			ContainerForVideoPlayer.style.position = "absolute";
			ContainerForVideoPlayer.style.left = "50%";
			ContainerForVideoPlayer.style.top = "50%";
			ContainerForVideoPlayer.style.marginLeft = "-400px";
			ContainerForVideoPlayer.style.marginTop = "-300px";
			ContainerForVideoPlayer.style.backgroundColor = "black";
			ContainerForVideoPlayer.style.width = "800px";
			ContainerForVideoPlayer.style.height = "400px";
			#endregion

			#region ContainerForDetails
			var ContainerForDetails = new IHTMLDiv().AttachTo(Context);

			ContainerForDetails.style.position = "absolute";
			ContainerForDetails.style.left = "50%";
			ContainerForDetails.style.top = "50%";
			ContainerForDetails.style.marginLeft = "-400px";
			ContainerForDetails.style.marginTop = "120px";
			//ContainerForDetails.style.backgroundColor = "red";
			ContainerForDetails.style.width = "800px";
			ContainerForDetails.style.height = "180px";
			ContainerForDetails.style.overflow = "auto";
			ContainerForDetails.style.color = "white";

			//            ContainerForDetails.innerHTML = @"
			//<a href='http://www.imdb.com/title/tt0421715/'  title='Movie: The Curious Case of Benjamin Button 2008'><img  src='http://tinyurl.com/dh5xay'  align='right'  /></a>
			//<h2><a href='http://piratebay.org/torrent/4710971/El.Curioso.Caso.De.Benjamin.Button.[2009].[Spanish].[DVD-Screene'>Movie: The Curious Case of Benjamin Button 2008</a></h2>
			//<div title='raiting'>8.2/10</div>
			//<div title='runtime'>166 min, 1.36 GiB</div>
			//<div title='tagline'>Life isn't measured in minutes, but in moments</div>
			//<div title='genres'>Drama, Fantasy, Mystery, Romance</div>
			//<div title='episode'></div>
			//<a href='http://tinyurl.com/cjhc2g'  title='Movie: The Curious Case of Benjamin Button 2008'><img  src='http://static.thepiratebay.org/img/dl.gif'    /> El.Curioso.Caso.De.Benjamin.Button.[2009].[Spanish].[DVD-Screene</a>
			//";
			#endregion


			YouTubePlayer.Create(ContainerForVideoPlayer, 0, 0,
				VideoPlayer =>
				{
					MovieItem CurrentVideo = null;

					#region ShowVideo
					Action<MovieItem> ShowVideo =
						k =>
						{
							if (k == null)
							{
								VideoPlayer.pauseVideo();

								VideoPlayer.width = 0;
								VideoPlayer.height = 0;

								ContainerForVideoPlayer.style.top = "-100%";
								//ContainerForVideoPlayer.style.display = "none";
								Shadow.style.display = "none";
								SuggestionDialog.style.display = "none";

								ContainerForDetails.style.display = "none";
								Context.Document.title = "zmovies";
								return;
							}

							//ContainerForVideoPlayer.style.display = "block";

							ContainerForVideoPlayer.style.top = "50%";

							VideoPlayer.width = 800;
							VideoPlayer.height = 400;

							if (CurrentVideo == k)
							{
								VideoPlayer.playVideo();
							}
							else
							{
								CurrentVideo = k;
								VideoPlayer.loadVideoById(k.YouTubeKey);
							}

							Context.Document.title = k.SmartTitle;

							Shadow.style.display = "block";
							ContainerForDetails.style.display = "block";

							ContainerForDetails.innerHTML = k.ToDetails();


						};
					#endregion

					ShowVideo(null);


					Shadow.style.cursor = "pointer";
					Shadow.onclick +=
						delegate
						{
							ShowVideo(null);
						};

					// document.getElementById('" + context.Element.id + "')['" + addfeeditem.Token + @"'](result.feed.entries[i].content);

					//{
					//      "title":"Movie: Yes Man 2008",
					//      "link":"http://feedproxy.google.com/~r/zmovies/~3/qVZESWhb0vQ/dfvlm8",
					//      "author":"",
					//      "publishedDate":"",
					//      "contentSnippet":"\n \n \n \n \n \n \n\n\nMovie: Yes Man 2008\n7.2/10\n104 min, 704.11 MiB\nOne word can change everything.\nComedy|Romance\n\n ...",
					//      "content":"\n \n \n \n \u003cembed src\u003d\"http://www.youtube.com/v/Q-Z_CUYh2Sk\u0026amp;hl\u003den\u0026amp;fs\u003d1\" allowScriptAccess\u003d\"never\" allowFullScreen\u003d\"true\" width\u003d\"640\" height\u003d\"385\" wmode\u003d\"transparent\" type\u003d\"application/x-shockwave-flash\"\u003e\u003c/embed\u003e\n \n \n\u003ca href\u003d\"http://www.youtube.com/v/Q-Z_CUYh2Sk\u0026amp;hl\u003den\u0026amp;fs\u003d1\"\u003e\u003cimg alt\u003d\"Q-Z_CUYh2Sk\" src\u003d\"http://img.youtube.com/vi/Q-Z_CUYh2Sk/0.jpg\" align\u003d\"left\"\u003e\u003c/a\u003e\n\u003ca href\u003d\"http://www.imdb.com/title/tt1068680/\" title\u003d\"Movie: Yes Man 2008\"\u003e\u003cimg src\u003d\"http://tinyurl.com/cuc2uo\" align\u003d\"right\"\u003e\u003c/a\u003e\n\u003ch2\u003e\u003ca href\u003d\"http://piratebay.org/torrent/4797620/Yes.Man.2009.DVDRip.XviD-NoRar_\"\u003eMovie: Yes Man 2008\u003c/a\u003e\u003c/h2\u003e\n\u003cdiv title\u003d\"raiting\"\u003e7.2/10\u003c/div\u003e\n\u003cdiv title\u003d\"runtime\"\u003e104 min, 704.11 MiB\u003c/div\u003e\n\u003cdiv title\u003d\"tagline\"\u003eOne word can change everything.\u003c/div\u003e\n\u003cdiv title\u003d\"genres\"\u003eComedy|Romance\u003c/div\u003e\n\u003cdiv title\u003d\"episode\"\u003e\u003c/div\u003e\n\u003ca href\u003d\"http://tinyurl.com/dfvlm8\" title\u003d\"Movie: Yes Man 2008\"\u003e\u003cimg src\u003d\"http://static.thepiratebay.org/img/dl.gif\"\u003e Yes.Man.2009.DVDRip.XviD-NoRar™\u003c/a\u003e\u003cimg src\u003d\"http://feeds2.feedburner.com/~r/zmovies/~4/qVZESWhb0vQ\" height\u003d\"1\" width\u003d\"1\"\u003e",
					//      "categories":[
					//         "Movies",
					//         "Comedy",
					//         "Romance"
					//      ]
					//   },



					VideoPlayer.onStateChange +=
						state =>
						{
							if (state == YouTubePlayer.States.ended)
							{
								ShowVideo(KnownMovies.Where(x => KnownMoviesFilter(x.Movie)).Random().Movie);
							}
						};

					var PendingList = new List<MovieItem>();

					Action PendingListToPosters =
						delegate
						{
							while (PendingList.Count > 0)
							{
								var GroupLeader = PendingList.First();

								PendingList.Remove(GroupLeader);

								Func<MovieItem, bool> Filter = k => k.IMDBTagline == GroupLeader.IMDBTagline;

								if (string.IsNullOrEmpty(GroupLeader.IMDBTagline))
									Filter = k => k.SmartTitleWithoutQuotes == GroupLeader.SmartTitleWithoutQuotes;

								var GroupMembers = PendingList.Where(Filter).ToArray();

								GroupMembers.ForEach(k => PendingList.Remove(k));

								var Group = new[] { GroupLeader }.Concat(GroupMembers).ToArray();

								foreach (var n in Group)
								{
									var poster = new IHTMLImage
									{
										src = n.PosterLink,
										alt = " ",
										title = n.SmartTitleWithoutQuotes + " | " + n.TorrentName + " | " + n.FeedIndex + " of " + n.FeedCapacity
									}.AttachTo(PostersGroup);

									if (n != GroupLeader)
									{
										poster.style.marginLeft = "-3em";
									}

									KnownMovies.Add(
										new MovieItemWithPoster
										{
											Movie = n,
											Poster = poster
										}
									);

									poster.style.cursor = "pointer";
									poster.onclick +=
										delegate
										{
											ShowVideo(n);
										};
								}

							}
						};


					var GFeedReader = Context.ToExternal<object[]>(
						data =>
						{
							var Title = (string)data[0];
							var Link = (string)data[1];
							var Content = (string)data[2];
							var Categories = (string[])data[3];
							var FeedIndex = (int)data[4];
							var FeedCapacity = (int)data[5];

							Content.ParseMovieItem(
								n =>
								{
									// use feedburner link for stats
									n.TorrentLink = Link;
									n.FeedIndex = FeedIndex;
									n.FeedCapacity = FeedCapacity;

									PendingList.Add(n);

									if (FeedIndex + 1 == FeedCapacity)
										PendingListToPosters();
								}
							);


						}
					);

					1.ExternalAtDelay(@"

window['piper'] = function (dummy, result)
{
    for (var i=0; i<result.feed.entries.length; i++) 
    {
		var _x = result.feed.entries[i];
       if (_x.content) 
	   {
			var _v = [_x.title, _x.link, _x.content, _x.categories, i, result.feed.entries.length];
			document.getElementById('" + Context.Element.id + "')['" + GFeedReader + @"'](_v);
       }
    }
};

					");

					// <script src='http://www.google.com/uds/Gfeeds?callback=piper&scoring=h&context=0&num=100&hl=en&output=json&q=http://feeds2.feedburner.com/zmovies&v=1.0&nocache=0'></script>

					// google seems to cache 250 items

					new IHTMLScript
					{
						type = "text/javascript",
						src = "http://www.google.com/uds/Gfeeds?callback=piper&scoring=h&context=0&num=250&hl=en&output=json&q=http://feeds2.feedburner.com/zmovies&v=1.0&nocache=0"
					}.AttachTo(Context);

					// http://flagcounter.com/

					// rect (top, right, bottom, left)

					FooterGroup.innerHTML =
					@"
					<div style='padding: 4em;'>

<center>
						<div style='width:420px;text-align:center;margin:0;padding:0;'>
							<embed src='http://widgets.amung.us/flash/v2map.swf' 
								quality='high' pluginspage='http://www.macromedia.com/go/getflashplayer' 
								wmode='transparent'
								allowScriptAccess='always' 
								allowNetworking='all' type='application/x-shockwave-flash' 
								flashvars='wausitehash=eop2ht6aucqf&map=heatmap&pin=star-blue&link=no' 
								width='420' height='210' />
							<div style='width:420px;height:210px;position:relative;margin:0 auto;margin-top:-210px;'><a href='http://whos.amung.us/stats/eop2ht6aucqf/'><img src='http://maps.amung.us/ping/eop2ht6aucqf.gif' border='0' width='420' height='210' /></a></div>
						</div>
</center>
<br />
						<div>
						<a href='http://s03.flagcounter.com/more/Y7bi'><img src='http://s03.flagcounter.com/count/Y7bi/bg=000000/txt=FFFFFF/border=CCCCCC/columns=8/maxflags=32/viewers=3/labels=0/pageviews=1/' alt='free counters' border='0'></a>
						</div>
					</div>
										";
				}
			);






		}


	}

}