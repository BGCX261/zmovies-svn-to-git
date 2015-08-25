using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using System.IO;

namespace MovieAgent.Server.Library
{
	[Script]
	public abstract class WorkTask : NamedTask
	{
		// depends
		// input


	
		public WorkTask(NamedTasks Tasks, string Name)
			: base(Tasks, Name, null)
		{
			this.Yield =
				(Task, Arguments) =>
				{
				

					if (Task.HasActiveDependencies)
					{
						Console.WriteLine("blocked");
						return null;
					}

					var Commit = default(Action);

					//Console.WriteLine(Task + " before FetchInput");
					this.FetchInput(
						Input =>
						{
							Commit = this.YieldWork(this, Input);
						}
					);
					//Console.WriteLine(Task + " after FetchInput");

					return delegate
					{
						if (Commit != null)
							Commit();
					};
				};
		}


		public Func<WorkTask, FileInfo, Action> YieldWork;

		public void AddWork(string key, string value)
		{
			AddWork(1, key, value);
		}

		public void AddWork(int interval, string key, string value)
		{
			var DropZone = this.Context.CreateSubdirectory("Input").CreateSubdirectory("" + interval);

			File.WriteAllText(
				Path.Combine(DropZone.FullName, key), value
			);
		}

		public FileInfo AddWork(int interval, string key)
		{
			var DropZone = this.Context.CreateSubdirectory("Input").CreateSubdirectory("" + interval);

			return new FileInfo(Path.Combine(DropZone.FullName, key));
		}
	}
}
