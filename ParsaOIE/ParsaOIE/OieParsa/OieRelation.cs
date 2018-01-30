using System.Collections.Generic;
using System.IO;
using System.Linq;
using RahatCoreNlp.Data;

namespace RahatCoreNlp.OieParsa
{
    public class OieRelation
    {
        public OieRelation()
        {
            relTokensIdx = new List<int>();
            arg1TokensIdx = new List<int>();
            arg2TokensIdx = new List<int>();
        }

        public bool IsEqual(OieRelation relation)
        {
            if (rel == relation.rel && arg1 == relation.arg1 && arg2 == relation.arg2)
                return true;
            return false;
        }

        // this variavble is added for debugging
        public int matchedPatternLine; /*which line in Patterns.txt produces this relation*/
        // this variavble is added for debugging
        public int matchedPatternIndex; /*which pattern inside that line produces this relation*/
        public string rel;
        public string arg1;
        public string arg2;
        public List<int> relTokensIdx;
        public List<int> arg1TokensIdx;
        public List<int> arg2TokensIdx;

        public double confidenceScore;
        public int label;

        // to keep the parsed structure of the original sentnece
        public List<DependencyParseNode> SentenceParsedStructure { get; set; }

        public bool RelationHasGeneratedParts
        {
            // for example in the sentence Jack saw Susan at home. OLLIE produces (Susan; be saw at; home)
            // <be saw at> is a generated part and this property should be set true for this relation
            get; set;
        }

        public string ConvertBackToSentenceForm()
        {
            List<int> allIndeics = relTokensIdx.Concat(arg1TokensIdx.Concat(arg2TokensIdx)).ToList();
            allIndeics.Sort();
            string result = "";
            foreach (int x in allIndeics)
            {
                if (x > 0)
                    result += SentenceParsedStructure[x].UnicodeWord + " ";
            }

            return result.Trim();
        }

        public static void Save(string filepath, List<DependencyParseNode> parsedStructure, List<OieRelation> relations, int sentenceId, string sentence, List<PatternPackage> patterns, bool append = true)
        {
            StreamWriter sw = new StreamWriter(filepath, append);
            foreach (OieRelation relation in relations)
            {
                sw.Write("{0}\t{1}\n", sentenceId, sentence);
                sw.Write("$\n");
                sw.Write("{0}\t{1}\t{2}\n", relation.matchedPatternLine, relation.matchedPatternIndex, patterns == null ? "" : patterns[relation.matchedPatternLine - 1].patterns[relation.matchedPatternIndex - 1].patternStringRawFromFile);
                sw.Write("{0:0.000}\tscore\n", relation.confidenceScore);
                sw.Write("#\n");
                foreach (int address in relation.arg1TokensIdx)
                {
                    if (address >= 0)
                        sw.Write("{0}\t{1}\n", address, parsedStructure[address].UnicodeWord);
                    else if (address == -1)
                        sw.Write("{0}\t{1}\n", "-1", "VAPE");
                    else if (address == -2)
                        sw.Write("{0}\t{1}\n", "-2", "TOBE");
                }
                sw.Write("#\n");
                foreach (int address in relation.arg2TokensIdx)
                {
                    if (address >= 0)
                        sw.Write("{0}\t{1}\n", address, parsedStructure[address].UnicodeWord);
                    else if (address == -1)
                        sw.Write("{0}\t{1}\n", "-1", "VAPE");
                    else if (address == -2)
                        sw.Write("{0}\t{1}\n", "-2", "TOBE");
                }
                sw.Write("#\n");
                foreach (int address in relation.relTokensIdx)
                {
                    if (address >= 0)
                        sw.Write("{0}\t{1}\n", address, parsedStructure[address].UnicodeWord);
                    else if (address == -1)
                        sw.Write("{0}\t{1}\n", "-1", "VAPE");
                    else if (address == -2)
                        sw.Write("{0}\t{1}\n", "-2", "TOBE");
                }
                sw.Write("\n");
            }
            sw.Close();
        }
    }
}