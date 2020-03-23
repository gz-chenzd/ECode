using System.Collections;
using ECode.Utility;

namespace ECode.Tokenizer
{
    public class PhraseDictionary
    {
        public static readonly PhraseDictionary     Empty   = new PhraseDictionary();


        private Hashtable       dictionary  = UtilFunctions.CreateCaseInsensitiveHashtable();


        public void AddPhrase(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
            { return; }

            phrase = phrase.Trim();
            var reader = new PhraseReader(phrase);

            var token = reader.Read();
            var portion = (PhrasePortion)dictionary[token.Portion];
            if (portion == null)
            {
                portion = new PhrasePortion(token.Portion);
                dictionary[token.Portion] = portion;
            }

            portion.AddNext(phrase, reader);
        }

        public PhrasePortion HitPortion(string portion)
        {
            return (PhrasePortion)dictionary[portion];
        }
    }
}