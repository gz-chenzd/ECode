using System.Collections;
using ECode.Utility;

namespace ECode.Tokenizer
{
    public class PhrasePortion
    {
        public string Portion
        { get; private set; }

        public string Phrase
        { get; private set; }

        public bool IsCompleted
        {
            get { return this.Phrase != null; }
        }


        private Hashtable NextPortions
        { get; set; } = UtilFunctions.CreateCaseInsensitiveHashtable();


        public PhrasePortion(string portion)
        {
            this.Portion = portion;
        }


        public PhrasePortion HitNext(string portion)
        {
            if (this.NextPortions.Count < 1)
            { return null; }

            return (PhrasePortion)this.NextPortions[portion];
        }

        internal void AddNext(string phrase, PhraseReader reader)
        {
            var token = reader.Read();
            if (token == null)
            {
                this.Phrase = phrase;
                return;
            }

            var nextPortion = (PhrasePortion)this.NextPortions[token.Portion];
            if (nextPortion == null)
            {
                nextPortion = new PhrasePortion(token.Portion);
                this.NextPortions[token.Portion] = nextPortion;
            }

            nextPortion.AddNext(phrase, reader);
        }
    }
}