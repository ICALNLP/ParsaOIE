using System.Collections.Generic;
using System.Linq;
using RahatCoreNlp.Data;

namespace RahatCoreNlp.OieParsa
{
    public class OieTreePattern
    {
        // returns Sentence list of matchings
        static List<Dictionary<string, int>> MatchTree(List<PatternNode> SentneceTree, List<PatternNode> PatternTree, ref List<int> matchedSentneceIdx, List<DependencyParseNode> parsedStructure)
        {
            // here I keep all the results.
            List<Dictionary<string, int>> results = new List<Dictionary<string, int>>();

            // I need first element of paternTree afterwards
            PatternNode PatternTreeFirstNode = PatternTree[0];
            //PatternTree.RemoveAt(0);

            // skip sentnece root root.
            for (int i = 1; i < SentneceTree.Count(); i++)
            {
                // in this case we would find no match so skip it.
                if (SentneceTree[i].childs.Count() < PatternTreeFirstNode.childs.Count())
                    continue;

                if (PatternTreeFirstNode.POS.Count() == 0 || PatternTreeFirstNode.POS.Contains(SentneceTree[i].POS[0]))
                {
                    GeneralTreeMatching TreeMatching = new GeneralTreeMatching();
                    List<List<PairedNodes>> FullMatchingsList = new List<List<PairedNodes>>();
                    TreeMatching.DoMatching(SentneceTree[i], PatternTree[0], ref FullMatchingsList, parsedStructure);
                    List<Dictionary<string, int>> matching = OieConvert.OieConvertor.ConvertGeneralTreeMatchings2TreeInclusionMatching(FullMatchingsList);

                    // to fix bug from sentence : آدولف بورن ، طراح ، کاریکاتوریست ، تصویرساز و نقاش در ۱۹۳۰ در شهر بودجویس از جمهوری چک به دنیا آمد .
                    // instead of adding matched indexes to matchedSentneceIdx, I first add them to a temp variable named 
                    // tempMatchedSentneceIdx to let all the current matchings happen. after current matching
                    // no other new rule should be able to match on this index. all the matching with the same rule
                    // on this index are good.
                    List<int> tempMatchedSentneceIdx = new List<int>();
                    foreach(Dictionary<string, int> onematch in matching)
                    {
                        bool skipThisMatching = false;
                        // on position i I found Sentence match on this node in the sentence.
                        // make sure not to continue matching lower order patterns for node i
                        foreach (KeyValuePair<string, int> entry in onematch)
                        {
                            // The head is the place in the extraction pattern that we add it the matched index and we do not match any other lower order pattern in the package.
                            // The head matching name has a dot at the begining.
                            if (entry.Key[0] == '.')
                            {
                                if (matchedSentneceIdx.Contains(entry.Value))
                                {
                                    // If one pattern inside Sentence package matches correctly with Sentence sentence node
                                    // remaining patterns inside that packages must be skipped for that node
                                    // since for example if the overt subject is inside sentencen
                                    // we do not want the prime version of that relation produces any
                                    // extraction. (footnote: prime version is the same but without 
                                    // overt subjecy and with Verb Personal Ending as subject instead.)
                                    skipThisMatching = true;
                                    break;
                                }

                                tempMatchedSentneceIdx.Add(entry.Value);
                            }
                        }
                        if (skipThisMatching)
                            continue;

                        results.Add(onematch);
                    }

                    // now I add every index back to matchedSentneceIdx to stop any new pattern
                    // inside this package to match with these indexes. WOW. Huge Bug fixed
                    foreach (int index in tempMatchedSentneceIdx)
                        matchedSentneceIdx.Add(index);
                }
            }
            return results;
        }

