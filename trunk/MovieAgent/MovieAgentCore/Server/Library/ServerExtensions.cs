using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using System.IO;
using MovieAgent.Shared;

namespace MovieAgent.Server.Library
{
	[Script]
	public static class ServerExtensions
	{
		public static void WriteLines(this StreamWriter e, params string[] text)
		{
			foreach (var k in text)
			{
				e.WriteLine(k);
			}
		}

		public static string ToHexString(this byte[] e)
		{
			var w = new StringBuilder();

			foreach (var v in e)
			{
				w.Append(v.ToHexString());
			}

			return w.ToString();
		}

		public static string ToHexString(this byte e)
		{
			const string u = "0123456789abcdef";

			return u.Substring((e >> 4) & 0xF, 1) + u.Substring((e >> 0) & 0xF, 1);
		}

		public static byte[] ToMD5Bytes(this string e)
		{
			var buffer = new byte[e.Length];

			for (int i = 0; i < e.Length; i++)
			{
				buffer[i] = (byte)(e[i] & 0xff);
			}

			return ToMD5Bytes(buffer);
		}

		public static byte[] ToMD5Bytes(this byte[] buffer)
		{
			var x = new System.Security.Cryptography.MD5CryptoServiceProvider();


			return x.ComputeHash(buffer);
		}

		public static void InvokeByModulus(this Action[] e, int c)
		{
			e[c % e.Length]();
		}

		public static DirectoryInfo FirstDirectoryOrDefault(this DirectoryInfo e)
		{
			var x = default(DirectoryInfo);

			foreach (var k in e.GetDirectories())
			{
				x = k;
				break;
			}

			return x;
		}

		public static FileInfo FirstFileOrDefault(this DirectoryInfo e)
		{
			var x = default(FileInfo);

			foreach (var k in e.GetFiles())
			{
				x = k;
				break;
			}

			return x;
		}

		public static int Random(this int e)
		{
			var r = new Random();

			return r.Next(e);
		}

		public static Uri ToUri(this string e)
		{
			return new Uri(e);
		}



		public static Uri WithoutQuery(this Uri e)
		{
			const string SchemeDelimiter = "://";

			return new Uri(e.Scheme + SchemeDelimiter + e.Host  + e.AbsolutePath);
		}

		public static Uri ToCoralCache(this Uri e)
		{
			const string SchemeDelimiter = "://";
			const string CoralSuffix = ".nyud.net";

			if (e.Host.EndsWith(CoralSuffix))
				return e;

			return new Uri(e.Scheme + SchemeDelimiter + e.Host + CoralSuffix + e.PathAndQuery);
		}

		public static void ToConsole(this string e)
		{
			Console.WriteLine(e);
		}

		public static string ToRelativePath(this string e)
		{

			if (e.StartsWith(Environment.CurrentDirectory))
				return e.Substring(Environment.CurrentDirectory.Length + 1);

			return e;
		}

		public static void Apply<T>(this T e, Action<T> handler)
			where T : class
		{
			if (e == default(T))
				return;

			handler(e);
		}

		public static void WhenStartsWith(this string source, string e, Action<string> handler)
		{
			if (source.ToLower().StartsWith(e.ToLower()))
			{
				handler(source.Substring(e.Length));
			}
		}

		public static FileInfo ToFile(this DirectoryInfo source, string file)
		{
			return new FileInfo(
				Path.Combine(source.FullName, file)
			);
		}

		public static DirectoryInfo ToDirectory(this DirectoryInfo source, string sub)
		{
			return new DirectoryInfo(
				Path.Combine(source.FullName, sub)
			);
		}
		public static Action<A, B> FixLastParam<A, B, C>(this C c, Action<A, B, C> f)
		{
			return (a, b) => f(a, b, c);
		}

		public static string ToLink(this string text, Func<string, string> gethref)
		{
			return "<a href='" + gethref(text) + "'>" + text + "</a>";
		}

		public static string ToLink(this string href)
		{
			return "<a href='" + href + "'>" + href + "</a>";
		}



