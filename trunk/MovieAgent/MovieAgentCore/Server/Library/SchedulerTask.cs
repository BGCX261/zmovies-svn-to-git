using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using System.IO;
using System.Threading;

namespace MovieAgent.Server.Library
{
	[Script]
	public class SchedulerTask : NamedTask
	{
		public readonly int Interval;



		//public event Action Tick;

		public SchedulerTask(NamedTasks Tasks, string Name, int Interval)
			: base(Tasks, Name, null)
		{
			this.Interval = Interval;
			this.Description = "Scheduler task is responsible for dispatching tasks on regular basis. Interval: " + Interval;

			this.Yield =
				(Task, Arguments) =>
				{
					Console.WriteLine("Counter: " + this.Counter);

					ForkTasksWithInput();

					Thread.Sleep(Interval);

					return delegate
					{
						// tick complete.
					};
				};
		}

		private void ForkTasksWithInput()
		{
			var Counter = this.Counter;

			//Console.WriteLine("ForkTasksWithInput.Counter = " + Counter);

			foreach (var CurrentTask in this.Tasks)
			{
				if (CurrentTask != this)
				{
					if (!CurrentTask.HasActiveDependencies)
					{
						foreach (var InputPool in CurrentTask.ActiveInputPools)
						{
							// a valid interval
							// there are workitems int the pool
							if (Counter % InputPool.Interval == 0)
							{
								// we should wait for the requested interval
								CurrentTask.Fork();
								break;
							}
						}
					}
				}
			}
		}



	}
}
