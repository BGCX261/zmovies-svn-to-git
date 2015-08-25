using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;

namespace MovieAgent.Shared
{
	[Script]
	public class DynamicEnumerator<T> : IEnumerator<T>
	{

		public Action DynamicDispose;
		public Func<bool> DynamicMoveNext;
		public Func<T> DynamicCurrent;

		#region IEnumerator<T> Members

		public T Current
		{
			get { return DynamicCurrent(); }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			DynamicCurrent = null;
			if (DynamicDispose != null)
				DynamicDispose();
		}

		#endregion

		#region IEnumerator Members

		object System.Collections.IEnumerator.Current
		{
			get { return this.Current; }
		}

		public bool MoveNext()
		{
			return DynamicMoveNext();
		}

		public void Reset()
		{
			
		}

		#endregion
	}

	[Script]
	public class DynamicEnumerable<T> : IEnumerable<T>
	{
		public Func<IEnumerator<T>> DynamicGetEnumerator;

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return DynamicGetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