		public static string ToLink(this string text, string href, string title)
		{
			return "<a href='" + href + "' title='" + title + "'>" + text + "</a>";
		}

		public static string ToImage(this string src)
		{
			return "<img src='" + src + "' />";
		}

		public static void ToImageToConsole(this string src)
		{
			Console.WriteLine("<img src='" + src + "' />");
		}

		public static void ToImageToConsoleWithStyle(this string src, string style)
		{
			Console.WriteLine("<img src='" + src + "' style='" + style + "' />");
		}

		

		

		public static void FindSubstrings(this string source, string target, Func<int, int, int> handler)
		{
			Func<int, int> FindNext =
				offset =>
				{
					var i = source.IndexOf(target, offset);

					if (i < 0)
						return offset;

					var nextoffset = handler(i, target.Length);

					return nextoffset;
				};

			FindNext.ToChainedFunc((x, y) => y > x)(0);
		}

		public static void FindDigits(this string source, int digits, Action<int, int> handler)
		{
			Func<int, int> FindNext =
				offset =>
				{
					// scan to a digit

					for (int i = 0; i < digits; i++)
					{
						if (offset + i >= source.Length)
							return offset;

						var c = source[offset + i];

						if (!char.IsNumber(c))
							return offset + i + 1;

					}


					handler(offset, digits);

					return offset + digits;
				};

			FindNext.ToChainedFunc((x, y) => y > x)(0);
		}

		public static void FindUpperCase(this string source, int digits, Func<int, int, int> handler)
		{
			Func<int, int> FindNext =
				offset =>
				{
					// scan to a digit

					for (int i = 0; i < digits; i++)
					{
						if (offset + i >= source.Length)
							return offset;

						var c = source.Substring(offset + i, 1);

						if (c.ToUpper() == c.ToLower())
							return offset + i + 1;

						if (c.ToUpper() != c)
							return offset + i + 1;
					}


					var nextoffset = handler(offset, digits);

					return nextoffset;
				};

			FindNext.ToChainedFunc((x, y) => y > x)(0);
		}

		public static bool EnsureChars(this string source, int offset, int length, string mask)
		{
			for (int i = 0; i < length; i++)
			{
				if (offset + i >= source.Length)
					return true;

				if (mask.IndexOf(source.Substring(offset + i, 1)) < 0)
					return false;
			}

			return true;
		}

		public static int Max(this int x, int y)
		{
			if (x > y)
				return x;

			return y;
		}

		public static int Min(this int x, int y)
		{
			if (x < y)
				return x;

			return y;
		}


		public static string URLEncode(this string title)
		{
			var w = new StringBuilder();

			const string Unreserved = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_.~";
			const string Reserved = "!*'();:@&=+$,/?%#[] ";
			var ReservedValues = new[] { 
				"%21",
				"%2A",
				"%27",
				"%28",
				"%29",
				"%3B",
				"%3A",
				"%40",
				"%26",
				"%3D",
				"%2B",
				"%24",
				"%2C",
				"%2F",
				"%3F",
				"%25",
				"%23",
				"%5B",
				"%5D",
				"%20"
			};

			for (int i = 0; i < title.Length; i++)
			{
				var c = title.Substring(i, 1);

				if (Unreserved.Contains(c))
				{
					w.Append(c);
				}
				else
				{
					var j = Reserved.IndexOf(c);

					if (j >= 0)
						w.Append(ReservedValues[j]);

				}
			}
			var e = w.ToString();
			return e;
		}

		public static void Split(this string source, string delimiter, Action<string, int> handler)
		{
			var c = -1;

			Func<int, int> FindNext =
				offset =>
				{
					// scan to a digit

					var p = source.IndexOf(delimiter, offset);

					if (p < 0)
					{
						c++;
						handler(source.Substring(offset), c);

						return -1;
					};

					var x = source.Substring(offset, p - offset);

					c++;
					handler(x, c);

					return p + delimiter.Length;
				};

			FindNext.ToChainedFunc((x, y) => y > x)(0);
		}



	}
}
