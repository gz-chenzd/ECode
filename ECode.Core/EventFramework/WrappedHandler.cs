using System;

namespace ECode.EventFramework
{
    class WrappedHandler
    {
        private readonly Type   handlerType     = null;
        private IEventHandler   handler         = null;


        public Type Type
        {
            get { return handlerType; }
        }


        public WrappedHandler(Type handlerType)
        {
            this.handlerType = handlerType;
        }


        public void Process(object sender, EventEventArgs e)
        {
            if (handler == null)
            {
                lock (this)
                {
                    if (handler == null)
                    {
                        handler = Activator.CreateInstance(handlerType) as IEventHandler;
                    }
                }
            }

            handler.Process(sender, e);
        }
    }
}