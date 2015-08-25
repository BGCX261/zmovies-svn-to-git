using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using java.io;
using java.net;
using javax.servlet.http;
using TidyDocsGoogleApplication.Server.Library;
using ScriptCoreLib;
using ScriptCoreLibJava.Extensions;

namespace TidyDocsGoogleApplication.Server
{
	[Script]
	[ConfigurationProvider.UrlPattern(UrlPattern + "/*")]
	public class TidyDocsServlet : HttpServlet
	{
		public const string UrlPattern = "/tidy-docs";


		protected override void doGet(HttpServletRequest req, HttpServletResponse resp)
		{
			try
			{
				var PathAndQuery = req.GetPathAndQuery().Substring(UrlPattern.Length);

				// not allowed to do so:
				// http://groups.google.com/group/google-appengine/browse_thread/thread/68a480cb7bec869e
				// http://www.ozelwebtasarim.com/index.php/google/10004-google-app-engine-java-utf-8-character-encoding-problem
				//resp.setHeader("Content-Encoding", "utf-8");

				if (PathAndQuery.Length > 1)
				{
					resp.setContentType("text/html; charset=utf-8");
					resp.getWriter().println(Launch(PathAndQuery.Substring(1)));
				}
				else
				{
					resp.setContentType("text/html; charset=utf-8");
					resp.getWriter().println(Launch(null));
				}
			}
			catch
			{
				// either swallow of throw a runtime exception
			}
		}


		private static string Launch(string docID)
		{
			var w = new StringBuilder();

			if (docID == null)
			{
				w.AppendLine("<h1>tidy-docs</h1>");


				w.AppendLine("<style>b {color: blue;}</style><p>If you have a google document and you need it to be tidy, take the id shown in bold http://docs.google.com/Doc?id=<b>xyxyxyxyxyxyxy</b>&hl=en and go to /?<b>xyxyxyxyxyxyxy</b></p>");

				w.AppendLine(@"<p><a  href='http://www.w3.org/QA/Tools/Donate'><img  src='http://www.w3.org/QA/Tools/I_heart_validator_lg'></a></p>");

				w.AppendLine("<br /><a href='http://zproxy.wordpress.com'>created by zproxy</a>");
			}
			else
			{
				var u = "http://docs.google.com/View?docID=" + docID + "&revision=_latest&hgd=1";



				var tidy = new Uri("http://infohound.net/tidy/tidy.pl?_function=tidy&_html=&_file=&alt-text=&clean=y&doctype=auto&drop-empty-paras=y&drop-proprietary-attributes=y&fix-backslash=y&fix-bad-comments=y&fix-uri=y&hide-comments=y&hide-endtags=y&join-styles=y&lower-literals=y&ncr=y&new-blocklevel-tags=&new-empty-tags=&new-inline-tags=&new-pre-tags=&output-xhtml=y&quote-ampersand=y&quote-nbsp=y&indent=auto&indent-spaces=2&tab-size=4&wrap=90&wrap-asp=y&wrap-jste=y&wrap-php=y&wrap-sections=y&ascii-chars=y&char-encoding=utf8&input-encoding=latin1&output-bom=auto&output-encoding=utf8&_output=warn&_url=" + u);

				var tidymanager = tidy.ToWebString();

				var trigger = "tidy.pl?_function=download&amp;file=";
				var trigger_i = tidymanager.IndexOf(trigger) + trigger.Length;
				var trigger_j = tidymanager.IndexOf("&amp;", trigger_i);
				var trigger_value = tidymanager.Substring(trigger_i, trigger_j - trigger_i);

				var tidydownload = new Uri("http://infohound.net/tidy/tidy.pl?_function=download&file=" + trigger_value);



				var cleandoc = tidydownload.ToWebString();

				//("<p> " + cleandoc.Length + " bytes</p>").ToConsole();
				//cleandoc = cleandoc.Replace("Arvo Sulakatko", "Ken Martin");

				//("<p> " + cleandoc.Length + " bytes</p>").ToConsole();



				var footer_trigger_start = "<div id=\"google-view-footer\">";
				var footer_trigger_end = "</body>";



				if (!cleandoc.Contains("<!DOCTYPE"))
					cleandoc =
						"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\" >" +
						cleandoc;

				var leandoc = "<!-- tidy-docs -->";

				leandoc += cleandoc.Substring(0, cleandoc.IndexOf(footer_trigger_start));
				leandoc += ("<p><a href='" + u.Replace("&", "&amp;") + "'>Google Docs</a></p>");
				leandoc += ("<p><a href='" + tidydownload.ToString().Replace("&", "&amp;") + "'>Tidy</a></p>");
				leandoc += (@"<center>
    <a href='http://validator.w3.org/check?uri=referer'><img
        src='http://www.w3.org/Icons/valid-xhtml10'
        alt='Valid XHTML 1.0 Transitional' height='31' width='88' /></a>
  </center>"
					);
				leandoc += cleandoc.Substring(cleandoc.IndexOf(footer_trigger_end));


				for (int i = 0; i < 10; i++)
				{
					leandoc = leandoc.Replace("id=\"" + i, "id=\"_" + i);
					leandoc = leandoc.Replace("name=\"" + i, "name=\"_" + i);

				}
				leandoc = leandoc.Replace("RANGE!", "RANGE_");


				w.AppendLine(leandoc);
			}

			return w.ToString();
		}
	}
}
