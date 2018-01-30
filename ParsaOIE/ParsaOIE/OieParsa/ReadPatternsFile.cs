using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RahatCoreNlp.Data;

namespace RahatCoreNlp.OieParsa
{
    public class PatternPackage
    {
        public List<MatchExtractTuple> patterns = new List<MatchExtractTuple>();
    }

    public class ExtractionNode
    {
        public List<string> names = new List<string>();
        public Dictionary<string/*variable name*/, List<string>/*list of expand lables*/> expands = new Dictionary<string, List<string>>();
        public Dictionary<string/*variable name*/, List<string>/*list of not expand lables*/> notExpands = new Dictionary<string, List<string>>();
        public Dictionary<string/*variable name*/, List<string>/*list of not expand lables*/> lexExpand = new Dictionary<string, List<string>>();
        public Dictionary<string/*variable name*/, List<string>/*list of not expand lables*/> lexNotExpand = new Dictionary<string, List<string>>();
    }
    public class ExtractionTriple
    {
        public ExtractionNode arg1 = new ExtractionNode();
        public ExtractionNode arg2 = new ExtractionNode();
        public ExtractionNode rel = new ExtractionNode();
    }
    public class PatternNode : IEnumerable<PatternNode>
    {
        public char targetParsedStructure; //B basic, C (default) CCprocessed dependency parsed
        public int ParsedStructureAddress;
        public string name;
        public List<string> link = new List<string>();
        public List<string> lex = new List<string>();
        public List<string> POS = new List<string>();
        public List<PatternNode> childs = new List<PatternNode>();
        public List<List<Tuple<string, int, int>>> levelMatchings = new List<List<Tuple<string, int, int>>>();
        public PatternNode CorrespondingNodeInPattern;
        public int totalMatchNumber;
        public PatternNode Parent { get; private set; }

        public PatternNode(string _name)
        {
            name = _name;
            targetParsedStructure = 'C';
        }

        public PatternNode()
        {            
        }

        public List<PatternNode> Depth
        {
            get
            {
                var path = new List<PatternNode>();
                foreach (var node in childs)
                {
                    var tmp = node.Depth;
                    if (tmp.Count > path.Count)
                        path = tmp;
                }
                path.Insert(0, this);
                return path;
            }
        }

        public static bool IsMatch(PatternNode referenceNode, PatternNode pattern, List<DependencyParseNode> parsedStructure, PatternNode fatherNode)
        {                       
            if (referenceNode.link.Count == 0)
                return false;

            if (pattern.lex.Count != 0)
            {
                bool leximMatched = false;
                foreach (string lexim in pattern.lex)
                {
                    // can we find lexim phrase string inside sentnece node
                    // for example finding گو inside of میگویند
                    if (referenceNode.name.IndexOf(lexim, StringComparison.Ordinal) != -1)
                    {
                        leximMatched = true;
                        break;
                    }
                }
                if (!leximMatched)
                    return false;
            }


            if (pattern.link.Count == 0)
            {
                if (pattern.POS.Count() == 0)
                    return true;
                if (pattern.POS.Contains(referenceNode.POS[0]))
                    return true;
                return false;
            }


            #region bug fix
            // this piece of code let us to handle DAG(directed asyclic graphes which in them a node may have multiple fathers!)
            // sice there might be multiple parents for the current node. 
            // for now we just want the link to current father to solve the bug in sentnec It is the 17th year that the gallery has invited an artist to dress their Christmas tree.
            // with template {.rel {arg1 link-nsubj link-nsubjpass}{arg2 link-dobj}}	(arg1; .rel cat-verb; arg2)
            // which does not match with <an artist dress thier christmas tree>
            if (fatherNode != null && referenceNode.link.Count > 1)
            {
                string label2Father =
                    parsedStructure[fatherNode.ParsedStructureAddress].Deps.Find(
                        x => x.Value == referenceNode.ParsedStructureAddress).Key;
                if (pattern.link.Contains(label2Father))
                {
                    if (pattern.POS.Count() == 0)
                        return true;
                    if (pattern.POS.Contains(referenceNode.POS[0]))
                        return true;
                }
                return false;
            }
            #endregion


            if (pattern.link.Contains(referenceNode.link[0]))
            {
                if (pattern.POS.Count() == 0)
                    return true;
                else if (pattern.POS.Contains(referenceNode.POS[0]))
                    return true;
            }
            return false;
        }