        public static string ExpandNode(List<DependencyParseNode> parsedStructure, ExtractionNode baseNode, Dictionary<string, int> match, out List<int> argumentTokensIdx)
        {
            string result = "";
            argumentTokensIdx = new List<int>();
            foreach (string variable in baseNode.names)
            {
                if (ReadPatternsFile.myLiterals.Keys.Contains(variable))
                {
                    // no expansion is required
                    result += ReadPatternsFile.myLiterals[variable] + " ";
                    if (variable == "<ISA>")
                        argumentTokensIdx.Add(-3);
                    if (variable == "<TOBE>")
                        argumentTokensIdx.Add(-2);
                    if (variable == "VAPE")
                        argumentTokensIdx.Add(-1);
                    continue;
                }
                List<DependencyParseNode> expandedNode = new List<DependencyParseNode>();
                FindAllChildDeps(parsedStructure[match[variable]], parsedStructure, expandedNode, (baseNode.expands.Keys.Contains(variable) ? baseNode.expands[variable] : null), (baseNode.notExpands.Keys.Contains(variable) ? baseNode.notExpands[variable] : null), (baseNode.lexExpand.Keys.Contains(variable) ? baseNode.lexExpand[variable] : null), (baseNode.lexNotExpand.Keys.Contains(variable) ? baseNode.lexNotExpand[variable] : null));
                string expandedNodeString;
                List<int> temp = GetOrderedStringFromListOfDeps(expandedNode, out expandedNodeString);
                argumentTokensIdx.AddRange(temp);
                result += expandedNodeString + " ";
            }
            return result.Trim();
        }

        public static List<int> GetOrderedStringFromListOfDeps(List<DependencyParseNode> phrase, out string argumentText)
        {
            string output = "";
            List<int> argumentTokensIdx = new List<int>();
            // make a copy from phrase
            List<DependencyParseNode> temp = new List<DependencyParseNode>(phrase);

            while (temp.Count > 0)
            {
                //find minimum Address node inside temp
                int minIdx = 0;
                int minVal = temp[0].Address;
                for (int i = 1; i < temp.Count; i++)
                {
                    if (temp[i].Address < minVal)
                    {
                        minVal = temp[i].Address;
                        minIdx = i;
                    }
                }
                if (output != "")
                    output += " ";
                output += temp[minIdx].UnicodeWord;
                argumentTokensIdx.Add(temp[minIdx].Address);
                temp.RemoveAt(minIdx);
            }
            argumentText = output;
            return argumentTokensIdx;
        }

        public static void FindAllChildDeps(DependencyParseNode baseNode, List<DependencyParseNode> parsedStructure, List<DependencyParseNode> discoveredChildDeps, List<string> includeRelationsList, List<string> excludeRelationsList, List<string> includeLex, List<string> excludeLex)
        {
            // this condition is added to handle multiple parent in graph. some nodes have multiple parents and
            // may come to final results twice!
            // example sentnece and output is 
            // The messages will be "unwrapped" by sculptor Richard Wentworth, who is responsible for decorating the tree with broken plates and light bulbs.
            // and output relations from [7,1] 0.000 (sculptor Richard Wentworth , <-> is responsible <-> for decorating the tree with broken plates and light bulbs)
            // and template {arg1 {.rel link-nsubjAcl {arg2 link-advcl}}}	(arg1 cat-aclNotExpand nexp-case; .rel cat-verb; arg2)
            if (!discoveredChildDeps.Contains(baseNode))
                discoveredChildDeps.Add(baseNode);
            foreach (KeyValuePair<string, int> child in baseNode.Deps)
            {
                if (excludeLex != null && excludeLex.Contains(parsedStructure[child.Value].UnicodeWord))
                {
                    // if the child lex is inside excludeLex
                    continue;
                }
                if (includeLex != null && includeLex.Contains(parsedStructure[child.Value].UnicodeWord))
                {
                    FindAllChildDeps(parsedStructure[child.Value], parsedStructure, discoveredChildDeps, null,
                        null, includeLex, excludeLex);
                }

                if (excludeRelationsList != null && excludeRelationsList.Contains(child.Key))
                {
                    // if the child Rel2Head is inside excludeList
                    continue;
                }

                if (includeRelationsList != null)
                {
                    if (includeRelationsList.Contains(child.Key))
                    {
                        // if the child Rel2Head is inside includList
                        FindAllChildDeps(parsedStructure[child.Value], parsedStructure, discoveredChildDeps, null,
                            null, includeLex, excludeLex);
                    }
                }
                else
                {
                    // find all childs with no limitation
                    FindAllChildDeps(parsedStructure[child.Value], parsedStructure, discoveredChildDeps, includeRelationsList,
                        excludeRelationsList, includeLex, excludeLex);
                }
            }
        }

