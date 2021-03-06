﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;

namespace MovieAgent.Shared
{
	[Script]
	public static class SharedExtensions
	{
		// extensions defined here shall support all languages including:
		// actionscript,
		// javascript
		// php
		// java

		public const string HomePageText = "Visit jsc.sourceforge.net";
		public const string HomePage = "http://jsc.sf.net/";

		public const string TemplateSourceCode = "http://jsc.svn.sourceforge.net/viewvc/jsc/templates/OrcasAvalonWebApplication/";

		public static string WithBranding(this string text)
		{
			return text + " - powered by jsc";
		}

		public static Func<A, T> ToAnonymousConstructor<T, A>(this T prototype, Func<A, T> ctor)
		{
			return ctor;
		}



		[Script]
		public interface IParseAttributeToken_ParseAttribute : IParseAttributeToken_ParseContent, IParseAttributeToken_Parse
		{
			string Name { get; }



		}

		[Script]
		public interface IParseAttributeToken_ParseContent : IParseAttributeToken_Parse
		{

		}



		[Script]
		public interface IParseAttributeToken_Parse
		{
			IParseAttributeToken_ParseAttribute AttributeHandler { get; }

			Action<string> SetValue { get; }


			IParseAttributeToken_Context Context { get; }
		}

		[Script]
		public interface IParseAttributeToken_Context
		{
			string Source { get; }
		}

		[Script]
		public class ParseAttributeToken : IParseAttributeToken_ParseAttribute, IParseAttributeToken_Context
		{
			public string Name { get; set; }
			public Action<string> SetValue { get; set; }

			public string Source { get; set; }

			public IParseAttributeToken_Context Context { get; set; }

			public IParseAttributeToken_ParseAttribute AttributeHandler { get; set; }
		}

		public static IParseAttributeToken_ParseAttribute ParseAttribute(this string element, string name, Action<string> setvalue)
		{
			var Context = new ParseAttributeToken { Source = element };

			return new ParseAttributeToken
			{
				Name = name,
				SetValue = setvalue,
				Context = Context,
			};
		}

		public static IParseAttributeToken_ParseAttribute ParseAttribute(this IParseAttributeToken_ParseAttribute element, string name, Action<string> setvalue)
		{
			return new ParseAttributeToken
			{
				AttributeHandler = element,
				Name = name,
				SetValue = setvalue,
				Context = element.Context,
			};
		}

		public static IParseAttributeToken_Parse ParseContent(this IParseAttributeToken_ParseAttribute element, Action<string> setvalue)
		{
			return new ParseAttributeToken
			{
				AttributeHandler = element,
				SetValue = setvalue,
				Context = element.Context,
			};
		}

		public static void Parse(this IParseAttributeToken_Parse element)
		{
			Parse(element, "");
		}

		public static void Parse(this IParseAttributeToken_Parse element, string etag)
		{
			var Source = element.Context.Source;

			var element_start = Source.IndexOf("<" + etag);

			var attributes_start = Source.IndexOf(" ", element_start);

			var attibutes_end = Source.IndexOf(">", element_start);

			var tag = "";
			var element_fast_end = -1;

			if (attributes_start < attibutes_end)
			{
				if (attributes_start < 0)
					return;


				tag = Source.Substring(element_start + 1, attributes_start - element_start - 1);

				// seek for attributes

				element.AttributeHandler.InternalParseAttributes(Source.Substring(attributes_start, attibutes_end - attributes_start));
				element_fast_end = Source.IndexOf("/>", attributes_start);
			}
			else
			{
				if (attibutes_end < 0)
					return;

				tag = Source.Substring(element_start + 1, attibutes_end - element_start - 1);

				element_fast_end = Source.IndexOf("/>", attibutes_end);
				// seek for no attributes
			}

			var element_end = Source.IndexOf("</" + tag, attibutes_end);

			if (element_fast_end >= 0)
			{
				if (element_end < 0)
					return;
			}

			// we are unable to find the closing tag
			// as we do not care about the content we could
			// just ignore this and return
			if (element.SetValue == null)
				if (element_end < 0)
					return;

			var content = Source.Substring(attibutes_end + 1, element_end - attibutes_end - 1);
			element.SetValue(content);
		}

