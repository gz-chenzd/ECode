using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECode.Utility
{
    public static class StringUtil
    {
        /// <summary>
        /// Gets if specified string is valid "token" value.
        /// </summary>
        /// <param name="text">String value to check.</param>
        /// <returns>Returns true if specified string value is valid "token" value.</returns>
        /// <exception cref="ArgumentNullException">Is raised if <b>value</b> is null.</exception>
        public static bool IsToken(string text)
        {
            AssertUtil.ArgumentNotEmpty(text, nameof(text));

            /* This syntax is taken from rfc 3261, but token must be universal so ... .
                token    =  1*(alphanum / "-" / "." / "!" / "%" / "*" / "_" / "+" / "`" / "'" / "~" )
                alphanum = ALPHA / DIGIT
                ALPHA    =  %x41-5A / %x61-7A   ; A-Z / a-z
                DIGIT    =  %x30-39             ; 0-9
            */

            var tokenChars = new char[] { '-', '.', '!', '%', '*', '_', '+', '`', '\'', '~' };
            foreach (char c in text)
            {
                // We don't have letter or digit, so we only may have token char.
                if (!((c >= 0x41 && c <= 0x5A) || (c >= 0x61 && c <= 0x7A) || (c >= 0x30 && c <= 0x39)))
                {
                    if (!tokenChars.Contains(c))
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary> 
        /// Match a String against the given pattern, supporting the following simple
        /// pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
        /// </summary>
        /// <param name="pattern">
        /// the pattern to match against
        /// </param>
        /// <param name="text">
        /// the String to match
        /// </param>
        /// <returns> 
        /// whether the String matches the given pattern
        /// </returns>
        public static bool IsMatch(string pattern, string text)
        {
            AssertUtil.ArgumentNotEmpty(pattern, nameof(pattern));
            AssertUtil.ArgumentNotEmpty(text, nameof(text));

            if (string.Equals("*", pattern, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (pattern.StartsWith("*", StringComparison.InvariantCultureIgnoreCase)
                && pattern.EndsWith("*", StringComparison.InvariantCultureIgnoreCase))
            {
                pattern = pattern.Trim('*');
                return text.IndexOf(pattern, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            else if (pattern.StartsWith("*", StringComparison.InvariantCultureIgnoreCase))
            {
                pattern = pattern.TrimStart('*');
                return text.EndsWith(pattern, StringComparison.InvariantCultureIgnoreCase);
            }
            else if (pattern.EndsWith("*", StringComparison.InvariantCultureIgnoreCase))
            {
                pattern = pattern.TrimEnd('*');
                return text.StartsWith(pattern, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        /// <summary> 
        /// Match a String against the given patterns, supporting the following simple
        /// pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
        /// </summary>
        /// <param name="patterns">
        /// the patterns to match against
        /// </param>
        /// <param name="text">
        /// the String to match
        /// </param>
        /// <returns> 
        /// whether the String matches any of the given patterns
        /// </returns>
        public static bool IsMatch(string[] patterns, string text)
        {
            AssertUtil.ArgumentNotEmpty(patterns, nameof(patterns));
            AssertUtil.ArgumentNotEmpty(text, nameof(text));

            for (int i = 0; i < patterns.Length; i++)
            {
                if (IsMatch(patterns[i], text))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> 
        /// Match a String against the given pattern, supporting the following simple
        /// pattern styles: "*xxx*xxx" and "xxx*xxx..." matches, as well as direct equality.
        /// </summary>
        /// <param name="pattern">
        /// the pattern to match against
        /// </param>
        /// <param name="text">
        /// the String to match
        /// </param>
        /// <returns> 
        /// whether the String matches the given pattern
        /// </returns>
        public static bool IsAstericMatch(string pattern, string text)
        {
            AssertUtil.ArgumentNotEmpty(pattern, nameof(pattern));
            AssertUtil.ArgumentNotEmpty(text, nameof(text));

            while (pattern.Length > 0)
            {
                // *xxx[*xxx...]
                if (pattern.StartsWith("*", StringComparison.InvariantCultureIgnoreCase))
                {
                    // *xxx*xxx
                    var patternParts = pattern.TrimStart('*').Split('*', 2);
                    if (patternParts.Length == 2)
                    {
                        int indexPos = text.IndexOf(patternParts[0], StringComparison.InvariantCultureIgnoreCase);
                        if (indexPos == -1)
                        {
                            return false;
                        }

                        pattern = patternParts[1];
                        text = text.Substring(indexPos + patternParts[0].Length);
                    }
                    // *xxx   This is last pattern  
                    else
                    {
                        return text.EndsWith(patternParts[0], StringComparison.InvariantCultureIgnoreCase);
                    }
                }
                // xxx*[xxx...]
                else if (pattern.IndexOf('*') > -1)
                {
                    var patternParts = pattern.Split('*', 2);
                    if (patternParts.Length == 2)
                    {
                        // Text must startwith
                        if (!text.StartsWith(patternParts[0], StringComparison.InvariantCultureIgnoreCase))
                        {
                            return false;
                        }

                        pattern = patternParts[1];
                        text = text.Substring(patternParts[0].Length);
                    }
                    // xxx*   This is last pattern  
                    else
                    {
                        return text.StartsWith(patternParts[0], StringComparison.InvariantCultureIgnoreCase);
                    }
                }
                // xxx
                else
                {
                    return text == pattern;
                }
            }

            return true;
        }


        /// <summary>
        /// Qoutes string and escapes fishy('\',"') chars.
        /// </summary>
        /// <param name="text">Text to quote.</param>
        public static string QuoteString(string text)
        {
            if (text == null)
            { return null; }

            // String is already quoted-string.
            if (text.StartsWith("\"", StringComparison.InvariantCultureIgnoreCase)
                && text.EndsWith("\"", StringComparison.InvariantCultureIgnoreCase))
            {
                return text;
            }

            StringBuilder retVal = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\\')
                {
                    retVal.Append("\\\\");
                }
                else if (c == '\"')
                {
                    retVal.Append("\\\"");
                }
                else
                {
                    retVal.Append(c);
                }
            }

            return "\"" + retVal.ToString() + "\"";
        }

        /// <summary>
        /// Unquotes and unescapes escaped chars specified text. 
        /// For example "xxx" will become to 'xxx', "escaped quote \"", will become to escaped 'quote "'.
        /// </summary>
        /// <param name="text">Text to unquote.</param>
        public static string UnQuoteString(string text)
        {
            if (text == null)
            { return null; }

            int startPosInText = 0;
            int endPosInText   = text.Length;

            //--- Trim. We can't use standard string.Trim(), it's slow. ----//
            for (int i = 0; i < endPosInText; i++)
            {
                char c = text[i];
                if (c == ' ' || c == '\t')
                {
                    startPosInText++;
                }
                else
                {
                    break;
                }
            }

            for (int i = endPosInText - 1; i > 0; i--)
            {
                char c = text[i];
                if (c == ' ' || c == '\t')
                {
                    endPosInText--;
                }
                else
                {
                    break;
                }
            }
            //--------------------------------------------------------------//

            // All text trimmed
            if ((endPosInText - startPosInText) <= 0)
            { return ""; }

            // Remove starting and ending quotes.         
            if (text[startPosInText] == '\"')
            {
                startPosInText++;
            }

            if (text[endPosInText - 1] == '\"')
            {
                endPosInText--;
            }

            // Just '"'
            if (endPosInText == startPosInText - 1)
            { return ""; }

            var chars = new char[endPosInText - startPosInText];

            int posInChars = 0;
            bool charIsEscaped = false;
            for (int i = startPosInText; i < endPosInText; i++)
            {
                char c = text[i];

                // Escaping char
                if (!charIsEscaped && c == '\\')
                {
                    charIsEscaped = true;
                }
                // Escaped char
                else if (charIsEscaped)
                {
                    // TODO: replace \n,\r,\t,\v ???
                    chars[posInChars] = c;
                    posInChars++;
                    charIsEscaped = false;
                }
                // Normal char
                else
                {
                    chars[posInChars] = c;
                    posInChars++;
                    charIsEscaped = false;
                }
            }

            return new string(chars, 0, posInChars);
        }


        /// <summary>
        /// Escapes specified chars in the specified string.
        /// </summary>
        /// <param name="text">Text to escape.</param>
        /// <param name="charsToEscape">Chars to escape.</param>
        public static string EscapeString(string text, char[] charsToEscape)
        {
            if (text == null)
            { return null; }

            // Create worst scenario buffer, assume all chars must be escaped
            var buffer = new char[text.Length * 2];
            int nChars = 0;
            foreach (char c in text)
            {
                if (charsToEscape.Contains(c))
                {
                    buffer[nChars++] = '\\';
                }

                buffer[nChars++] = c;
            }

            return new string(buffer, 0, nChars);
        }

        /// <summary>
        /// Unescapes all escaped chars.
        /// </summary>
        /// <param name="text">Text to unescape.</param>
        public static string UnEscapeString(string text)
        {
            if (text == null)
            { return null; }

            // Create worst scenarion buffer, non of the chars escaped.
            var buffer = new char[text.Length];
            int nChars = 0;
            bool escapedCahr = false;
            foreach (char c in text)
            {
                if (!escapedCahr && c == '\\')
                {
                    escapedCahr = true;
                }
                else
                {
                    buffer[nChars++] = c;
                    escapedCahr = false;
                }
            }

            return new string(buffer, 0, nChars);
        }


        /// <summary>
        /// Splits string into string arrays. This split method won't split qouted strings, but only text outside of qouted string.
        /// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
        /// </summary>
        /// <param name="text">Text to split.</param>
        /// <param name="splitChar">Char that splits text.</param>
        public static string[] SplitQuotedString(string text, char splitChar)
        {
            return SplitQuotedString(text, splitChar, false);
        }

        /// <summary>
        /// Splits string into string arrays. This split method won't split qouted strings, but only text outside of qouted string.
        /// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
        /// </summary>
        /// <param name="text">Text to split.</param>
        /// <param name="splitChar">Char that splits text.</param>
        /// <param name="unquote">If true, splitted parst will be unqouted if they are qouted.</param>
        public static string[] SplitQuotedString(string text, char splitChar, bool unquote)
        {
            return SplitQuotedString(text, splitChar, unquote, int.MaxValue);
        }

        /// <summary>
        /// Splits string into string arrays. This split method won't split qouted strings, but only text outside of qouted string.
        /// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
        /// </summary>
        /// <param name="text">Text to split.</param>
        /// <param name="splitChar">Char that splits text.</param>
        /// <param name="unquote">If true, splitted parst will be unqouted if they are qouted.</param>
        /// <param name="count">Maximum number of substrings to return.</param>
        /// <returns>Returns splitted string.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>text</b> is null reference.</exception>
        public static string[] SplitQuotedString(string text, char splitChar, bool unquote, int count)
        {
            AssertUtil.ArgumentNotNull(text, nameof(text));

            var     splitParts      = new List<string>();  // Holds splitted parts
            int     startPos        = 0;
            bool    inQuotedString  = false;               // Holds flag if position is quoted string or not
            char    lastChar        = '0';

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // We have exceeded maximum allowed splitted parts.
                if ((splitParts.Count + 1) >= count)
                {
                    break;
                }

                // We have quoted string start/end.
                if (lastChar != '\\' && c == '\"')
                {
                    inQuotedString = !inQuotedString;
                }
                // We have escaped or normal char.
                //else{

                // We igonre split char in quoted-string.
                if (!inQuotedString)
                {
                    // We have split char, do split.
                    if (c == splitChar)
                    {
                        if (unquote)
                        {
                            splitParts.Add(UnQuoteString(text.Substring(startPos, i - startPos)));
                        }
                        else
                        {
                            splitParts.Add(text.Substring(startPos, i - startPos));
                        }

                        // Store new split part start position.
                        startPos = i + 1;
                    }
                }
                //else{

                lastChar = c;
            }

            // Add last split part to splitted parts list
            if (unquote)
            {
                splitParts.Add(UnQuoteString(text.Substring(startPos, text.Length - startPos)));
            }
            else
            {
                splitParts.Add(text.Substring(startPos, text.Length - startPos));
            }

            return splitParts.ToArray();
        }


        /// <summary>
        /// Gets first index of specified char. The specified char in quoted string is skipped.
        /// Returns -1 if specified char doesn't exist.
        /// </summary>
        /// <param name="text">Text in what to check.</param>
        /// <param name="indexChar">Char what index to get.</param>
        public static int QuotedIndexOf(string text, char indexChar)
        {
            AssertUtil.ArgumentNotNull(text, nameof(text));

            int  retVal         = -1;
            bool inQuotedString = false; // Holds flag if position is quoted string or not          
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\"')
                {
                    // Start/end quoted string area
                    inQuotedString = !inQuotedString;
                }

                // Current char is what index we want and it isn't in quoted string, return it's index
                if (!inQuotedString && c == indexChar)
                {
                    return i;
                }
            }

            return retVal;
        }
    }
}