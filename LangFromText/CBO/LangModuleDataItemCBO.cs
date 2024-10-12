using Jpinsoft.CompactStorage.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.CBO
{
    public class LangModuleDataItemCBO : CompactDataBase
    {
        public int Score { get; set; }

        public List<string> ScoreData { get; set; } = new List<string>();
    }
}
