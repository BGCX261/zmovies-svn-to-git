using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLibAppJet;
using ScriptCoreLibAppJet.JavaScript.Library;
using MovieAgentAppJet.Library;

namespace MovieAgentAppJet
{
	[Script]
	public static class Server
	{
		[Script, Serializable]
		public sealed class DataItem
		{
			public string id;

			public string Key;
			public string Value;
		}

		[Script, Serializable]
		public sealed class SmartClient
		{
			public string id;

			public string url;
			public string data;
		}


		public static void Render()
		{
			// /* appjet:version 0.1 */ 

			Native.page.setMode("plain");

			var swf = "swf".ToStorableObject(new SmartClient { url = "http://zproxy.planet.ee/zmovies/MovieAgentGadget.swf" });

			if (swf.data != null)
				if (Native.request.path == "/swf")
				{
					Native.response.setContentType("application/x-shockwave-flash");
					Native.response.setCacheable(true);
					Native.response.writeBytes(swf.data);
					return;
				}
				else if (Native.request.path == "/")
				{
					@"
						<body style='margin: 0; overflow: hidden;'><object type='application/x-shockwave-flash' data='/swf' id='id633755145574040000' name='id633755145574040000' width='0' height='0' allowFullScreen='true' allowNetworking='all' allowScriptAccess='always'>
						  <param name='movie' value='/swf' />
						</object>
".ToConsole();
					return;
				}


			("data: " + (swf.data != null) + " via " + swf.url).ToConsole();

			if (swf.data == null)
			{
				var x = Native.wget(swf.url, null, new WebRequestOptions());

				if (x.status == 200)
				{
					x.contentType.ToConsole();
					swf.data = x.data;
					"<p>swf installed...</p>".ToConsole();
				}
			}
			else
			{
				("<p>swf already installed... " + swf.data.Length + " bytes</p>").ToConsole();
			}
			"<p><a href='/swf'>flash</a></p>".ToConsole();

			var c = "stream1".ToStorableCollection();

			("<p>" + c.size() + "<p>").ToConsole();

			c.add(
				new DataItem
				{
					Key = "hello",
					Value = "world"
				}
			);

			"<ol>".ToConsole();

			for (int i = 0; i < c.size(); i++)
			{
				var k = (DataItem)c.At(i);

				("<li>" + k.Key + ": " + k.Value + "</li>").ToConsole();
			}

			"</ol>".ToConsole();


			if (!Native.storage.Contains("counter"))
				Native.storage.SetValue("counter", 0);

			Native.storage.SetValue("counter", (Native.storage.GetValue<int>("counter") + 1));

			Native.print("hello world: " + Native.storage.GetValue<int>("counter"));

			"<hr />".ToConsole();

			if (Native.request.isGet)
			{
				if (Native.request.path == "/x")
				{
					"<p>you found a <b>secret</b>!</p>".ToConsole();
				}
				else
				{
					"<a href='/x'>Go left</a>".ToConsole();
				}

				if (Native.request.path.StartsWith("/y"))
				{
					"<p>you found another <b>secret</b>!</p>".ToConsole();
				}
				else
				{
					"<a href='/y0'>Go right</a>".ToConsole();

				}
			}

			//            @"
			//<embed src='http://www.seeqpod.com/cache/seeqpodEmbed.swf' wmode='transparent' width='425' height='350' type='application/x-shockwave-flash' flashvars='domain=http://www.seeqpod.com&playlist=adfa1ff90e'></embed><br/><a href='http://www.seeqpod.com/search'>SeeqPod - Playable Search</a>
			//			".ToConsole();

		}

		static Server()
		{
			Native.import("storage");
			Render();
		}
	}
}
