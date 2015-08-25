using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Shared;

namespace MovieAgent.Server.Library
{

	[Script]
	public class IStyle
	{
		public string @float;
		public string overflow;
		public string color;
		public string backgroundColor;
		public string position;
		public string left;
		public string top;
		public string width;
		public string height;
		public string textAlign;
		public string borderWidth;
		public string paddingTop;
		public string display;

		public double opacity = 1;

		public override string ToString()
		{
			var w = new StringBuilder();

			Action<string, string> Append =
				(key, value) =>
				{
					if (value == null)
						return;

					w.Append(key).Append(": ").Append(value).Append("; ");
				};

			Append("float", @float);
			Append("display", display);
			Append("color", color);
			Append("left", left);
			Append("top", top);
			Append("width", width);
			Append("height", height);
			Append("background-color", backgroundColor);
			Append("position", position);
			Append("text-align", textAlign);
			Append("border-width", borderWidth);
			Append("padding-top", paddingTop);
			Append("overflow", overflow);

			if (opacity >= 0)
				if (opacity < 1)
				{
					Append("opacity", "" + opacity);
					Append("filter", "Alpha(Opacity=" + (opacity * 100) + ")");
				}



			return w.ToString();
		}

	}

	[Script]
	public class IHTMLInput : IHTMLElement
	{
		public IHTMLInput()
		{
			this.Tag = "input";
		}

		public string Type;
		public string Value;
		public bool IsReadOnly;

		public override string ToString()
		{
			var style = "";

			if (this.Style != null)
				style = this.Style.ToString();

			var _readonly = "";

			if (this.IsReadOnly)
				_readonly = "readonly='readonly'";

			return "<" + Tag + " " + _readonly + " value='" + Value + "' type='" + Type + "' style='" + style + "' />";
		}
	}

	[Script]
	public class IHTMLMeta : IHTMLElement
	{
		// http://www.w3schools.com/tags/tag_meta.asp


		[Script]
		public class Generator : IHTMLMeta
		{
			public Generator()
			{
				this.MetaName = "fenerator";
			}

			public static implicit operator Generator(string e)
			{
				return new Generator { MetaContent = e };
			}
		}

		[Script]
		public class Description : IHTMLMeta
		{
			public Description()
			{
				this.MetaName = "description";
			}

			public static implicit operator Description(string e)
			{
				return new Description { MetaContent = e };
			}
		}

		[Script]
		public class Keywords : IHTMLMeta
		{
			public Keywords()
			{
				this.MetaName = "keywords";
			}

			public static implicit operator Keywords(string e)
			{
				return new Keywords { MetaContent = e };
			}
		}

		public IHTMLMeta()
		{
			this.Tag = "meta";
		}


		public string MetaName { get; set; }
		public string MetaContent { get; set; }


		public override string ToString()
		{
			var name = "";
			if (this.MetaName != null)
				name = "name='" + this.MetaName + "'";

			var content = "";
			if (this.MetaContent != null)
				content = "content='" + this.MetaContent + "'";






			return "<" + Tag + " " + name + " " + content + " />";
		}
	}

	[Script]
	public class IHTMLTitle : IHTMLElement
	{
		public IHTMLTitle()
		{
			this.Tag = "title";
		}

		public static implicit operator IHTMLTitle(string e)
		{
			return new IHTMLTitle { innerHTML = e };
		}
	}

	[Script]
	public class IHTMLStyle : IHTMLElement
	{
		public IHTMLStyle()
		{
			this.Tag = "style";
		}

		public static implicit operator IHTMLStyle(string e)
		{
			return new IHTMLStyle { innerHTML = e };
		}
	}

	[Script]
	public class IHTMLOrderedList : IHTMLElement
	{
		public IHTMLOrderedList()
		{
			this.Tag = "ol";
		}

		public static implicit operator IHTMLOrderedList(string e)
		{
			return new IHTMLOrderedList { innerHTML = e };
		}
	}

	[Script]
	public class IHTMLUnorderedList : IHTMLElement
	{
		public IHTMLUnorderedList()
		{
			this.Tag = "ul";
		}

		public static implicit operator IHTMLUnorderedList(string e)
		{
			return new IHTMLUnorderedList { innerHTML = e };
		}
	}



