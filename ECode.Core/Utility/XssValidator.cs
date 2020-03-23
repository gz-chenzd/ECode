using System;
using System.Collections.Generic;
using System.IO;

namespace ECode.Utility
{
    public static class XssValidator
    {
        static Dictionary<string, HashSet<string>>  whiteList   = new Dictionary<string, HashSet<string>>() {
            { "a", new HashSet<string>(new string[] { "target", "href", "title", "style", "class", "id" }) },
            { "abbr", new HashSet<string>(new string[] { "title", "style", "class", "id" }) },
            { "address", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "area", new HashSet<string>(new string[] {"shape", "coords", "href", "alt", "style", "class", "id" }) },
            { "article", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "aside", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "audio", new HashSet<string>(new string[] { "autoplay", "controls", "loop", "preload", "src", "style", "class", "id" }) },
            { "b", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "bdi", new HashSet<string>(new string[] { "dir" }) },
            { "bdo", new HashSet<string>(new string[] { "dir" }) },
            { "big", new HashSet<string>(new string[] {  }) },
            { "blockquote", new HashSet<string>(new string[] { "cite", "style", "class", "id" }) },
            { "br", new HashSet<string>(new string[] {  }) },
            { "caption", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "center", new HashSet<string>(new string[] {  }) },
            { "cite", new HashSet<string>(new string[] {  }) },
            { "code", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "col", new HashSet<string>(new string[] { "align", "valign", "span", "width", "style", "class", "id" }) },
            { "colgroup", new HashSet<string>(new string[] { "align", "valign", "span", "width", "style", "class", "id" }) },
            { "dd", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "del", new HashSet<string>(new string[] { "datetime", "style", "class", "id" }) },
            { "details", new HashSet<string>(new string[] { "open", "style", "class", "id" }) },
            { "div", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "dl", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "dt", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "em", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "embed", new HashSet<string>(new string[] { "style", "class", "id", "_url", "type", "pluginspage", "src", "width", "height", "wmode", "play", "loop", "menu", "allowscriptaccess", "allowfullscreen" }) },
            { "font", new HashSet<string>(new string[] { "color", "size", "face", "style", "class", "id" }) },
            { "footer", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "h1", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "h2", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "h3", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "h4", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "h5", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "h6", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "header", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "hr", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "i", new HashSet<string>(new string[] { "style", "class", "id" }) },
            //{ "iframe", new HashSet<string>(new string[] { "style", "class", "id", "src", "frameborder", "data-latex" }) },
            { "img", new HashSet<string>(new string[] { "src", "alt", "title", "width", "height", "style", "class", "id", "_url" }) },
            { "ins", new HashSet<string>(new string[] { "datetime", "style", "class", "id" }) },
            { "li", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "mark", new HashSet<string>(new string[] {  }) },
            { "nav", new HashSet<string>(new string[] {  }) },
            { "ol", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "p", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "pre", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "s", new HashSet<string>(new string[] {  }) },
            { "section", new HashSet<string>(new string[] {  }) },
            { "small", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "span", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "sub", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "sup", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "strong", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "table", new HashSet<string>(new string[] { "width", "border", "align", "valign", "style", "class", "id" }) },
            { "tbody", new HashSet<string>(new string[] { "align", "valign", "style", "class", "id" }) },
            { "td", new HashSet<string>(new string[] { "width", "rowspan", "colspan", "align", "valign", "style", "class", "id" }) },
            { "tfoot", new HashSet<string>(new string[] { "align", "valign", "style", "class", "id" }) },
            { "th", new HashSet<string>(new string[] { "width", "rowspan", "colspan", "align", "valign", "style", "class", "id" }) },
            { "thead", new HashSet<string>(new string[] { "align", "valign", "style", "class", "id" }) },
            { "tr", new HashSet<string>(new string[] { "rowspan", "align", "valign", "style", "class", "id" }) },
            { "tt", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "u", new HashSet<string>(new string[] {  }) },
            { "ul", new HashSet<string>(new string[] { "style", "class", "id" }) },
            { "svg", new HashSet<string>(new string[] { "style", "class", "id", "width", "height", "xmlns", "fill", "viewBox" }) },
            { "video", new HashSet<string>(new string[] { "autoplay", "controls", "loop", "preload", "src", "height", "width", "style", "class", "id" }) },
        };

        public static Dictionary<string, HashSet<string>> WhiteList
        {
            get { return whiteList; }

            set
            {
                if (value == null)
                { throw new ArgumentNullException(nameof(WhiteList)); }

                whiteList = value;
            }
        }


        private static bool IsWhiteSpace(int ch)
        {
            return char.IsWhiteSpace((char)ch);
        }

