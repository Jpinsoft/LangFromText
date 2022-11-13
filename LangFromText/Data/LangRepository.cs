using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Enums;
using Jpinsoft.LangTainer.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpinsoft.LangTainer.Data
{
    public class LangRepository
    {
        public Dictionary<string, WordCBO> WordsBank { get; set; } = new Dictionary<string, WordCBO>();
        public Dictionary<int, WordCBO> WordsBankById { get; set; } = new Dictionary<int, WordCBO>();

        public List<PhraseCBO> Sentences { get; set; } = new List<PhraseCBO>();
        public List<TextSourceCBO> TextSources { get; set; } = new List<TextSourceCBO>();

        public const int CN_ARRAY_END = -100;
        public const int CN_SECTION_END = -1000;

        string dbFile;

        #region STATIC

        private static int sourceIdCounter = 0;

        public static int SOURCE_ID_COUNTER
        {
            get
            {
                sourceIdCounter++;
                return sourceIdCounter;
            }
        }

        private static int sentenceIdCounter = 0;

        public static int SENTENCE_ID_COUNTER
        {
            get
            {
                sentenceIdCounter++;
                return sentenceIdCounter;
            }
        }

        private static int wordIdCounter = 0;

        public static int WORD_ID_COUNTER
        {
            get
            {
                wordIdCounter++;
                return wordIdCounter;
            }
        }

        public static IEnumerable<LangDatabaseInfo> GetLangDatabaseInfo(string databaseFolder)
        {
            foreach (FileInfo wbFile in new DirectoryInfo(databaseFolder).GetFiles("*.ltdb"))
            {
                LangDatabaseInfo dbInfo = new LangDatabaseInfo();

                try
                {
                    dbInfo.DBName = Path.GetFileNameWithoutExtension(wbFile.Name);
                    dbInfo.DataSource = databaseFolder;
                    dbInfo.Size = wbFile.Length;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to load {wbFile} database." + ex.Message);
                    continue;
                }

                yield return dbInfo;
            }
        }

        #endregion

        public LangRepository(string connString)
        {
            LangDatabaseInfo ldbInfo = new LangDatabaseInfo(connString);
            dbFile = Path.Combine(ldbInfo.DataSource, ldbInfo.DBName + ".ltdb");
        }

        public async Task Load()
        {
            await Task.Run(() =>
            {
                using (BinaryReader sr = new BinaryReader(File.Open(dbFile, FileMode.Open), Encoding.UTF8))
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    if (sr.BaseStream.Length > 0)
                    {
                        LoadWords(sr);
                        LoadPhrases(sr);
                        LoadTextSources(sr);
                    }

                    sw.Stop();
                    Console.WriteLine("Load LangDB Time: {0}", sw.ElapsedMilliseconds);
                }
            });
        }

        private void LoadWords(BinaryReader sr)
        {
            int id;

            while ((id = sr.ReadInt32()) != CN_SECTION_END)
            {
                WordCBO w = new WordCBO
                {
                    ID = id,
                    Value = sr.ReadString(), // VAL
                    PocetVyskytov = sr.ReadInt32(),
                    PocetZdrojov = sr.ReadInt32(),
                    IsPlaceholderWord = false
                };

                if (WordsBank.ContainsKey(w.Value) || WordsBankById.ContainsKey(w.ID))
                    throw new Exception($"Database is corrupted. Unable to load WordsBank. Word '{w.Value}' is not unique.");

                WordsBank.Add(w.Value, w);
                WordsBankById.Add(w.ID, w);
            }

            // UPLNE ZBYTOCNE - terajsie metody nevyzaduju pritomnost PlaceholderWord v containery
            // WordsBank.Add(LangFromTextManager.CN_KEYWORD_PLACEHOLDER, WordCBO.PlaceholderWord);
            // WordsBankById.Add(WordCBO.PlaceholderWord.ID, WordCBO.PlaceholderWord);

            if (WordsBankById?.Count > 0)
                wordIdCounter = WordsBankById.Max(kvp => kvp.Key);
        }

        private void LoadPhrases(BinaryReader sr)
        {
            int id;

            while ((id = sr.ReadInt32()) != CN_SECTION_END)
            {
                PhraseCBO phrase = new PhraseCBO
                {
                    Id = id, // ID
                    PocetVyskytov = sr.ReadInt32(), // ID
                    PocetZdrojov = sr.ReadInt32()
                };

                Sentences.Add(phrase);

                int wordId;

                while ((wordId = sr.ReadInt32()) != CN_ARRAY_END)
                {
                    // TODO PERFORMANCE
                    // Phrases.Last().Words.Add(WordsBank.First(w => w.Value.ID == wId).Value);
                    phrase.Words.Add(WordsBankById[wordId]);
                }
            }

            if (Sentences?.Count > 0)
                sentenceIdCounter = Sentences.Max(s => s.Id);
        }

        private void LoadTextSources(BinaryReader sr)
        {
            int id;

            while ((id = sr.ReadInt32()) != CN_SECTION_END)
            {
                TextSourceCBO textSourceCBO = new TextSourceCBO
                {
                    Id = id, // ID
                    Created = DateTime.FromBinary(sr.ReadInt64()),
                    TextSourceType = (TextSourceTypeEnum)sr.ReadByte(),
                    Sha1Hash = sr.ReadBytes(20),
                    Address = sr.ReadString()
                };

                TextSources.Add(textSourceCBO);

                int phraseId;

                while ((phraseId = sr.ReadInt32()) != CN_ARRAY_END)
                {
                    textSourceCBO.Sentences.Add(Sentences.First(p => p.Id == phraseId));
                }
            }

            if (TextSources?.Count > 0)
                sourceIdCounter = TextSources.Max(t => t.Id);
        }

        public void Save()
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(dbFile, FileMode.Create), Encoding.UTF8))
            {
                SaveWords(bw);
                SavePhrases(bw);
                SaveTextSources(bw);
            }
        }

        private void SaveWords(BinaryWriter bw)
        {
            foreach (KeyValuePair<string, WordCBO> wPair in WordsBank)
            {
                if (wPair.Value.IsPlaceholderWord)
                    continue;

                bw.Write(wPair.Value.ID);
                bw.Write(wPair.Value.Value);
                bw.Write(wPair.Value.PocetVyskytov);
                bw.Write(wPair.Value.PocetZdrojov);
            }

            bw.Write(CN_SECTION_END); // -100 je ukoncenie sekcie Words
        }

        private void SavePhrases(BinaryWriter bw)
        {
            foreach (PhraseCBO phrase in Sentences)
            {
                bw.Write(phrase.Id);
                bw.Write(phrase.PocetVyskytov);
                bw.Write(phrase.PocetZdrojov);

                foreach (WordCBO w in phrase.Words)
                    bw.Write(w.ID);

                bw.Write(CN_ARRAY_END);
            }

            bw.Write(CN_SECTION_END); // -100 je ukoncenie sekcie Words
        }

        private void SaveTextSources(BinaryWriter bw)
        {
            foreach (TextSourceCBO tSource in TextSources)
            {
                bw.Write(tSource.Id);
                bw.Write(tSource.Created.ToBinary());
                bw.Write((byte)tSource.TextSourceType);
                bw.Write(tSource.Sha1Hash);
                bw.Write(tSource.Address);

                foreach (PhraseCBO phrase in tSource.Sentences)
                    bw.Write(phrase.Id);

                bw.Write(CN_ARRAY_END);
            }

            bw.Write(CN_SECTION_END); // -100 je ukoncenie sekcie Words
        }
    }
}
