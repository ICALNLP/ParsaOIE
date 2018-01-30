using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RahatCoreNlp.Data;

namespace RahatCoreNlp.Service
{
    public class SimpleTreeNode
    {
        public string unicodeWord = "";
        public string tag = "";
        public List<SimpleTreeNode> Children;

        public SimpleTreeNode()
        {
            Children = new List<SimpleTreeNode>();
        }

        public string PrintPretty(string indent, bool last, ref string output)
        {
            output += indent;
            if (last)
            {
                output += "//---";
                indent += "  ";
            }
            else
            {
                output += "|---";
                indent += "|   ";
            }
            output += (tag + " " + unicodeWord).Trim() + "\n";

            for (int i = 0; i < Children.Count; i++)
                Children[i].PrintPretty(indent, i == Children.Count - 1, ref output);

            return output;
        }
    }

    public class ChunkParser
    {

        public SimpleTreeNode Parse3(string chunkStr)
        {
            //input something like 
            //(S
            //  (NP \u0645\u0646/PRO)
            //  \u060c/PUNC
            //  (NP \u0632\u0647\u0631\u0627\u06cc/Ne \u0632\u06cc\u0628\u0627/AJ)
            //  (POSTP \u0631\u0627/POSTP)
            //  (VP \u062f\u06cc\u062f\u0645/V)
            //  ./PUNC)

            //output a tree 


            // to make sure every line starts and ends with ()
            chunkStr += '\n';
            string newStr = "";
            string regexKeyValue = @"[\n]";
            MatchCollection matchCollection = Regex.Matches(chunkStr, regexKeyValue);
            int startIdx = 0;
            foreach (Match match in matchCollection)
            {
                string line = chunkStr.Substring(startIdx, match.Index - startIdx).Trim();
                if (line.Length < 2)
                {
                    newStr += line;
                    continue;
                }                    
                //if (line[0] != '(')
                //    newStr += '(' + line + ')';
                //else 
                    newStr += line;
                startIdx = match.Index + match.Length;
            }

            chunkStr = newStr;
            chunkStr = chunkStr.Replace("\n", "");
            chunkStr = chunkStr.Replace("(/PUNC", "\\u0028/PUNC");
            chunkStr = chunkStr.Replace(")/PUNC", "\\u0029/PUNC");

            Stack<SimpleTreeNode> Stack = new Stack<SimpleTreeNode>();
            SimpleTreeNode node = new SimpleTreeNode();
            SimpleTreeNode firstNode = new SimpleTreeNode();

            for (int i = 0; i < chunkStr.Length;i++ )
            {
                char c = chunkStr[i];
                if (c == '(')
                {
                    // do nothing
                }
                else if (c == ')')
                {
                    firstNode = Stack.Pop();
                }
                else if (c == ' ')
                    continue;
                else
                {
                    node = new SimpleTreeNode();
                    // find value                    
                    string value = "";
                    for (; i < chunkStr.Length; i++)
                    {
                        value += chunkStr[i];
                        if (i + 1 < chunkStr.Length && chunkStr[i + 1] == '(' || chunkStr[i + 1] == ')' 
                            || (chunkStr[i - 1] == '(' && chunkStr[i] == 'S') /*This is added to break after seeing S(root) in chunkstr*/)
                            break;
                    }
                    
                    string tempValue = value.Trim();
                    node.tag = tempValue;
                    string[] tokens = tempValue.Split(' ');
                    bool ParentNode = true;
                    if (tokens.Length > 1)
                    {
                        node.tag = tokens[0];
                        for (int k = 1; k < tokens.Length; k++)
                        {
                            int index = tokens[k].IndexOf('/');
                            string label = tokens[k].Substring(index + 1);
                            string word = tokens[k].Substring(0, tokens[k].Length - label.Length - 1);
                            node.Children.Add(new SimpleTreeNode() { tag = label, unicodeWord = DependencyParser.UnicodeToString(word) });
                        }
                    }
                    else
                    {
                        // to handle:   \u0648/CONJ       :/PUNC         \xab/PUNC        ./PUNC
                        string[] temps = tempValue.Split('/');
                        if (temps.Count() == 2)
                        {
                            ParentNode = false;
                            node.tag = temps[1];
                            node.unicodeWord = DependencyParser.UnicodeToString(temps[0]);
                        }
                    }

                    if (Stack.Count != 0)
                    {
                        Stack.Peek().Children.Add(node);
                    }
                    if (ParentNode)
                        Stack.Push(node);
                }
            }

            //string print = "";
            //firstNode.PrintPretty("", true, ref print);
            //return print;
            return firstNode;
        }

