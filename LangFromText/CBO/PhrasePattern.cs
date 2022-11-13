using Jpinsoft.LangTainer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jpinsoft.LangTainer.CBO
{
    public class PhrasePattern
    {
        public int Length { get { return Words.Count; } }

        /// <summary>
        /// Pocet hviezdiciek v tejto fraze
        /// </summary>
        public int AsterixCount { get { return Words.Count(w => w.IsPlaceholderWord); } }

        public List<WordCBO> Words { get; set; }

        //public List<PhraseCBO> OwnedPhrases { get; set; }

        public string TextResult
        {
            get { return this.ToString(); }
        }

        public PhrasePattern()
        {
        }

        public static bool operator ==(PhrasePattern a, PhrasePattern b)
        {
            if (object.ReferenceEquals(null, a))
                return object.ReferenceEquals(null, b);

            if (object.ReferenceEquals(null, b))
                return object.ReferenceEquals(null, a);

            return a.Equals(b);
        }

        public static bool operator !=(PhrasePattern a, PhrasePattern b)
        {
            if (object.ReferenceEquals(null, a))
                return !object.ReferenceEquals(null, b);

            if (object.ReferenceEquals(null, b))
                return !object.ReferenceEquals(null, a);

            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            PhrasePattern pPattern = obj as PhrasePattern;

            if (pPattern == null || this.Words.Count != pPattern.Words.Count)
                return false;

            for (int i = 0; i < this.Words.Count; i++)
            {
                if (this.Words[i].Value != pPattern.Words[i].Value)
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}", ArrayTools.ArrayValuesToString(Words.ToArray()));
        }
    }
}
