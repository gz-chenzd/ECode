
namespace ECode.IO
{
    public enum SizeExceededAction
    {
        /// <summary>
        /// Junks all data what exceeds maximum allowed size.
        /// </summary>
        Junk = 0,

        /// <summary>
        /// Throws exception at once when maximum size exceeded.
        /// </summary>
        ThrowException = 1,
    }
}