        private static void IgnoreWhiteSpace(TextReader reader, TextWriter writer, bool ignoreWrite = true)
        {
            int ch = -1;
            while ((ch = reader.Peek()) != -1)
            {
                if (!IsWhiteSpace(ch))
                { break; }

                if (!ignoreWrite)
                { writer.Write((char)ch); }

                reader.Read();
            }
        }

        private static void IgnoreNotTag(TextReader reader, TextWriter writer, bool ignoreWrite = true)
        {
            int ch = -1;
            while ((ch = reader.Peek()) != -1)
            {
                if (ch == '<' || ch == '>')
                { break; }

                if (!ignoreWrite)
                { writer.Write((char)ch); }

                reader.Read();
            }
        }

        private static void IgnoreComment(TextReader reader)
        {
            int index = -1;
            int[] buffer = new int[2];

            int ch = -1;
            while ((ch = reader.Read()) != -1)
            {
                if (ch == '>')
                {
                    if (buffer[0] == '-' && buffer[1] == '-')
                    { return; }
                }

                buffer[++index % 2] = ch;
            }

            throw new FormatException("Invalid incomplete html block.");
        }

        private static string ReadTagName(TextReader reader)
        {
            var chars = new List<char>();

            int ch = -1;
            while ((ch = reader.Peek()) != -1)
            {
                if (IsWhiteSpace(ch) || ch == '/' || ch == '>')
                { return new string(chars.ToArray()); }

                chars.Add((char)ch);

                reader.Read();
            }

            throw new FormatException("Invalid incomplete html block.");
        }

        private static string ReadAttrName(TextReader reader)
        {
            var chars = new List<char>();

            int ch = -1;
            while ((ch = reader.Peek()) != -1)
            {
                if (IsWhiteSpace(ch) || ch == '=' || ch == '/' || ch == '>')
                { return new string(chars.ToArray()); }

                chars.Add((char)ch);

                reader.Read();
            }

            throw new FormatException("Invalid incomplete html block.");
        }

        private static string ReadAttrValue(TextReader reader)
        {
            int separator = ' ';
            var chars = new List<char>();

            int ch = reader.Read();
            if (ch == '"')
            { separator = '"'; }
            else if (ch == '\'')
            { separator = '\''; }

            chars.Add((char)ch);

            while ((ch = reader.Peek()) != -1)
            {
                if (separator == ' ')
                {
                    if (IsWhiteSpace(ch) || ch == '/' || ch == '>')
                    { return new string(chars.ToArray()); }

                    chars.Add((char)ch);
                }
                else
                {
                    chars.Add((char)ch);

                    if (ch == separator)
                    {
                        reader.Read();  // ignore separator
                        return new string(chars.ToArray());
                    }
                }

                reader.Read();
            }

            throw new FormatException("Invalid incomplete html block.");
        }


        private static bool CheckTagName(string tagName)
        {
            return whiteList.ContainsKey(tagName.ToLower());
        }

        private static bool CheckAttrName(string tagName, string attrName)
        {
            var attrList = whiteList[tagName.ToLower()];
            if (attrList == null)
            { return false; }

            return attrList.Contains(attrName.ToLower());
        }


