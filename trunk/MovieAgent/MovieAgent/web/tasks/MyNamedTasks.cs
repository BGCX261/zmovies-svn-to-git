using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieAgent.Server.Library;
using ScriptCoreLib;
using System.IO;
using System.Threading;
using MovieAgent.Server.Library;

namespace MovieAgent.web.tasks
{

	[Script]
	public class MyNamedTasks : NamedTasks
	{
		public readonly Task1.Task1 Task1_GatherSupply;
		public readonly Task2.Task2 Task2_AliasSearch;

		public readonly Task3_FindTorrents.Task3_FindTorrents Task3_FindTorrents;
		public readonly Task4_PrepareMedia.Task4_PrepareMedia Task4_PrepareMedia;
		public readonly Task5_CloneMedia.Task5_CloneMedia Task5_CloneMedia;
		public readonly Task6_MediaCollector.Task6_MediaCollector Task6_MediaCollector;

		public readonly DirectoryInfo Output;

		public MyNamedTasks()
			: base(new DirectoryInfo("tasks"))
		{
			this.Output = new DirectoryInfo("output");

#if DEBUG
			this.Scheduler = new SchedulerTask(this, "Scheduler", 400);
#else
			this.Scheduler = new SchedulerTask(this, "Scheduler", 4000);
#endif


			this.Task1_GatherSupply = new Task1.Task1(this);
			// dummy task
			//this.Task2_AliasSearch = new Task2.Task2(this);
			this.Task3_FindTorrents = new Task3_FindTorrents.Task3_FindTorrents(this);
			this.Task4_PrepareMedia = new Task4_PrepareMedia.Task4_PrepareMedia(this);
			//this.Task5_CloneMedia = new Task5_CloneMedia.Task5_CloneMedia(this);
			this.Task6_MediaCollector = new Task6_MediaCollector.Task6_MediaCollector(this);

			this.Disabled =
				delegate
				{
					Console.WriteLine("tasks are disabled (create subfolder tasks chmod 777)");
				};

			this.Default =
				Query =>
				{
					if (Query == "console")
					{
						"<html>".ToConsole();
						"<head>".ToConsole();
						new IHTMLLink
						{
							Relationship = "alternate",
							URL = "http://localhost/jsc/zmovies/output/" + this.Task6_MediaCollector.Feed.Name,
							//URL = "http://localhost:60177?feed0",
							Type = "application/rss+xml",
							Title = "Goodies"
						}.ToString().ToConsole();

						"</head>".ToConsole();

						"<body><center>".ToConsole();

						if (this.Task6_MediaCollector.Feed.Exists)
						{
							("age: " + (DateTime.Now - this.Task6_MediaCollector.Feed.LastWriteTime).Hours).ToConsole();
						}

						"Show tasks".ToLink("?overview", "Frame " + this.Scheduler.Counter).ToConsole();

						"</center></body>".ToConsole();
						"</html>".ToConsole();

						return;
					}


					if (Query == "overview")
					{
						ShowTasks();
						return;
					}
				};
		}

		private void ShowTasks()
		{
			//Console.WriteLine("<meta http-equiv='refresh' content='60' />");

			var OrderedList = new IHTMLOrderedList();

			IHTMLListHeader Header = "Tasks";

			OrderedList.innerHTML += Header;

			foreach (var Task in this)
			{
				var ListItem = new IHTMLListItem();

				var Details = new IHTMLUnorderedList();

				//Details.Content += (IHTMLListHeader)"Details";
				Details.innerHTML += (IHTMLListItem)(
					"" + new IHTMLAcronym
					{
						Title = Task.CounterPath,
						innerHTML = "Counter is " + Task.Counter
					}.ToString() +
					" and " + new IHTMLAcronym
					{
						Title = "memory allows to filter out already happened events",
						innerHTML = "memory is " + Task.Memory.Count
					}.ToString() +
					" within context of " + new IHTMLAcronym
					{
						Title = Task.Context.FullName,
						innerHTML = Task.Context.Name
					}.ToString()
				);


				var DepensdsOnText = new IHTMLAcronym { Title = "This task will not execute until dependencies are cleared from work", innerHTML = "Depends on" }.ToString();

				foreach (var DependsOn in Task.ActiveDependencies)
				{

					if (DependsOn.IsActive)
						Details.innerHTML += (IHTMLListItem)(DepensdsOnText + " (active) " + DependsOn.Task.Name.ToLink(k => "?" + k));
					else
						Details.innerHTML += (IHTMLListItem)(DepensdsOnText + " " + DependsOn.Task.Name.ToLink(k => "?" + k));
				}

				var InputPoolText = new IHTMLAcronym { Title = "Pools are grouped by the intervals they represent. Smaller intervals will be executed before larger intervals", innerHTML = "Input Pool" }.ToString();

				foreach (var InputPool in Task.ActiveInputPools)
				{

					var InputPoolHeader = (InputPoolText + " " + (IHTMLStrong)("" + InputPool.Interval) +
						" estimated time " + InputPool.Total + " for " + InputPool.Files.Length + " items");

					var Files = Task.VisualizedWorkItems(InputPool.Files);


					if (InputPool.ShouldPreferOthers)
						Details.innerHTML += (IHTMLListItem)("<span style='color: gray;'>(blocked) " + InputPoolHeader + Files + "</span>");
					else
						Details.innerHTML += (IHTMLListItem)(InputPoolHeader + Files);
				}



				if (Task.HasActiveDependencies)
					ListItem.innerHTML += "(blocked) ";

				ListItem.innerHTML += Task.Name.ToLink(k => "?" + k);



				if (!string.IsNullOrEmpty(Task.Description))
				{
					ListItem.innerHTML += "<pre>" + Task.Description.Trim() + "</pre>";
				}


				ListItem.innerHTML += "<textarea style='width: 100%; height: 20%;'>" + Task.Log.Trim() + "</textarea>";


				ListItem.innerHTML += Details;

				if (Task.HasActiveDependencies)
					OrderedList.innerHTML += "<span style='color: gray;'>" + ListItem + "</span>";
				else
					OrderedList.innerHTML += ListItem;
			}

			Console.WriteLine(OrderedList);
		}




	}

}