        public static void FindAllChildDepsCasscakeIncludeExclude(DependencyParseNode baseNode, List<DependencyParseNode> parsedStructure, List<DependencyParseNode> discoveredChildDeps, List<string> includeRelationsList, List<string> excludeRelationsList)
        {
            discoveredChildDeps.Add(baseNode);
            foreach (KeyValuePair<string, int> child in baseNode.Deps)
            {
                if (excludeRelationsList != null && excludeRelationsList.Contains(child.Key))
                {
                    // if the child Rel2Head is inside excludeList
                    continue;
                }
                else if (includeRelationsList != null)
                {
                    if (includeRelationsList.Contains(child.Key))
                    {
                        // if the child Rel2Head is inside includList
                        FindAllChildDepsCasscakeIncludeExclude(parsedStructure[child.Value], parsedStructure, discoveredChildDeps, includeRelationsList,
                            excludeRelationsList);
                    }
                }
                else
                {
                    // find all childs with no limitation
                    FindAllChildDepsCasscakeIncludeExclude(parsedStructure[child.Value], parsedStructure, discoveredChildDeps, includeRelationsList,
                        excludeRelationsList);
                }
            }
        }


        public static void SimplifyComplexDepRelations(ref List<DependencyParseNode> parsedStructure)
        {
            for (int i = 0; i < parsedStructure.Count(); i++)
            {
                for (int j = 0; j < parsedStructure[i].Deps.Count(); j++)
                {
                    string [] tokens = parsedStructure[i].Deps[j].Key.Split('-');
                    if (tokens.Length > 1)
                        parsedStructure[i].Deps[j] = new KeyValuePair<string, int>(tokens[1], parsedStructure[i].Deps[j].Value);
                    // the slash (/) show that this token has an Enclitic. پدرش، مادرش، خواهشمندیم و ...
                    // the first label is the head and the second label is the enclitic label
                    string[] tokens2 = parsedStructure[i].Deps[j].Key.Split('/');
                    if (tokens2.Length > 1)
                        parsedStructure[i].Deps[j] = new KeyValuePair<string, int>(tokens2[0], parsedStructure[i].Deps[j].Value);
                }
            }
        }