	[Script]
	public class IHTMLListItem : IHTMLElement
	{
		public IHTMLListItem()
		{
			this.Tag = "li";
		}

		public static implicit operator IHTMLListItem(string e)
		{
			return new IHTMLListItem { innerHTML = e };
		}
	}

	[Script]
	public class IHTMLListHeader : IHTMLElement
	{
		public IHTMLListHeader()
		{
			this.Tag = "lh";
		}

		public static implicit operator IHTMLListHeader(string e)
		{
			return new IHTMLListHeader { innerHTML = e };
		}
	}

	[Script]
	public class IHTMLStrong : IHTMLElement
	{
		public IHTMLStrong()
		{
			this.Tag = "strong";
		}

		public static implicit operator IHTMLStrong(string e)
		{
			return new IHTMLStrong { innerHTML = e };
		}
	}

	[Script]
	public class IHTMLHead : IHTMLElement
	{
		public IHTMLHead()
		{
			this.Tag = "head";
		}
	}

	[Script]
	public class IHTMLBreak : IHTMLElement
	{



		public IHTMLBreak()
		{
			this.Tag = "br";
		}


		public override string ToString()
		{

			return "<" + Tag + "/>";
		}
	}

	// http://www.w3schools.com/tags/tag_acronym.asp
	[Script]
	public class IHTMLAcronym : IHTMLElement
	{
		public IHTMLAcronym()
		{
			this.Tag = "acronym";
		}


		public string Title;

		public override string ToString()
		{

			var title = "";
			if (!string.IsNullOrEmpty(this.Title))
				title = "title='" + this.Title.Replace("'", "&apos;") + "'";

			var style = "";
			if (this.Style != null)
				style = " style='" + this.Style.ToString() + "'";

			return "<" + Tag
				+ " " + title
				+ " " + style
				+ " >" + innerHTML + "</" + Tag + ">";
		}
	}

	[Script]
	public class IHTMLAnchor : IHTMLElement
	{



		public IHTMLAnchor()
		{
			this.Tag = "a";
		}

		/// <summary>
		/// Sets or retrieves the destination URL or anchor point. 
		/// </summary>
		public string URL { get; set; }

		public string Title { get; set; }
		public string Name { get; set; }

		public override string ToString()
		{

			return "<" + Tag
					+ " " + this.URL.ToAttributeString("href")
					+ " " + this.Name.ToAttributeString("name") 
					+ " " + this.Title.ToAttributeString("title") + ">" + this.innerHTML + "</" + Tag + ">";
		}
	}


	[Script]
	public class IHTMLLink : IHTMLElement
	{
		[Script]
		public class RSS : IHTMLLink
		{
			public RSS()
			{
				this.Relationship = "alternate";
				this.Type = "application/rss+xml";
				this.Title = "RSS 2.0";
			}

			public static implicit operator RSS(string e)
			{
				return new RSS { URL = e };
			}
		}

		// http://msdn.microsoft.com/en-us/library/ms535848(VS.85).aspx#

		public IHTMLLink()
		{
			this.Tag = "link";
		}

		/// <summary>
		/// Sets or retrieves the relationship between the object and the destination of the link.
		/// </summary>
		public string Relationship { get; set; }


		/// <summary>
		/// Sets or retrieves the MIME type of the object. 
		/// </summary>
		public string Type { get; set; }


		public string Title { get; set; }

		/// <summary>
		/// Sets or retrieves the destination URL or anchor point. 
		/// </summary>
		public string URL { get; set; }

		public override string ToString()
		{
			var rel = "";
			if (this.Relationship != null)
				rel = "rel='" + this.Relationship + "'";

			var type = "";
			if (this.Type != null)
				type = "type='" + this.Type + "'";




			var title = "";
			if (this.Title != null)
				title = "title='" + this.Title + "'";

			var href = "";
			if (this.URL != null)
				href = "href='" + this.URL + "'";



			return "<" + Tag + " " + rel + " " + type + " " + title + " " + href + " />";
		}
	}

	[Script]
	public class IHTMLScript : IHTMLElement
	{
		// http://msdn.microsoft.com/en-us/library/ms535848(VS.85).aspx#

		public IHTMLScript()
		{
			this.Tag = "script";
			this.Type = "text/javascript";
		}


