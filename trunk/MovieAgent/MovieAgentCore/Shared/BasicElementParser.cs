﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;

namespace MovieAgent.Shared
{
	[Script]
	public class BasicElementParser
	{
		public event Action<string, int> AddContent;

		public BasicElementParser()
		{

		}

		public static void Parse(string data, string tag, Action<string, int> AddContent)
		{
			var p = new BasicElementParser();

			p.AddContent += AddContent;

			p.Parse(data, tag);
		}

		public void Parse(string data, string tag)
		{
			var i = 0;
			int index = 0;

			while (i >= 0)
			{
				i = ParseSingle(data, tag, i, index);


				index++;
			}

		}

		int ParseSingle(string data, string tag, int offset, int index)
		{
			var tagstartopen = data.IndexOf("<" + tag, offset);

			if (tagstartopen < 0)
				return -1;

			//Console.WriteLine("tagstartopen: " + tagstartopen);

			var tagstartclose = data.IndexOf(">", tagstartopen);

			if (tagstartclose < 0)
				return -1;

			//Console.WriteLine("tagstartclose: " + tagstartopen);

			var tagend = data.IndexOf("</" + tag + ">", tagstartclose);

			if (tagend < 0)
				return -1;

			//Console.WriteLine("tagend: " + tagend);


			if (AddContent != null)
				AddContent(data.Substring(tagstartclose + 1, tagend - tagstartclose - 1), index);

			var result = tagend + tag.Length + 3;

			//var info = new { result, offset, tagend };

			//Console.WriteLine(info.ToString());


			return result;
		}



		public static string GetContent(string data, string tag)
		{
			var content = "";

			BasicElementParser.Parse(data, tag,
				(value, index) =>
				{
					if (index == 0)
						content = value;
				}
			);

			return content;
		}


	
	}
}