        public ChunkerDataStructure Parse2(string chunkStr)
        {
            ChunkerDataStructure chunkerDS = new ChunkerDataStructure();

            chunkStr.Replace('\n', ' ');
            chunkStr = chunkStr.Replace("(/PUNC", "\\u0028/PUNC");
            chunkStr = chunkStr.Replace(")/PUNC", "\\u0029/PUNC");
            chunkStr = chunkStr.Substring(2, chunkStr.Length - 3);

            string regexKeyValue = @"(\/PUNC|\))";
            MatchCollection matchCollection = Regex.Matches(chunkStr, regexKeyValue);
            int startIdx = 0;
            foreach (Match match in matchCollection)
            {
                string chunk = chunkStr.Substring(startIdx, match.Index - startIdx);
                chunk = chunk.Replace('(', ' ');
                int g=0;
                startIdx = match.Index + match.Length;

                Chunk c = new Chunk();
                if(match.Value == "/PUNC")
                {
                    chunk = chunk.Replace("\n", "");
                    chunk = chunk.Trim();
                    c.Phrase = DependencyParser.UnicodeToString(chunk);
                    c.Tag = "PUNC";
                }else
                {
                    int index = chunk.IndexOf('\\');
                    if (index == -1)
                    {
                        chunk = chunk.Trim();
                        index = chunk.IndexOf(' ') + 1;
                    }
                    c.Tag = chunk.Substring(0,index).Replace("\n", "").Replace(" ", "");
                    chunk = chunk.Substring(index, chunk.Length - index);

                    string phrase = "";
                    bool addTo = true;
                    for (int kl = 0; kl < chunk.Length; kl++)
                    {
                        if (chunk[kl] == '/')
                            addTo = false;
                        if (chunk[kl] == ' ')
                            addTo = true;
                        if (addTo)
                            phrase += chunk[kl];
                    }
                    phrase = phrase.Replace("u", string.Empty);                    
                    c.Phrase = DependencyParser.UnicodeToString(phrase);
                }
                chunkerDS.Chunks.Add(c);
            }
            return chunkerDS;
        }

        public ChunkerDataStructure Parse(string chunkStr)
        {
            ChunkerDataStructure chunkerDS = new ChunkerDataStructure();

            string regexKeyValue = @"\[[^(\])]+\]";
            MatchCollection matchCollection = Regex.Matches(chunkStr, regexKeyValue);
            foreach (Match match in matchCollection)
            {
                var value = match.Value;
                var splitted = value.Split(' ');
                var chunk = new Chunk();
                chunk.Tag = splitted[splitted.Length - 1].Replace("]","");

                chunk.Phrase = value.Substring(1, value.Length - (chunk.Tag.Length + 3)).Replace("[","");

                if (chunkStr.Length >= match.Length + 1  && chunkStr[match.Length + 1] != '[')
                {
                    int firstBracketIdx = chunkStr.IndexOf('[', match.Length);
                    if (firstBracketIdx == -1)
                        firstBracketIdx = chunkStr.Length;
                    chunk.OutOfChunkPhrase = chunkStr.Substring(match.Length, firstBracketIdx - match.Length);
                    chunkStr = chunkStr.Substring(match.Length + chunk.OutOfChunkPhrase.Length, chunkStr.Length - match.Length - chunk.OutOfChunkPhrase.Length);
                    chunk.OutOfChunkPhrase = chunk.OutOfChunkPhrase.Trim();
                }else if (chunkStr.Length == match.Length)
                {
                    chunkStr = "";
                }
                else
                    chunkStr = chunkStr.Substring(match.Length + 1, chunkStr.Length - match.Length - 1);

                chunkerDS.Chunks.Add(chunk);
            }
            return chunkerDS;
        } 
    }
}
