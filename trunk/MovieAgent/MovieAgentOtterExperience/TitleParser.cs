using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieAgentOtterExperience
{
	public class TitleParser
	{
		public readonly string Type;
		public readonly string Title;
		public readonly string Year;

		public TitleParser(string title)
		{
			var i = title.IndexOf(":");
			this.Type = title.Substring(0, i);
			this.Title = title.Substring(i + 2, title.Length - i - 7);
			this.Year = title.Substring(title.Length - 4);
		}
	}
}
