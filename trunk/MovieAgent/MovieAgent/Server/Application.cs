using ScriptCoreLib;
using ScriptCoreLib.Shared;

using ScriptCoreLib.PHP;
using System;
using System.Text;
using System.IO;
using MovieAgent.Client.Avalon;
using MovieAgent.Client.Java;
using MovieAgent.Shared;
using System.Threading;
using MovieAgent.web.tasks;
using MovieAgent.Server.Library;
using System.Diagnostics;
using System.Collections.Generic;
using MovieAgent.Server.Services;

namespace MovieAgent.Server
{
	[Script]
	public static partial class Application
	{
		public const string Filename = "index.php";

		// change: C:\util\xampplite\apache\conf\httpd.conf

		// http://localhost/jsc/zmovies

		//Alias /jsc/zmovies "C:\work\code.google\zmovies\MovieAgent\MovieAgent\bin\Release\web"
		//<Directory "C:\work\code.google\zmovies\MovieAgent\MovieAgent\bin\Release\web">
		//       Options Indexes FollowSymLinks ExecCGI
		//       AllowOverride All
		//       Order allow,deny
		//       Allow from all
		//</Directory>



		public static void Application_Entrypoint(string Query, Action<string> Fork)
		{
			//DemoMPDB();
			//DemoOMDB();
			//DemoGooTube();
			//DemoCreateTinyArrowsToLetMeGoogleThatForYou();
			//DemoList();
			//DownloadGame();		
			//ShowTinyURL();
			//ShowTitleSearch();
			//DemoTinEye();
			//ShowGoogleVideo();
			//ShowExampleDotCom();
			//DemoPosters();
			//DemoFindPosters();
			//DemoFindMovieAlias();

			//ShowPirateBayPosters();
			//ShowPirateBayWithoutVideo();
			//ShowPirateBayFast();
			//ShowPirateBayFastWithMemory();

			//DemoGetPosterViaTinEyeAndStreamItViaPirateBayImage("Shrek", "2007", new MemoryDirectory(new DirectoryInfo("memory")));
			//DemoGetPosterViaTinEyeAndStreamIt("Madea Goes to Jail");
			//DemoGetPosterViaTinEye("Madea Goes to Jail");
			//DemoGetPosterViaTinEye("Shrek 2");
			//DemoGetPosterViaTinEye("Bolt");



			//return;





			var MyTasks = new MyNamedTasks
			{
				Fork = Fork,
			};

			MyTasks.Invoke(Query);
		}


	





		/// <summary>
		/// php script will invoke this method
		/// </summary>
		[Script(NoDecoration = true)]
		public static void Application_Entrypoint()
		{
			Native.API.date_default_timezone_set(Native.API.date_default_timezone_get());


			// http://ee2.php.net/features.connection-handling
			Native.API.ignore_user_abort(true);




			Application_Entrypoint(Native.QueryString, BasicWebCrawler.Fork);



		}







	}
}
