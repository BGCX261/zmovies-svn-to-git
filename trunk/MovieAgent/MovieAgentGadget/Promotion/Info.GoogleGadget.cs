using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieAgentGadget.Promotion
{
	public static partial class Info
	{
		public static class GoogleGadget
		{

			public const string Category1 = "lifestyle";
			public const string Category2 = "funandgames";

			public const string Module = "http://zproxy.planet.ee/zmovies/MovieAgentGadget.GoogleGadget.xml";

			public const string AddLink = "http://fusion.google.com/ig/add?synd=open&source=ggyp&moduleurl=" + Module;

			public const string AddImage = "http://www.gmodules.com/ig/images/plus_google.gif";
			public const string AddImageWide = "http://buttons.googlesyndication.com/fusion/add.gif";

			public const string AddToWebPageLink = "http://www.google.com/ig/creator?url=" + Module;
			public const string AddToYourWebPageLink = "http://www.gmodules.com/ig/creator?synd=open&hl=en&url=" + Module;

		}
	}
}
