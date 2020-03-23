
namespace ECode.EventFramework
{
    public static class EventExtensions
    {
        public static void RaiseEvent(this object sender, string name, object data = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            { return; }

            EventCore.RaiseEvent(sender, new EventEventArgs(name, data));
        }
    }
}