using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieAgent.Server.Library;
using ScriptCoreLib.Tools;
using System.IO;

namespace MovieAgentOtterExperience.GFeed
{
	public class Document
	{
		public FeedContainer responseData;
		public string responseDetails;
		public int responseStatus;

		public static implicit operator Document(Uri src)
		{
			var n = new Document();
			var c = new BasicWebCrawler(src.Host, 80);

			c.DataReceived +=
				document =>
				{
					using (var s = new MemoryStream(Encoding.ASCII.GetBytes(document)))
						JSONSerializer.Deserialize(n, s);
				};

			c.Crawl(src.PathAndQuery);

			return n;
		}

		public static implicit operator Document(FileInfo src)
		{
			var n = new Document();

			var document = File.ReadAllText(src.FullName);
			using (var s = new MemoryStream(Encoding.ASCII.GetBytes(document)))
				JSONSerializer.Deserialize(n, s);

			return n;
		}
	}

	public class FeedContainer
	{
		public Feed feed;
	}

	public class Feed
	{
		public string title;
		public string link;
		public string author;
		public string description;
		public string type;
		public FeedEntry[] entries;
	}

	public class FeedEntry
	{
		public string title;
		public string link;
		public string author;
		public string publishedDate;
		public string contentSnippet;
		public string content;
		public string[] categories;
	}

