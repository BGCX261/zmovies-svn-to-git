using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
using ScriptCoreLib.ActionScript.flash.display;
using ScriptCoreLib.ActionScript.flash.external;
using ScriptCoreLib.ActionScript.flash.system;
using ScriptCoreLib.ActionScript.flash.text;
using ScriptCoreLib.ActionScript.flash.ui;
using ScriptCoreLib.ActionScript.flash.utils;
using ScriptCoreLib.Shared.Lambda;
using MovieAgent.Shared;

namespace MovieAgentGadget.ActionScript
{

	[GoogleGadget(
	   author_email = Info.EMail,
	   author_link = Info.Blog,
	   author = Info.Author,
	   category = Info.GoogleGadget.Category1,
	   category2 = Info.GoogleGadget.Category2,
	   screenshot = "",
	   thumbnail = "",
	   description = Info.Description,
	   width = DefaultWidth,
	   height = DefaultHeight,
	   title = Info.Title,
	   title_url = "http://zmovies.tk",
	   src = Info.ClientSource
	)]
	[Script, ScriptApplicationEntryPoint(Width = 0, Height = 0, Feed = DefaultFeed)]
	[SWF(width = DefaultWidth, height = DefaultHeight)]
	public partial class MovieAgentGadget : Sprite
	{
		// public const string DefaultFeed = "http://zproxy.planet.ee/zmovies/feed.xml?5";
		public const string DefaultFeed = "http://feeds2.feedburner.com/zmovies";

		public const int DefaultWidth = 840;
		public const int DefaultHeight = 640;

		// // change: C:\util\xampplite\apache\conf\httpd.conf

		// http://localhost/jsc/MovieAgentGadget/MovieAgentGadget.htm

		//Alias /jsc/MovieAgentGadget "C:\work\code.google\zmovies\MovieAgent\MovieAgentGadget\bin\Release\web"
		//<Directory "C:\work\code.google\zmovies\MovieAgent\MovieAgentGadget\bin\Release\web">
		//       Options Indexes FollowSymLinks ExecCGI
		//       AllowOverride All
		//       Order allow,deny
		//       Allow from all
		//</Directory>

		// http://curtismorley.com/2008/11/01/actionscript-security-error-2060-security-sandbox-violation/
		// http://mihai.bazon.net/blog/flash-s-externalinterface-and-ie
		// http://bugs.adobe.com/jira/browse/FP-692
		// http://team.mixmedia.com/index.php?title=ie_flash9_0_16_0_externalinterface_unkno&more=1&c=1&tb=1&pb=1



		/// <summary>
		/// Default constructor
		/// </summary>
		public MovieAgentGadget()
		{
			Security.allowDomain("*");

			ExternalContext.ExternalAuthentication(Initialize);

		}

		static MovieAgentGadget()
		{
			// add resources to be found by ImageSource
			KnownEmbeddedAssets.RegisterTo(
				KnownEmbeddedResources.Default.Handlers
			);
		}

	}

	[Script]
	public class KnownEmbeddedAssets
	{
		[EmbedByFileName]
		public static Class ByFileName(string e)
		{
			throw new NotImplementedException();
		}

		public static void RegisterTo(List<Converter<string, Class>> Handlers)
		{
			// assets from current assembly
			Handlers.Add(e => ByFileName(e));

			//AvalonUgh.Assets.ActionScript.KnownEmbeddedAssets.RegisterTo(Handlers);

			//// assets from referenced assemblies
			//Handlers.Add(e => global::ScriptCoreLib.ActionScript.Avalon.Cursors.EmbeddedAssets.Default[e]);
			//Handlers.Add(e => global::ScriptCoreLib.ActionScript.Avalon.TiledImageButton.Assets.Default[e]);

		}
	}

}