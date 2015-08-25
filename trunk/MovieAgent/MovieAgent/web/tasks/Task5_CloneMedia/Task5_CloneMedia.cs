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

namespace MovieAgent.web.tasks.Task5_CloneMedia
{
	[Script]
	public class Task5_CloneMedia : WorkTask
	{
		[Script]
		public class Entry
		{
			public BasicPirateBaySearch.SearchEntry PirateBay = new BasicPirateBaySearch.SearchEntry();
			public BasicIMDBCrawler.Entry IMDB = new BasicIMDBCrawler.Entry();
			public string TinEyeHash;
			public string IMDBKey;
			public string BayImgKey;
			public string YouTubeKey;

			public BasicPirateBayImage.Entry BayImg
			{
				get
				{
					return new BasicPirateBayImage.Entry(this.BayImgKey);
				}
			}
			public string SmartTitle
			{
				get
				{
					return this.PirateBay.Name + " | " + this.IMDB.SmartTitle + " | " + this.PirateBay.SmartName.SeasonAndEpisode;
				}
			}

			public string YouTubeImage
			{
				get
				{
					return "http://img.youtube.com/vi/" + this.YouTubeKey + @"/0.jpg";
				}
			}

			public string YouTubeLink
			{
				get
				{
					return "http://www.youtube.com/v/" + this.YouTubeKey + @"&hl=en&fs=1";
				}
			}

			public string YouTubeQuery
			{
				get
				{
					var Query = this.IMDB.MediumPosterTitle;

					var SeasonAndEpisode = this.PirateBay.SmartName.SeasonAndEpisode;

					if (!string.IsNullOrEmpty(SeasonAndEpisode))
						Query += " " + SeasonAndEpisode;
					else
						Query += " trailer";

					return Query;
				}
			}
			public string IMDBLink
			{
				get
				{
					return BasicIMDBCrawler.ToLink(IMDBKey);
				}
			}

			public string TinEyeImageLink
			{
				get
				{
					return "http://tineye.com/query/" + TinEyeHash;
				}
			}
		}

		public Task5_CloneMedia(MyNamedTasks Tasks)
			: base(Tasks, "Task5_CloneMedia")
		{
			var NamedTasks = Tasks;

			this.Description = @"
Clone poster to imgbay.
			";

			this.YieldWork =
				(Task, Input) =>
				{
					var Entry = new Entry().FromFile(Input);

					Input.Delete();

					if (string.IsNullOrEmpty(Entry.TinEyeHash))
					{
						#region TinEyeHash
						AppendLog("in " + Entry.PirateBay.Name + " without tineye");

						BasicTinEyeSearch.Search(Entry.IMDB.MediumPosterImage,
							h =>
							{
								AppendLog("as " + h.Hash);

								Entry.TinEyeHash = h.Hash;
								Entry.ToFile(Input);
							}
						);
						#endregion
					}
					else if (string.IsNullOrEmpty(Entry.BayImgKey))
					{
						#region BayImgKey
						AppendLog("in " + Entry.PirateBay.Name + " without bayimg");

						// do we have it in memory?

						var BayImg = this.Memory[Entry.TinEyeHash].FirstDirectoryOrDefault();

						if (BayImg == null)
						{
							BasicPirateBayImage.Clone(new Uri(Entry.TinEyeImageLink),
								e =>
								{
									Entry.BayImgKey = e.Key;
									Entry.ToFile(Input);

									BayImg = this.Memory[Entry.TinEyeHash].CreateSubdirectory(e.Key);
									AppendLog("in " + Entry.PirateBay.Name + " now known as " + Entry.BayImgKey);
								}
							);
						}
						else
						{
							Entry.BayImgKey = BayImg.Name;
							Entry.ToFile(Input);

							AppendLog("in " + Entry.PirateBay.Name + " already known as " + Entry.BayImgKey);
						}
						#endregion

					}
					else if (string.IsNullOrEmpty(Entry.YouTubeKey))
					{
						AppendLog("in " + Entry.PirateBay.Name + " looking for video...");



						BasicGoogleVideoCrawler.Search(Entry.YouTubeQuery,
							(v, src) =>
							{
								AppendLog("as " + v);

								Entry.YouTubeKey = v;
								Entry.ToFile(Input);
							}
						);
					}
					else
					{
						var NextWork = NamedTasks.Task6_MediaCollector.AddWork(5, Entry.PirateBay.Hash);

						if (NextWork.Exists)
							AppendLog("out already exists " + Entry.PirateBay.Name + " as " + Entry.PirateBay.Hash);
						else
						{
							AppendLog("out " + Entry.PirateBay.Name + " as " + Entry.PirateBay.Hash);


							Entry.ToFile(NextWork);
						}
					}

					// if we fail, we this work item will not be retried
					//Input.Delete();

					return delegate
					{
						// this will be called if the task is still active
					};
				};
		}


		public override IHTMLOrderedList VisualizedWorkItems(FileInfo[] a)
		{
			var o = new IHTMLOrderedList();

			foreach (var f in a)
			{
				var k = new BasicPirateBaySearch.SearchEntry().FromFile(f);

				var WorkItem = new IHTMLAnchor
				{
					URL = f.FullName.ToRelativePath(),
					innerHTML = k.Name
				}.ToString() + " - <b>" + k.SmartName.ToString() + "</b>";

				o.innerHTML += (IHTMLListItem)WorkItem;
			}

			return o;
		}
	}

	[Script]
	public static class Task5_CloneMediaExtensions
	{
		public static void ToFile(this Task5_CloneMedia.Entry e, FileInfo f)
		{
			using (var s = new StreamWriter(f.OpenWrite()))
			{
				ToFile(e, s);
			}
		}

		public static void ToFile(this Task5_CloneMedia.Entry e, StreamWriter s)
		{
			e.PirateBay.ToFile(s);
			e.IMDB.ToFile(s);

			s.WriteLines(
				e.TinEyeHash,
				e.IMDBKey,
				e.BayImgKey,
				e.YouTubeKey
			);
		}

		public static Task5_CloneMedia.Entry FromFile(this Task5_CloneMedia.Entry e, FileInfo f)
		{
			using (var s = new StreamReader(f.OpenRead()))
			{
				FromFile(e, s);
			}

			return e;
		}

		public static void FromFile(this Task5_CloneMedia.Entry e, StreamReader s)
		{
			e.PirateBay.FromFile(s);
			e.IMDB.FromFile(s);
			e.TinEyeHash = s.ReadLine();
			e.IMDBKey = s.ReadLine();
			e.BayImgKey = s.ReadLine();
			e.YouTubeKey = s.ReadLine();

		}

	}
}