        public static List<OieRelation> ExtractRelations(List<DependencyParseNode> parsedStructure, List<PatternPackage> patterns, bool CalcConfidenceScore = true)
        {
            SimplifyComplexDepRelations(ref parsedStructure);
            List<OieRelation> relations = new List<OieRelation>();
            List<PatternNode> SentneceTree = OieConvert.OieConvertor.ConvertParseTree2OiePatternTree(parsedStructure);
            // loop over all pattern packages
            for (int i = 0; i < patterns.Count(); i++)
            {
                // this list says that for the current sentnece and pattern packages which nodes 
                // are allready matched with Sentence higher order pattern. we skip these nodes inside
                // MatchTree. To handle the abbility to skip lower order patterns when Sentence
                // higher order pattern alreadyy matched in that palce of sentence
                List<int> matchedSentneceIdx = new List<int>();
                // loop over all patterns inside Sentence pattern package
                for (int k = 0; k < patterns[i].patterns.Count(); k++)
                {
                    MatchExtractTuple tuple = patterns[i].patterns[k];
                    // convert pattern tree to Sentence list
                    List<PatternNode> patternList = new List<PatternNode>();
                    // first list member is pattern root
                    patternList.Add(tuple.MatchPattern);
                    for (int j = 0; j < tuple.MatchPattern.childs.Count(); j++)
                    {
                        patternList.Add(tuple.MatchPattern.childs[j]);
                    }
                    List<Dictionary<string, int>> matches = MatchTree(SentneceTree, patternList, ref matchedSentneceIdx, parsedStructure);

                    foreach (Dictionary<string, int> match in matches)
                    {
                        OieRelation relation = new OieRelation();
                        relation.rel = ExpandNode(parsedStructure, tuple.ExtractionPattern.rel, match, out relation.relTokensIdx);
                        if (tuple.ExtractionPattern.rel.names.Contains("<TOBE>") || tuple.ExtractionPattern.rel.names.Contains("<ISA>"))
                            relation.RelationHasGeneratedParts = true;
                        relation.arg1 = ExpandNode(parsedStructure, tuple.ExtractionPattern.arg1, match, out relation.arg1TokensIdx);
                        relation.arg2 = ExpandNode(parsedStructure, tuple.ExtractionPattern.arg2, match, out relation.arg2TokensIdx);
                        relation.matchedPatternLine = i + 1;
                        relation.matchedPatternIndex = k + 1;
                        // if we trained the confidence function. Go ahead and use it to compute the relation score.
                        if (ConfidenceLevel.ConfidenceFunctionPersian.regression != null && CalcConfidenceScore)
                        {
                            FeatureExtractorClPersian fe = new FeatureExtractorClPersian();
                            double[] feature = fe.ExtractFeatures(relation, parsedStructure);
                            relation.confidenceScore = ConfidenceFunctionPersian.regression.Compute(feature);
                        }
                        else relation.confidenceScore = 0;

                        // to keep the parsed structure of the original sentnece
                        relation.SentenceParsedStructure = parsedStructure;

                        relations.Add(relation);
                    }
                }
            }

            #region extract Hearst Relations and append to the results

            var hearstPatternMatcher = new HearstPatternMatcher();
            List<OieRelation> hearstRelations = hearstPatternMatcher.Match(parsedStructure);
            relations.AddRange(hearstRelations);
            #endregion

            // sort list of relations based on confidence score.
            List <OieRelation> sortedList = relations.OrderByDescending(o => o.confidenceScore).ToList();
            return sortedList;
        }


