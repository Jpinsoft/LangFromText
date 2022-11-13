using System;
using System.Collections.Generic;
using System.Text;
using Jpinsoft.LangTainer.Utils;

namespace Jpinsoft.LangTainer.CBO
{
    public class PhraseCBO
    {
        public int Id { get; set; }

        public List<WordCBO> Words;

        public int PocetVyskytov { get; set; }

        public int PocetZdrojov { get; set; }

        public int Rating
        {
            get
            {
                return 0;
            }
        }

        public string TextResult { get { return this.ToString(); } }

        ///// <summary>
        ///// Computed Prop
        ///// </summary>
        //public List<SentenceViewCBO> SentenceListView { get; set; }

        public PhraseCBO()
        {
            Words = new List<WordCBO>();
            //SentenceListView = new List<SentenceViewCBO>();
        }

        public static bool operator ==(PhraseCBO a, PhraseCBO b)
        {
            if (object.ReferenceEquals(null, a))
                return object.ReferenceEquals(null, b);

            if (object.ReferenceEquals(null, b))
                return object.ReferenceEquals(null, a);

            return a.Equals(b);
        }

        public static bool operator !=(PhraseCBO a, PhraseCBO b)
        {
            if (object.ReferenceEquals(null, a))
                return !object.ReferenceEquals(null, b);

            if (object.ReferenceEquals(null, b))
                return !object.ReferenceEquals(null, a);

            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            PhraseCBO sen = obj as PhraseCBO;

            if (sen == null || this.Words.Count != sen.Words.Count)
                return false;

            for (int i = 0; i < this.Words.Count; i++)
            {
                if (this.Words[i].Value != sen.Words[i].Value)
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
            return ArrayTools.ArrayValuesToString(Words.ToArray());
        }

        /// <summary>
        /// Ak najde subPhraseToFind v tejto Sentence, tak vrati index zaciatku, inak vrati -1
        /// Metoda podporuje aj PLACEHOLDER (*) vo vstupnej subPhraseToFind
        /// Metoda nie je Thread Safe a musi byt zabezpecena z vyssej vrstvy, ktora ju vola
        /// </summary>
        public int FindSubphraseIndex(WordCBO[] subPhraseToFind)
        {
            for (int i = 0; i < this.Words.Count; i++)
            {
                bool contains = true;

                for (int j = 0; j < subPhraseToFind.Length; j++)
                {
                    if (j + i >= this.Words.Count)
                    {
                        contains = false;
                        break;
                    }

                    if (subPhraseToFind[j].IsPlaceholderWord)
                        continue;

                    if (j + i >= this.Words.Count || subPhraseToFind[j] != this.Words[j + i])
                    {
                        contains = false;
                        break;
                    }
                }

                if (contains)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Vrati pocet slov, ktore tato veta obsahuje
        /// </summary>
        public int ContainsWords(WordCBO[] wordsToFind, ref int startIndex, ref int resultLength)
        {
            int minIndex = int.MaxValue;
            int maxIndex = 0;
            int matchCount = 0;

            foreach (WordCBO w in wordsToFind)
            {
                int wIndex = this.Words.IndexOf(w, startIndex);

                if (wIndex > 0)
                {
                    matchCount++;

                    if (wIndex < minIndex)
                        minIndex = wIndex;

                    if (wIndex > maxIndex)
                        maxIndex = wIndex;
                }
            }

            startIndex = minIndex;
            resultLength = maxIndex - startIndex + 1;

            return matchCount;
        }
    }
}
