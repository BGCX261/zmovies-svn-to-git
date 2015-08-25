using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;
using System.Threading;
using System.IO;

namespace MovieAgent.web.tasks.Task1
{
	[Script]
	public class Task1 : WorkTask
	{
		// task 1 will run at 10 tick interval
		// the default input file will not be removed
		// and this will trigger retriggering


		// as we are generating work items for task2
		// we will wait until task2 is complete

		public Task1(MyNamedTasks Tasks_)
			: base(Tasks_, "Task1")
		{
			// jsc emits bad base ctor call without this workaround, why?
			var Tasks = Tasks_;

			this.Description = @"
task 1 will run at 10 tick interval
the default input file will not be removed
and this will trigger retriggering

as we are generating work items for task2
we will wait until task2 is complete
			";

			this.YieldWork =
				(Task, Input) =>
				{
					var Counter = Task.Counter;

					this.AppendLog("creating work!");

					new Action[]
					{
						//delegate
						//{
						//    var DropZone = Tasks.Task2_AliasSearch.Context.ToDirectory("Input/3");

						//    DropZone.Create();

						//    Tasks.Task2_AliasSearch.AddWork(3, Input.Name + "-" + Counter, "dummy work data");
						//},
						delegate
						{
						    Tasks.Task3_FindTorrents.AddWork(50, "piratebay", "");
						}
					}.InvokeByModulus(Counter);

					Thread.Sleep(2000);


					return delegate
					{
						// this will be called if the task is still active
					};
				};
		}
	}
}
