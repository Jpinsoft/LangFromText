using Jpinsoft.LangTainer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.CBO
{
    public class TextSourceCBO
    {
        public int Id { get; set; }

        public string Address { get; set; }

        public TextSourceTypeEnum TextSourceType { get; set; }

        public byte[] Sha1Hash { get; set; }

        public DateTime Created { get; set; }

        public List<PhraseCBO> Sentences { get; set; }

        /// <summary>
        /// Not persisted
        /// </summary>
        public StringBuilder Text { get; set; }

        public TextSourceCBO()
        {
            Sentences = new List<PhraseCBO>();
        }

        public override string ToString()
        {
            return $"{TextSourceType} - {Address}";
        }
    }
}
