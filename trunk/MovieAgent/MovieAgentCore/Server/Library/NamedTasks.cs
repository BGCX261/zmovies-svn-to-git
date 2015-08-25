using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using System.IO;
using MovieAgent.Server.Library;
using System.Collections;
using MovieAgent.Shared;

namespace MovieAgent.Server.Library
{
	[Script]
	public abstract class NamedTasks : IEnumerable<NamedTask>
	{
		public SchedulerTask Scheduler { get; set; }

		public readonly DirectoryInfo TasksContext;

		public NamedTasks(DirectoryInfo Tasks)
		{
			this.TasksContext = Tasks;
		}



		public Action Disabled { get; set; }
		public Action<string> Default { get; set; }
		public Action<string> Fork { get; set; }

		public void Invoke(string Query)
		{
			if (!this.TasksContext.Exists)
			{
				if (this.Disabled != null)
					this.Disabled();

				return;
			}

			var ArgumentsIndex = Query.IndexOf("?");
			var Arguments = "";

			if (ArgumentsIndex > 0)
			{
				Arguments = Query.Substring(ArgumentsIndex + 1);
				Query = Query.Substring(0, ArgumentsIndex);
			}

			var TaskAtHand = this[Query];
			if (TaskAtHand == null)
			{
				if (this.Default != null)
					this.Default(Query);
			}
			else
			{
				Invoke(TaskAtHand, Arguments);
			}

			// if the scheduler is not running, this is the time to start it
			YieldToScheduler();
		}

		void YieldToScheduler()
		{
			this.Scheduler.Context.Refresh();
			if (this.Scheduler.Context.Exists)
			{
				// is the lock way too old?
				ValidateLock(this.Scheduler);

				this.Scheduler.Lock.Refresh();
				if (this.Scheduler.Lock.Exists)
				{
					return;
				}
			}

			this.Scheduler.Fork();
		}

		protected void ValidateLock(NamedTask Task)
		{
			Task.Lock.Refresh();
			if (Task.Lock.Exists)
			{
				var Age = DateTime.Now - Task.Lock.LastWriteTime;


				if (Age.TotalMilliseconds > 10000)
				{
					Console.WriteLine("ValidateLock: " + Task.Lock.FullName);

					Task.Lock.Delete();
				}
			}

		}

		void Invoke(NamedTask TaskAtHand, string Arguments)
		{
			var done = false;
			foreach (var h in TaskAtHand.ArgumentHandlers)
			{
				if (h(TaskAtHand, Arguments))
				{
					done = true;
					break;
				}
			}


			if (done)
				return;

			TaskAtHand.Context.Refresh();
			if (TaskAtHand.Context.Exists)
			{
				// is the lock way too old?
				ValidateLock(TaskAtHand);

				TaskAtHand.Lock.Refresh();
				if (TaskAtHand.Lock.Exists)
				{
					return;
				}

			}
			else
			{
				Console.WriteLine(TaskAtHand.Context.FullName);

				TaskAtHand.Context.Create();
			}

			TaskAtHand.Lock.Create();


			var Commit = TaskAtHand.Yield(TaskAtHand, Arguments);

			TaskAtHand.Context.Refresh();
			if (TaskAtHand.Context.Exists)
			{
				TaskAtHand.Lock.Refresh();
				if (TaskAtHand.Lock.Exists)
				{
					if (Commit != null)
						Commit();

					TaskAtHand.Counter++;
					TaskAtHand.Lock.Delete();
				}
			}

		}

		public NamedTask.ChainLink Items;

		public NamedTask this[string Name]
		{
			get
			{
				var p = this.Items;

				while (p != null)
				{
					if (p.Current.Name == Name)
					{
						return p.Current;
					}

					p = p.Link;
				}

				return null;
			}
		}


		#region IEnumerable<NamedTask> Members

		public IEnumerator<NamedTask> GetEnumerator()
		{
			var x = this.Items;
			var p = default(NamedTask.ChainLink);

			return new DynamicEnumerator<NamedTask>
			{
				DynamicCurrent = () => p.Current,
				DynamicMoveNext =
					delegate
					{
						if (x == null)
							return false;

						p = x;
						x = p.Link;

						return true;
					}
			};
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion



	}
}