        public IEnumerator<PatternNode> GetEnumerator()
        {
            return childs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(PatternNode item)
        {
            item.Parent = this;
            childs.Add(item);
        }

        public static PatternNode BuildTree(string tree)
        {
            var lines = tree.Split(new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);

            var result = new PatternNode("TreeRoot");
            var list = new List<PatternNode> { result };

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                var indent = line.Length - trimmedLine.Length;

                var child = new PatternNode(trimmedLine);
                list[indent].Add(child);

                if (indent + 1 < list.Count)
                {
                    list[indent + 1] = child;
                }
                else
                {
                    list.Add(child);
                }
            }

            return result.childs[0];
        }

        public static string BuildString(PatternNode tree)
        {
            var sb = new StringBuilder();

            BuildString(sb, tree, 0);

            return sb.ToString();
        }

        private static void BuildString(StringBuilder sb, PatternNode node, int depth)
        {
            sb.AppendLine(node.name.PadLeft(node.name.Length + depth));

            foreach (var child in node)
            {
                BuildString(sb, child, depth + 1);
            }
        }
    }

    public class ReadPatternsFile
    {
        public static Dictionary<string, string> myLiterals = new Dictionary<string, string>() { 
                {"<TOBE>","است"},
                {"<ISA>", "ISA-HAS-IS"},
                {"VAPE"/*Verb Attached Personal Ending*/,"شناسه فعل"}
            };        
        static Dictionary<string/*category name*/, Dictionary<string/*exp or nexp*/, List<string>/*label*/>> extractionCategories;

        public static List<PatternPackage> ReadFromFile(string filename)
        {
            List<string> lines = File.ReadAllText(filename).Replace("\r\n", "\n").Replace("\r", "").Split('\n').ToList();
            int currentLine = 0;
            bool catStarted = false;
            extractionCategories = new Dictionary<string, Dictionary<string, List<string>>>();
            for (; currentLine < lines.Count(); currentLine++)
            {
                if (lines[currentLine] == "<<CATS>>")
                {
                    catStarted = true;
                    continue;
                }
                if (!catStarted)
                    continue;

                List<string> tokens = lines[currentLine].Split('\t').ToList();
                
                Dictionary<string,List<string>> categories = new Dictionary<string,List<string>>();
                extractionCategories.Add(tokens[0], categories);

                for (int p = 1; p < tokens.Count(); p++)
                {
                    string[] values = tokens[p].Split('-');
                    if (!extractionCategories[tokens[0]].Keys.Contains(values[0]))
                    {
                        extractionCategories[tokens[0]].Add(values[0], new List<string>());
                    }
                    extractionCategories[tokens[0]][values[0]].Add(values[1]);
                }
            }

            List<PatternPackage> patterns = new List<PatternPackage>();
            for (int i = 0; i < lines.Count(); i++)
            {
                if (lines[i] == "<<CATS>>")
                    break;

                // each line contains Sentence pattern package!
                PatternPackage patternPackage = new PatternPackage();

                List<string> tokens = lines[i].Split('\t').ToList();
                for (int p = 0; p < tokens.Count(); p+=2)
                {
                    MatchExtractTuple met = new MatchExtractTuple();
                    // first read Matching Pattern
                    met.MatchPattern = ParsePattern(tokens[p]);

                    // then read Extraction pattern
                    met.ExtractionPattern = ParseExtractionPattern(tokens[p + 1]);
                    met.patternStringRawFromFile = string.Format("{0}\t{1}", tokens[p], tokens[p + 1]);
                    patternPackage.patterns.Add(met);
                }
                patterns.Add(patternPackage);
            }
            return patterns;
        }

        static PatternNode ParsePattern(string input)
        {
            char targetParsedStructure = 'C';  // default is C for CcProcessed dependency tree
            if (input != "" && input[0] != '{')
            {
                targetParsedStructure = input[0];
                input = input.Substring(1);
            }

            Stack<PatternNode> Stack = new Stack<PatternNode>();
            PatternNode currentNode = null;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '{')
                {
                    i++;
                    string token = "";
                    PatternNode node = new PatternNode();
                    while (true)
                    {
                        if (token != "" && (input.Length == i || input[i] == ' ' || input[i] == '{' || input[i] == '}'))
                        {
                            List<string> subtoken = token.Split('-').ToList();
                            if (subtoken.Count() == 1)
                            {
                                node.name = subtoken[0];
                            }
                            else if (subtoken.Count() == 2)
                            {
                                if (subtoken[0] == "link")
                                    node.link.Add(subtoken[1]);
                                else if (subtoken[0] == "lex")
                                {
                                    foreach (string lexim in extractionCategories[subtoken[1]]["exp"])
                                    {
                                        node.lex.Add(lexim);
                                    }
                                }
                                else if (subtoken[0] == "pos")
                                    node.POS.Add(subtoken[1]);
                            }
                            token = "";
                        }
                        else
                            token += input[i];
                        if (input.Length == i || input[i] == '{' || input[i] == '}')
                        {
                            break;
                        }
                        i++;
                    }
                    if (currentNode != null)
                        Stack.Push(currentNode);
                    currentNode = node;
                    i--;
                }
                else if (c == '}')
                {
                    if (Stack.Count() != 0)
                        Stack.Peek().childs.Add(currentNode);
                    if (Stack.Count() != 0)
                        currentNode = Stack.Pop();
                }
            }

