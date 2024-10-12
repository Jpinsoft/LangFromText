using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Jpinsoft.LangTainer.ContainerStorage;
using System.IO;
using Jpinsoft.LangTainer.Types;
using Jpinsoft.CompactStorage;
using Jpinsoft.CompactStorage.Types;

namespace Jpinsoft.LangTainer.Data
{
    public class LangModulesDataRepository
    {
        private string repositoryFolder;

        public List<ICompactStorage<LangModuleDataItemCBO>> Repository { get; private set; }

        public LangModulesDataRepository(string repositoryFolder)
        {
            Repository = new List<ICompactStorage<LangModuleDataItemCBO>>();
            this.repositoryFolder = repositoryFolder;
        }

        public ICompactStorage<LangModuleDataItemCBO> this[string storageKey]
        {
            get
            {
                ICompactStorage<LangModuleDataItemCBO> st = Repository.FirstOrDefault(s => s.StorageKey == storageKey);

                if (st == null)
                {
                    st = new JsonFileCompactStorage<LangModuleDataItemCBO>(storageKey, repositoryFolder);

                    try
                    {
                        st.Load();
                    }
                    catch (ArgumentException) { throw; }
                    catch (Exception ex)
                    {
                        st.Clear();
                        st.Save();
                        // throw new InfoException($"Unable to load '{storageKey}' module data. Module data file has been reset.");
                    }

                    Repository.Add(st);
                }

                return st;
            }
        }
    }
}
