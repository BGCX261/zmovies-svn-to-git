using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using System.IO;

namespace MovieAgent.Server.Library
{
	[Script]
	public class MemoryDirectory : ICollection<string>
	{
		public readonly DirectoryInfo Container;
		public MemoryDirectory(DirectoryInfo Container)
		{
			this.Container = Container;
		}

		#region ICollection<string> Members

		public void Add(string item)
		{
			var a = item.Substring(0, 2);
			var b = item.Substring(2);

			// simple bucket support
			this.Container.CreateSubdirectory(a).CreateSubdirectory(b);
		}

		public DirectoryInfo this[string item]
		{
			get
			{
				var a = item.Substring(0, 2);
				var b = item.Substring(2);

				// simple bucket support
				return this.Container.CreateSubdirectory(a).CreateSubdirectory(b);
			}
		}

		public void Clear()
		{
			throw new NotImplementedException("");
		}

		public bool Contains(string item)
		{
			var a = item.Substring(0, 2);
			var b = item.Substring(2);

			if (this.Container.ToDirectory(a).Exists)
				if (this.Container.ToDirectory(a).ToDirectory(b).Exists)
					return true;

			return false;
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			throw new NotImplementedException("");
		}

		public int Count
		{
			get
			{
				var c = 0;

				foreach (var i in Container.GetDirectories())
				{
					c += Directory.GetDirectories(i.FullName).Length;
				}

				return c;
			}
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(""); }
		}

		public bool Remove(string item)
		{
			throw new NotImplementedException("");
		}

		#endregion

		#region IEnumerable<string> Members

		public IEnumerator<string> GetEnumerator()
		{
			throw new NotImplementedException("");
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException("");
		}

		#endregion
	}
}
