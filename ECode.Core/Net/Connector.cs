using System;
using ECode.Core;

namespace ECode.Net
{
    public abstract class Connector : IDisposable
    {
        internal string ID { get; } = ObjectId.NewId();

        internal ConnectionPool Owner { get; set; }

        internal bool IsLoanedOut { get; set; } = true;

        internal DateTime LoanedOutTime { get; set; } = DateTime.Now;

        internal DateTime ReturnedTime { get; set; } = DateTime.MinValue;


        public abstract bool IsDisposed { get; }

        public abstract bool IsConnected { get; }

        public abstract DateTime ConnectTime { get; }

        public abstract DateTime LastActivity { get; }

        public abstract int ReadWriteTimeout { get; set; }


        public void Dispose()
        {
            if (this.Owner != null)
            {
                this.ReturnToPool();
            }
            else
            {
                this.OnDispose();
            }
        }

        private void ReturnToPool()
        {
            this.Owner.ReturnToPool(this);
        }

        protected virtual void OnDispose()
        {

        }


        public abstract void Connect(string host, int port, bool ssl);
    }
}
