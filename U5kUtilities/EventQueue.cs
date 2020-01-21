using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Utilities
{
	public delegate void QEventHandler();

	public class EventQueue
	{
        /// <summary>
        /// 
        /// </summary>
		public void Start()
		{
			Debug.Assert(_Stop);

			_NewEvent = new AutoResetEvent(false);
			_StopEvent = new ManualResetEvent(false);
			_WorkingThread = new Thread(ProcessEvents);

			_Stop = false;
			_WorkingThread.Start();
		}

        /// <summary>
        /// 
        /// </summary>
		public void Stop()
		{
			lock (_Queue)
			{
				if (_Stop)
				{
					return;
				}

				_Stop = true;
			}

			_StopEvent.Set();
			_WorkingThread.Join();
			_Queue.Clear();
			_NewEvent.Close();
			_StopEvent.Close();
		}

        /// <summary>
        /// 
        /// </summary>
		public void InternalStop()
		{
			lock (_Queue)
			{
				_Stop = true;
			}

			_Queue.Clear();
			_NewEvent.Close();
			_StopEvent.Close();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="handler"></param>
		public void Enqueue(string id, QEventHandler handler)
		{
			if (Thread.CurrentThread.ManagedThreadId == _WorkingThread.ManagedThreadId)
			{
				if (!_Stop)
				{
					try
					{
						handler();
					}
					catch (Exception ex)
					{
						throw new Exception("ERROR running " + id + ": " + ex.Message);
					}
				}
			}
			else
			{
				lock (_Queue)
				{
					if (!_Stop)
					{
						_Queue.Enqueue(new Event(id, handler));
						_NewEvent.Set();
					}
				}
			}
		}

		#region Private Members

		class Event
		{
			public string Id;
			public QEventHandler Handler;
			public bool Valid;

			public Event(string id, QEventHandler handler)
			{
				Id = id;
				Handler = handler;
				Valid = true;
			}
		}
        
		private bool _Stop = true;
		private Queue<Event> _Queue = new Queue<Event>(100);
		private AutoResetEvent _NewEvent;
		private ManualResetEvent _StopEvent;
		private Thread _WorkingThread;

		private void ProcessEvents()
		{
			WaitHandle[] waitHandles = new WaitHandle[] { _StopEvent, _NewEvent };

			while (!_Stop)
			{
				Event ev = null;

				lock (_Queue)
				{
					if (_Queue.Count > 0)
					{
						ev = _Queue.Dequeue();
					}
					else
					{
						_NewEvent.Reset();
					}
				}

				if (ev == null)
				{
					if (WaitHandle.WaitAny(waitHandles) == 0)
					{
						break;
					}
					else
					{
						lock (_Queue)
						{
							Debug.Assert(_Queue.Count > 0);
							ev = _Queue.Dequeue();
						}
					}
				}

				if (ev.Valid)
				{
					try
					{
						ev.Handler();
					}
					catch (Exception ex)
					{
                        throw new Exception("ERROR running " + ev.Id + ": " + ex.Message);
					}
				}
			}
		}

		#endregion
	}
}
