using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using javax.servlet.http;
using java.net;
using java.io;

using MovieAgentGoogleApplicationPosterDispatch.Server.Library;

namespace MovieAgentGoogleApplicationPosterDispatch.Server
{
	[Script]
	public class HelloAppEngineServlet : HttpServlet
	{
		protected override void doGet(HttpServletRequest req, HttpServletResponse resp)
		{
			try
			{
				var Path = req.getServletPath();
				var Query = req.getQueryString();
				var PathAndQuery = Path;
				if (Query != null)
					PathAndQuery += "?" + Query;

				if (PathAndQuery != "/")
				{
					//resp.setContentType("text/html");
					//resp.getWriter().println(
					resp.sendRedirect(
						GetTinEyeVersion(GetPosterLink(PathAndQuery))
						//.ToImage()
					);
				}
				else
				{
					resp.setContentType("text/html");
					resp.getWriter().println(Launch(PathAndQuery));
				}
			}
			catch
			{
				// either swallow of throw a runtime exception
			}
		}


		private static string Launch(string PathAndQuery)
		{
			var w = new StringBuilder();

			w.AppendLine("<p>This application was written in C# and was crosscompiled to java by <a href='http://jsc.sf.net'>jsc</a>.</p>");
			w.AppendLine("<p>Visit <a href='http://zproxy.wordpress.com'>author's blog</a>.</p>");
			w.AppendLine("<p>Look at the <a href='http://jsc.svn.sourceforge.net/viewvc/jsc/templates/MovieAgentGoogleApplicationPosterDispatch/MovieAgentGoogleApplicationPosterDispatch/'>source code</a>.</p>");


			w.AppendLine("<pre>PathAndQuery: " + PathAndQuery + "</pre>");

			var x = new Uri("http://example.com/").ToWebString();

			w.AppendLine(x);

			// http://code.google.com/appengine/docs/java/urlfetch/usingjavanet.html

			w.AppendLine(
				GetTinEyeVersion(GetPosterLink("lost")).ToImage()
			);


			

			return w.ToString();
		}

		private static string GetTinEyeVersion(string image)
		{
			var location = "";

			try
			{
				var Matrix = "http://tineye.com/search/?url=" + image;

				var m = (HttpURLConnection)new URL(Matrix).openConnection();

				m.setRequestMethod("HEAD");
				m.setInstanceFollowRedirects(false);

				// is getHeaderField broken?
				location = m.getHeaderField("location").Replace("search", "query");

				if (location.StartsWith("//"))
					location = "http:" + location;
			}
			catch
			{
			}

			return location;
		}

		private static string GetPosterLink(string q)
		{
			var u = new Uri("http://images.google.ee/images?gbv=1&q=" + q + "+movie+poster");
			var r = u.ToWebString();

			var trigger1 = "<table align=center";
			var data1 = r.Substring(r.IndexOf(trigger1));

			var trigger2 = "<img src=";
			var data2 = data1.Substring(data1.IndexOf(trigger2) + trigger2.Length);

			var data3 = data2.Substring(0, data2.IndexOf(" "));
			return data3;
		}
	}
}
