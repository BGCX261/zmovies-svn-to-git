using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLibAppJet;
using ScriptCoreLibAppJet.JavaScript.Library;
using MovieAgentAppjetPosterDispatch.Library;
using ScriptCoreLibAppJet.JavaScript;

namespace MovieAgentAppjetPosterDispatch
{
	[Script]
	public static class Server
	{
		//[Script, Serializable]
		//public sealed class DataItem
		//{
		//    public string id;

		//    public string Key;
		//    public string Value;
		//}

		//[Script, Serializable]
		//public sealed class SmartClient
		//{
		//    public string id;

		//    public string url;
		//    public string data;
		//}


		public static void Render()
		{
			// /* appjet:version 0.1 */ 

			Native.page.setMode("plain");



			//var c = "stream1".ToStorableCollection();

			//("<p>" + c.size() + "<p>").ToConsole();

			//c.add(
			//    new DataItem
			//    {
			//        Key = "hello",
			//        Value = "world"
			//    }
			//);

			//"<ol>".ToConsole();

			//for (int i = 0; i < c.size(); i++)
			//{
			//    var k = (DataItem)c.At(i);

			//    ("<li>" + k.Key + ": " + k.Value + "</li>").ToConsole();
			//}

			//"</ol>".ToConsole();


			//if (!Native.storage.Contains("counter"))
			//    Native.storage.SetValue("counter", 0);

			//Native.storage.SetValue("counter", (Native.storage.GetValue<int>("counter") + 1));

			//Native.print("counter: " + Native.storage.GetValue<int>("counter"));
			var q = Native.request.path.Substring(1);


			var ch = (CoralHeaders)Native.request.headers;


			if (q == "")
			{
				Native.response.redirect("http://" + ch.Host + "/" + "matrix");
				return;
			}

			if (ch.Via == null || !ch.Via.Contains("http://coralcdn.org/"))
			{
				Native.response.redirect("http://" + ch.Host + ".nyud.net/" + q);
				return;
			}





		

			//q.ToConsole();

			var poster = GetPosterLink(q);

			if (string.IsNullOrEmpty(poster))
			{
				Native.response.setStatusCode(404);
				return;
			}

			//data3.ToImage().ToConsole();
			//poster.ToConsole();

			var src = GetTinEyeVersion(poster);

			//src.ToImage().ToConsole();

			Native.response.redirect(src);
		}

		private static string GetTinEyeVersion(string poster)
		{
			var r = Native.wget("http://tineye.com/search/?url=" + poster, null, new WebRequestOptions { followRedirects = false });
			//Native.print(r);

			var h = (TineyeHeaders)r.headers;
			var src = h.location[0].Replace("search", "query");
			return src;
		}

		[Script(HasNoPrototype = true)]
		public class TineyeHeaders
		{
			public string[] location;
		}

		[Script(HasNoPrototype = true)]
		public class CoralHeaders
		{
			// Via	HTTP/1.0 131.246.191.42:8080 (CoralWebPrx/0.1.19 (See http://coralcdn.org/))
			public string Via;
			public string Host;
		}

		private static string GetPosterLink(string q)
		{
			var u = "http://images.google.ee/images?gbv=1&q=" + q + "+movie+poster";
			var r = Native.wget(u, null, new WebRequestOptions());

			var trigger1 = "<table align=center";
			var data1 = r.data.Substring(r.data.IndexOf(trigger1));

			var trigger2 = "<img src=";
			var data2 = data1.Substring(data1.IndexOf(trigger2) + trigger2.Length);

			var data3 = data2.Substring(0, data2.IndexOf(" "));
			return data3;
		}

		static Server()
		{
			Native.import("storage");
			Render();
		}
	}
}
