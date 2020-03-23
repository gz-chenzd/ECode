using System;
using System.Threading;

namespace ECode.Core
{
    public class TimerEx : IDisposable
    {
        private Timer       timer       = null;
        private bool        enabled     = false;
        private int         interval    = int.MaxValue;  // ms


        private bool IsDisposed
        { get; set; }

        public bool Enabled
        {
            get
            {
                ThrowIfObjectDisposed();

                return enabled;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (enabled == value)
                { return; }

                if (value)
                { Start(); }
                else
                { Stop(); }
            }
        }

        public int Interval
        {
            get
            {
                ThrowIfObjectDisposed();

                return interval;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(Interval), $"Property '{nameof(Interval)}' value must be > 0.");
                }

                if (interval == value)
                { return; }

                interval = value;

                if (timer != null)
                { timer.Change(interval, interval); }
            }
        }


        public event EventHandler Elapsed;


        public TimerEx()
            : this(int.MaxValue)
        { }

        public TimerEx(int interval)
        {
            if (interval <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(interval), $"Argument '{nameof(interval)}' value must be > 0.");
            }

            this.interval = interval;
        }


        private void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        public void Start()
        {
            ThrowIfObjectDisposed();

            if (enabled)
            { return; }

            lock (this)
            {
                if (enabled)
                { return; }

                enabled = true;

                timer = new Timer((o) =>
                {
                    if (this.Elapsed != null)
                    {
                        this.Elapsed(this, EventArgs.Empty);
                    }
                }, null, interval, interval);
            }
        }

        public void Stop()
        {
            ThrowIfObjectDisposed();

            if (timer == null)
            { return; }

            lock (this)
            {
                if (timer == null)
                { return; }

                enabled = false;

                timer.Dispose();
                timer = null;
            }
        }


        #region IDisposable Implementation

        private bool disposedValue = false; // 要检测冗余调用ArgumentOutOfRangeException

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    Stop();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~TimerEx() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);

            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);

            this.IsDisposed = true;
        }

        #endregion
    }
}