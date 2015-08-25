using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieAgentOtterExperience.GFeed;
using ScriptCoreLib.Shared.Lambda;

namespace MovieAgentOtterExperience.IDX5711
{
	using Options = Dictionary<string, Action>;
	using MovieAgent.Server.Services;
	using MovieAgentOtterExperience.Otter;
	using System.IO;
	using MovieAgentGadget.Data;

	class DecisionEngine
	{


		public static void Invoke(Document doc)
		{
			var query = from k in doc.responseData.feed.entries
						let t = new TitleParser(k.title)
						select new { k, t };

			var ruleset = new StringBuilder();

			ruleset.AppendLine(@"
set(hyper_res).
set(factor).
set(print_kept).
formula_list(sos).
			");

			#region VariableEqualsToAny
			ParamsFunc<string, string, string> VariableEqualsToAny =
				(variable, values) =>
				{
					var w = new StringBuilder();

					w.Append("(");
					values.ForEach(
						(k, i) =>
						{
							if (i > 0)
								w.AppendLine(" | ");
							w.AppendLine("$EQ(" + variable + ", " + k.GetHashCode() + @") %" + k);

						}
					);

					w.Append(")");

					return w.ToString();
				};

			ParamsFunc<string, int, string> VariableEqualsToAnyInteger =
				(variable, values) =>
				{
					var w = new StringBuilder();

					w.Append("(");
					values.ForEach(
						(k, i) =>
						{
							if (i > 0)
								w.AppendLine(" | ");
							w.AppendLine("$EQ(" + variable + ", " + k + @")");

						}
					);

					w.Append(")");

					return w.ToString();
				};
			#endregion

			#region Question 1
			ruleset.AppendLine(@"
% Question 1
all x all z all u (
    facts(x,category,u) & " + VariableEqualsToAny("u", "Animation") + @" -> ForChildren(x)
).
");

			ruleset.AppendLine(@"
all x all z all u (
    facts(x,category,u) & " + VariableEqualsToAny("u", "Animation", "Comedy", "Family", "Fantasy") + @" -> ForFamily(x)
).
");

			ruleset.AppendLine(@"
all x all z all u (
    facts(x,category,u) & " + VariableEqualsToAny("u", "Adventure", "Drama", "Fantasy", "Mystery", "Thriller", "Action", "Crime") + @" -> ForAdults(x)
).
");
			#endregion

			#region Question 2

			ruleset.AppendLine(@"
% Question 2
all x all z all u (
    facts(x,year,u) & " + VariableEqualsToAnyInteger("u", DateTime.Now.Year, DateTime.Now.Year + 1) + @" -> ForGeeks(x)
).
");

			ruleset.AppendLine(@"
all x all z all u (
    facts(x,year,u) & " + VariableEqualsToAnyInteger("u", DateTime.Now.Year, DateTime.Now.Year + 1, DateTime.Now.Year - 1, DateTime.Now.Year - 2) + @" -> ForRecent(x)
).
");

			#endregion

			#region Question 3
			ruleset.AppendLine(@"
% Question 3
all x all z all u (
    facts(x,raiting,u) & $GT(u, 65)
    -> GoodRaiting(x)
).

all x all z all u (
    facts(x,raiting,u) & $LT(u, 70)
    -> BadRaiting(x)
).
");
			#endregion

			#region Question 4

			ruleset.AppendLine(@"
% Question 4
all x all z all u (
    facts(x,category,u) & " + VariableEqualsToAny("u", "TV shows") + @" -> IsTVShow(x)
).

all x all z all u (
    facts(x,category,u) & " + VariableEqualsToAny("u", "Movies") + @" -> IsMovie(x)
).
");
			#endregion

			#region facts
			foreach (var entry in query)
			{

				entry.k.content.ParseMovieItem(
					m =>
					{
						ruleset.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', raiting, " + Convert.ToInt32(m.Raiting * 100) + ").");
					}
				);

				var p = new BasicFileNameParser(entry.t.Title);

				ruleset.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', year, " + entry.t.Year + ").");

				//if (p.Season != null)
				//    ruleset.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', season, " + int.Parse(p.Season) + ").");
				//if (p.Episode != null)
				//    ruleset.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', episode, " + int.Parse(p.Episode) + ").");

				foreach (var category in entry.k.categories)
				{
					ruleset.AppendLine("facts('" + entry.t.Title.Replace("'", @"-") + "', category, " + category.GetHashCode() + "). %" + category);

				}
			}
			#endregion





			var Filters = new List<string>();

			Func<string, Action> f =
				filter => () => Filters.Add(filter);

			ConsoleQuestions.Ask(
				new Options
				{
					{ "There are some kids over here", f("ForChildren(x)")},
					{ "My family is in the room, keep it decent", f("ForFamily(x)")},
					{ "There are only some dudes in the room", f("ForAdults(x)")},
					{ "Neither", () => {}},
				},
				new Options
				{
					{ "I am looking for new stuff", f("ForGeeks(x)")},
					{ "I haven't watched that much tv recently!", f("ForRecent(x)")},
					{ "I do not like recent movies at all!", f("-ForRecent(x)")},
					{ "Neither", () => {}},
				},
				new Options
				{
					{ "I am looking for having a good time", f("GoodRaiting(x)")},
					{ "I want to suggest someething really bad to a friend", f("BadRaiting(x)")},
					{ "I cannot decide between options above", () => {}},
				},
				new Options
				{
					{ "I cannot watch tv very long", f("IsTVShow(x)")},
					{ "30 minutes is not enough for me",  f("IsMovie(x)")},
					{ "I cannot decide between options above", () => {}},
				}
			);

			#region IsSelected
			ruleset.AppendLine(@"
% The anwser
all x  (
"
);

			Filters.ForEach(
				(k, i) =>
				{
					if (i > 0)
						ruleset.AppendLine(" & ");

					ruleset.AppendLine(k);
				}
			);

			ruleset.AppendLine(@"
	-> IsSelected(x)
)."
		);
			#endregion



			var otter = new OtterApplication(ruleset.ToString());

			Console.WriteLine();
			Console.WriteLine("Movies for you");
			otter.ToConsole(k => k == "IsSelected");

			File.WriteAllText(@"Data\IDX5711.in", ruleset.ToString());
			File.WriteAllText(@"Data\IDX5711.out", otter.Output);



		}


	}
}