		/// <summary>
		/// Sets or retrieves the MIME type of the object. 
		/// </summary>
		public string Type { get; set; }


		/// <summary>
		/// Sets or retrieves the destination URL or anchor point. 
		/// </summary>
		public string URL { get; set; }

		public override string ToString()
		{

			var type = "";
			if (this.Type != null)
				type = "type='" + this.Type + "'";



			var href = "";
			if (this.URL != null)
				href = "src='" + this.URL + "'";



			return "<" + Tag + " " + type + " " + href + " >" + innerHTML + "</" + Tag + ">";
		}
	}


	[Script]
	public class IHTMLButton : IHTMLElement
	{
		public string onclick;

		public IHTMLButton()
		{
			this.Tag = "button";
		}




		public override string ToString()
		{

			var onclick = "";
			if (this.onclick != null)
				onclick = "onclick='" + this.onclick + "'";





			return "<" + Tag + " " + onclick + " >" + innerHTML + "</" + Tag + ">";
		}
	}

	[Script]
	public class IHTMLEmbed : IHTMLElement
	{
		// http://msdn.microsoft.com/en-us/library/ms535848(VS.85).aspx#

		public IHTMLEmbed()
		{
			this.Tag = "embed";
			this.Type = "application/x-shockwave-flash";
			this.AllowScriptAccess = "always";
			this.AllowNetworking = "all";
		}


		public string AllowNetworking { get; set; }
		public string AllowScriptAccess { get; set; }
		public string Type { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		/// <summary>
		/// Sets or retrieves the destination URL or anchor point. 
		/// </summary>
		public string URL { get; set; }

		public override string ToString()
		{

			var type = "";
			if (this.Type != null)
				type = "type='" + this.Type + "'";



			var href = "";
			if (this.URL != null)
				href = "src='" + this.URL + "'";

			var allowNetworking = "";
			if (this.URL != null)
				allowNetworking = "allowNetworking='" + this.AllowNetworking + "'";

			var allowScriptAccess = "";
			if (this.URL != null)
				allowScriptAccess = "allowScriptAccess='" + this.AllowScriptAccess + "'";

			var width = "";
			if (this.Width >= 0)
				width = "width='" + this.Width + "'";

			var height = "";
			if (this.Height >= 0)
				height = "height='" + this.Height + "'";

			var style = "";
			if (this.Style != null)
				style = " style='" + this.Style.ToString() + "'";

			return "<" + Tag
				+ " " + type
				+ " " + href
				+ " " + allowNetworking
				+ " " + allowScriptAccess
				+ " " + width
				+ " " + height
				+ " " + style
				+ " ></" + Tag + ">";
		}
	}

	[Script]
	public class IHTMLImage : IHTMLElement
	{
		public IHTMLImage()
		{
			this.Tag = "img";
		}

		public static implicit operator IHTMLImage(string Source)
		{
			return new IHTMLImage { src = Source };
		}


		public string alt;
		public string align;
		public string src;

		public override string ToString()
		{
		
		
			var style = "";
			if (this.Style != null)
				style = " style='" + this.Style.ToString() + "'";

		

			return "<" + Tag
				+ " " + this.alt.ToAttributeString("alt")
				+ " " + this.src.ToAttributeString("src")
				+ " " + this.title.ToAttributeString("title")
				+ " " + this.align.ToAttributeString("align")
				+ " " + style
				+ " />";
		}
	}

	[Script]
	public class IHTMLElement
	{
		[Script]
		public class Hidden : IHTMLElement
		{
			public Hidden()
			{
				this.Style = new IStyle
				{
					display = "none"
				};
			}
		}

		public string Tag = "div";

		public IStyle Style;

		public string innerHTML;
		public Func<string> GetContent;

		public string Class;


		public string title;

		public override string ToString()
		{
			var _class = "";

			if (this.Class != null)
				_class = " class='" + this.Class + "'";

			var style = "";

			if (this.Style != null)
				style = " style='" + this.Style.ToString() + "'";

			var Content = this.innerHTML;

			if (GetContent != null)
				Content = GetContent();

			return "<" + Tag + _class + style 
				
				+ " " + this.title.ToAttributeString("title")
				+ ">" + Content + "</" + Tag + ">";
		}

		public static implicit operator string(IHTMLElement e)
		{
			return e.ToString();
		}
	}

}
