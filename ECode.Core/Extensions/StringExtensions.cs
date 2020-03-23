using System;
using System.Collections;
using System.Text;
using ECode.Utility;

namespace ECode.Core
{
    public static class StringExtensions
    {
        public static byte[] ToBytes(this string str)
        {
            return ToBytes(str, Encoding.UTF8);
        }

        public static byte[] ToBytes(this string str, Encoding encoding)
        {
            AssertUtil.ArgumentNotNull(encoding, nameof(encoding));

            if (str == null)
            { return new byte[0]; }

            return encoding.GetBytes(str);
        }


        /// <summary>
        /// Tokenize the given string into a string array.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If <paramref name="str"/> is null, returns an empty string array.
        /// </p>
        /// <p>
        /// If <paramref name="delimiters"/> is null or empty, 
        /// returns a string array with one element: <paramref name="str"/> itself.
        /// </p>
        /// </remarks>
        /// <param name="str">The string to tokenize.</param>
        /// <param name="delimiters">
        /// The delimiter characters, assembled as a string.
        /// </param>
        /// <param name="trimTokens">
        /// Trim the tokens via <see cref="System.String.Trim()"/>.
        /// </param>
        /// <param name="ignoreEmptyTokens">
        /// Omit empty tokens from the result array.
        /// </param>
        /// <returns>An array of the tokens.</returns>
        public static string[] Split(this string str, string delimiters, bool trimTokens, bool ignoreEmptyTokens)
        {
            return Split(str, delimiters, trimTokens, ignoreEmptyTokens, null);
        }

        /// <summary>
        /// Tokenize the given string into a string array.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If <paramref name="str"/> is null, returns an empty string array.
        /// </p>
        /// <p>
        /// If <paramref name="delimiters"/> is null or empty,
        ///  returns a string array with one element: <paramref name="str"/> itself.
        /// </p>
        /// </remarks>
        /// <param name="str">The string to tokenize.</param>
        /// <param name="delimiters">
        /// The delimiter characters, assembled as a string.
        /// </param>
        /// <param name="trimTokens">
        /// Trim the tokens via <see cref="System.String.Trim()"/>.
        /// </param>
        /// <param name="ignoreEmptyTokens">
        /// Omit empty tokens from the result array.
        /// </param>
        /// <param name="quoteChars">
        /// Pairs of quote characters. <paramref name="delimiters"/> within a pair of quotes are ignored
        /// </param>
        /// <returns>An array of the tokens.</returns>
        public static string[] Split(this string str, string delimiters, bool trimTokens, bool ignoreEmptyTokens, string quoteChars)
        {
            if (str == null)
            { return new string[0]; }

            if (delimiters == null || delimiters.Length == 0)
            { return new string[] { str }; }

            if (quoteChars == null)
            { quoteChars = string.Empty; }

            if (quoteChars.Length % 2 != 0)
            { throw new ArgumentException($"the number of '{nameof(quoteChars)}' must be even."); }

            var delimiterChars = delimiters.ToCharArray();

            // scan separator positions
            var delimiterPositions = new int[str.Length];
            var count = MarkDelimiterPositions(str, delimiterChars, quoteChars, delimiterPositions);

            int startIndex   = 0;
            var tokens = new ArrayList(count + 1);
            for (int ixSep = 0; ixSep < count; ixSep++)
            {
                var token = str.Substring(startIndex, delimiterPositions[ixSep] - startIndex);

                if (trimTokens)
                {
                    token = token.Trim();
                }

                if (!(ignoreEmptyTokens && token.Length == 0))
                {
                    tokens.Add(token);
                }

                startIndex = delimiterPositions[ixSep] + 1;
            }

            // add remainder            
            if (startIndex < str.Length)
            {
                var token = str.Substring(startIndex);

                if (trimTokens)
                {
                    token = token.Trim();
                }

                if (!(ignoreEmptyTokens && token.Length == 0))
                {
                    tokens.Add(token);
                }
            }
            else if (startIndex == str.Length)
            {
                if (!(ignoreEmptyTokens))
                {
                    tokens.Add(string.Empty);
                }
            }

            return (string[])tokens.ToArray(typeof(string));
        }

        private static int MarkDelimiterPositions(string str, char[] delimiters, string quoteChars, int[] delimiterPositions)
        {
            int  count                  = 0;
            int  quoteNestingDepth      = 0;
            char expectedQuoteOpenChar  = '\0';
            char expectedQuoteCloseChar = '\0';

            for (int ixCurChar = 0; ixCurChar < str.Length; ixCurChar++)
            {
                char curChar = str[ixCurChar];

                for (int ixCurDelim = 0; ixCurDelim < delimiters.Length; ixCurDelim++)
                {
                    if (delimiters[ixCurDelim] == curChar)
                    {
                        if (quoteNestingDepth == 0)
                        {
                            delimiterPositions[count] = ixCurChar;
                            count++;
                            break;
                        }
                    }

                    if (quoteNestingDepth == 0)
                    {
                        // check, if we're facing an opening char
                        for (int ixCurQuoteChar = 0; ixCurQuoteChar < quoteChars.Length; ixCurQuoteChar += 2)
                        {
                            if (quoteChars[ixCurQuoteChar] == curChar)
                            {
                                quoteNestingDepth++;
                                expectedQuoteOpenChar = curChar;
                                expectedQuoteCloseChar = quoteChars[ixCurQuoteChar + 1];
                                break;
                            }
                        }
                    }
                    else
                    {
                        // check if we're facing an expected open or close char
                        if (curChar == expectedQuoteOpenChar)
                        {
                            quoteNestingDepth++;
                        }
                        else if (curChar == expectedQuoteCloseChar)
                        {
                            quoteNestingDepth--;
                        }
                    }
                }
            }

            return count;
        }
    }
}