		static void InternalParseAttributes(this IParseAttributeToken_ParseAttribute element, string data)
		{
			Action<string, string> SetValue =
				(name, value) =>
				{
					var p = element;

					while (p != null)
					{
						if (p.Name == name)
						{
							if (p.SetValue != null)
								p.SetValue(value);

							p = null;
						}
						else
						{
							p = p.AttributeHandler;
						}
					}
				};

			Func<int, int> ParseAttribute =
				offset =>
				{
					var equals = data.IndexOf("=", offset);

					if (equals < 0)
						return offset;

					var name = data.Substring(offset + 1, equals - offset - 1);

					var value_quot = "\"";
					var value_start = data.IndexOf(value_quot, equals);

					var value_start_apos = data.IndexOf("'", equals);
					if (value_start_apos > -1)
						if (value_start < 0)
						{
							value_start = value_start_apos;
							value_quot = "'";
						}
						else if (value_start_apos < value_start)
						{
							value_quot = "'";
							value_start = value_start_apos;
						}

					if (value_start < 0)
						return offset;

					var value_end = data.IndexOf(value_quot, value_start + 1);

				

					if (value_end < 0)
						return offset;

					var value = data.Substring(value_start + 1, value_end - value_start - 1);

					SetValue(name, value);

					return value_end + 1;
				};

			ParseAttribute.ToChainedFunc((x, y) => y > x)(0);
		}



		public static int ParseElements(this string Source, Action<string, int, string> AddElement)
		{
			var index = -1;

			Func<int, int> ParseSingleElement =
				offset =>
				{
					var element_start = Source.IndexOf("<", offset);

					if (element_start < 0)
						return offset;

					var attributes_start = Source.IndexOf(" ", element_start);
					var attibutes_end = Source.IndexOf(">", element_start);

					var tag = "";
					var element_fast_end = -1;

					if (attributes_start < attibutes_end)
					{
						if (attributes_start < 0)
							return offset;

						tag = Source.Substring(element_start + 1, attributes_start - element_start - 1);
						element_fast_end = Source.IndexOf("/>", attributes_start);
						// seek for attributes

					}
					else
					{
						if (attibutes_end < 0)
							return offset;

						tag = Source.Substring(element_start + 1, attibutes_end - element_start - 1);
						element_fast_end = Source.IndexOf("/>", attibutes_end);
						// seek for no attributes
					}

					var element_end = Source.IndexOf("</" + tag + ">", attibutes_end);

					if (element_fast_end >= 0)
					{
						if (element_end < 0)
						{
							var next_element = element_fast_end + 2;
							var element = Source.Substring(element_start, next_element - element_start);
							index++;
							AddElement(tag, index, element);
							return next_element;
						}
					}


					{



						if (element_end < 0)
							return offset;

						var next_element = element_end + 3 + tag.Length;
						var element = Source.Substring(element_start, next_element - element_start);
						index++;
						AddElement(tag, index, element);
						return next_element;
					}
				};

			ParseSingleElement.ToChainedFunc((x, y) => y > x)(0);

			return index;
		}

		public static Func<T, T> ToChainedFunc<T>(this Func<T, T> e, int count)
		{
			return
				value =>
				{
					var p = e(value);


					for (int i = 1; i < count; i++)
					{
						p = e(p);
					}

					return p;
				};
		}

		public static Func<T, T> ToChainedFunc<T>(this Func<T, T> e, Func<T, T, bool> reinvoke)
		{
			return
				value =>
				{
					var x = value;
					var p = e(x);

					while (reinvoke(x, p))
					{
						x = p;
						p = e(x);
					}

					return p;
				};
		}

		public static int XorBytes(this string e)
		{
			var x = 0;

			for (int i = 0; i < e.Length; i++)
			{
				x ^= (byte)(e[i]);
			}

			return x;
		}


		public static string Substring(this string e, string a, string b)
		{
			var i = e.IndexOf(a);
			var j = e.IndexOf(b, i);

			return e.Substring(i + a.Length, j - i - a.Length);
		}

		public static string[] Trim(this string[] e)
		{
			var u = new string[e.Length];

			for (int i = 0; i < e.Length; i++)
			{
				u[i] = e[i].Trim();
			}

			return u;
		}

		public static string ToAttributeString(this string value, string key)
		{
			if (string.IsNullOrEmpty(value))
				return "";

			return key + "='" + value
				.Replace("&", "&amp;")
				.Replace("'", "&apos;")
				.Replace("\"", "&qout;") + "'";
		}

		public static string ToLink(this string text, string href)
		{
			return "<a href='" + href + "'>" + text + "</a>";
		}
	}
}
