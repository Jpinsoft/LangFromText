using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.CBO
{
    public class WordCBO
    {
        private static WordCBO placeholderWord;

        public static WordCBO PlaceholderWord
        {
            get
            {
                if (placeholderWord == null)
                    placeholderWord = new WordCBO { ID = 0, Value = "  *", IsPlaceholderWord = true };

                return placeholderWord;
            }
        }

        public int ID { get; set; }

        /// <summary>
        /// Celkovy pocet vyskytov vo vetach v ktorych sa slovo nachadza.
        /// Viacnasobne sa zapocitaju sa aj viacnasobne vyskyty v jednej vete
        /// </summary>
        public int PocetVyskytov { get; set; }

        public int PocetZdrojov { get; set; }

        public int Rating
        {
            get { return PocetVyskytov + (PocetZdrojov * 100); }
        }

        public string Value { get; set; }

        public bool IsPlaceholderWord { get; set; }

        public override string ToString()
        {
            return Value;

        }
    }
}
