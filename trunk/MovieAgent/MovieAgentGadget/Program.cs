using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieAgent.Shared;
using MovieAgentGadget.Data;

namespace MovieAgentGadget
{
	class Program
	{
		public static void Main()
		{
			var data = @"
		<embed src=""http://www.youtube.com/v/Q-Z_CUYh2Sk&amp;hl=en&amp;fs=1"" allowScriptAccess=""never"" allowFullScreen=""true"" width=""640"" height=""385"" wmode=""transparent"" type=""application/x-shockwave-flash""></embed>
	
							
<a href=""http://www.youtube.com/v/Q-Z_CUYh2Sk&amp;hl=en&amp;fs=1""><img alt=""Q-Z_CUYh2Sk"" src=""http://img.youtube.com/vi/Q-Z_CUYh2Sk/0.jpg"" align=""left""></a>
<a href=""http://www.imdb.com/title/tt1068680/"" title=""Movie: Yes Man 2008""><img src=""http://tinyurl.com/cuc2uo"" align=""right""></a>
<h2><a href=""http://piratebay.org/torrent/4797620/Yes.Man.2009.DVDRip.XviD-NoRar_"">Movie: Yes Man 2008</a></h2>
<div title=""raiting"">7.2/10</div>
<div title=""runtime"">104 min, 704.11 MiB</div>
<div title=""tagline"">One word can change everything.</div>
<div title=""genres"">Comedy|Romance</div>
<div title=""episode""></div>
<a href=""http://tinyurl.com/dfvlm8"" title=""Movie: Yes Man 2008""><img src=""http://static.thepiratebay.org/img/dl.gif""> Yes.Man.2009.DVDRip.XviD-NoRar™</a><img src=""http://feeds2.feedburner.com/~r/zmovies/~4/qVZESWhb0vQ"" height=""1"" width=""1"">
";

			data.ParseMovieItem(
				n =>
				{

				}
			);
		}
	}
}
