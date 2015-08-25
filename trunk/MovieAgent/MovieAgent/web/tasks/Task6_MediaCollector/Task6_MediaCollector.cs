using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;
using System.Threading;
using MovieAgent.Server.Services;
using System.IO;
using MovieAgent.Server.Library;
using MovieAgent.web.tasks.Task5_CloneMedia;
using ScriptCoreLib.PHP;

namespace MovieAgent.web.tasks.Task6_MediaCollector
{
	[Script]
	public class Task6_MediaCollector : WorkTask
	{
		public FileInfo Feed
		{
			get
			{
				return this.NamedTasks.Output.ToFile("MediaFeed1.xml");
			}
		}

		public readonly MyNamedTasks NamedTasks;

		public Task6_MediaCollector(MyNamedTasks Tasks)
			: base(Tasks, "Task6_MediaCollector")
		{
			this.NamedTasks = Tasks;
			this.Description = @"
We all information. Now we must collect all entries to generate a feed.
A new feed should not be created faster than 3 hours.
			";

			this.ArgumentHandlers.Add(
				(Task, Arguments) =>
				{

					return false;
				}
			);

			#region YieldWork
			this.YieldWork =
				(Task, Input) =>
				{
					if (this.Feed.Exists)
					{
						var AgeHours = (DateTime.Now - this.Feed.LastWriteTime).Hours;


						if (AgeHours < 12)
						{
							this.AppendLog("Media feed is " + AgeHours + " hours old");
							return null;
						}

						this.AppendLog("We need to update the feed...");
					}
					else
						this.AppendLog("Creating media feed for the first time");

					using (var w = new StreamWriter(this.Feed.OpenWrite()))
					{
						w.BaseStream.SetLength(0);

						w.WriteLine(@"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<rss version='2.0' xmlns:media='http://search.yahoo.com/mrss' xmlns:atom='http://www.w3.org/2005/Atom'>
  <channel>
    <title>zmovies</title>
    <description>zmovies can haz entertainment</description>
    <link>http://zproxy.planet.ee/zmovies</link>
						");

						foreach (var i in this.ActiveInputPools)
						{
							foreach (var j in i.Files)
							{
								w.WriteLine(File.ReadAllText(j.FullName));
							}
						}

						w.WriteLine(@"
	  </channel>
</rss>");

					}

					foreach (var i in this.ActiveInputPools)
					{
						foreach (var j in i.Files)
						{
							j.Delete();
						}
					}

					return delegate
					{
						// this will be called if the task is still active
					};
				};
			#endregion

		}




	}

}
