using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using System.IO;
using MovieAgent.Shared;
using System.Runtime.CompilerServices;

namespace MovieAgent.Server.Library
{
	[Script]
	public class NamedTask
	{
		public readonly List<Func<NamedTask, string, bool>> ArgumentHandlers = new List<Func<NamedTask, string, bool>>();


		public string Description { get; set; }

		public MemoryDirectory Memory
		{
			get
			{
				return new MemoryDirectory(this.Context.CreateSubdirectory("Memory"));
			}
		}


		[Script]
		public class ChainLink
		{
			public readonly NamedTask Current;

			public ChainLink Link;

			public ChainLink(NamedTask Current)
			{
				this.Current = Current;
			}
		}

		public readonly NamedTasks Tasks;
		public readonly string Name;

		public Func<NamedTask, string, Action> Yield;



		public readonly DirectoryInfo Context;
		public readonly DirectoryInfo Lock;

		public int Counter
		{
			get
			{
				var Counter = 0;

				if (File.Exists(CounterPath))
					Counter = int.Parse(File.ReadAllText(CounterPath));

				return Counter;
			}

			set
			{
				File.WriteAllText(CounterPath, "" + value);
			}
		}
		public readonly string CounterPath;

		public string Log
		{
			get
			{
				var v = "";

				if (this.Context.Exists)
				{
					var f = this.Context.ToFile("Log");

					if (f.Exists)
						v = File.ReadAllText(f.FullName);
				}

				return v;
			}
			set
			{
				if (this.Context.Exists)
				{
					File.WriteAllText(this.Context.ToFile("Log").FullName, value);
				}
			}
		}

		public void AppendLog(string e)
		{
			//Console.WriteLine(e);

			var x = Log;

			if (x.Length > 1024)
				x = x.Substring(0, 1024);

			Log = DateTime.Now + " " + e + Environment.NewLine + x;
		}

		public NamedTask(NamedTasks Tasks, string Name, Func<NamedTask, string, Action> Yield)
		{
			if (Tasks == null)
				throw new NotSupportedException("Tasks null");

		
			this.Tasks = Tasks;
			this.Name = Name;

			if (Yield != null)
				this.Yield = Yield;

			this.Tasks.Items = new NamedTask.ChainLink(this) { Link = this.Tasks.Items };

			this.Context = Tasks.TasksContext.CreateSubdirectory(Name);

			this.Lock = new DirectoryInfo(Path.Combine(this.Context.FullName, "Lock"));

			this.CounterPath = Path.Combine(this.Context.FullName, "Counter");
		}




		public void Fork()
		{
			this.Tasks.Fork(this.Name);
		}


		[Script]
		public class DependsInfo
		{
			public DirectoryInfo Target;

			public NamedTask Task;


			public bool IsActive
			{
				get
				{
					var value = false;

					foreach (var i in Task.ActiveInputPools)
					{
						value = true;
						break;
					}

					if (!value)
						value = this.Task.HasActiveDependencies;


					return value;
				}
			}
		}

		public bool HasActiveDependencies
		{
			get
			{
				var value = false;

				foreach (var i in this.ActiveDependencies)
				{
					if (i.IsActive)
					{
						value = true;
						break;
					}
				}

				return value;
			}
		}

		public DependsInfo[] Dependencies
		{
			get
			{
				var Depends = this.Context.ToDirectory("Depends");
				if (!Depends.Exists)
					return new DependsInfo[0];

				var a = Depends.GetDirectories();
				var x = new DependsInfo[a.Length];

				for (int i = 0; i < a.Length; i++)
				{
					x[i] = new DependsInfo
					{
						Target = a[i],
						Task = this.Tasks[a[i].Name]
					};
				}

				return x;
			}
		}

