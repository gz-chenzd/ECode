using System;
using System.Collections.Generic;
using System.IO;

namespace ECode.Tokenizer
{
    public class PhraseTokenizer : AbstractTokenizer
    {
        private PhraseReader            reader          = null;
        private PhraseDictionary        dictionary      = PhraseDictionary.Empty;

        private Stack<HitPortion>       hits            = new Stack<HitPortion>();
        private Stack<HitPortion>       loops           = new Stack<HitPortion>();


        public PhraseDictionary Dictionary
        {
            get { return dictionary; }

            set
            {
                if (value == null)
                { throw new ArgumentNullException(nameof(Dictionary)); }

                dictionary = value;
            }
        }


        public PhraseTokenizer(string text)
        {
            if (text == null)
            { throw new ArgumentNullException(nameof(text)); }

            reader = new PhraseReader(text);
        }

        public PhraseTokenizer(Stream stream)
        {
            if (stream == null)
            { throw new ArgumentNullException(nameof(stream)); }

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read.", nameof(stream)); }

            reader = new PhraseReader(stream);
        }


        public override TokenResult Next()
        {
        PopHits:
            while (hits.Count > 0)
            {
                var hit = hits.Pop();
                loops.Push(hit);

                if (hit.Portion.IsCompleted)
                { return new TokenResult(hit.Portion.Phrase, hit.Offset, hit.Length); }
            }

            PortionToken token = null;
            while ((token = reader.Read()) != null)
            {
                while (loops.Count > 0)
                {
                    var hit = loops.Pop();
                    var nextPortion = hit.Portion.HitNext(token.Portion);
                    if (nextPortion != null)
                    {
                        hit.Portion = nextPortion;
                        hit.Length = (token.Offset + token.Length) - hit.Offset;
                        hits.Push(hit);
                    }
                }

                var portion = dictionary.HitPortion(token.Portion);
                if (portion != null)
                {
                    var hit = new HitPortion(token.Offset, portion);
                    hit.Length = token.Length;
                    hits.Push(hit);
                }

                if (hits.Count > 0)
                { goto PopHits; }
            }

            return null;
        }
    }
}