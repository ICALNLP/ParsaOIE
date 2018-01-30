using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using RahatCoreNlp.Data;

namespace RahatCoreNlp.Service
{
    public class DependencyParser
    {
        public static List<DependencyParseNode> LoadCoNLL09FormatSentence(string CoNLL09_sentence)
        {
            // this code is not complete. just reads Dependency data.

            // columns order:
            // ID FORM LEMMA PLEMMA POS PPOS FEAT PFEAT HEAD PHEAD DEPREL PDEPREL FILLPRED PRED APREDs
            // link for more information: https://ufal.mff.cuni.cz/conll2009-st/task-description.html

            // first read entire string. make a list of lines in the string
            List<string> Lines = new List<string>();
            string aLine;
            StringReader strReader = new StringReader(CoNLL09_sentence);
            while (true)
            {
                aLine = strReader.ReadLine();
                if (aLine != null)
                {
                    Lines.Add(aLine);
                }
                else break;
            }

            List<DependencyParseNode> parsedStructure = new List<DependencyParseNode>();
            // first create root node.
            DependencyParseNode root = new DependencyParseNode();
            root.Address = 0;
            root.CTag = "TOP";
            root.Tag = "ROOT";
            root.UnicodeWord = "None";
            root.Deps = new List<KeyValuePair<string, int>>();
            parsedStructure.Add(root);
            // needs two pass to form deps
            foreach (string line in Lines)
            {
                if (line == "")
                    continue;
                string[] tokens = line.Split('\t');
                DependencyParseNode node = new DependencyParseNode();
                node.Address = Convert.ToInt32(tokens[0]);
                node.UnicodeWord = tokens[1];
                node.UnicodeLemma = tokens[2];
                node.CTag = tokens[4];
                // in order to unify POS tags with Hazm Library
                node.CTag = OieConvert.OieConvertor.ConvertPosTagDadegan2Hazm(node.CTag);
                node.Feats = tokens[6];
                node.Deps = new List<KeyValuePair<string, int>>();
                parsedStructure.Add(node);
            }

            for (int i = 0; i < Lines.Count(); i++)
            {
                string line = Lines[i];
                if (line == "")
                    continue;
                string[] tokens = line.Split('\t');
                parsedStructure[Convert.ToInt32(tokens[9])].Deps.Add(new KeyValuePair<string, int>(tokens[11], i + 1));
                parsedStructure[i + 1].Head = Convert.ToInt32(tokens[9]);
                parsedStructure[i + 1].Rel2Head = tokens[11];
            }

            return parsedStructure;
        }

        public static List<DependencyParseNode> LoadCoNLLFormatSentence(string CoNLL_sentence)
        {
            // first read entire string. make a list of lines in the string
            List<string> Lines = new List<string>();
            string aLine;
            StringReader strReader = new StringReader(CoNLL_sentence);
            while (true)
            {
                aLine = strReader.ReadLine();
                if (aLine != null)
                {
                    Lines.Add(aLine);
                }
                else break;
            }
            
            List<DependencyParseNode> parsedStructure = new List<DependencyParseNode>();
            // first create root node.
            DependencyParseNode root = new DependencyParseNode();
            root.Address = 0;
            root.CTag = "TOP";
            root.Tag = "ROOT";
            root.UnicodeWord = "None";
            root.Deps = new List<KeyValuePair<string, int>>();
            parsedStructure.Add(root);
            // needs two pass to form deps
            foreach (string line in Lines)
            {
                if (line == "")
                    continue;
                string[] tokens = line.Split('\t');
                DependencyParseNode node = new DependencyParseNode();
                node.Address = Convert.ToInt32(tokens[0]);
                node.UnicodeWord = tokens[1];
                node.UnicodeLemma = tokens[2];
                node.CTag = tokens[3];
                // in order to unify POS tags with Hazm Library
                node.CTag = OieConvert.OieConvertor.ConvertPosTagDadegan2Hazm(node.CTag);
                node.Feats = tokens[4];
                node.Deps = new List<KeyValuePair<string, int>>();
                parsedStructure.Add(node);
            }

            for (int i = 0; i < Lines.Count(); i++)
            {
                string line = Lines[i];
                if (line == "")
                    continue;
                string[] tokens = line.Split('\t');
                parsedStructure[Convert.ToInt32(tokens[6])].Deps.Add(new KeyValuePair<string, int>(tokens[7], i + 1));
                parsedStructure[i + 1].Head = Convert.ToInt32(tokens[6]);
                parsedStructure[i + 1].Rel2Head = tokens[7];
            }

            return parsedStructure;
        }