		public IEnumerable<DependsInfo> ActiveDependencies
		{
			get
			{
				return new DynamicEnumerable<DependsInfo>
				{
					DynamicGetEnumerator =
						delegate
						{
							var Dependencies = this.Dependencies;
							var Current = default(DependsInfo);
							var i = -1;

							Func<bool> MoveNext =
								delegate
								{
									i++;

									if (i >= Dependencies.Length)
										return false;

									return true;
								};

							return new DynamicEnumerator<DependsInfo>
							{
								DynamicCurrent = () => Current,
								DynamicMoveNext =
									delegate
									{
										while (true)
										{
											if (!MoveNext())
											{
												Current = null;
												return false;
											}

											Current = Dependencies[i];

											if (Current.Task != null)
												return true;
										}
									}
							};
						}
				};
			}
		}

		[Script]
		public class InputPool
		{
			public NamedTask Task;

			public TimeSpan Total
			{
				get
				{
					return TimeSpan.FromMilliseconds(this.TotalMilliseconds);
				}
			}

			public int TotalMilliseconds
			{
				get
				{
					return this.Task.Tasks.Scheduler.Interval * this.Files.Length * this.Interval;
				}
			}

			public DirectoryInfo Target;

			public int Interval = -1;

			public FileInfo[] Files;

			public bool ShouldPreferOthers
			{
				get
				{
					var value = false;

					foreach (var v in this.Task.ActiveInputPools)
					{
						if (v.Interval < this.Interval)
						{
							value = true;
							break;
						}
					}

					return value;
				}
			}
		}

		public InputPool[] InputPoolsCache;

		public InputPool[] InputPools
		{
			get
			{
				if (InputPoolsCache != null)
					return InputPoolsCache;

				var Input = this.Context.ToDirectory("Input");
				if (!Input.Exists)
					return new InputPool[0];

				var a = Input.GetDirectories();
				var x = new InputPool[a.Length];

				for (int i = 0; i < a.Length; i++)
				{
					var n = new InputPool
					{
						Target = a[i],
						Task = this,
					};

					var c = n.Target.Name;

					if (c.EnsureChars(0, c.Length, "0123456789"))
						n.Interval = int.Parse(c);

					if (n.Interval > 0)
					{
						n.Files = n.Target.GetFiles();
					}

					x[i] = n;
				}

				this.InputPoolsCache = x;
				return x;
			}
		}

		public IEnumerable<InputPool> ActiveInputPools
		{
			get
			{
				return new DynamicEnumerable<InputPool>
				{
					DynamicGetEnumerator =
						delegate
						{
							var InputPools = this.InputPools;
							var Current = default(InputPool);
							var i = -1;

							Func<bool> MoveNext =
								delegate
								{
									i++;

									if (i >= InputPools.Length)
										return false;

									return true;
								};

							return new DynamicEnumerator<InputPool>
							{
								DynamicCurrent = () => Current,
								DynamicMoveNext =
									delegate
									{
										while (true)
										{
											if (!MoveNext())
											{
												Current = null;
												return false;
											}

											Current = InputPools[i];

											if (Current.Interval > 0)
												if (Current.Files.Length > 0)
													return true;
										}
									}
							};
						}
				};
			}
		}


		public void FetchInput(Action<FileInfo> handler)
		{
			var done = false;
			foreach (var i in this.ActiveInputPools)
			{
				if (i.ShouldPreferOthers)
					continue;

				if (done)
					break;

				foreach (var f in i.Files)
				{
					handler(f);
					done = true;
					break;
				}
			}
		}

		public override string ToString()
		{
			return this.Name + "#" + this.Counter;
		}




		public virtual IHTMLOrderedList VisualizedWorkItems(FileInfo[] a)
		{
			var o = new IHTMLOrderedList();

			foreach (var f in a)
			{
				var WorkItem = new IHTMLAnchor
				{
					URL = f.FullName.ToRelativePath(),
					innerHTML = f.Name
				}.ToString();

				o.innerHTML += (IHTMLListItem)WorkItem;
			}

			return o;
		}
	}
}
