using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Jpinsoft.LangTainer.CBO
{
    public class LangDatabaseInfo
    {
        public LangDatabaseInfo()
        {

        }

        public LangDatabaseInfo(string connString)
        {
            NameValueCollection nvcol = new NameValueCollection();

            foreach (string keyValueString in connString.Split(';'))
            {
                if (!string.IsNullOrEmpty(keyValueString))
                {
                    string[] keyValueData = keyValueString.Trim().Split('=');
                    nvcol.Add(keyValueData[0], keyValueData[1]);
                }
            }

            this.DBName = nvcol["DBName"];
            this.DataSource = nvcol["DataSource"];
        }

        public string DataSource { get; set; }

        public string DBName { get; set; }

        public string ConnString
        {
            get
            {
                return string.Format("DataSource={0}; DBName={1}", DataSource, DBName);
            }
        }

        public long Size { get; set; }

        public float SizeMB { get { return Size / (1024f * 1024f); } }
    }
}
