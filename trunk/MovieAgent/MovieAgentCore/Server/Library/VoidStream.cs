using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using System.IO;

namespace MovieAgent.Server.Library
{
	[Script]
	public class VoidStream : Stream
	{
		public override bool CanRead
		{
			get { throw new NotImplementedException(""); }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override void Flush()
		{
		}

		public override long Length
		{
			get { throw new NotImplementedException(""); }
		}

		long InternalPosition;

		public override long Position
		{
			get
			{
				return InternalPosition;
			}
			set
			{
				InternalPosition = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException("");
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException("");
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException("");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.InternalPosition += count;
		}
	}
}
