using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;
using System.Threading;

namespace MovieAgent.web.tasks.Task2
{
	[Script]
	public class Task2 : WorkTask
	{

		public Task2(MyNamedTasks Tasks)
			: base(Tasks, "Task2")
		{
			this.Description = @"
This is a subtask. Task1 depends on our completion. 
This fact makes Task2 more important than Task1
			";

			this.YieldWork =
				(Task, Input) =>
				{
					var c = Input.ToFieldBuilder();

					FileMappedField 
						TorrentName = c,
						TorrentLink = c,
						TorrentSize = c,
						TorrentComments = c;

					c.FromFile();

					c[TorrentName] =
						delegate
						{

						};
					
					Input.Delete();


					this.AppendLog(Input.Name);

					return c.ToFile;
				};
		}


	}
}
