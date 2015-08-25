using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLib.ActionScript.DOM.HTML;
using ScriptCoreLib.ActionScript.DOM.Extensions;
using ScriptCoreLib.ActionScript.Extensions;

namespace MovieAgentGadget.ActionScript.Library
{
	// http://code.google.com/apis/youtube/js_api_reference.html
	// http://code.google.com/apis/ajax/playground/?exp=youtube#chromeless_player
	[Script]
	public class YouTubePlayer : IHTMLObject
	{
		public enum States
		{
			unstarted = -1,
			ended = 0,
			playing = 1,
			paused = 2,
			buffering = 3,
			video_cued = 5
		}

		/// <summary>
		/// This event is fired whenever the player's state changes. Possible values are unstarted (-1), ended (0), playing (1), paused (2), buffering (3), video cued (5). When the SWF is first loaded it will broadcast an unstarted (-1) event. When the video is cued and ready to play it will broadcast a video cued event (5).
		/// </summary>
		public event Action<States> onStateChange
		{
			add
			{
				if (this.Token.Context == null)
					throw new ArgumentNullException("Token.Context");


				var InboundToken = this.Token.Context.ToExternal(
					(States state) =>
					{
						value(state);
					}
				);

				var Outbound = this.Token.Context.ToOutbound<States>("_state",
					"document.getElementById('" + this.Token.Context.Element.id + "')['" + InboundToken + "'](_state);"
				);

				this.Token.Context.ExternalContext_getElementById_call_string_string(
					this.id, "addEventListener", "onStateChange", Outbound.Token
				);

			}
			remove
			{
				throw new NotImplementedException("");
			}
		}

		/// <summary>
		/// Load and plays the specified video
		/// </summary>
		/// <param name="videoId"></param>
		public void loadVideoById(string videoId)
		{
			if (this.Token.Context == null)
				throw new ArgumentNullException("context");


			this.Token.Context.ExternalContext_getElementById_call_string(this.id, "loadVideoById", videoId);
		}

		/// <summary>
		/// Mutes the player.
		/// </summary>
		public void mute()
		{
			if (this.Token.Context == null)
				throw new ArgumentNullException("context");

			this.Token.Context.ExternalContext_getElementById_call(this.id, "mute");
		}

		/// <summary>
		/// Unmutes the player.
		/// </summary>
		public void unMute()
		{
			if (this.Token.Context == null)
				throw new ArgumentNullException("context");

			this.Token.Context.ExternalContext_getElementById_call(this.id, "unMute");
		}

		/// <summary>
		/// Stops the current video. This function also closes the NetStream object and cancels the loading of the video. Once stopVideo() is called, a video cannot be resumed without reloading the player or loading a new video (chromeless player only). When calling stopVideo(), the player broadcasts an end event (0).
		/// </summary>
		public void stopVideo()
		{
			if (this.Token.Context == null)
				throw new ArgumentNullException("context");

			this.Token.Context.ExternalContext_getElementById_call(this.id, "stopVideo");
		}

		/// <summary>
		/// Plays the currently cued/loaded video.
		/// </summary>
		public void playVideo()
		{
			if (this.Token.Context == null)
				throw new ArgumentNullException("context");

			this.Token.Context.ExternalContext_getElementById_call(this.id, "playVideo");
		}

		/// <summary>
		/// Pauses the currently playing video.
		/// </summary>
		public void pauseVideo()
		{
			if (this.Token.Context == null)
				throw new ArgumentNullException("context");

			this.Token.Context.ExternalContext_getElementById_call(this.id, "pauseVideo");
		}

		public static string Create_onYouTubePlayerReady_token;
		public static event Action<string> Create_onYouTubePlayerReady;

		public static void Create(IHTMLDiv Container, int width, int height, Action<YouTubePlayer> handler)
		{
			var Context = Container.Token.Context;

			var n = new YouTubePlayer { id = Context.CreateToken() };

			if (Create_onYouTubePlayerReady_token == null)
			{
				Create_onYouTubePlayerReady_token = Context.CreateToken();
				Create_onYouTubePlayerReady_token.External(
					(string id) =>
					{
						if (Create_onYouTubePlayerReady != null)
							Create_onYouTubePlayerReady(id);
					}
				);

				1.ExternalAtDelay(
					"window['onYouTubePlayerReady'] = function (_id) {document.getElementById('" + Context.Element.id + "')['" + Create_onYouTubePlayerReady_token + "'](_id); };" 
				);
			}

			Container.innerHTML = @"<object 
				type='application/x-shockwave-flash' 
				data='http://www.youtube.com/apiplayer?enablejsapi=1&playerapiid=" + n.id + @"' 
				width='" + width + @"' 
				height='" + height + @"' 
				
				wmode='window' 
				id='" + n.id + @"'
				name='" + n.id + @"'
			
				allowFullScreen='true' 
				allowNetworking='all' 
				allowScriptAccess='always'>
			  <param name='movie' value='http://www.youtube.com/apiplayer?enablejsapi=1&playerapiid=" + n.id + @"' />
			</object>";

			n.Token.Context = Container.Token.Context;

			Create_onYouTubePlayerReady +=
				id =>
				{
					if (n == null)
						return;

					if (id == n.id)
					{
						handler(n);
						n = null;
					}
				};
		}
	}
}
