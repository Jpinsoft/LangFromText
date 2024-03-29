﻿using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.LangAdapters;
using Jpinsoft.LangTainer.Types;
using LangFromTextWinApp.Controls;
using LangFromTextWinApp.Helpers;
using LangFromTextWinApp.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LangFromTextWinApp.ViewModel
{
    public class LangDataViewModel : ViewModelBase
    {
        #region Props

        private Dictionary<string, WordCBO> words;

        public Dictionary<string, WordCBO> Words
        {
            get { return words; }
            set
            {
                SetProperty(ref words, value);
            }
        }

        private KeyValuePair<string, WordCBO>? selectedWord;

        public KeyValuePair<string, WordCBO>? SelectedWord
        {
            get { return selectedWord; }
            set
            {
                SetProperty(ref selectedWord, value);
                OnPropertyChanged(nameof(IsWordSelected));
            }
        }

        public bool IsWordSelected { get { return SelectedWord != null; } }

        private List<TextSourceCBO> textSources;

        public List<TextSourceCBO> TextSources
        {
            get { return textSources; }
            set
            {
                SetProperty(ref textSources, value);
            }
        }

        private TextSourceCBO selectedTextSource;

        public TextSourceCBO SelectedTextSource
        {
            get { return selectedTextSource; }
            set
            {
                SetProperty(ref selectedTextSource, value);

                if (value != null)
                    Phrases = selectedTextSource.Sentences;
            }
        }

        private List<PhraseCBO> phrases;

        public List<PhraseCBO> Phrases
        {
            get { return phrases; }
            set
            {
                SetProperty(ref phrases, value);
            }
        }


        private PhraseCBO selectedPhrase;

        public PhraseCBO SelectedPhrase
        {
            get { return selectedPhrase; }
            set
            {
                SetProperty(ref selectedPhrase, value);
            }
        }

        #endregion

        public LangDataViewModel()
        {
            Init();
        }

        public void Init()
        {
            Words = null;
            TextSources = null;
            Words = FEContext.LangFromText.GetWordsBank(null);
            TextSources = FEContext.LangFromText.GetTextSources(null).OrderByDescending(ts => ts.Created).ToList();
        }

        public void SearchWord(string filterString)
        {
            if (string.IsNullOrEmpty(filterString))
                Words = FEContext.LangFromText.GetWordsBank();
            else
                Words = FEContext.LangFromText.GetWordsBank(w => w.Value.Value.Contains(filterString));
        }

        public async Task IndexFiles(IProgressControl progressControl, string[] files)
        {
            ILangAdapter langAdapter = new FileLangAdapter();

            try
            {
                progressControl.ShowProgress(false);

                foreach (string file in files)
                {
                    progressControl.Title = $"File Indexing: {file}";

                    FEContext.MainWin.MenuMainVertical.IsEnabled = false;

                    TextSourceCBO textSourceCBO = langAdapter.GetTextSources(file).First();

                    await Task.Run(() => { FEContext.LangFromText.IndexSource(textSourceCBO); });
                }

                await Task.Run(() => { FEContext.LangFromText.SaveDatabase(); });
            }
            finally
            {
                progressControl.CloseProgress();

                // nesmie byt v Await v inom vlakne
                FEContext.MainWin.RefreshState();
                Init();

                FEContext.MainWin.MenuMainVertical.IsEnabled = true;
            }

            MessageBoxWPF.ShowInfoFormat(Application.Current.MainWindow, MessageBoxButton.OK, Resources.T069, files.Length);
        }

        public async Task RemoveTextSource(IProgressControl progressControl, IEnumerable<TextSourceCBO> tSourceList)
        {
            if (tSourceList?.Count() > 0)
            {
                try
                {
                    progressControl.ShowProgress(false);
                    FEContext.MainWin.MenuMainVertical.IsEnabled = false;

                    await Task.Run(() =>
                    {
                        foreach (var item in tSourceList)
                            FEContext.LangFromText.RemoveTextSource(item);

                        FEContext.LangFromText.SaveDatabase();
                        SelectedTextSource = null;
                    });

                    // nesmie byt v Await v inom vlakne
                    FEContext.MainWin.RefreshState();
                    Init();
                }
                finally
                {
                    progressControl.CloseProgress();
                    FEContext.MainWin.MenuMainVertical.IsEnabled = true;
                }
            }
        }

        public void TranslateWord()
        {
            if (selectedWord != null)
                WPFHelpers.OpenTranslator(SelectedWord.Value.Value.ToString());
        }
    }
}
