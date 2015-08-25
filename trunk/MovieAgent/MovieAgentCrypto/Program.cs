using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using MovieAgent.Server.Services;
using MovieAgent.Server.Library;
using MovieAgent.Shared;

namespace MovieAgentCrypto
{
	class PartialKey : IEnumerable<byte>
	{
		internal byte[] InternalValue;

		public static implicit operator byte[](PartialKey e)
		{
			return e.InternalValue;
		}

		public static implicit operator PartialKey(byte[] e)
		{
			return new PartialKey { InternalValue = e };
		}

		public static PartialKey operator ~(PartialKey a)
		{
			return a.InternalValue.Select(k => (byte)~k).ToArray();
		}



		public static PartialKey operator ^(PartialKey a, PartialKey b)
		{
			if (a == null) return b;
			if (b == null) return null;

			var x = Math.Max(a.InternalValue.Length, b.InternalValue.Length);
			var y = new byte[x];
			for (int i = 0; i < x; i++)
			{
				y[i] = (byte)(a.InternalValue[i % a.InternalValue.Length] ^ b.InternalValue[i % b.InternalValue.Length]);
			}

			return y;
		}

		public static implicit operator PartialKey(Uri[] a)
		{
			var x = default(PartialKey);

			foreach (var k in a)
			{
				x ^= BasicTinEyeSearch.ToBytes(k);
			}

			return x;
		}

	

		public byte[] MD5Bytes
		{
			get
			{
				return this.InternalValue.ToMD5Bytes();
			}
		}

		public override string ToString()
		{
			return this.MD5Bytes.ToHexString();
		}

		public byte[] ToKey(int length)
		{
			var x = new byte[length];

			for (int i = 0; i < this.InternalValue.Length; i++)
			{
				x[i % length] ^= this.InternalValue[i];
			}

			return x;
		}



		#region IEnumerable<byte> Members

		public IEnumerator<byte> GetEnumerator()
		{
			return this.InternalValue.AsEnumerable().GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.InternalValue.AsEnumerable().GetEnumerator();
		}

		#endregion
	}

	

	class Program
	{
		// http://crypto.hurlant.com/demo/

		static void Main(string[] args)
		{
			PartialKey salt = new[]
			{
				new Uri("http://tineye.com/images/tineye_logo_big.png"),
			    new Uri("http://www.google.ee/intl/en_com/images/logo_plain.png"),
			    new Uri("http://de543dbf.pastebin.com/favicon.ico"),
			};


			var c = 8;

			for (int i = 0; i < c; i++)
			{
				File.WriteAllBytes("Data/tech.png." + i, (~salt ^ File.ReadAllBytes("Data/tech.png")).Where((k, j) => j % c == i).ToArray());
			}

			File.WriteAllBytes("Data/__tech.png",
				~salt ^ Enumerable.Range(0, c).Select(i => File.ReadAllBytes("Data/tech.png." + i).AsEnumerable()).Deinterlace().ToArray()
			);

		}

		
	}

	static class Extensions
	{
		public static IEnumerable<T> Deinterlace<T>(this IEnumerable<IEnumerable<T>> source)
		{
			var q = new Queue<IEnumerator<T>>(source.Select(k => k.GetEnumerator()));

			while (q.Count > 0)
			{
				var x = q.Dequeue();

				if (x.MoveNext())
				{
					yield return x.Current;
					q.Enqueue(x);
				}
				else
				{
					x.Dispose();
				}
			}
			
		}
	}
}
