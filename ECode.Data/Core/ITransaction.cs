
namespace ECode.Data
{
    public interface ITransaction
    {
        string ID { get; }

        bool IsActive { get; }


        void Commit();

        void Rollback();
    }
}
