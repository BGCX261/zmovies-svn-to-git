using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;
using MovieAgent.Server.Services;
using System.Threading;

namespace MovieAgent.web.tasks.Task3_FindTorrents
{
	[Script]
	public class Task3_FindTorrents : WorkTask
	{

		public Task3_FindTorrents(MyNamedTasks Tasks)
			: base(Tasks, "Task3_FindTorrents")
		{
			var NamedTasks = Tasks;

			this.Description = @"
This task will fetch torrent links and spawn work for them. 
This could be extended to support multiple torrent services. 
Default implmentation will assume <a href='http://piratebay.org/'>piratebay</a>.

This task should not run when there is work for Task4_PrepareMedia 
			";

			this.YieldWork =
				(Task, Input) =>
				{
					// if we fail, we this work item will not be retried
					Input.Delete();

					var DismissedItems = 0;

					BasicPirateBaySearch.Search(
						k => Memory.Contains(k.Hash),
						(entry, deferred) =>
						{
							if (deferred)
							{
								DismissedItems++;
								return;
							}

							Memory.Add(entry.Hash);

							AppendLog("output " + entry.Name);

							var c = NamedTasks.Task4_PrepareMedia.AddWork(5, entry.Hash).ToFieldBuilder();
							
							FileMappedField
								TorrentName = c[entry.Name],
								TorrentLink = c[entry.TorrentLink],
								TorrentSize = c[entry.Size],
								TorrentComments = c[entry.Link];

							c.ToFile();
							
						}
					);

					if (DismissedItems > 0)
						AppendLog("dismissed " + DismissedItems);

					return delegate
					{
						// this will be called if the task is still active
					};
				};
		}



		
	}
}