        public List<DependencyParseNode> ParseHazmDepParserOutput(string dictionary)
        {
            var dependencyParseStructureList = new List<DependencyParseNode>();

            DependencyParseNode dependencyParserDS = null;

            string regexKeyValue = @"u'[^']+': ";
            string deps = dictionary;   //remove string {u'deps': }
            MatchCollection matchCollection = Regex.Matches(deps, regexKeyValue);
            foreach (Match match in matchCollection)
            {
                string key, value;

                KeyValueExtractor2(match, deps, out key, out value);
                value = value.Replace("u", "").Replace("'", "");

                if (key == "address")
                {
                    if (dependencyParserDS != null)
                    {
                        dependencyParseStructureList.Add(dependencyParserDS);
                    }
                    dependencyParserDS = new DependencyParseNode();
                }

                if (key == "deps")
                {
                    List<KeyValuePair<string, int>> dependencies = new List<KeyValuePair<string, int>>();
                    value = value.Replace("[", "").Replace("]", "");
                    if (value.Contains(','))
                    {
                        // to handle something like <<<<u'ADV': [2, 3]>>>> this situation
                        string[] multipleValues = value.Split(',');
                        foreach (string tempValue in multipleValues)
                        {
                            dependencies.Add(new KeyValuePair<string, int>(tempValue, Convert.ToInt32(tempValue)));
                        }
                    }
                    else if (value != "")
                        dependencies.Add(new KeyValuePair<string, int>(value, Convert.ToInt32(value)));
                    dependencyParserDS.Deps = dependencies;
                }
                else
                    SetDsProperties(dependencyParserDS, key, value);
            }

            // to add last node
            if (dependencyParserDS != null)
            {
                dependencyParseStructureList.Add(dependencyParserDS);
            }

            foreach(DependencyParseNode dpn in dependencyParseStructureList)
            {
                for (int iii=0; iii<dpn.Deps.Count;iii++)
                {
                    dpn.Deps[iii] = new KeyValuePair<string, int>(dependencyParseStructureList[dpn.Deps[iii].Value].Rel2Head, dpn.Deps[iii].Value);
                }
            }
            return dependencyParseStructureList;
        }