            if (currentNode!= null && targetParsedStructure != 'C')
                currentNode.targetParsedStructure = targetParsedStructure;

            return currentNode;
        }

        static ExtractionTriple ParseExtractionPattern(string input)
        {
            ExtractionTriple triple = new ExtractionTriple();

            int arg1StartIdx = input.IndexOf('(');
            int arg1EndIdx = input.IndexOf(';');
            string arg1 = input.Substring(arg1StartIdx + 1, arg1EndIdx - arg1StartIdx - 1).Trim();

            input = input.Substring(arg1EndIdx + 1);
            int arg2EndIdx = input.IndexOf(';');
            string arg2 = input.Substring(0, arg2EndIdx).Trim();

            input = input.Substring(arg2EndIdx + 1);
            int relEndIdx = input.IndexOf(')');
            string rel = input.Substring(0, relEndIdx).Trim();

            triple.arg1 = ParseExtractionNode(arg1);
            triple.arg2 = ParseExtractionNode(arg2);
            triple.rel = ParseExtractionNode(rel);

            return triple;
        }

        static ExtractionNode ParseExtractionNode(string input)
        {

            ExtractionNode node = new ExtractionNode();
            List<string> subNodes = input.Split(' ').ToList();
            foreach (string subnode in subNodes)
            {
                string[] attributes = subnode.Split('-');
                if (attributes.Length == 1)
                {
                    node.names.Add(attributes[0]);
                }
                else
                {
                    if (attributes[0] == "exp")
                    {
                        if (!node.expands.Keys.Contains(node.names.Last()))
                            node.expands.Add(node.names.Last(), new List<string>() { attributes[1] });
                        else
                            node.expands[node.names.Last()].Add(attributes[1]);
                    }
                    else if (attributes[0] == "nexp")
                    {
                        if (!node.notExpands.Keys.Contains(node.names.Last()))
                            node.notExpands.Add(node.names.Last(), new List<string>() { attributes[1] });
                        else
                            node.notExpands[node.names.Last()].Add(attributes[1]);
                    }
                    else if (attributes[0] == "cat")
                    {
                        if (extractionCategories[attributes[1]].Keys.Contains("exp"))
                            foreach (string label in extractionCategories[attributes[1]]["exp"])
                            {
                                if (!node.expands.Keys.Contains(node.names.Last()))
                                    node.expands.Add(node.names.Last(), new List<string>() { label });
                                else
                                    node.expands[node.names.Last()].Add(label);
                            }
                        if (extractionCategories[attributes[1]].Keys.Contains("nexp"))
                            foreach (string label in extractionCategories[attributes[1]]["nexp"])
                            {
                                if (!node.notExpands.Keys.Contains(node.names.Last()))
                                    node.notExpands.Add(node.names.Last(), new List<string>() { label });
                                else
                                    node.notExpands[node.names.Last()].Add(label);
                            }
                        if (extractionCategories[attributes[1]].Keys.Contains("lexExp"))
                            foreach (string label in extractionCategories[attributes[1]]["lexExp"])
                            {
                                if (!node.lexExpand.Keys.Contains(node.names.Last()))
                                    node.lexExpand.Add(node.names.Last(), new List<string>() { label });
                                else
                                    node.lexExpand[node.names.Last()].Add(label);
                            }
                        if (extractionCategories[attributes[1]].Keys.Contains("lexNotExp"))
                            foreach (string label in extractionCategories[attributes[1]]["lexNotExp"])
                            {
                                if (!node.lexNotExpand.Keys.Contains(node.names.Last()))
                                    node.lexNotExpand.Add(node.names.Last(), new List<string>() { label });
                                else
                                    node.lexNotExpand[node.names.Last()].Add(label);
                            }
                    }
                }
            }
            return node;
        }
    }
}
