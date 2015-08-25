using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLib.ActionScript.flash.external;
using ScriptCoreLib.ActionScript.Extensions;
using ScriptCoreLib.ActionScript.flash.utils;
using ScriptCoreLib.ActionScript.DOM.HTML;
using ScriptCoreLib.ActionScript.DOM;
using MovieAgent.Shared;

namespace MovieAgentGadget.ActionScript
{
	/// <summary>
	/// This class defines the extension methods for this project
	/// </summary>
	[Script]
	internal static class Extensions
	{

		public static string ToImageMiddle(this string src)
		{
			return "<img " + "middle".ToAttributeString("valign") + " " + src.ToAttributeString("src")
				+ " " + "0".ToAttributeString("border") + " />";
		}


		public static string ToImage(this string src)
		{
			return "<img " + src.ToAttributeString("src") 
				+ " " + "0".ToAttributeString("border") + " />";
		}

		public static string ToImageForPixlr(this string src)
		{
			return "<a href='http://www.pixlr.com/editor/?image=" + src + "' title='Edit in pixlr'><img src='" + src + "' /></a>";
		}

		public static string ToTinEyeImageLink(this string e)
		{
			return "http://tineye.com.nyud.net/query/" + e;
		}

		public static Timer AtInterval(this int e, Action h)
		{
			var t = new Timer(e);
			t.timer += delegate { h(); };
			t.start();
			return t;
		}

		public static Timer AtDelay(this int e, Action h)
		{
			var t = new Timer(e, 1);
			t.timer += delegate { h(); };
			t.start();
			return t;

		}

		
	}
}
