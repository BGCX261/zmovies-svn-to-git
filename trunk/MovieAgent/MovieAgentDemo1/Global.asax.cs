using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MovieAgentDemo1
{
	public class Global : System.Web.HttpApplication
	{
		static readonly Queue<string> Application_ForkList = new Queue<string>();
		static Timer Application_Timer;

		public static void Application_Entrypoint(string Query)
		{
			System.Diagnostics.Debug.WriteLine("Application_Entrypoint: " + Query);

			MovieAgent.Server.Application.Application_Entrypoint(Query,
				NextQuery =>
				{
					lock (Application_ForkList)
						Application_ForkList.Enqueue(NextQuery);
				}
			);
		}

		protected void Application_Start(object sender, EventArgs e)
		{
			this.Server.ScriptTimeout = 5;

			// data dir
			//Environment.CurrentDirectory = @"C:\work\code.google\zmovies\MovieAgent\MovieAgent\bin\Release\web";
			Environment.CurrentDirectory = @"C:\work\code.google\zmovies\MovieAgent\MovieAgent\bin\Debug\web";

			Application_Timer = new Timer(
				delegate
				{

					System.Diagnostics.Debug.WriteLine("tick");

					string NextQuery = null;
					lock (Application_ForkList)
					{
						if (Application_ForkList.Count > 0)
							NextQuery = Application_ForkList.Dequeue();
					}

					if (NextQuery != null)
					{
						try
						{
							Application_Entrypoint(NextQuery);
						}
						catch (ThreadAbortException)
						{
							// we dont care about that
						}
						catch
						{
							Debugger.Break();
						}
					}
				},
				null, 0, 1000);


		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
			// we better have only 1 active client...
			Console.SetOut(new ConsoleToHTTPResponse(this.Context.Response));



			Global.Application_Entrypoint(
				this.Context.Request.Url.Query.Length > 0 ?
				this.Context.Request.Url.Query.Substring(1) : ""
			);

			this.Context.Response.End();
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{
			Application_Timer.Dispose();
			Application_Timer = null;
		}

		class ConsoleToHTTPResponse : TextWriter
		{
			public readonly HttpResponse Response;

			public ConsoleToHTTPResponse(HttpResponse Response)
			{
				this.Response = Response;
			}

			public override System.Text.Encoding Encoding
			{
				get { throw new NotImplementedException(); }
			}

			public override void Write(string value)
			{
				this.Response.Write(value);
			}

			public override void WriteLine(string value)
			{
				this.Response.Write(value + Environment.NewLine);
			}

			public override void WriteLine()
			{
				this.Response.Write(Environment.NewLine);
			}
		}


	}
}