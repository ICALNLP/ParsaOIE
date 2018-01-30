using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Accord;
using RahatCoreNlp.Data;
using RahatCoreNlp.OieParsa;
using RahatCoreNlp.Service;
using RahatCoreNlp.Utility;

namespace RahatCoreNlp.Seraji
{
    public static class ParsPer
    {
        public static string BatchParseFast(List<string> sentences)
        {
            StringBuilder conllFormatReadyForParsing = new StringBuilder();
            for (int j = 0; j < sentences.Count; j++)
            {
                // string normalizedText = Normalizer.Normalize(sentences[j]);
                // to speed up use hazm normalizer
                string normalizedText = HazmService.Normalizer(sentences[j]);

                string[] tokens = Tokenizer.GetTokens(normalizedText);

                string[] wordTags = TagPer.GetTags(tokens);
                for (int i = 0; i < wordTags.Length; i++)
                {
                    if (wordTags[i] == "")
                        continue;
                    string[] splited = wordTags[i].Split('\t');
                    string finePosTags = splited[1];
                    // exctract course pos tag from fine pos tag
                    // example finePosTag:N_SING    -> course tag: N
                    // example finePosTag:V_PRS    -> course tag: V
                    string coursePosTag = finePosTags;
                    string[] splitTag = finePosTags.Split('_');
                    if (splitTag.Length > 1)
                    {
                        coursePosTag = splitTag[0];
                    }
                    conllFormatReadyForParsing.Append(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\n"
                    , i + 1 /*0*/, splited[0]/*1*/, "_"/*2*/, "_"/*3*/, coursePosTag/*4*/, "_"/*adding fine Postags here will ruine the results! 5*/, "_"/*6*/, "_", "_", "_", "_", "_", "_"/*12*/));
                }

                conllFormatReadyForParsing.Append("\n"); // be ready for the next sentence
            }
            string result = Parse(conllFormatReadyForParsing.ToString(), false);
            return result;
        }

        public static string Parse(string text, bool doPreProcess = true)
        {
            StringBuilder fileContent = new StringBuilder();
            if (doPreProcess)
            {
                string normalizedText = Normalizer.Normalize(text);
                string[] tokens = Tokenizer.GetTokens(normalizedText);
                string[] wordTags = TagPer.GetTags(tokens);
                for (int i = 0; i < wordTags.Length; i++)
                {
                    if (wordTags[i] == "")
                        continue;
                    string[] splited = wordTags[i].Split('\t');
                    string finePosTags = splited[1];
                    // exctract course pos tag from fine pos tag
                    // example finePosTag:N_SING    -> course tag: N
                    // example finePosTag:V_PRS    -> course tag: V
                    string coursePosTag = finePosTags;
                    string[] splitTag = finePosTags.Split('_');
                    if (splitTag.Length > 1)
                    {
                        coursePosTag = splitTag[0];
                    }
                    fileContent.Append(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\n"
                    , i + 1 /*0*/, splited[0]/*1*/, "_"/*2*/, "_"/*3*/, coursePosTag/*4*/, "_"/*adding fine Postags here will ruine the results! 5*/, "_"/*6*/, "_", "_", "_", "_", "_", "_"/*12*/));
                }
            }
            else
                fileContent.Append(text);

            var utf8WithoutBom = new UTF8Encoding(false);
            //            var executionPath = ConfigurationManager.AppSettings["ExecutionPath"];
            var executionPath = File.ReadAllText(@"c:\executionPath.txt");
            var ticks = DateTime.Now.Ticks;
            File.WriteAllText(executionPath + $"../../../../ParsPer/input{ticks}.conll", fileContent.ToString(), utf8WithoutBom);
            File.Delete(executionPath + $"../../../../ParsPer/output{ticks}.conll");

