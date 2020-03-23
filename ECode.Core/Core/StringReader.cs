using System;
using System.Text;
using ECode.Utility;

namespace ECode.Core
{
    public class StringReader
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>source</b> is null.</exception>
        public StringReader(string source)
        {
            AssertUtil.ArgumentNotNull(source, nameof(source));

            this.SourceString = source;
        }


        /// <summary>
        /// Appends specified string to SourceString.
        /// </summary>
        /// <param name="value">String value to append.</param>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        public void AppendString(string value)
        {
            AssertUtil.ArgumentNotNull(value, nameof(value));

            this.SourceString += value;
        }


        /// <summary>
        /// Reads to first char, skips white-space(SP,VTAB,HTAB,CR,LF) from the beginning of source string.
        /// </summary>
        /// <returns>Returns white-space chars which was readed.</returns>
        public string ReadToFirstChar()
        {
            int whiteSpaces = 0;
            for (int i = 0; i < this.SourceString.Length; i++)
            {
                if (!char.IsWhiteSpace(this.SourceString[i]))
                { break; }

                whiteSpaces++;
            }

            var whiteSpaceChars = this.SourceString.Substring(0, whiteSpaces);
            this.SourceString = this.SourceString.Substring(whiteSpaces);

            return whiteSpaceChars;
        }


        /// <summary>
        /// Reads string with specified length. Throws exception if read length is bigger than source string length.
        /// </summary>
        /// <param name="length">Number of chars to read.</param>
        public string ReadSpecifiedLength(int length)
        {
            if (this.SourceString.Length < length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"Argument '{nameof(length)}' value exceeds the maximum length of '{nameof(SourceString)}'.");
            }

            var retVal = this.SourceString.Substring(0, length);
            this.SourceString = this.SourceString.Substring(length);

