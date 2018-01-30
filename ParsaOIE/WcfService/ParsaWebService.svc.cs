using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using FSM_Simple;
using RahatCoreNlp.Data;
using RahatCoreNlp.FiniteStateMachines;
using RahatCoreNlp.OieConvert;
using RahatCoreNlp.OieParsa;
using RahatCoreNlp.Seraji;
using RahatCoreNlp.Service;
using RahatCoreNlp.Summarization.Second_Approach;
using RahatCoreNlp.TFIDF;
using RahatCoreNlp.Utility;

namespace WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ParsaWebService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ParsaWebService.svc or ParsaWebService.svc.cs at the Solution Explorer and start debugging.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]

    public class ParsaWebService : IParsaWebService
    {
        private string processName = "ParsaWebService";
        public string Parsa_Oie(string text)
        {
            // first parse input sentence
            string parsedString = ParsPer.Parse(text);
            List<DependencyParseNode> parsedStructure = DependencyParser.LoadCoNLL09FormatSentence(parsedString);

            // then match OiePatterns to sentence
            List<OieRelation> relations = OieTreePattern.ExtractRelations(parsedStructure,
                ReadPatternsFile.ReadFromFile("c:\\Parsa-OIE-Patterns-Farsi.txt"));

            StringBuilder result = new StringBuilder();

            foreach (OieRelation relation in relations)
            {
                result.Append(string.Format("[{0},{1}] {5:0.000} ({2} <-> {3} <-> {4})\n",
                    relation.matchedPatternLine, relation.matchedPatternIndex, relation.arg1, relation.arg2,
                    relation.rel, relation.confidenceScore));
            }


            return result.ToString();
        }
    }
}
