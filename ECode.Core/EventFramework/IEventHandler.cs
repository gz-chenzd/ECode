
namespace ECode.EventFramework
{
    public interface IEventHandler
    {
        void Process(object sender, EventEventArgs e);
    }
}