using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using MovieAgentGadget.ActionScript.Library;
using MovieAgentGadget.Data;
using MovieAgentGadget.Promotion;
using MovieAgentGadget.Shared;
using ScriptCoreLib;
using ScriptCoreLib.ActionScript;
using ScriptCoreLib.ActionScript.DOM;
using ScriptCoreLib.ActionScript.DOM.Extensions;
using ScriptCoreLib.ActionScript.DOM.HTML;
using ScriptCoreLib.ActionScript.Extensions;
using ScriptCoreLib.Shared.Lambda;
using MovieAgent.Shared;

namespace MovieAgentGadget.ActionScript
{

	partial class MovieAgentGadget
	{
		private IHTMLDiv InitializeSuggestMovie(ExternalContext Context, List<MovieItemWithPoster> KnownMovies, IHTMLDiv Toolbar, IHTMLDiv Shadow)
		{
			#region SuggestMovie
			var SuggestionDialog = new IHTMLDiv().AttachTo(Context);

			SuggestionDialog.style.position = "absolute";
			SuggestionDialog.style.left = "50%";
			SuggestionDialog.style.top = "50%";
			SuggestionDialog.style.marginLeft = (-400 - -40) + "px";
			SuggestionDialog.style.marginTop = (-300 - -40) + "px";
			SuggestionDialog.style.backgroundColor = "black";
			SuggestionDialog.style.width = (800 + -40 * 2) + "px";
			SuggestionDialog.style.height = (600 + -40 * 2) + "px";

			SuggestionDialog.style.color = "white";
			SuggestionDialog.style.display = "none";

			SuggestionDialog.style.overflow = "auto";


			var InputSection = new IHTMLDiv { innerHTML = "<h2>What do you like to do?</h2>" }.AttachTo(SuggestionDialog);
			var OuputSection = new IHTMLDiv { innerHTML = "<h2>Here what I think!</h2>" }.AttachTo(SuggestionDialog);

			var Reasonbox = new IHTMLDiv().AttachTo(OuputSection);

			OuputSection.style.display = "none";

			var GroupSelectedOptionChanged = default(Action);
			var GroupSelectedOptions = new List<Func<MovieItem, bool>>();

			var Reasoning = new List<Func<string>>();

			#region AddSuggestionGroup
			Func<Func<string, string, Func<MovieItem, bool>, Action>> AddSuggestionGroup =
				() =>
				{
					var Group = new IHTMLDiv { innerHTML = "<h3>Step " + (GroupSelectedOptions.Count + 1) + "</h3>" }.AttachTo(InputSection);

					
					var GroupContent = new IHTMLDiv().AttachTo(InputSection);
					var GroupSelectedOption = default(Func<MovieItem, bool>);

					var CurrentReason = "";

					Reasoning.Add(() => CurrentReason);
					GroupSelectedOptions.Add(k => GroupSelectedOption(k));

					GroupContent.style.marginLeft = "3em";

					var GroupSelectedOptionButton = default(IHTMLButton);

					Func<string, string, Func<MovieItem, bool>, Action> GroupOption =
						(text, reason, filter) =>
						{
							var btn = new IHTMLButton { innerHTML = text }.AttachTo(GroupContent);

							btn.style.display = "block";

							Action a =
								delegate
								{
									CurrentReason = reason;

									if (GroupSelectedOptionButton != null)
										GroupSelectedOptionButton.style.color = "";

									GroupSelectedOptionButton = btn;
									GroupSelectedOptionButton.style.color = "blue";

									GroupSelectedOption = filter;

									if (GroupSelectedOptionChanged != null)
										GroupSelectedOptionChanged();
								};

							btn.onclick += a;
							return a;
						};


					return GroupOption;

				};
			#endregion

			AddSuggestionGroup().Apply(
				g =>
				{
					var ForChildren = new[] { "Animation" };
					var ForFamily = new[] { "Animation", "Comedy", "Family", "Fantasy" };
					var ForAdults = new[] { "Adventure", "Drama", "Fantasy", "Mystery", "Thriller", "Action", "Crime" };

					g(
						"There are some kids over here",
						"Kids love cartoons.",
						k => ForChildren.Any(x => k.IMDBGenres.ToLower().Contains(x.ToLower()))
					);
					g(
						"My family is in the room, keep it decent",
						"You can feel comfortable watching comedy with your family.",
						k => ForFamily.Any(x => k.IMDBGenres.ToLower().Contains(x.ToLower()))
					);
					g(
						"There are only some dudes in the room",
						"A beer and a thriller - just for you.",
						k => ForAdults.Any(x => k.IMDBGenres.ToLower().Contains(x.ToLower()))
					);
					g(
						"Neither",
						"Cartoons and horror movies coming straight up!",
						k => true
					)();
				}
			);

			AddSuggestionGroup().Apply(
				g =>
				{
					var Year = DateTime.Now.Year;
					var ForGeeks = new[] { "" + (Year), "" + (Year + 1), };
					var ForRecent = 
						new[] { 
							"" + (Year + 1),
							"" + (Year), 
							"" + (Year - 1),
							"" + (Year - 2),
						};

					g(
						"I am looking for new stuff",
						"You should only be interested in new movies.",
						k => ForGeeks.Any(x => k.SmartTitle.ToLower().Contains(x.ToLower()))
					);

					g(
						"I haven't watched that much tv recently!",
						"You should only be interested in recent movies.",
						k => ForRecent.Any(x => k.SmartTitle.ToLower().Contains(x.ToLower()))
					);

					g(
						"I do not like recent movies at all!",
						"You like older movies!",
						k => ForRecent.Any(x => !k.SmartTitle.ToLower().Contains(x.ToLower()))
					);


					g(
						"Neither",
						"It does not matter to you how old the movie is.",
						k => true
					)();
				}
			);

			AddSuggestionGroup().Apply(
				g =>
				{
					g("I am looking for having a good time",
					  "I guess you do not like movies without story? Me too! They are a waste of my and your time!",
					  k => k.Raiting > 0.65
					);
					g(
						"I want to suggest someething really bad to a friend",
						"Why watch something great if you could watch something really bad?",
						k => k.Raiting < 0.7
					);
					g(
						"I cannot decide between options above",
						"You don't trust others raiting the movies and would like to do it yourself.",
						k => true
					)();
				}
			);

	

			AddSuggestionGroup().Apply(
				g =>
				{
					g("I cannot watch tv very long", "TV shows are known to be short so thats what you get.", k => k.SmartTitle.ToLower().StartsWith("tv show:"));
					g("30 minutes is not enough for me", "You are going to skip the TV shows and see the movies.", k => k.SmartTitle.ToLower().StartsWith("movie:"));
					g("I cannot decide between options above", "You are going to watch tv shows and movies.", k => true)();
				}
			);

			new IHTMLDiv { innerHTML = "<hr />" }.AttachTo(InputSection);

			var NextButton = new IHTMLButton { innerHTML = "Next &raquo;" }.AttachTo(InputSection);
			NextButton.onclick +=
				delegate
				{
					GroupSelectedOptionChanged();

					InputSection.style.display = "none";
					OuputSection.style.display = "block";
				};


			new IHTMLDiv { innerHTML = "<hr />" }.AttachTo(OuputSection);

			var BackButton = new IHTMLButton { innerHTML = "&laquo; Back" }.AttachTo(OuputSection);
			BackButton.onclick +=
				delegate
				{
					OuputSection.style.display = "none";
					InputSection.style.display = "block";
				};

			var CloseButton = new IHTMLButton { innerHTML = "Hide this dialog" }.AttachTo(OuputSection);
			CloseButton.onclick +=
				delegate
				{
					SuggestionDialog.style.display = "none";
					Shadow.style.display = "none";
				};

			this.KnownMoviesFilter = k => GroupSelectedOptions.All(x => x(k));

			GroupSelectedOptionChanged +=
				delegate
				{
					var w = new StringBuilder();
					Reasoning.ForEach(i => w.AppendLine(i()));

					var c = KnownMovies.Count(k => this.KnownMoviesFilter(k.Movie));

					if (c > 0)
					{
						if (c > 1)
							w.AppendLine("There are " + c + " things I am able to suggest you.");
						else
							w.AppendLine("There is only one thing I was able to suggest you.");

						w.AppendLine("I might be interested in " + KnownMovies.Random(k => this.KnownMoviesFilter(k.Movie)).Movie.SmartTitle);
					}
					else
					{
						w.AppendLine("Oops. There are no such things here at this time. Maybe later? Maybe change your anwsers instead?");
					}

					Reasonbox.innerHTML = w.ToString();

					foreach (var k in KnownMovies)
					{
						if (this.KnownMoviesFilter(k.Movie))
							k.Poster.style.display = "";
						else
							k.Poster.style.display = "none";
					}
				};
			GroupSelectedOptionChanged();

			var SuggestMovie = new IHTMLSpan { innerHTML = "&raquo; suggest a movie" }.AttachTo(Toolbar);

			SuggestMovie.style.Apply(
				s =>
				{
					s.border = "1px dotted white";
					s.cursor = "pointer";
					s.backgroundColor = "black";
					s.color = "white";
					s.marginLeft = "1em";
				}
			);

			SuggestMovie.onclick +=
				delegate
				{
					// clear if any and return

					Shadow.style.display = "block";
					SuggestionDialog.style.display = "block";
				};
			#endregion
			return SuggestionDialog;
		}




	}

}