        public static bool Validate(string html)
        {
            var ch              = -1;
            var isTagClosed     = false;
            var tagName         = string.Empty;
            var attrName        = string.Empty;
            var attrValue       = string.Empty;
            var nestedTags      = new Stack<string>();

            using (var reader = new StringReader(html))
            using (var writer = new StringWriter())
            {
                while (true)
                {
                    IgnoreNotTag(reader, writer, true);

                    ch = reader.Read();  // '<'
                    if (ch == -1)
                    { break; }
                    else if (ch == '>')
                    { throw new FormatException("Invalid html tag '>'."); }

                    ch = reader.Peek();
                    if (ch == -1)
                    { throw new FormatException("Invalid incomplete html block."); }
                    else if (ch == '/')  // '</xxxxx'
                    {
                        reader.Read();  // ignore '/'
                        isTagClosed = true;
                    }
                    else if (ch == '!')  // '<!xxxxx' or  '<!DOCTYPE'
                    {
                        reader.Read();  // ignore '!'

                        ch = reader.Peek();
                        if (ch == -1)
                        { throw new FormatException("Invalid incomplete html block."); }
                        else if (ch == '-')  // '<!--'
                        {
                            reader.Read();  // ignore '-'

                            ch = reader.Read();
                            if (ch == -1)
                            { throw new FormatException("Invalid incomplete html block."); }
                            else if (ch != '-')
                            { throw new FormatException($"Cannot parse comment '<!-{(char)ch}'."); }

                            IgnoreComment(reader);
                            continue;
                        }
                        else if (ch == 'd' || ch == 'D')  // '<!DOCTYPE'
                        {
                            var doctype = ReadTagName(reader);
                            if (!doctype.Equals("DOCTYPE", StringComparison.InvariantCultureIgnoreCase))
                            { throw new FormatException($"Cannot parse tag '<!{doctype}'."); }

                            var inQuoted        = false;
                            var quotedSeparator = -1;
                            while ((ch = reader.Read()) != -1)  // ignored until '>'
                            {
                                if (inQuoted)
                                {
                                    if (quotedSeparator != ch)
                                    { continue; }

                                    inQuoted = false;
                                    continue;
                                }
                                else if (ch == '"' || ch == '\'')
                                {
                                    inQuoted = true;
                                    quotedSeparator = ch;
                                    continue;
                                }
                                else if (ch == '>')
                                { break; }
                            }

                            if (ch == -1)
                            { throw new FormatException("Invalid incomplete html block."); }

                            continue;
                        }
                        else
                        { throw new FormatException($"Cannot parse tag '<!{(char)ch}'."); }
                    }

                    tagName = ReadTagName(reader);
                    if (!CheckTagName(tagName))
                    { throw new InvalidDataException($"Tag '{tagName}' isnot in the white list."); }

                    if (!isTagClosed)
                    {
                        nestedTags.Push(tagName);

                        while (true)
                        {
                            IgnoreWhiteSpace(reader, writer, true);
                            if (reader.Peek() == -1)
                            { throw new FormatException("Invalid incomplete html block."); }

                            attrName = ReadAttrName(reader);
                            if (string.IsNullOrWhiteSpace(attrName))
                            { break; }

                            IgnoreWhiteSpace(reader, writer, true);
                            if (reader.Peek() == -1)
                            { throw new FormatException("Invalid incomplete html block."); }

                            attrValue = null;

                            ch = reader.Peek();
                            if (ch == '=')
                            {
                                reader.Read();  // ignore '='
                                IgnoreWhiteSpace(reader, writer, true);
                                if (reader.Peek() == -1)
                                { throw new FormatException("Invalid incomplete html block."); }

                                attrValue = ReadAttrValue(reader);
                                if (string.IsNullOrWhiteSpace(attrValue))
                                { throw new FormatException($"Invalid attribute set '{attrName}='."); }
                                else if (attrValue.Contains("\\"))
                                { throw new FormatException($"Invalid attribute set '{attrName}={attrValue}'."); }
                            }

                            if (!CheckAttrName(tagName, attrName))
                            { throw new InvalidDataException($"Attribute '{attrName}' in tag '{tagName}' isnot in the white list."); }
                        }

                        ch = reader.Read();
                        if (ch == -1)
                        { throw new FormatException("Invalid incomplete html block."); }

                        if (ch == '/')
                        {
                            ch = reader.Read();
                            if (ch == -1)
                            { throw new FormatException("Invalid incomplete html block."); }
                            else if (ch != '>')
                            { throw new FormatException($"Cannot parse tag '/{(char)ch}'."); }

                            nestedTags.Pop();
                        }
                        else if (ch == '>')
                        { continue; }
                        else
                        { continue;  /* cannot reach */ }
                    }
                    else  // '</xxxx>'
                    {
                        isTagClosed = false;

                        ch = reader.Read();
                        if (ch == -1)
                        { throw new FormatException("Invalid incomplete html block."); }
                        else if (ch != '>')
                        { throw new FormatException($"Cannot parse tag '</{tagName}{(char)ch}'."); }

                        if (tagName.Equals(nestedTags.Peek(), StringComparison.InvariantCultureIgnoreCase))
                        { nestedTags.Pop(); }
                        else
                        { throw new FormatException($"Invalid nested tag '</{tagName}>'."); }
                    }
                }

                if (nestedTags.Count > 0)
                { throw new FormatException("Invalid html: Contains unpaired tags."); }

                return true;
            }
        }

