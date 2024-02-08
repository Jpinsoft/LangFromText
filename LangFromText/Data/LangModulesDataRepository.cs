using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Jpinsoft.LangTainer.ContainerStorage;
using System.IO;
using Jpinsoft.LangTainer.Types;

namespace Jpinsoft.LangTainer.Data
{
    public class LangModulesDataRepository
    {
        private string repositoryFolder;

        public List<ISmartStorage<LangModuleDataItemCBO>> Repository { get; private set; }

        public LangModulesDataRepository(string repositoryFolder)
        {
            Repository = new List<ISmartStorage<LangModuleDataItemCBO>>();
            this.repositoryFolder = repositoryFolder;
        }

        public ISmartStorage<LangModuleDataItemCBO> this[string storageKey]
        {
            get
            {
                ISmartStorage<LangModuleDataItemCBO> st = Repository.FirstOrDefault(s => s.KeyName == storageKey);

                if (st == null)
                {
                    st = new JsonFileSmartStorage<LangModuleDataItemCBO>();

                    try
                    {
                        st.InitStorage(storageKey, repositoryFolder);
                    }
                    catch (Exception ex)
                    {
                        DeleteStorage(storageKey);
                        st.InitStorage(storageKey, repositoryFolder);

                        // throw new InfoException($"Unable to load '{storageKey}' module data. Module data file was reset.");
                    }

                    Repository.Add(st);
                }

                return st;
            }
        }

        private void DeleteStorage(string storageKey)
        {
            string storageFile = Path.Combine(repositoryFolder, storageKey + ".json");
            File.Delete(storageFile);
        }
    }
}