        public static List<OieRelation> ExtractRelationsEnglish(List<DependencyParseNode> parsedStructureBasic,
            List<DependencyParseNode> parsedStructureCcProcessed, List<PatternPackage> patterns,
            bool CalcConfidenceScore = true, bool movePrepositionFromArg2rel = true)
        {
            List<OieRelation> relations = new List<OieRelation>();

            if (parsedStructureBasic[parsedStructureBasic.Count - 1].UnicodeWord == "?")
            {
                // this sentence is a question. so it has no information
                return relations;
            }

            List<PatternNode> sentneceTreeBasic =
                OieConvert.OieConvertor.ConvertParseTree2OiePatternTree(parsedStructureBasic);
            List<PatternNode> sentneceTreeCCprocessed =
                OieConvert.OieConvertor.ConvertParseTree2OiePatternTree(parsedStructureCcProcessed);

            List<PatternNode> sentneceTree;
            List<DependencyParseNode> parsedStructure = null;

            // loop over all pattern packages
            for (int i = 0; i < patterns.Count(); i++)
            {
                // this list says that for the current sentnece and pattern packages which nodes 
                // are allready matched with Sentence higher order pattern. we skip these nodes inside
                // MatchTree. To handle the abbility to skip lower order patterns when Sentence
                // higher order pattern alreadyy matched in that palce of sentence
                List<int> matchedSentneceIdx = new List<int>();
                // loop over all patterns inside Sentence pattern package
                for (int k = 0; k < patterns[i].patterns.Count(); k++)
                {
                    MatchExtractTuple tuple = patterns[i].patterns[k];
                    // convert pattern tree to Sentence list
                    List<PatternNode> patternList = new List<PatternNode>();
                    // first list member is pattern root
                    patternList.Add(tuple.MatchPattern);
                    for (int j = 0; j < tuple.MatchPattern.childs.Count(); j++)
                    {
                        patternList.Add(tuple.MatchPattern.childs[j]);
                    }
                    if (tuple.MatchPattern.targetParsedStructure == 'B')
                    {
                        sentneceTree = sentneceTreeBasic;
                        parsedStructure = parsedStructureBasic;
                    }
                    else
                    {
                        sentneceTree = sentneceTreeCCprocessed;
                        parsedStructure = parsedStructureCcProcessed;
                    }

                    List<Dictionary<string, int>> matches = MatchTree(sentneceTree, patternList, ref matchedSentneceIdx,
                        parsedStructure);

                    foreach (Dictionary<string, int> match in matches)
                    {
                        OieRelation relation = new OieRelation();
                        relation.rel = ExpandNode(parsedStructure, tuple.ExtractionPattern.arg2, match,
                            out relation.relTokensIdx);
                        relation.arg1 = ExpandNode(parsedStructure, tuple.ExtractionPattern.arg1, match,
                            out relation.arg1TokensIdx);
                        relation.arg2 = ExpandNode(parsedStructure, tuple.ExtractionPattern.rel, match,
                            out relation.arg2TokensIdx);
                        relation.matchedPatternLine = i + 1;
                        relation.matchedPatternIndex = k + 1;

                        relations.Add(relation);
                    }
                }
            }

            #region extract Hearst Relations and append to the results

            // not implemented yet for English

            #endregion

            #region omit duplicate relations

            for (int write = 0; write < relations.Count; write++)
            {
                for (int sort = 0; sort < relations.Count - 1; sort++)
                {
                    if(write == sort)
                        continue;

                    if (relations[write].IsEqual(relations[sort]))
                    {
                        relations[write].matchedPatternIndex = relations[write].matchedPatternIndex*1000 +
                                                               relations[sort].matchedPatternIndex;
                        relations[write].matchedPatternLine = relations[write].matchedPatternLine * 1000 +
                                                               relations[sort].matchedPatternLine;
                        relations.RemoveAt(sort);
                    }
                }
            }
            #endregion

            #region move preposition from beginning of arg2 to end of rel phrase
            if (movePrepositionFromArg2rel)
            {
                // here we move pp's from begining of the arg2 to end of relation phrase
                // like in "I saw him at school."  (I ,at school ,saw him) -> (I ,school ,saw him at)
                foreach (var oieRelation in relations)
                {
                    if (oieRelation.relTokensIdx[0] > 0 /* to make sure it is not a TOBE or something costum-made like that*/
                        && oieRelation.arg2TokensIdx.Count > 0
                        && (parsedStructure[oieRelation.arg2TokensIdx[0]].CTag == "IN"/*Part-of-speech tagging for prepositional phrase*/
                        || parsedStructure[oieRelation.arg2TokensIdx[0]].CTag == "TO"/*Part-of-speech tagging for <to>*/)
                        && parsedStructure[oieRelation.arg2TokensIdx[0]].UnicodeWord != "if" /*to stop if from moving in this sentence <It said the production could go ahead if the organisers "expunge all the offending parts".>*/)
                    {
                        // now we move the first token of arg2 to end of relation phrase
                        oieRelation.relTokensIdx.Add(oieRelation.arg2TokensIdx[0]);
                        oieRelation.arg2TokensIdx.RemoveAt(0);
                        string[] tokens = oieRelation.arg2.Split(' ');
                        oieRelation.arg2 = string.Join(" ", tokens.Skip(1).Select(x => x).ToArray()).Trim();
                        string ppToken = tokens[0];
                        oieRelation.rel += " " + ppToken;
                    }
                }
            }
            #endregion

            #region compute confidence score            

            foreach (var oieRelation in relations)
            {
                if (ConfidenceFunctionPersian.regression != null && CalcConfidenceScore)
                {
                    FeatureExtractorClEnglish fe = new FeatureExtractorClEnglish();
                    Dictionary<string, double> feature = fe.ExtractFeatures(oieRelation, parsedStructure);
                    oieRelation.confidenceScore = ConfidenceFunctionEnglish.regression.Compute(feature.Values.ToArray());
                }
                else oieRelation.confidenceScore = 0;
            }

            #endregion

            // sort list of relations based on confidence score.
            List<OieRelation> sortedList = relations.OrderByDescending(o => o.confidenceScore).ToList();
            return sortedList;
        }

    }
}
