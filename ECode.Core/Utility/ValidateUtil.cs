using System.Net;
using System.Net.Mail;

namespace ECode.Utility
{
    public static class ValidateUtil
    {
        /// <summary>
        /// Gets if the specified string is ASCII string.
        /// </summary>
        /// <param name="value">String value.</param>
        /// <returns>Returns true if specified string is ASCII string, otherwise false.</returns>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        public static bool IsAscii(string value)
        {
            AssertUtil.ArgumentNotNull(value, nameof(value));

            foreach (char c in value)
            {
                if (c > 127)
                { return false; }
            }

            return true;
        }

        /// <summary>
        /// Checks if specified string is integer(int/long).
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns true if specified string is integer.</returns>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        public static bool IsInteger(string value)
        {
            AssertUtil.ArgumentNotEmpty(value, nameof(value));

            return long.TryParse(value, out long l);
        }

        /// <summary>
        /// Gets if the specified string value is IP address.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns true if specified value is IP address.</returns>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        public static bool IsIPAddress(string value)
        {
            AssertUtil.ArgumentNotEmpty(value, nameof(value));

            return IPAddress.TryParse(value, out IPAddress ip);
        }

        /// <summary>
        /// Gets if specified mail address has valid syntax.
        /// </summary>
        /// <param name="value">mail address, eg. ivar@lumisoft.ee.</param>
        /// <returns>Returns ture if address is valid, otherwise false.</returns>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        public static bool IsMailAddress(string value)
        {
            AssertUtil.ArgumentNotEmpty(value, nameof(value));

            try
            {
                var mailAddr = new MailAddress(value);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}