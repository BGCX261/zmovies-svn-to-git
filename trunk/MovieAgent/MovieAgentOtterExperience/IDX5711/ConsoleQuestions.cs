using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib.Shared.Lambda;

namespace MovieAgentOtterExperience.IDX5711
{
	class ConsoleQuestions
	{
		public static void Ask(params Dictionary<string, Action>[] a)
		{
			a.ForEach(
				(k, i) =>
				{
					Console.WriteLine();
					Console.WriteLine("Question " + (i + 1));
					AskSingle(k);
				}
			);
		}

		public static string AskSingle(Dictionary<ConsoleKey, string> a)
		{
			a.ForEach(
				(KeyValuePair<ConsoleKey, string> k, int i) =>
				{
					Console.WriteLine("  " + k.Key.ToString().Substring(1) + ". " + k.Value);
				}
			);

			while (true)
			{
				var key = Console.ReadKey(true);

				if (a.ContainsKey(key.Key))
				{
					Console.WriteLine(">>" + key.Key.ToString().Substring(1) + ". " + a[key.Key]);

					return a[key.Key];
				}

				Console.WriteLine("Try again.");
			}
		}

		public static void AskSingle(Dictionary<string, Action> a)
		{
			var keys = new[]
			{
				ConsoleKey.D1,
				ConsoleKey.D2,
				ConsoleKey.D3,
				ConsoleKey.D4,
				ConsoleKey.D5,
			};

			var x = new Dictionary<ConsoleKey, string>();

			a.ForEach(
				(KeyValuePair<string, Action> k, int i) =>
				{
					x[keys[i]] = k.Key;
				}
			);

			var y = AskSingle(x);

			a[y]();
		}
	}
}