	/* example data
{
	"responseData": {
		"feed": {
			"title": "zmovies", 
			"link": "http://zproxy.planet.ee/zmovies", 
			"author": "", 
			"description": "zmovies can haz entertainment", 
			"type": "rss20", 
			"entries": [
				{
					"title": "Movie: Spring Breakdown 2009", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/q3jTfQKRETY/ovd9bc", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nMovie: Spring Breakdown 2009\n5.9/10\nArgentina:84 min (DVD version), 698.53 MiB\nPayback&#x27;s A ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/8pT4wouP2Ac&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/8pT4wouP2Ac&amp;hl=en&amp;fs=1\"><img alt=\"8pT4wouP2Ac\" src=\"http://img.youtube.com/vi/8pT4wouP2Ac/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt0814331/\" title=\"Movie: Spring Breakdown 2009\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4915661/Spring_Breakdown_(2009)_DVDRip-DiVERSE\">Movie: Spring Breakdown 2009</a></h2>\n<div title=\"raiting\">5.9/10</div>\n<div title=\"runtime\">Argentina:84 min (DVD version), 698.53 MiB</div>\n<div title=\"tagline\">Payback&#39;s A Beach!</div>\n<div title=\"genres\">Comedy</div>\n<div title=\"episode\"></div>\n<a href=\"http://tinyurl.com/ovd9bc\" title=\"Movie: Spring Breakdown 2009\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> Spring Breakdown (2009) DVDRip-DiVERSE</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/q3jTfQKRETY\" height=\"1\" width=\"1\">", 
					"categories": [
						"Movies", 
						"Comedy"
					]
				}, 
				{
					"title": "Movie: Night at the Museum: Battle of the Smithsonian 2009", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/tGToAyE-07Y/qypfje", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nMovie: Night at the Museum: Battle of the Smithsonian 2009\n6.2/10\n105 min, 699.9 MiB\nWhen the lights ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/QCR_fgG0ydY&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/QCR_fgG0ydY&amp;hl=en&amp;fs=1\"><img alt=\"QCR_fgG0ydY\" src=\"http://img.youtube.com/vi/QCR_fgG0ydY/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt1078912/\" title=\"Movie: Night at the Museum: Battle of the Smithsonian 2009\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4911483/Night.at.the.Museum.2.Battle.of.the.Smithsonian.2009.Cam.XviD-Li\">Movie: Night at the Museum: Battle of the Smithsonian 2009</a></h2>\n<div title=\"raiting\">6.2/10</div>\n<div title=\"runtime\">105 min, 699.9 MiB</div>\n<div title=\"tagline\">When the lights go off the battle is on.</div>\n<div title=\"genres\">Action, Comedy</div>\n<div title=\"episode\"></div>\n<a href=\"http://tinyurl.com/qypfje\" title=\"Movie: Night at the Museum: Battle of the Smithsonian 2009\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> Night.at.the.Museum.2.Battle.of.the.Smithsonian.2009.Cam.XviD-Li</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/tGToAyE-07Y\" height=\"1\" width=\"1\">", 
					"categories": [
						"Movies", 
						"Action", 
						"Comedy"
					]
				}, 
				{
					"title": "Movie: Night at the Museum: Battle of the Smithsonian 2009", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/Aor4FCBAFXo/p2uphy", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nMovie: Night at the Museum: Battle of the Smithsonian 2009\n6.2/10\n105 min, 1.42 GiB\nWhen the lights go ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/QCR_fgG0ydY&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/QCR_fgG0ydY&amp;hl=en&amp;fs=1\"><img alt=\"QCR_fgG0ydY\" src=\"http://img.youtube.com/vi/QCR_fgG0ydY/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt1078912/\" title=\"Movie: Night at the Museum: Battle of the Smithsonian 2009\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4911528/Night.At.The.Museum.Battle.Of.The.Smithsonian.2009.TELESYNC\">Movie: Night at the Museum: Battle of the Smithsonian 2009</a></h2>\n<div title=\"raiting\">6.2/10</div>\n<div title=\"runtime\">105 min, 1.42 GiB</div>\n<div title=\"tagline\">When the lights go off the battle is on.</div>\n<div title=\"genres\">Action, Comedy</div>\n<div title=\"episode\"></div>\n<a href=\"http://tinyurl.com/p2uphy\" title=\"Movie: Night at the Museum: Battle of the Smithsonian 2009\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> Night.At.The.Museum.Battle.Of.The.Smithsonian.2009.TELESYNC</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/Aor4FCBAFXo\" height=\"1\" width=\"1\">", 
					"categories": [
						"Movies", 
						"Action", 
						"Comedy"
					]
				}, 
				{
					"title": "Movie: Night at the Museum: Battle of the Smithsonian 2009", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/wI1Q-2eFm1g/pb5bks", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nMovie: Night at the Museum: Battle of the Smithsonian 2009\n6.2/10\n105 min, 1.42 GiB\nWhen the lights go ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/QCR_fgG0ydY&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/QCR_fgG0ydY&amp;hl=en&amp;fs=1\"><img alt=\"QCR_fgG0ydY\" src=\"http://img.youtube.com/vi/QCR_fgG0ydY/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt1078912/\" title=\"Movie: Night at the Museum: Battle of the Smithsonian 2009\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4910279/Night_At_The_Museum__Battle_Of_The_Smithsonian_2009_TELESYNC_AAC\">Movie: Night at the Museum: Battle of the Smithsonian 2009</a></h2>\n<div title=\"raiting\">6.2/10</div>\n<div title=\"runtime\">105 min, 1.42 GiB</div>\n<div title=\"tagline\">When the lights go off the battle is on.</div>\n<div title=\"genres\">Action, Comedy</div>\n<div title=\"episode\"></div>\n<a href=\"http://tinyurl.com/pb5bks\" title=\"Movie: Night at the Museum: Battle of the Smithsonian 2009\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> Night At The Museum: Battle Of The Smithsonian 2009 TELESYNC AAC</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/wI1Q-2eFm1g\" height=\"1\" width=\"1\">", 
					"categories": [
						"Movies", 
						"Action", 
						"Comedy"
					]
				}, 
				{
					"title": "Movie: X-Men Origins: Wolverine 2009", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/fZNrKuu7bgo/pfubor", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nMovie: X-Men Origins: Wolverine 2009\n6.8/10\n107 min, 4.1 GiB\n\nAction, Fantasy, Sci-Fi, Thriller\n\n ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/LPmbGzQaOCs&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/LPmbGzQaOCs&amp;hl=en&amp;fs=1\"><img alt=\"LPmbGzQaOCs\" src=\"http://img.youtube.com/vi/LPmbGzQaOCs/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt0458525/\" title=\"Movie: X-Men Origins: Wolverine 2009\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4911147/X-Men.Origins.Wolverine.PROPER.R5.DVDR_Multi-sailo1\">Movie: X-Men Origins: Wolverine 2009</a></h2>\n<div title=\"raiting\">6.8/10</div>\n<div title=\"runtime\">107 min, 4.1 GiB</div>\n<div title=\"tagline\"></div>\n<div title=\"genres\">Action, Fantasy, Sci-Fi, Thriller</div>\n<div title=\"episode\"></div>\n<a href=\"http://tinyurl.com/pfubor\" title=\"Movie: X-Men Origins: Wolverine 2009\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> X-Men.Origins.Wolverine.PROPER.R5.DVDR Multi-sailo1</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/fZNrKuu7bgo\" height=\"1\" width=\"1\">", 
					"categories": [
						"Movies", 
						"Action", 
						"Fantasy", 
						"Sci-Fi", 
						"Thriller"
					]
				}, 
				{
					"title": "TV show: Prison Break: The Final Break S04E23 2009", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/4LAe9ZJAVAY/pbalyw", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nTV show: Prison Break: The Final Break S04E23 2009\n9.7/10\n, 704.45 MiB\nPrepare yourself for the ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/QLC9E5JXRyU&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/QLC9E5JXRyU&amp;hl=en&amp;fs=1\"><img alt=\"QLC9E5JXRyU\" src=\"http://img.youtube.com/vi/QLC9E5JXRyU/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt1131748/\" title=\"TV show: Prison Break: The Final Break S04E23 2009\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4912431/Prison_Break__The_Final_Break_S04E23-24_%5BWS.PDTV.XviD-iLM%5D_%5BENG%5D\">TV show: Prison Break: The Final Break S04E23 2009</a></h2>\n<div title=\"raiting\">9.7/10</div>\n<div title=\"runtime\">, 704.45 MiB</div>\n<div title=\"tagline\">Prepare yourself for the truth!</div>\n<div title=\"genres\">Action, Drama, Thriller</div>\n<div title=\"episode\">S04E23</div>\n<a href=\"http://tinyurl.com/pbalyw\" title=\"TV show: Prison Break: The Final Break S04E23 2009\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> Prison Break: The Final Break S04E23-24 [WS.PDTV.XviD-iLM] [ENG]</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/4LAe9ZJAVAY\" height=\"1\" width=\"1\">", 
					"categories": [
						"TV shows", 
						"Action", 
						"Drama", 
						"Thriller"
					]
				}, 
				{
					"title": "Movie: Terminator Salvation 2009", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/ulL_8V99lUk/qq62fp", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nMovie: Terminator Salvation 2009\n7.5/10\n130 min, 837.34 MiB\nThe End Begins\nAction, Adventure, Sci-Fi, ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/mRhCOSseP6Y&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/mRhCOSseP6Y&amp;hl=en&amp;fs=1\"><img alt=\"mRhCOSseP6Y\" src=\"http://img.youtube.com/vi/mRhCOSseP6Y/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt0438488/\" title=\"Movie: Terminator Salvation 2009\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4912179/Terminator.Salvation.CAM.XViD-InD\">Movie: Terminator Salvation 2009</a></h2>\n<div title=\"raiting\">7.5/10</div>\n<div title=\"runtime\">130 min, 837.34 MiB</div>\n<div title=\"tagline\">The End Begins</div>\n<div title=\"genres\">Action, Adventure, Sci-Fi, Thriller</div>\n<div title=\"episode\"></div>\n<a href=\"http://tinyurl.com/qq62fp\" title=\"Movie: Terminator Salvation 2009\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> Terminator.Salvation.CAM.XViD-InD</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/ulL_8V99lUk\" height=\"1\" width=\"1\">", 
					"categories": [
						"Movies", 
						"Action", 
						"Adventure", 
						"Sci-Fi", 
						"Thriller"
					]
				}, 
				{
					"title": "Movie: Airplane! 1980", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/W2cKpBKmwRA/p5qob9", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nMovie: Airplane! 1980\n7.8/10\n88 min, 702.57 MiB\nYou&#x27;ve read the ad, now see the movie!\nComedy, ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/qaXvFT_UyI8&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/qaXvFT_UyI8&amp;hl=en&amp;fs=1\"><img alt=\"qaXvFT_UyI8\" src=\"http://img.youtube.com/vi/qaXvFT_UyI8/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt0080339/\" title=\"Movie: Airplane! 1980\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4723144/Kentucky-Fried-Movie%5B1977%5DDVDrip--rEACTOr--\">Movie: Airplane! 1980</a></h2>\n<div title=\"raiting\">7.8/10</div>\n<div title=\"runtime\">88 min, 702.57 MiB</div>\n<div title=\"tagline\">You&#39;ve read the ad, now see the movie!</div>\n<div title=\"genres\">Comedy, Romance</div>\n<div title=\"episode\"></div>\n<a href=\"http://tinyurl.com/p5qob9\" title=\"Movie: Airplane! 1980\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> Kentucky-Fried-Movie[1977]DVDrip--rEACTOr--</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/W2cKpBKmwRA\" height=\"1\" width=\"1\">", 
					"categories": [
						"Movies", 
						"Comedy", 
						"Romance"
					]
				}, 
				{
					"title": "Movie: X-Men Origins: Wolverine 2009", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/w-pQhAu80Pw/qcr4fg", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nMovie: X-Men Origins: Wolverine 2009\n6.8/10\n107 min, 699.7 MiB\n\nAction, Fantasy, Sci-Fi, Thriller\n\n ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/LPmbGzQaOCs&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/LPmbGzQaOCs&amp;hl=en&amp;fs=1\"><img alt=\"LPmbGzQaOCs\" src=\"http://img.youtube.com/vi/LPmbGzQaOCs/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt0458525/\" title=\"Movie: X-Men Origins: Wolverine 2009\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4910355/X-Men.Origins.Wolverine.PROPER.R5.XViD-NO\">Movie: X-Men Origins: Wolverine 2009</a></h2>\n<div title=\"raiting\">6.8/10</div>\n<div title=\"runtime\">107 min, 699.7 MiB</div>\n<div title=\"tagline\"></div>\n<div title=\"genres\">Action, Fantasy, Sci-Fi, Thriller</div>\n<div title=\"episode\"></div>\n<a href=\"http://tinyurl.com/qcr4fg\" title=\"Movie: X-Men Origins: Wolverine 2009\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> X-Men.Origins.Wolverine.PROPER.R5.XViD-NO</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/w-pQhAu80Pw\" height=\"1\" width=\"1\">", 
					"categories": [
						"Movies", 
						"Action", 
						"Fantasy", 
						"Sci-Fi", 
						"Thriller"
					]
				}, 
				{
					"title": "Movie: Lesser of Three Evils 2005", 
					"link": "http://feedproxy.google.com/~r/zmovies/~3/33l24XZB9h8/os6ra9", 
					"author": "", 
					"publishedDate": "", 
					"contentSnippet": "\n\t\t\n\t\t\n\t\t\n\t\t\n\t\n\t\t\t\t\t\t\t\n\n\nMovie: Lesser of Three Evils 2005\n4.1/10\n88 min, 845.25 MiB\nSome sins can never be forgiven.\nAction, ...", 
					"content": "\n\t\t\n\t\t\n\t\t\n\t\t<embed src=\"http://www.youtube.com/v/wf0Z-UP9aZA&amp;hl=en&amp;fs=1\" allowScriptAccess=\"never\" allowFullScreen=\"true\" width=\"640\" height=\"385\" wmode=\"transparent\" type=\"application/x-shockwave-flash\"></embed>\n\t\n\t\t\t\t\t\t\t\n<a href=\"http://www.youtube.com/v/wf0Z-UP9aZA&amp;hl=en&amp;fs=1\"><img alt=\"wf0Z-UP9aZA\" src=\"http://img.youtube.com/vi/wf0Z-UP9aZA/0.jpg\" align=\"left\"></a>\n<a href=\"http://www.imdb.com/title/tt1274418/\" title=\"Movie: Lesser of Three Evils 2005\"><img src=\"http://tinyurl.com/clr7qy\" align=\"right\"></a>\n<h2><a href=\"http://piratebay.org/torrent/4901366/Fist.Of.The.Warrior.2009.DVDRip.XviD-NoGrp\">Movie: Lesser of Three Evils 2005</a></h2>\n<div title=\"raiting\">4.1/10</div>\n<div title=\"runtime\">88 min, 845.25 MiB</div>\n<div title=\"tagline\">Some sins can never be forgiven.</div>\n<div title=\"genres\">Action, Crime, Drama, Thriller</div>\n<div title=\"episode\"></div>\n<a href=\"http://tinyurl.com/os6ra9\" title=\"Movie: Lesser of Three Evils 2005\"><img src=\"http://static.thepiratebay.org/img/dl.gif\"> Fist.Of.The.Warrior.2009.DVDRip.XviD-NoGrp</a><img src=\"http://feeds2.feedburner.com/~r/zmovies/~4/33l24XZB9h8\" height=\"1\" width=\"1\">", 
					"categories": [
						"Movies", 
						"Action", 
						"Crime", 
						"Drama", 
						"Thriller"
					]
				}
			]
		}
	}, 
	"responseDetails": null, 
	"responseStatus": 200
}	 
	 
	 */
}
