using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jpinsoft.LangTainer.SentenceParsers
{
    public class ENSentenceParser : ISentenceParser
    {
        private const char substituteChar = '_';
        // private const char[] enChars = { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'b', 'c', 'a', 'b', 'c', 'a', 'b', 'c' };

        private const int MIN_SENTENCE_LENGTH = 4;
        private const int MIN_SENTENCE_WORDS_COUNT = 2;
        private const int MAX_WORD_LENGTH = 25;

        private Regex myRegex = new Regex(@"\w'\w|\w’\w");

        public List<string> ParseToWords(string sentence)
        {
            sentence = sentence.Trim();

            // Prec vsetky znaky od zaciatku po najblizsi AlfaNum znak.. napr. '10:22:10 ;Tu zacina veta.'
            for (int i = 0; i < sentence.Length; i++)
            {
                if (i == 0 && char.IsLetter(sentence[i]))
                    break; // Ak zacina znakom, nemusime nic odstranovat

                if (i > 0 && char.IsLetter(sentence[i]))
                {
                    Console.WriteLine($"---------\n INFO ParseSentence SKRATENIE - redukujem vetu o ne-alfanumericke znaky '{sentence}'.");
                    sentence = sentence.Substring(i);
                    Console.WriteLine($"---------\n INFO ParseSentence SKRATENIE - vysledok redukcie '{sentence}'. \n");
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(sentence) || sentence.Length < MIN_SENTENCE_LENGTH || sentence.Count(c => char.IsDigit(c)) > 0)
            {
                Console.WriteLine($"WARNING ParseSentence - vyradujem VETU '{sentence}'. Kratka, alebo cislice.");
                return new List<string>(); ;
            }

            // Nahrada za ' v slovach there's you'll atd.. aby ostala zachovana
            StringBuilder sb = new StringBuilder(sentence);

            foreach (Match m in myRegex.Matches(sentence))
            {
                sb[m.Index + 1] = substituteChar;
            }

            if (sb.Length > 0)
                sentence = sb.ToString();

            // TODO: vsetky slova oddelene len prazdnymi znakmi
            // Toto celkom dobre fungovalo '?\w[\w']*(?:-\w+)*'?
            Regex regex = new Regex(@"\w*");
            Match match = regex.Match(sentence);
            List<string> res = new List<string>();

            while (match.Success)
            {
                if (!string.IsNullOrEmpty(match.Value) && match.Value.Length <= MAX_WORD_LENGTH)
                {
                    string value = match.Value.ToLower().Trim();
                    value = value.Replace(substituteChar, '\'');

                    res.Add(value);
                }
                else if (!string.IsNullOrEmpty(match.Value))
                    Console.WriteLine($"WARNING ParseSentence - vyradujem slovo '{match.Value}'. Prilis dlhe.");

                match = match.NextMatch();
            }

            // Napr. ak sentence obsahuje ---,,,,, moze nastat tento pripad
            if (res.Count < MIN_SENTENCE_WORDS_COUNT)
            {
                Console.WriteLine($"WARNING ParseSentence - vyradujem VETU '{sentence}'. Maly pocet slov.");
                return new List<string>();
            }

            return res;
        }
    }
}
