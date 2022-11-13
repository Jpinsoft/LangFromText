using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.ContainerStorage.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Jpinsoft.LangTainer.ContainerStorage;

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
                    st.InitStorage(storageKey, repositoryFolder);
                    Repository.Add(st);
                }

                return st;
            }
        }

    }
}
