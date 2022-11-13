using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Jpinsoft.LangTainer.LangAdapters
{
    public class FileLangAdapter : ILangAdapter
    {
        public object AdapterSettings { get; set; }

        public void CancelOperation()
        {
            throw new NotImplementedException();
        }

        public FileLangAdapter()
        {
        }

        public List<TextSourceCBO> GetTextSources(string sourceAddress, Action<object> progressChanged = null)
        {
            List<TextSourceCBO> res = new List<TextSourceCBO>();

            StringBuilder sb = new StringBuilder(File.ReadAllText(sourceAddress));

            res.Add(new TextSourceCBO
            {
                Address = sourceAddress,
                Created = DateTime.Now,
                Sha1Hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())),
                Text = sb,
                TextSourceType = Enums.TextSourceTypeEnum.File
            });

            return res;
        }
    }
}
