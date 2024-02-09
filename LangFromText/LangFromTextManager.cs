using Jpinsoft.LangTainer.Data;
using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.LangAdapters;
using Jpinsoft.LangTainer.SentenceParsers;
using Jpinsoft.LangTainer.Types;
using Jpinsoft.LangTainer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jpinsoft.LangTainer
{
    public class LangFromTextManager
    {
        #region Fields&Props

        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        private LangRepository langRepository;
        private Random rnd = new Random();

        public ISentenceParser SentenceParser { get; set; } = new ENSentenceParser();

        public const string CN_KEYWORD_PLACEHOLDER = "placexurnholder";
        public const double CN_MAX_RATING = 1_000_000;
        public const int CN_MIN_WORDS_COUNT = 500;
        public const float CN_MIN_TEXTSOURCES_INDEX = 2;
        private const string CN_ERRR_MSG_1 = "Not enought words in Database, you need more data. Please index web or text files and populate database.";

        public int WordsCount
        {
            get
            {
                rwLock.EnterWriteLock();

                try
                {
                    if (langRepository?.WordsBank != null)
                        return langRepository.WordsBank.Count;
                    else
                        return 0;
                }
                finally { rwLock.ExitWriteLock(); }
            }
        }

        public bool HasEnoughtWords
        {
            get { return WordsCount > CN_MIN_WORDS_COUNT; }
        }

        public float TextSourcesPer1000WordsIndex
        {
            get
            {
                int tSourcesCount = GetTextSources(null).Count;

                // Kedze Min Words je 500 zacina od 1,99
                float textSourcesPer500WordsIndex = (tSourcesCount / ((float)(WordsCount + 1) / 1000f));

                return textSourcesPer500WordsIndex;
            }
        }

        #endregion

        public LangFromTextManager(string connString)
        {
            langRepository = new LangRepository(connString);
        }

        public async Task Init()
        {
            await langRepository.Load();
        }

        public static double RatingSumRatingTotal(double ratingSum)
        {
            double ratingTotal = Math.Truncate(ratingSum / CN_MAX_RATING) * 25;
            double asterixRating = ratingSum - (Math.Truncate(ratingSum / CN_MAX_RATING) * CN_MAX_RATING);

            return ratingTotal + asterixRating;
        }

        public static double RatingSumToPercentage(double ratingSum)
        {
            // TODO: Max limit pre Asterix 1 - 50% a Asterix 2 - 25%
            double asterixRating = ratingSum - (Math.Truncate(ratingSum / CN_MAX_RATING) * CN_MAX_RATING);
            asterixRating = asterixRating > 50 ? 50 : asterixRating;

            double rating = Math.Truncate(ratingSum / CN_MAX_RATING) * 25; // 25% je vaha najvyssieho PP kde asterix = 0
            rating = rating + asterixRating;

            return rating > 100 ? 100 : rating;
        }

        public void IndexSource(TextSourceCBO textSource)
        {
            if (langRepository.TextSources.Count(src => ArrayTools.CompareArrays(src.Sha1Hash, textSource.Sha1Hash)) > 0)
            {
                throw new Exception($"IndexSource warning. The same content from '{textSource.Address}' has been already indexed. Operation was aborted.");
            }

            Dictionary<int, WordCBO> distinctWords = new Dictionary<int, WordCBO>();
            Dictionary<int, PhraseCBO> distinctPhrases = new Dictionary<int, PhraseCBO>();

            foreach (string s in SplitToSentences(textSource.Text))
            {
                List<string> sentenceWords = SentenceParser.ParseToWords(s);

                if (sentenceWords?.Count > 0)
                {
                    // Save WORDS to DB
                    PhraseCBO parsedPhrase = new PhraseCBO();

                    foreach (string w in sentenceWords)
                    {
                        // Vlozi iba ak este nebol vlozeny
                        WordCBO wordFromDB = this.GetOrCreateWord(w);
                        wordFromDB.PocetVyskytov++;

                        parsedPhrase.Words.Add(wordFromDB);

                        if (!distinctWords.ContainsKey(wordFromDB.ID))
                            distinctWords.Add(wordFromDB.ID, wordFromDB);
                    }

                    // TODO do ThreadSafe InsertPhrase
                    PhraseCBO phraseFromDB = langRepository.Sentences.FirstOrDefault(item => item == parsedPhrase);

                    if (phraseFromDB == null)
                    {
                        phraseFromDB = parsedPhrase;
                        phraseFromDB.Id = LangRepository.SENTENCE_ID_COUNTER;
                        langRepository.Sentences.Add(phraseFromDB);
                    }

                    phraseFromDB.PocetVyskytov++;
                    textSource.Sentences.Add(phraseFromDB);

                    if (!distinctPhrases.ContainsKey(phraseFromDB.Id))
                        distinctPhrases.Add(phraseFromDB.Id, phraseFromDB);
                }
            }

            // Set Pocet Zdrojov len pre jedinecne polozky
            foreach (var item in distinctPhrases)
                item.Value.PocetZdrojov++;

            foreach (var item in distinctWords)
                item.Value.PocetZdrojov++;

            // Insert TextSource
            langRepository.TextSources.Add(new TextSourceCBO
            {
                Id = LangRepository.SOURCE_ID_COUNTER,
                Address = textSource.Address,
                Created = textSource.Created,
                Sha1Hash = textSource.Sha1Hash,
                TextSourceType = textSource.TextSourceType,
                Sentences = textSource.Sentences
            });

            langRepository.Sentences = langRepository.Sentences.OrderByDescending(s => s.PocetVyskytov).ToList();
            langRepository.WordsBank = langRepository.WordsBank.OrderByDescending(w => w.Value.PocetVyskytov).ToDictionary(sele => sele.Key, sele => sele.Value);
        }

        private string[] SplitToSentences(StringBuilder sb)
        {
            sb = sb.Replace('?', '.');
            sb = sb.Replace('!', '.');

            string[] sentences = sb.ToString().Split('.');

            return sentences;
        }


        #region Thread Safe Data

        public void SaveDatabase()
        {
            rwLock.EnterWriteLock();

            try
            {
                langRepository.Save();
            }
            finally { rwLock.ExitWriteLock(); }
        }

        /// <summary>
        /// ThreadSafe, vytvori prida slovo do WordsBank ak tam este nie je a vrati ho, pripadne vrati existujuce Word ak uz existovalo.
        /// </summary>
        private WordCBO GetOrCreateWord(string word)
        {
            rwLock.EnterWriteLock();

            try
            {
                if (!this.langRepository.WordsBank.ContainsKey(word))
                {
                    WordCBO newWord = new WordCBO { ID = LangRepository.WORD_ID_COUNTER, Value = word };
                    langRepository.WordsBank.Add(word, newWord);
                    langRepository.WordsBankById.Add(newWord.ID, newWord);
                    return newWord;
                }
                else
                    return this.langRepository.WordsBank[word];
            }
            finally { rwLock.ExitWriteLock(); }
        }


        public WordCBO GetWordFromBank(string word, bool throwOnNullResult = true)
        {
            rwLock.EnterReadLock();

            try
            {
                if (this.langRepository.WordsBank.ContainsKey(word))
                    return this.langRepository.WordsBank[word];
                else
                {
                    if (throwOnNullResult)
                        throw new InfoException($"Database does not contain word '{word}'. Please check word or index database.");
                    else
                        return null;
                }
            }
            finally { rwLock.ExitReadLock(); }
        }

        public Dictionary<string, WordCBO> GetWordsBank(Func<KeyValuePair<string, WordCBO>, bool> predicate = null)
        {
            rwLock.EnterReadLock();

            try
            {
                if (predicate == null)
                    return this.langRepository.WordsBank.ToDictionary(ks => ks.Key, ks => ks.Value);

                else
                    return this.langRepository.WordsBank.Where(predicate).ToDictionary(ks => ks.Key, ks => ks.Value);
            }
            finally { rwLock.ExitReadLock(); }
        }

        public List<PhraseCBO> GetPhrases(Func<PhraseCBO, bool> predicate = null)
        {
            rwLock.EnterReadLock();

            try
            {
                if (predicate == null)
                    return this.langRepository.Sentences.ToList();

                List<PhraseCBO> r = this.langRepository.Sentences.Where(predicate).ToList();
                return r;
            }
            finally { rwLock.ExitReadLock(); }
        }

        public List<TextSourceCBO> GetTextSources(Func<TextSourceCBO, bool> predicate = null)
        {
            rwLock.EnterReadLock();

            try
            {
                if (predicate == null)
                    return this.langRepository.TextSources.ToList();

                List<TextSourceCBO> r = this.langRepository.TextSources.Where(predicate).ToList();
                return r;
            }
            finally { rwLock.ExitReadLock(); }
        }

        public void RemoveTextSource(TextSourceCBO textSource)
        {
            Dictionary<int, WordCBO> tempWords = new Dictionary<int, WordCBO>();
            Dictionary<int, PhraseCBO> distinctPhrases = new Dictionary<int, PhraseCBO>();

            rwLock.EnterWriteLock();

            try
            {
                if (textSource != null && this.langRepository.TextSources.Contains(textSource))
                {
                    foreach (PhraseCBO sentence in textSource.Sentences)
                    {
                        // -------------- Clean Sentence
                        sentence.PocetVyskytov--;

                        // Remove Sentence from DB
                        if (sentence.PocetVyskytov == 0)
                            this.langRepository.Sentences.Remove(sentence);

                        if (!distinctPhrases.ContainsKey(sentence.Id))
                        {
                            distinctPhrases.Add(sentence.Id, sentence);
                            sentence.PocetZdrojov--;
                        }

                        // -------------- Clean Words
                        foreach (WordCBO word in sentence.Words)
                        {
                            if (!tempWords.ContainsKey(word.ID))
                            {
                                tempWords.Add(word.ID, word);
                                word.PocetZdrojov--;
                            }

                            word.PocetVyskytov--;

                            // Remove Word from DB
                            if (word.PocetVyskytov == 0)
                            {
                                this.langRepository.WordsBank.Remove(word.Value);
                                this.langRepository.WordsBankById.Remove(word.ID);
                            }
                        }
                    }

                    // Remove TextSource from DB
                    this.langRepository.TextSources.Remove(textSource);
                }

                textSource = null;
            }
            finally { rwLock.ExitWriteLock(); }
        }

        #endregion

        #region Search&Special

        public List<WordCBO> GetRandomWords(int maxCount = 10, int minRating = 5, int minWordLength = 1)
        {
            // TODO ZRYCHLIT CEZ RND INDEX
            List<WordCBO> res = langRepository.WordsBank.Where(kp => kp.Value.Rating >= minRating && kp.Value.Value.Length >= minWordLength).Select(kp => kp.Value).ToList();

            RandomTools.Shuffle<WordCBO>(res, rnd);

            res = res.Take(maxCount).ToList();

            if (res.Count == 0)
                throw new InfoException(CN_ERRR_MSG_1);

            return res;
        }

        public List<PhraseCBO> GetRandomSentences(int maxCount = 10, int minLength = 5)
        {
            // TODO ZRYCHLIT CEZ RND INDEX
            List<PhraseCBO> res = langRepository.Sentences.Where(sen => sen.Words.Count >= minLength).ToList();

            RandomTools.Shuffle<PhraseCBO>(res, rnd);

            res = res.Take(maxCount).ToList();

            if (res.Count == 0)
                throw new InfoException(CN_ERRR_MSG_1);

            return res;
        }

        ///// <summary>
        ///// Overi ci existuju vety s presne takouto postupnostou slov
        ///// </summary>
        ///// <param name="phrase"></param>
        //public List<PhraseCBO> CheckPhraseExact(string phrase)
        //{
        //    List<string> pWords = SentenceParser.ParseSentence(phrase);
        //    List<PhraseCBO> res = new List<PhraseCBO>();

        //    foreach (PhraseCBO item in langRepository.Sentences)
        //    {
        //        if (ArrayTools.FindSubArrayInArray(item.Words.ToArray(), pWords.ToArray()) != -1)
        //            res.Add(item);
        //    }

        //    return res;
        //}

        ///// <summary>
        ///// Overi ci existuju vety, ktore obsahuju slova aj v inom poradi
        ///// </summary>
        ///// <param name="phrase"></param>
        //public List<PhraseCBO> CheckPhraseSimilar(string phrase, int percentageMatch)
        //{
        //    WordCBO[] pWords = SentenceParser.ParseSentence(phrase).Select(wString => this.GetWordFromBank(wString)).ToArray();
        //    List<PhraseCBO> res = new List<PhraseCBO>();

        //    foreach (var item in langRepository.Sentences)
        //    {
        //        if (ArrayTools.ContainsElements(item.Words.ToArray(), pWords) == pWords.Length)
        //            res.Add(item);
        //    }

        //    return res;
        //}

        public PhraseCBO ParseTextToPhrase(string phraseText)
        {
            List<WordCBO> words = SentenceParser.ParseToWords(phraseText).Select(wString => this.GetWordFromBank(wString)).ToList();
            return new PhraseCBO { Words = words };
        }

        /// <summary>
        /// Zo vstupnej frazy vygeneruje vsetky kombinacie PP s maxAsterixCount
        /// </summary>
        public List<PhrasePattern> GenerateAsterixPatterns(string phraseText, int maxAsterixCount)
        {
            // Pri 16 slovach = 39203 patternov
            // Pri 12 = 2510 patternov
            List<WordCBO> words = SentenceParser.ParseToWords(phraseText).Select(wString => this.GetWordFromBank(wString)).ToList();
            List<PhrasePattern> res = new List<PhrasePattern>();

            res.Add(new PhrasePattern { Words = new List<WordCBO>(words) });
            GenerateAsterixPatterns(res, words, maxAsterixCount);

            return res.OrderBy(pp => pp.AsterixCount).ToList();
        }

        private void GenerateAsterixPatterns(List<PhrasePattern> res, List<WordCBO> words, int maxAsterixCount)
        {
            //if (words.Count(w => w.IsPlaceholderWord) == asterixCount)
            //    return;

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i] == null || words[i].IsPlaceholderWord)
                    continue;

                PhrasePattern newPP = new PhrasePattern { Words = new List<WordCBO>(words) };
                newPP.Words[i] = WordCBO.PlaceholderWord;

                if (newPP.Words.Count(w => w.IsPlaceholderWord) >= (maxAsterixCount + 1) || res.Contains(newPP))
                    continue;

                res.Add(newPP);
                GenerateAsterixPatterns(res, newPP.Words, maxAsterixCount);
            }
        }

        ///// <summary>
        ///// Thread Safe metoda vrati novu instanciu PhrasePattern
        ///// </summary>
        ///// <param name="phraseText"></param>
        ///// <returns></returns>
        //public List<SearchResultCBO> ContainsPhrase(string phraseText)
        //{
        //    phraseText = " " + phraseText + " ";
        //    phraseText = phraseText.Replace(" * ", string.Format(" {0} ", CN_KEYWORD_PLACEHOLDER));

        //    // 2 nasobny replace riesi pripad, pri viacerych * * *  vedla seba
        //    phraseText = phraseText.Replace(" * ", string.Format(" {0} ", CN_KEYWORD_PLACEHOLDER));

        //    //  ContainsPhrase - potrebuj attached slova z WordBank !!!
        //    List<WordCBO> words = SentenceParser.ParseSentence(phraseText).Select(wString => this.GetWordFromBank(wString)).ToList();
        //    PhrasePattern pPattern = new PhrasePattern { Words = words };

        //    //// Ak je najake slovo null, tak sa nenaslo vo WordsBank
        //    //if (pPattern.Words.Contains(null))
        //    //    return pPattern;

        //    return ContainsPhrase(pPattern);
        //}

        public List<SearchResultCBO> ContainsPhrase(PhrasePattern pPattern, PhraseCBO excludePhraseFromAsterixResults = null)
        {
            if (pPattern.AsterixCount >= pPattern.Words.Count)
                throw new ArgumentException("PhrasePattern contains too much Asterix(* Placeholders)");

            List<SearchResultCBO> searchResults = new List<SearchResultCBO>();
            rwLock.EnterReadLock();

            try
            {
                foreach (PhraseCBO sentence in langRepository.Sentences)
                {
                    int index = sentence.FindSubphraseIndex(pPattern.Words.ToArray());

                    if (index >= 0)
                    {
                        PhraseCBO currentPhrase = new PhraseCBO { Words = sentence.Words.GetRange(index, pPattern.Length) };

                        // SearchResult - RATING
                        SearchResultCBO sRes = new SearchResultCBO { PPattern = pPattern, FoundedPhrase = currentPhrase, Sentence = sentence, Index = index };

                        switch (pPattern.AsterixCount)
                        {
                            case 0:
                                sRes.Rating = (int)CN_MAX_RATING;
                                break;

                            default:

                                if (excludePhraseFromAsterixResults == currentPhrase)
                                    continue;

                                //double koef = ((double)pPattern.Words.Count / ((pPattern.AsterixCount + 1) * (pPattern.AsterixCount + 1)));
                                //koef = koef / pPattern.Words.Count; // Max je 0,25, MIN sa blizi k 0
                                ////koef = koef * ((pPattern.AsterixCount) / (double)pPattern.Words.Count);  // Zohladnenie Percentualneho podielu ASterix vo Word Count
                                //sRes.Rating = (CN_MAX_RATING * koef);

                                double koef = (double)pPattern.Words.Count / pPattern.AsterixCount; // KLESA OD Words.Count -> 1

                                // Mocninove klesanie rating
                                sRes.Rating = koef / (pPattern.AsterixCount * pPattern.AsterixCount);

                                // Exponencialne klesanie
                                // sRes.Rating = koef / Math.Pow(2, pPattern.AsterixCount);

                                // Linearne klesanie
                                // sRes.Rating = koef;

                                break;
                        }

                        searchResults.Add(sRes);
                    }
                }
            }
            finally { rwLock.ExitReadLock(); }

            // pPattern.OwnedPhrases = pPattern.OwnedPhrases.OrderByDescending(p => p.SentencesCount).ToList();

            return searchResults;
        }


        public List<SearchResultCBO> ContainsWords(WordCBO[] words)
        {
            List<SearchResultCBO> searchResults = new List<SearchResultCBO>();
            rwLock.EnterReadLock();

            try
            {
                int maxFoundedPhraseLength = words.Length + 1 + (int)((float)words.Length * 0.2f);

                foreach (PhraseCBO sentence in langRepository.Sentences)
                {
                    int startIndex = 0;
                    int resPhraseLength = 0;
                    int minWordsMatch = words.Length; // Tu je mozne zmenit min pocet zhodnych slov
                    int matchWordsCount;

                    // Postupne posuvat startIndex z kym nebude najdena fraza takmer rovnako dlha ako hladany pocet slov
                    while ((matchWordsCount = sentence.ContainsWords(words, ref startIndex, ref resPhraseLength)) >= minWordsMatch)
                    {
                        if (resPhraseLength > maxFoundedPhraseLength)
                            startIndex++;
                        else
                            break;
                    }

                    if (matchWordsCount >= minWordsMatch)
                    {
                        // SearchResult - RATING
                        PhraseCBO foundedPhrase = new PhraseCBO
                        {
                            Words = sentence.Words.GetRange(startIndex, resPhraseLength)
                        };

                        SearchResultCBO sRes = new SearchResultCBO { PPattern = null, FoundedPhrase = foundedPhrase, Sentence = sentence, Index = startIndex };

                        // Mocninove klesanie rating
                        sRes.Rating = matchWordsCount / (Math.Pow((resPhraseLength - matchWordsCount) + 1, 2));
                        searchResults.Add(sRes);
                    }
                }
            }
            finally { rwLock.ExitReadLock(); }

            return searchResults;
        }

        // pPattern.OwnedPhrases = pPattern.OwnedPhrases.OrderByDescending(p => p.SentencesCount).ToList();

        #endregion
    }
}
