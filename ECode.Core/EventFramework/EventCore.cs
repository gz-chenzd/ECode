using System;
using System.Collections.Generic;
using ECode.Logging;

namespace ECode.EventFramework
{
    class EventCore
    {
        static readonly Logger  Log     = LogManager.GetLogger("EventFramework");

        static readonly Dictionary<string, List<WrappedHandler>>    HandlersByEvent
            = new Dictionary<string, List<WrappedHandler>>(StringComparer.InvariantCultureIgnoreCase);


        public static void RegisterHandler(string eventName, WrappedHandler handler)
        {
            if (!HandlersByEvent.ContainsKey(eventName))
            {
                HandlersByEvent[eventName] = new List<WrappedHandler>();
            }

            HandlersByEvent[eventName].Add(handler);
            Log.Debug($"Register handler '{handler.Type}' for event '{eventName}'.");
        }

        public static void RaiseEvent(object sender, EventEventArgs e)
        {
            if (HandlersByEvent.TryGetValue(e.Name, out List<WrappedHandler> handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        Log.Debug($"Invoke handler '{handler.Type}' for event '{e.Name}'.");

                        handler.Process(sender, e);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Handler '{handler}' throws exception while handling event '{e.Name}'.", ex);
                    }
                }
            }
        }
    }
}