            // run java command. first I change directory to YaraParser. then run command. then wait for 3 second for user to read the command prompet.
            var dirPath = Path.GetFullPath(executionPath + "../../../../ParsPer/");
            string strCmdText;
            strCmdText = "/C cd " + dirPath + $" & java -Xmx1G -cp anna-3.61.jar is2.parser.Parser -model model-all -test input{ticks}.conll -out output{ticks}.conll & ping 127.0.0.1 -n 3 > nul";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = strCmdText,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = dirPath
            };
            using (Process p = Process.Start(startInfo))
            {
                try
                {
                    p.WaitForExit();
                }
                catch (Exception)
                {

                    throw;
                }
            }
            string parsedString = File.ReadAllText(executionPath + $"../../../../ParsPer/output{ticks}.conll");
            return parsedString;
        }

        public enum PersianDependencyType
        {
            LeaveIntact,         // leave parsedStructure intact.
            CollapsedKeepPp,  // collapse dependencies keep pp nodes
            CollapsedDropPp     // collapse dependencies drop pp nodes
        }

        public static void RemoveTokenFromParsedStructureAt(List<DependencyParseNode> parsedStructure, int token_remove_address)
        {
            // removes token at token_address + traverses all nodes and shifts dependents properly

            parsedStructure.RemoveAt(token_remove_address);

            foreach (var dependencyParseNode in parsedStructure)
            {
                if (dependencyParseNode.Address > token_remove_address)
                {
                    // the token addresses need to be updated too
                    dependencyParseNode.Address--;
                }

                for (int i = 0; i < dependencyParseNode.Deps.Count; i++)
                {
                    var keyValuePair = dependencyParseNode.Deps[i];
                    if (keyValuePair.Value == token_remove_address)
                    {
                        // this token is removed. remove its dependencies too.
                        dependencyParseNode.Deps.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (keyValuePair.Value > token_remove_address)
                    {
                        // shift back dep poiters to handle token removal in the structure
                        dependencyParseNode.Deps[i] = new KeyValuePair<string, int>(keyValuePair.Key,
                            keyValuePair.Value - 1);
                    }
                }
            }
        }

        public static List<string> CheckCollapsingExceptions(List<DependencyParseNode> parsedStructure)
        {
            // for Perpositions check if any structure other than {node --prep--> PP --pobj--> node} or {node --prep--> PP / --mwe--> node / --pobj--> node} is there throw exception
            List<string> exceptionsTokens = new List<string>();
            for (int j = 0; j < parsedStructure.Count; j++)
            {
                var dependencyParseNode = parsedStructure[j];
                for (int i = 0; i < dependencyParseNode.Deps.Count; i++)
                {
                    var dep = dependencyParseNode.Deps[i];
                    if (dep.Key == "prep")
                    {
                        bool isException = false;
                        for (int k = 0; k < parsedStructure[dep.Value].Deps.Count; k++)
                        {
                            var pp = parsedStructure[dep.Value].Deps[k];
                            if (pp.Key != "pobj" && pp.Key != "mwe")
                                isException = true;
                        }
                        if (isException)
                            exceptionsTokens.Add(" abnormal pp token possible exception at token: " + dep.Value + " in collasping.");

                        bool havePobjLink = false;
                        for (int k = 0; k < parsedStructure[dep.Value].Deps.Count; k++)
                        {
                            if (parsedStructure[dep.Value].Deps[k].Key == "pobj")
                            {
                                havePobjLink = true;
                                break;
                            }
                        }
                        if (!havePobjLink)
                            exceptionsTokens.Add(" prep link wothout pobj. exception at token: " + dep.Value + " in collasping.");

                    }
                }
            }
            return exceptionsTokens;
        }

        public static void CollapseDependencies(List<DependencyParseNode> parsedStructure, PersianDependencyType type = PersianDependencyType.LeaveIntact)
        {
            List<int> nodesToRemove = new List<int>();

            // input and output are a CoNLL_sentence. in output some dependencies are collapsed like conj, ccomp, prep
            if (type == PersianDependencyType.LeaveIntact)
                return;

            // Hueristic modifications on tree by Mahmoud

            // first I change all prep-lvc links to lvc!
            for (int j = 0; j < parsedStructure.Count; j++)
            {
                var dependencyParseNode = parsedStructure[j];
                for (int i = 0; i < dependencyParseNode.Deps.Count; i++)
                {
                    var keyValuePair = dependencyParseNode.Deps[i];
                    if (keyValuePair.Key == "prep-lvc")
                    {
                        dependencyParseNode.Deps[i] = new KeyValuePair<string, int>("lvc", dependencyParseNode.Deps[i].Value);
                    }
                }
            }

            var exceptions = CheckCollapsingExceptions(parsedStructure);
            if (exceptions.Count > 0)
                foreach (var exception in exceptions)
                {
                    string sentence = string.Join(" ", parsedStructure.Select(x => x.UnicodeWord).ToArray());
                    Console.WriteLine("my exception: " + exception + " sentence: " + sentence);
                }

            #region Handle Prepositions
            for (int j = 0; j < parsedStructure.Count; j++)
            {
                var dependencyParseNode = parsedStructure[j];
                for (int i = 0; i < dependencyParseNode.Deps.Count; i++)
                {
                    var keyValuePair = dependencyParseNode.Deps[i];
                    if (keyValuePair.Key == "prep" && !keyValuePair.Key.Contains("prep^"))
                    {
                        List<KeyValuePair<string, int>> links2Move2PobjNode = new List<KeyValuePair<string, int>>();
                        var prepositionNode = parsedStructure[keyValuePair.Value];
                        // concat preposition nodes
                        string concatedPp = prepositionNode.UnicodeWord;
                        int pobjLinkAddress = -2; // -2 is error. This should not happen!

                        for (int k = 0; k < prepositionNode.Deps.Count; k++)
                        {
                            var valuePair = prepositionNode.Deps[k];
                            if (valuePair.Key.Contains("pobj"))
                            {
                                pobjLinkAddress = valuePair.Value;
                                continue;
                            }

                            if (valuePair.Key == "mwe")
                            {
                                // to solve mwe links like سرمربی تیم ملی والیبال ایران بعد از باخت برابر آمریکا به تغییر در ترکیب حریف اشاره کرد و گفت : آمریکا با به میدان فرستادن « راسل » ما را سورپرایز کرد .
                                concatedPp += "^" + parsedStructure[valuePair.Value].UnicodeWord;
                                if (type == PersianDependencyType.CollapsedDropPp)
                                {
                                    nodesToRemove.Add(valuePair.Value);
                                }
                                continue;
                            }

                            // here we handle sentences like below. we move punct and conj links to pobj.
                            //حتی وقتی دنیای تصویر نیز نمی‌تواند جوابگوی ذهن سرشار از ذوق ، خلاقیت و نوآوری‌اش باشد ، به نوشتن روی می‌آورد .
                            //o	از پشت شیشه نگاه می‌کند به : آب‌نبات ، شکلات ، کیت‌کت ، و نگاهش روی گز ثابت می‌ماند.
                            links2Move2PobjNode.Add(new KeyValuePair<string, int>(valuePair.Key, valuePair.Value));
                            prepositionNode.Deps.RemoveAt(k);
                            k--;
                        }
                        if (type == PersianDependencyType.CollapsedDropPp)
                        {
                            nodesToRemove.Add(keyValuePair.Value);
                        }

                        if (pobjLinkAddress != -2)
                        {
                            dependencyParseNode.Deps.Add(new KeyValuePair<string, int>("prep^" + concatedPp,
                                pobjLinkAddress));
                            foreach (var punctAndconjLink in links2Move2PobjNode)
                            {
                                parsedStructure[pobjLinkAddress].Deps.Add(new KeyValuePair<string, int>(punctAndconjLink.Key, punctAndconjLink.Value));
                                Console.WriteLine("my exception: moving " + punctAndconjLink.Key + " link from to node " + punctAndconjLink.Value + " sentence: " + parsedStructure.Skip(1).Select(x => x.UnicodeWord));
                            }
                        }
                    }
                }
            }

            if (type == PersianDependencyType.CollapsedDropPp)
            {
                // remove unwanted (prep + deps) nodes.

                nodesToRemove.Sort();
                for (int i = nodesToRemove.Count - 1; i >= 0; i--)
                {
                    RemoveTokenFromParsedStructureAt(parsedStructure, nodesToRemove[i]);
                }
            }
            #endregion

            #region propagate subject for conjuct + ccomp verbs
            for (int j = 0; j < parsedStructure.Count; j++)
            {
                var dependencyParseNode = parsedStructure[j];
                for (int i = 0; i < dependencyParseNode.Deps.Count; i++)
                {
                    var keyValuePair = dependencyParseNode.Deps[i];
                    if (keyValuePair.Key == "conj" || keyValuePair.Key == "ccomp")
                    {
                        var governer = dependencyParseNode;
                        // check if governer node has subjuct link
                        DependencyParseNode subject = null;
                        int s;
                        for (s = 0; s < governer.Deps.Count; s++)
                        {
                            var valuePair = governer.Deps[s];
                            if (valuePair.Key == "nsubj" || valuePair.Key == "nsubjpass")
                            {
                                subject = parsedStructure[valuePair.Value];
                                break;
                            }
                        }
                        if (subject != null)
                        {
                            var dependent = parsedStructure[keyValuePair.Value];
                            if (dependent.CTag == "V") // for some sentences, the verb has false conj with noun like من هم سیب خوردم و هم موز
                            {
                                // make sure dependent verb does not have it's own subj like سرمربی تیم ملی والیبال ایران بعد از باخت برابر آمریکا به تغییر در ترکیب حریف اشاره کرد و گفت : آمریکا با به میدان فرستادن « راسل » ما را سورپرایز کرد .
                                bool hasSubject = false;
                                foreach (var valuePair in dependent.Deps)
                                {
                                    if (valuePair.Key == "nsubj" || valuePair.Key == "nsubjpass")
                                    {
                                        hasSubject = true;
                                        break;
                                    }
                                }
                                if (!hasSubject)
                                {

                                    // make dependent verb string. if dependent node has aux or auxpass link, add them to the verb string
                                    List<DependencyParseNode> verb = new List<DependencyParseNode>();
                                    verb.Add(dependent);
                                    foreach (var valuePair in dependent.Deps)
                                    {
                                        if (valuePair.Key == "aux" || valuePair.Key == "auxpass")
                                            verb.Add(parsedStructure[valuePair.Value]);
                                    }

                                    string verbText = "";
                                    // get verb text
                                    OieTreePattern.GetOrderedStringFromListOfDeps(verb, out verbText);

                                    // find person for verb and subject. if both found, check they match, otherwise ignore propagation
                                    VerbProcessor.Person verbPerson = VerbProcessor.GetPersonAndNumberFromVerbPersonalEnding
                                        (
                                            VerbProcessor.GetVerbPersonalEnding(verbText));
                                    VerbProcessor.Person subjectPerson =
                                        PhraseProcessor.GetNounPhrasePersonAndNumber(subject.UnicodeWord);
                                    bool propagateSubject = verbPerson.Value == VerbProcessor.Person.UNKNOWN.Value ||
                                        subjectPerson.Value == VerbProcessor.Person.UNKNOWN.Value;
                                    if (verbPerson.Value == subjectPerson.Value)
                                        propagateSubject = true;

                                    if (propagateSubject)
                                        dependent.Deps.Add(new KeyValuePair<string, int>(governer.Deps[s].Key,
                                            governer.Deps[s].Value));
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Handle conjuctions

            // to remove cc links when we have conj, and remove punct link when we have conj

            if (type == PersianDependencyType.CollapsedDropPp)
            {
                for (int j = 0; j < parsedStructure.Count; j++)
                {
                    int ccIdx = -1;
                    int conjIdx = -1;
                    int punctIdx = -1;
                    var dependencyParseNode = parsedStructure[j];
                    for (int i = 0; i < dependencyParseNode.Deps.Count; i++)
                    {
                        var keyValuePair = dependencyParseNode.Deps[i];
                        if (keyValuePair.Key == "cc")
                            ccIdx = i;
                        if (keyValuePair.Key == "punct" /*&& parsedStructure[keyValuePair.Value].UnicodeWord == "," || parsedStructure[keyValuePair.Value].UnicodeWord == "،" || parsedStructure[keyValuePair.Value].UnicodeWord == "؛"*/)
                            punctIdx = i;
                        if (keyValuePair.Key.Contains("conj") || keyValuePair.Key.Contains("ccomp"))
                            conjIdx = i;
                    }
                    if (conjIdx != -1 && ccIdx != -1)
                    {
                        dependencyParseNode.Deps[conjIdx] = new KeyValuePair<string, int>(dependencyParseNode.Deps[conjIdx].Key + "^" + parsedStructure[dependencyParseNode.Deps[ccIdx].Value].UnicodeWord, dependencyParseNode.Deps[conjIdx].Value);
                        RemoveTokenFromParsedStructureAt(parsedStructure, dependencyParseNode.Deps[ccIdx].Value);
                    }
                    else if (conjIdx != -1 && punctIdx != -1)
                    {
                        dependencyParseNode.Deps[conjIdx] = new KeyValuePair<string, int>(dependencyParseNode.Deps[conjIdx].Key + "^" + parsedStructure[dependencyParseNode.Deps[punctIdx].Value].UnicodeWord, dependencyParseNode.Deps[conjIdx].Value);
                        RemoveTokenFromParsedStructureAt(parsedStructure, dependencyParseNode.Deps[punctIdx].Value);
                    }
                }
            }

            #endregion

            #region handle acc dobj links

            if (type == PersianDependencyType.CollapsedDropPp)
            {
                for (int j = 0; j < parsedStructure.Count; j++)
                {
                    int accIdx = -1;
                    var dependencyParseNode = parsedStructure[j];
                    for (int i = 0; i < dependencyParseNode.Deps.Count; i++)
                    {
                        // first find dobj node
                        var dobjNode = dependencyParseNode.Deps[i];
                        if(dobjNode.Key.Contains("dobj"))
                        {
                            // second find acc link
                            for(int k = 0; k < parsedStructure[dobjNode.Value].Deps.Count; k++)
                            {
                                var accNode = parsedStructure[dobjNode.Value].Deps[k];
                                if (accNode.Key == "acc")
                                {
                                    accIdx = accNode.Value;
                                    dependencyParseNode.Deps[i] = new KeyValuePair<string, int>(dependencyParseNode.Deps[i].Key + "^" + parsedStructure[accNode.Value].UnicodeWord, dependencyParseNode.Deps[i].Value);
                                }
                            }
                        }
                    }
                    if (accIdx != -1)
                        RemoveTokenFromParsedStructureAt(parsedStructure, accIdx);
                }
            }

            #endregion

        }
    }
}
