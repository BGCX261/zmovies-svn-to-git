using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLib.ActionScript.DOM;
using ScriptCoreLib.ActionScript.Extensions;
using ScriptCoreLib.Shared;

namespace MovieAgentGadget.ActionScript.Library
{
	partial class GoogleGears
	{
		[Script]
		public class GearsContext
		{
			public readonly ExternalContext Context;

			public readonly ExternalContext.Converter<string, string, string, string, bool> GoogleGears_Factory_getPermission;
			public readonly Converter<string, string> GoogleGears_Factory_getBuildInfo;
			public readonly Action<string, string, string, string> GoogleGears_Factory_create;
			public readonly Action<string, object[], string, string> GoogleGears_Database_execute;
			public readonly Converter<string, bool> GoogleGears_ResultSet_isValidRow;
			public readonly Action<string> GoogleGears_ResultSet_close;
			public readonly Converter<string, int> GoogleGears_ResultSet_fieldCount;
			public readonly Action<int, string, string> GoogleGears_ResultSet_field;
			public readonly Converter<string, object> GoogleGears_GetToken;

			public GearsContext(ExternalContext Context)
			{
				this.Context = Context;

				this.GoogleGears_Factory_getPermission =
					Context.ToExternalConverter<string, string, string, string, bool>(
						"_siteName", "_imageUrl", "extraMessage", "_i",
						"return window[_i].getPermission(_siteName, _imageUrl, extraMessage);"
					);


				this.GoogleGears_Factory_getBuildInfo =
					Context.ToExternalConverter<string, string>("_i", "return window[_i].getBuildInfo();");

				this.GoogleGears_Factory_create =
					Context.ToExternal<string, string, string, string>("_id", "_version", "_i", "_v", "window[_v] = window[_i].create(_id, _version);");

			

				this.GoogleGears_Database_execute =
					Context.ToExternal<string, object[], string, string>("_cmd", "_args", "_i", "_v",
						"window[_v] = window[_i].execute(_cmd, _args);"
					);

				this.GoogleGears_ResultSet_isValidRow =
					Context.ToExternalConverter<string, bool>("_i", "return window[_i].isValidRow();");

				

				this.GoogleGears_ResultSet_close =
					Context.ToExternal<string>("_i", "window[_i].close(); delete window[_i];");

				this.GoogleGears_ResultSet_fieldCount =
					Context.ToExternalConverter<string, int>("_i", "return window[_i].fieldCount();");

				this.GoogleGears_ResultSet_field =
					Context.ToExternal<int, string, string>("_index", "_i", "_v",
						"window[_v] = window[_i].field(_index);"
					);

				this.GoogleGears_GetToken =
					Context.ToExternalConverter<string, object>("_i", 
						"return window[_i];"
					);

			}
		}



	}
}