            return retVal;
        }


        /// <summary>
        /// Reads string to specified delimiter or to end of underlying string. Notes: Delimiter in quoted string is skipped.
        /// Delimiter is removed by default.
        /// For example: delimiter = ',', text = '"aaaa,eee",qqqq' - then result is '"aaaa,eee"'.
        /// </summary>
        /// <param name="delimiter">Data delimiter.</param>
        public string QuotedReadToDelimiter(char delimiter)
        {
            return QuotedReadToDelimiter(new char[] { delimiter });
        }

        /// <summary>
		/// Reads string to specified delimiter or to end of underlying string. Notes: Delimiters in quoted string is skipped.
        /// Delimiter is removed by default.
		/// For example: delimiter = ',', text = '"aaaa,eee",qqqq' - then result is '"aaaa,eee"'.
		/// </summary>
		/// <param name="delimiters">Data delimiters.</param>
		public string QuotedReadToDelimiter(char[] delimiters)
        {
            return QuotedReadToDelimiter(delimiters, true);
        }

        /// <summary>
        /// Reads string to specified delimiter or to end of underlying string. Notes: Delimiters in quoted string is skipped. 
        /// For example: delimiter = ',', text = '"aaaa,eee",qqqq' - then result is '"aaaa,eee"'.
        /// </summary>
        /// <param name="delimiters">Data delimiters.</param>
        /// <param name="removeDelimiter">Specifies if delimiter is removed from underlying string.</param>
        public string QuotedReadToDelimiter(char[] delimiters, bool removeDelimiter)
        {
            var     splitBuffer         = new StringBuilder(); // Holds active
            bool    inQuoted            = false;               // Holds flag if position is quoted string or not
            bool    doEscape            = false;

            for (int i = 0; i < this.SourceString.Length; i++)
            {
                char c = this.SourceString[i];

                if (doEscape)
                {
                    splitBuffer.Append(c);
                    doEscape = false;
                }
                else if (c == '\\')
                {
                    splitBuffer.Append(c);
                    doEscape = true;
                }
                else
                {
                    // Start/end quoted string area
                    if (c == '\"')
                    { inQuoted = !inQuoted; }

                    // See if char is delimiter
                    bool isDelimiter = false;
                    foreach (char delimiter in delimiters)
                    {
                        if (c == delimiter)
                        {
                            isDelimiter = true;
                            break;
                        }
                    }

                    // Current char is split char and it isn't in quoted string, do split
                    if (!inQuoted && isDelimiter)
                    {
                        var retVal = splitBuffer.ToString();

                        // Remove readed string + delimiter from source string
                        if (removeDelimiter)
                        {
                            this.SourceString = this.SourceString.Substring(i + 1);
                        }
                        // Remove readed string
                        else
                        {
                            this.SourceString = this.SourceString.Substring(i);
                        }

                        return retVal;
                    }
                    else
                    {
                        splitBuffer.Append(c);
                    }
                }
            }

            // If we reached so far then we are end of string, return it
            this.SourceString = "";
            return splitBuffer.ToString();
        }


        /// <summary>
        /// Reads word from string. Returns null if no word is available.
        /// Word reading begins from first char, for example if SP"text", then space is trimmed.
        /// </summary>
        public string ReadWord()
        {
            return ReadWord(true);
        }

        /// <summary>
		/// Reads word from string. Returns null if no word is available.
		/// Word reading begins from first char, for example if SP"text", then space is trimmed.
		/// </summary>
		/// <param name="unQuote">Specifies if quoted string word is unquoted.</param>
		public string ReadWord(bool unQuote)
        {
            return ReadWord(unQuote, new char[] { ' ', ',', ';', '{', '}', '(', ')', '[', ']', '<', '>', '\r', '\n' }, false);
        }

        /// <summary>
        /// Reads word from string. Returns null if no word is available.
        /// Word reading begins from first char, for example if SP"text", then space is trimmed.
        /// </summary>
        /// <param name="unQuote">Specifies if quoted string word is unquoted.</param>
        /// <param name="wordTerminatorChars">Specifies chars what terminate word.</param>
        /// <param name="removeWordTerminator">Specifies if work terminator is removed.</param>
        public string ReadWord(bool unQuote, char[] wordTerminatorChars, bool removeWordTerminator)
        {
            // Always start word reading from first char.
            ReadToFirstChar();

            if (this.Available == 0)
            { return null; }

            // quoted word can contain any char, " must be escaped with \
            // unqouted word can conatin any char except: SP VTAB HTAB,{}()[]<>

            if (this.SourceString.StartsWith("\"", StringComparison.InvariantCultureIgnoreCase))
            {
                if (unQuote)
                {
                    return StringUtil.UnQuoteString(QuotedReadToDelimiter(wordTerminatorChars, removeWordTerminator));
                }
                else
                {
                    return QuotedReadToDelimiter(wordTerminatorChars, removeWordTerminator);
                }
            }
            else
            {
                int wordLength = 0;
                for (int i = 0; i < this.SourceString.Length; i++)
                {
                    char c = this.SourceString[i];

                    bool isTerminator = false;
                    foreach (char terminator in wordTerminatorChars)
                    {
                        if (c == terminator)
                        {
                            isTerminator = true;
                            break;
                        }
                    }

                    if (isTerminator)
                    { break; }

                    wordLength++;
                }

                var retVal = this.SourceString.Substring(0, wordLength);
                if (removeWordTerminator)
                {
                    if (this.SourceString.Length >= wordLength + 1)
                    {
                        this.SourceString = this.SourceString.Substring(wordLength + 1);
                    }
                }
                else
                {
                    this.SourceString = this.SourceString.Substring(wordLength);
                }

                return retVal;
            }
        }


        /// <summary>
        /// Reads parenthesized value. Supports {},(),[],&lt;&gt; parenthesis. 
        /// Throws exception if there isn't parenthesized value or closing parenthesize is missing.
        /// </summary>
        public string ReadParenthesized()
        {
            ReadToFirstChar();

            char startingChar = ' ';
            char closingChar  = ' ';

            if (this.SourceString.StartsWith("{", StringComparison.InvariantCultureIgnoreCase))
            {
                startingChar = '{';
                closingChar = '}';
            }
            else if (this.SourceString.StartsWith("(", StringComparison.InvariantCultureIgnoreCase))
            {
                startingChar = '(';
                closingChar = ')';
            }
            else if (this.SourceString.StartsWith("[", StringComparison.InvariantCultureIgnoreCase))
            {
                startingChar = '[';
                closingChar = ']';
            }
            else if (this.SourceString.StartsWith("<", StringComparison.InvariantCultureIgnoreCase))
            {
                startingChar = '<';
                closingChar = '>';
            }
            else
            { throw new Exception("No parenthesized value '" + this.SourceString + "' !"); }

            bool inQuotedString = false; // Holds flag if position is quoted string or not
            bool skipNextChar   = false;

            int closingCharIndex = -1;
            int nestedStartingCharCounter = 0;
            for (int i = 1; i < this.SourceString.Length; i++)
            {
                // Skip this char.
                if (skipNextChar)
                {
                    skipNextChar = false;
                }
                // We have char escape '\', skip next char.
                else if (this.SourceString[i] == '\\')
                {
                    skipNextChar = true;
                }
                // Start/end quoted string area
                else if (this.SourceString[i] == '\"')
                {
                    inQuotedString = !inQuotedString;
                }
                // We need to skip parenthesis in quoted string
                else if (!inQuotedString)
                {
                    // There is nested parenthesis
                    if (this.SourceString[i] == startingChar)
                    {
                        nestedStartingCharCounter++;
                    }
                    // Closing char
                    else if (this.SourceString[i] == closingChar)
                    {
                        // There isn't nested parenthesis closing chars left, this is closing char what we want
                        if (nestedStartingCharCounter == 0)
                        {
                            closingCharIndex = i;
                            break;
                        }
                        // This is nested parenthesis closing char
                        else
                        {
                            nestedStartingCharCounter--;
                        }
                    }
                }
            }

            if (closingCharIndex == -1)
            {
                throw new Exception("There is no closing parenthesize for '" + this.SourceString + "' !");
            }
            else
            {
                var retVal = this.SourceString.Substring(1, closingCharIndex - 1);
                this.SourceString = this.SourceString.Substring(closingCharIndex + 1);

                return retVal;
            }
        }


        /// <summary>
        /// Reads all remaining string, returns null if no chars left to read.
        /// </summary>
        public string ReadToEnd()
        {
            if (this.Available == 0)
            { return null; }

            var retVal = this.SourceString;
            this.SourceString = "";

            return retVal;
        }


        /// <summary>
        /// Removes specified count of chars from the end of the source string.
        /// </summary>
        /// <param name="count">Char count.</param>
        /// <exception cref="System.ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void RemoveFromEnd(int count)
        {
            if (count < 0)
            { throw new ArgumentException($"Argument '{nameof(count)}' value must be >= 0."); }

            this.SourceString = this.SourceString.Substring(0, this.SourceString.Length - count);
        }


        /// <summary>
		/// Gets if source string starts with specified value. Compare is case-sensitive.
		/// </summary>
		/// <param name="value">Start string value.</param>
		/// <returns>Returns true if source string starts with specified value.</returns>
		public bool StartsWith(string value)
        {
            return this.SourceString.StartsWith(value, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Gets if source string starts with specified value.
        /// </summary>
        /// <param name="value">Start string value.</param>
        /// <param name="case_sensitive">Specifies if compare is case-sensitive.</param>
        /// <returns>Returns true if source string starts with specified value.</returns>
        public bool StartsWith(string value, bool case_sensitive)
        {
            if (case_sensitive)
            {
                return this.SourceString.StartsWith(value, StringComparison.InvariantCulture);
            }
            else
            {
                return this.SourceString.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);
            }
        }


        /// <summary>
        /// Gets if source string ends with specified value. Compare is case-sensitive.
        /// </summary>
        /// <param name="value">Start string value.</param>
        /// <returns>Returns true if source string ends with specified value.</returns>
        public bool EndsWith(string value)
        {
            return this.SourceString.EndsWith(value, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Gets if source string ends with specified value.
        /// </summary>
        /// <param name="value">Start string value.</param>
        /// <param name="case_sensitive">Specifies if compare is case-sensitive.</param>
        /// <returns>Returns true if source string ends with specified value.</returns>
        public bool EndsWith(string value, bool case_sensitive)
        {
            if (case_sensitive)
            {
                return this.SourceString.EndsWith(value, StringComparison.InvariantCulture);
            }
            else
            {
                return this.SourceString.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);
            }
        }


        /// <summary>
        /// Gets if current source string starts with word. For example if source string starts with
        /// whiter space or parenthesize, this method returns false.
        /// </summary>
        /// <returns></returns>
        public bool StartsWithWord()
        {
            if (this.SourceString.Length == 0)
            {
                return false;
            }

            if (char.IsWhiteSpace(this.SourceString[0]))
            {
                return false;
            }

            if (char.IsSeparator(this.SourceString[0]))
            {
                return false;
            }

            char[] wordTerminators = new char[] { ' ', ',', ';', '{', '}', '(', ')', '[', ']', '<', '>', '\r', '\n' };
            foreach (char c in wordTerminators)
            {
                if (c == this.SourceString[0])
                {
                    return false;
                }
            }

            return true;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets how many chars are available for reading.
        /// </summary>
        public long Available
        {
            get { return this.SourceString.Length; }
        }

        /// <summary>
        /// Gets currently remaining string.
        /// </summary>
        public string SourceString
        { get; private set; }

        #endregion
    }
}
