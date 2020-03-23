
namespace ECode.Core
{
    public static class CharExtensions
    {
        /// <summary>
        /// Converts SBC case to DBC case.（全角转半角）
        /// </summary>
        public static char ToDBC(this char ch)
        {
            if (ch == '\u3000')
            { ch = ' '; }

            if (ch > '\uFF00' && ch < '\uFF5F')
            { ch -= '\uFEE0'; }

            return ch;
        }
    }
}