        public static string Filter(string html)
        {
            var ch              = -1;
            var isTagClosed     = false;
            var tagName         = string.Empty;
            var attrName        = string.Empty;
            var attrValue       = string.Empty;
            var nestedTags      = new Stack<Tuple<string, bool>>();
            var ignoreWrite     = false;

            try
            {
                using (var reader = new StringReader(html))
                using (var writer = new StringWriter())
                {
                    while (true)
                    {
                        IgnoreNotTag(reader, writer, ignoreWrite);

                        ch = reader.Read();  // '<'
                        if (ch == -1)
                        { break; }
                        else if (ch == '>')
                        { continue; }

                        ch = reader.Peek();
                        if (ch == -1)
                        { return string.Empty; }
                        else if (ch == '/')  // '</xxxxx'
                        {
                            reader.Read();  // ignore '/'
                            isTagClosed = true;
                        }
                        else if (ch == '!')  // '<!xxxxx' or  '<!DOCTYPE'
                        {
                            reader.Read();  // ignore '!'

                            ch = reader.Peek();
                            if (ch == -1)
                            { return string.Empty; }
                            else if (ch == '-')  // '<!--'
                            {
                                reader.Read();  // ignore '-'

                                ch = reader.Read();
                                if (ch != '-')
                                { return string.Empty; }

                                IgnoreComment(reader);
                                continue;
                            }
                            else if (ch == 'd' || ch == 'D')  // '<!DOCTYPE'
                            {
                                var doctype = ReadTagName(reader);
                                if (!doctype.Equals("DOCTYPE", StringComparison.InvariantCultureIgnoreCase))
                                { return string.Empty; }

                                if (!ignoreWrite)
                                { writer.Write($"<!{doctype}"); }

                                var inQuoted        = false;
                                var quotedSeparator = -1;
                                while ((ch = reader.Read()) != -1)  // ignored until '>'
                                {
                                    if (!ignoreWrite)
                                    { writer.Write((char)ch); }

                                    if (inQuoted)
                                    {
                                        if (quotedSeparator != ch)
                                        { continue; }

                                        inQuoted = false;
                                        continue;
                                    }
                                    else if (ch == '"' || ch == '\'')
                                    {
                                        inQuoted = true;
                                        quotedSeparator = ch;
                                        continue;
                                    }
                                    else if (ch == '>')
                                    { break; }
                                }

                                if (ch == -1)
                                { return string.Empty; }

                                continue;
                            }
                            else
                            { return string.Empty; }
                        }

                        tagName = ReadTagName(reader);
                        if (!CheckTagName(tagName))
                        { ignoreWrite = true; }

                        if (!isTagClosed)
                        {
                            nestedTags.Push(new Tuple<string, bool>(tagName, ignoreWrite));

                            if (!ignoreWrite)
                            { writer.Write($"<{tagName}"); }

                            while (true)
                            {
                                IgnoreWhiteSpace(reader, writer, true);
                                if (reader.Peek() == -1)
                                { return string.Empty; }

                                attrName = ReadAttrName(reader);
                                if (string.IsNullOrWhiteSpace(attrName))
                                { break; }

                                IgnoreWhiteSpace(reader, writer, true);
                                if (reader.Peek() == -1)
                                { return string.Empty; }

                                attrValue = null;

                                ch = reader.Peek();
                                if (ch == '=')
                                {
                                    reader.Read();  // ignore '='
                                    IgnoreWhiteSpace(reader, writer);
                                    if (reader.Peek() == -1)
                                    { return string.Empty; }

                                    attrValue = ReadAttrValue(reader);
                                    if (string.IsNullOrWhiteSpace(attrValue))
                                    { return string.Empty; }
                                }

                                if (CheckAttrName(tagName, attrName) && !ignoreWrite)
                                {
                                    if (attrValue != null)
                                    {
                                        if (!attrValue.Contains("\\"))
                                        { writer.Write($" {attrName}={attrValue}"); }
                                    }
                                    else
                                    { writer.Write($" {attrName}"); }
                                }
                            }

                            ch = reader.Read();
                            if (ch == -1)
                            { return string.Empty; }

                            if (ch == '/')
                            {
                                ch = reader.Read();
                                if (ch != '>')
                                { return string.Empty; }

                                if (!ignoreWrite)
                                { writer.Write(" />"); }

                                nestedTags.Pop();

                                ignoreWrite = false;
                                if (nestedTags.Count > 0)
                                { ignoreWrite = nestedTags.Peek().Item2; }
                            }
                            else if (ch == '>')
                            {
                                if (!ignoreWrite)
                                { writer.Write(">"); }

                                continue;
                            }
                            else
                            { continue;  /* cannot reach */ }
                        }
                        else  // '</xxxx>'
                        {
                            isTagClosed = false;

                            ch = reader.Read();
                            if (ch != '>')
                            { return string.Empty; }

                            if (tagName.Equals(nestedTags.Peek().Item1, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (!ignoreWrite)
                                { writer.Write($"</{tagName}>"); }

                                nestedTags.Pop();

                                ignoreWrite = false;
                                if (nestedTags.Count > 0)
                                { ignoreWrite = nestedTags.Peek().Item2; }
                            }
                            else
                            { return string.Empty; }
                        }
                    }

                    if (nestedTags.Count > 0)
                    { return string.Empty; }

                    return writer.ToString();
                }
            }
            catch (Exception ex)
            { return string.Empty; }
        }
    }
}