        public List<DependencyParseNode> ParseJsonToStructure(string jsonStr)
        {
            var dependencyParseStructureList = new List<DependencyParseNode>();
            string regexMain = @"[0-9]+: ";
            String[] splitsMain = Regex.Split(jsonStr, regexMain);
            bool skipFirstSplit = true;
            foreach (var mainSplit in splitsMain)
            {
                var dependencyParserDS = new DependencyParseNode();
                if (skipFirstSplit)
                {
                    skipFirstSplit = false;
                    continue;
                }
                string regexDifaultDict = @"u'deps': defaultdict\([^)]+\),";
                Match defaultDictMatch = Regex.Match(mainSplit, regexDifaultDict);

                string source = Regex.Replace(mainSplit, regexDifaultDict, "", RegexOptions.None);

                string regexKeyValue = @"u'[^']+': ";

                //For defaultDict values
                string deps = defaultDictMatch.Value.Substring(9);   //remove string {u'deps': }
                MatchCollection matchCollection = Regex.Matches(deps, regexKeyValue);
                List<KeyValuePair<string, int>> dependencies = new List<KeyValuePair<string, int>>();
                foreach (Match match in matchCollection)
                {
                    string key, value;
                    KeyValueExtractor(match, deps, out key, out value);
                    value = value.Replace("[", "").Replace("]", "");
                    if (value.Contains(','))
                    {
                        // to handle something like <<<<u'ADV': [2, 3]>>>> this situation
                        string[] multipleValues = value.Split(',');
                        foreach (string tempValue in multipleValues)
                        {
                            dependencies.Add(new KeyValuePair<string, int>(key, Convert.ToInt32(tempValue)));
                        }
                    }else
                        dependencies.Add(new KeyValuePair<string, int>(key, Convert.ToInt32(value)));
                }
                dependencyParserDS.Deps = dependencies;

                //For other values
                matchCollection = Regex.Matches(source, regexKeyValue);
                foreach (Match match in matchCollection)
                {
                    string key, value;
                    KeyValueExtractor(match, source, out key, out value);
                    value = value.Replace("'", "").Replace("u", "");
                    SetDsProperties(dependencyParserDS, key, value);
                }

                dependencyParseStructureList.Add(dependencyParserDS);
            }
            return dependencyParseStructureList;
        }

        private void SetDsProperties(DependencyParseNode dependencyParserDs, string key, string value)
        {
            switch (key)
            {
                case "ctag":
                    dependencyParserDs.CTag = value;
                    break;
                case "head":
                    dependencyParserDs.Head = Convert.ToInt32(value);
                    break;
                case "word":
                    dependencyParserDs.Word = value;
                    dependencyParserDs.UnicodeWord = UnicodeToString(value);
                    break;
                case "rel":
                    dependencyParserDs.Rel2Head = value;
                    break;
                case "lemma":
                    dependencyParserDs.Lemma = value;
                    dependencyParserDs.UnicodeLemma = UnicodeToString(value);
                    break;
                case "tag":
                    dependencyParserDs.Tag = value;
                    break;
                case "address":
                    dependencyParserDs.Address = Convert.ToInt32(value);
                    break;
                case "feats":
                    dependencyParserDs.Feats = value;
                    break;
            }
        }

        public static string UnicodeToString(string unicode)
        {
            if(unicode == "\\xab")
                return "«";
            if(unicode == "\\xbb")
                return "»";
            
            if (!unicode.Contains("\\"))
                return unicode;
            // if it does not have //u format
            if (unicode[1]!='u')
                unicode = unicode.Replace("\\","\\u");
            StringBuilder output = new StringBuilder();
            string[] splited = unicode.Split('\\');
            foreach (string s in splited)
            {
                if (s == "")
                    continue;
                string c = '\\' + s;
                string test = DecodeEncodedNonAsciiCharacters(c);
                output.Append(test);
            }
            return output.ToString();
        }

        static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }

        private void KeyValueExtractor(Match match, string source, out string key, out string value)
        {
            key = match.Value.Substring(2, match.Value.Length - 5);
            var startIndex = match.Index + match.Value.Length;
            var indexOfComma = source.IndexOf(", u", startIndex);
            var endIndex = indexOfComma != -1 ? Math.Min(indexOfComma, source.IndexOf('}', startIndex)) : source.IndexOf('}', startIndex);
            value = source.Substring(startIndex, endIndex - startIndex);

        }

        private void KeyValueExtractor2(Match match, string source, out string key, out string value)
        {
            key = match.Value.Substring(2, match.Value.Length - 5);
            var startIndex = match.Index + match.Value.Length;
            var indexOfComma = source.IndexOf(",\n", startIndex);
            var endIndex = indexOfComma != -1 ? Math.Min(indexOfComma, source.IndexOf('}', startIndex)) : source.IndexOf('}', startIndex);
            value = source.Substring(startIndex, endIndex - startIndex);

        }
    }
}
