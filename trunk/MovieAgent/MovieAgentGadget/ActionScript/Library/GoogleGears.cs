using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLib.ActionScript.DOM;
using ScriptCoreLib.ActionScript.Extensions;

namespace MovieAgentGadget.ActionScript.Library
{
	[Script]
	public static partial class GoogleGears
	{


		// http://code.google.com/apis/gears/api_factory.html
		[Script]
		public partial class GearsFactory
		{
			public GearsContext Context;


			/// <summary>
			/// Lets a site manually trigger the Gears security dialog, optionally with UI customizations.
			/// siteName - Optional. Friendly name of the site requesting permission.
			/// imageUrl - Optional. URL of a .png file to display in the dialog.
			/// extraMessage - Optional. Site-specific text to display to users in the security dialog. 
			/// </summary>
			public Func<string, string, string, bool> GetPermission;
			public Func<string, string, string> Create;
			public Func<string> GetBuildInfo;
		}

		public static void Factory(ExternalContext Context, Action<GearsFactory> Handler)
		{
			var f = new GearsFactory
			{
				Context = new GearsContext(Context)
			};

			Context.ToPlugin("GearsFactory", "Gears.Factory", "application/x-googlegears",
				GoogleGears_Factory =>
				{
					if (GoogleGears_Factory == null)
					{
						Handler(null);
						return;
					}

					f.GetBuildInfo =
						() => f.Context.GoogleGears_Factory_getBuildInfo(GoogleGears_Factory);

					f.GetPermission =
						(siteName, imageUrl, extraMessage) =>
							f.Context.GoogleGears_Factory_getPermission(siteName, imageUrl, extraMessage, GoogleGears_Factory);

					f.Create =
						(id, version) =>
						{
							// database will be saved here
							var GoogleGears_Database = Context.CreateToken();

							f.Context.GoogleGears_Factory_create(id, version, GoogleGears_Factory, GoogleGears_Database);

							return GoogleGears_Database;
						};

					Handler(f);
				}
			);

		


		}


	}
}
