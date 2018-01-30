using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RahatCoreNlp.Data
{
    public class DependencyParseNode
    {
        public string CTag { get; set; }
        public int Head { get; set; }
        public string Word { get; set; }
        public string UnicodeWord { get; set; }
        public string Rel2Head { get; set; }
        public string Lemma { get; set; }
        public string UnicodeLemma { get; set; }
        public string Tag { get; set; }
        public List<KeyValuePair<string, int>> Deps { get; set; }  //string: label   int: address
        public int Address { get; set; }
        public string Feats { get; set; }

        public DependencyParseNode Clone()
        {
            DependencyParseNode node = new DependencyParseNode();
            node.CTag = CTag;
            node.Head = Head;
            node.Word = Word;
            node.UnicodeWord = UnicodeWord;
            node.Rel2Head = Rel2Head;
            node.Lemma = Lemma;
            node.UnicodeLemma = UnicodeLemma;
            node.Tag = Tag;
            node.Address = Address;
            node.Feats = Feats;
            node.Deps = new List<KeyValuePair<string, int>>();
            foreach (var keyValuePair in Deps)
            {
                node.Deps.Add(keyValuePair);
            }
            return node;
        }
    }
}
