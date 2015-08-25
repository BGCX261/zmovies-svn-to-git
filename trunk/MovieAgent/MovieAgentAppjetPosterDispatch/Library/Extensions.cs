using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;

namespace MovieAgentAppjetPosterDispatch.Library
{
	[Script]
	public static class Extensions
	{
		public static string ToImage(this string src)
		{
			return "<img src='" + src + "' />";
		}
	}
}
