using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Utilities
{
	public class ReaderLock : IDisposable
	{
		public ReaderLock(ReaderWriterLock sync)
		{
			_Sync = sync;
			_Sync.AcquireReaderLock(Timeout.Infinite);
		}

		#region IDisposable Members

		public void Dispose()
		{
			_Sync.ReleaseReaderLock();
		}

		#endregion

		#region Private Members

		private ReaderWriterLock _Sync;

		#endregion
	}

	public class WriterLock : IDisposable
	{
		public WriterLock(ReaderWriterLock sync)
		{
			_Sync = sync;
			_Sync.AcquireWriterLock(Timeout.Infinite);
		}

		#region IDisposable Members

		public void Dispose()
		{
			_Sync.ReleaseWriterLock();
		}

		#endregion

		#region Private Members

		private ReaderWriterLock _Sync;

		#endregion
	}
}
