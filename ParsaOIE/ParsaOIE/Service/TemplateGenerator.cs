using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Text.RegularExpressions;
using RahatCoreNlp.Data;
using RahatCoreNlp.DB;
using RahatCoreNlp.UI;
using RahatCoreNlp.Utility;

namespace RahatCoreNlp.Service
{
    public class TemplateGenerator
    {
        public static int TemplateTypeCount
        {
            get { return Enum.GetNames(typeof (TemplateType)).Length; }
        }

        public static BackgroundWorker UiBackWorker;
        public static int SentenceCount;
        public static int SentenceProned;
        public static int PronningThreshold;
        public static int CorpusSize;
        public static TemplateType TemplateType;
        public static string ElapsedTime;
        public ExtractedTemplates Templates { get; set; }

        private bool PpVisited; //Did we visit a Prepositional Phrase in template or not

        public TemplateGenerator()
        {
            Templates = new ExtractedTemplates();
        }

        public ExtractedTemplates ExtractTemplatesWithPronning(int corpusSize, TemplateType templateType, int pronningThreshold, int startIdx = 0)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            ExtractedTemplates simple = ExtractTemplates(corpusSize, templateType, 0, startIdx);
            if (pronningThreshold > 0)
            {
                var retVal = ExtractTemplates(corpusSize, templateType, pronningThreshold, startIdx);
                stopwatch.Stop();
                ElapsedTime = stopwatch.Elapsed.ToString();
                return retVal;
            }
            stopwatch.Stop();
            ElapsedTime = stopwatch.Elapsed.ToString();
            
            return simple;

        }

        private ExtractedTemplates ExtractTemplates(int corpusSize, TemplateType templateType, int pronningThreshold, int startIdx)
        {
            SentenceCount = 0;
            SentenceProned = 0;
            PronningThreshold = pronningThreshold;
            CorpusSize = corpusSize;
            TemplateType = templateType;

            DbService newsDbService = new DbService();
            
            string path = string.Format("{0}_{1}_{2}_TemplatesAccumulative.txt", CorpusSize, TemplateType, PronningThreshold);
            StreamWriter streamWriter = new StreamWriter(path);
            for (int newsId = 2; newsId < corpusSize && !(UiBackWorker!=null && UiBackWorker.CancellationPending); newsId++)
            {
                if (UiBackWorker != null)
                    UiBackWorker.ReportProgress(((pronningThreshold == 0 ? 0 : corpusSize) + newsId)*100/
                                                GenerateTemplateParam.BackWorkerDenominator);

                MehrNewsContent mehrNews = newsDbService.GetNewsById(startIdx + newsId);

                // rejecting empty and very big news texts.
                if (string.IsNullOrEmpty(mehrNews.NormalizedContent) || mehrNews.NormalizedContent.Length > 5000)
                    continue;
                string chunked = HazmService.Chunk(mehrNews.NormalizedContent);
                ChunkerDataStructure chunkedStructure = new ChunkParser().Parse(chunked);
                ExtractTemplateFromChunk(chunkedStructure.Chunks, TemplateType, streamWriter);
            }
            streamWriter.Close();

            return Templates;
        }

        public string[] ExtractTemplateFromChunk(List<Chunk> Chunks, TemplateType templateType, StreamWriter streamWriter = null)
        {
            string template = "", chunkedSentence = "", originalSentence = "", pronedSentence = "";
            PpVisited = false;

            foreach (var chunk in Chunks)
            {
                bool endOfSentence = false;
                switch (templateType)
                {
                    case TemplateType.RemovePpNoVerbDiscrimination:
                        endOfSentence = RemovePpNoVerbDiscrimination_Constraint(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                        break;
                    case TemplateType.RemovePpKeepToBeVerbs:
                        endOfSentence = RemovePpKeepToBeVerbs_Constraint(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                        break;
                    case TemplateType.RemovePpKeepAllVerbs:
                        endOfSentence = RemovePpKeepAllVerbs_Constraint(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                        break;

                    case TemplateType.KeepPpNoVerbDiscrimination:
                        endOfSentence = KeepPpNoVerbDiscrimination_Constraint(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                        break;
                    case TemplateType.KeepPpKeepToBeVerbs:
                        endOfSentence = KeepPpKeepToBeVetbs_Constraint(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                        break;
                    case TemplateType.KeepPpKeepAllVerbs:
                        endOfSentence = KeepPpKeepAllVerbs_Constraint(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                        break;

                }
                if (endOfSentence)
                {
                    if (PronningThreshold == 0)
                    {
                        if (streamWriter != null)
                            streamWriter.WriteLine(SentenceCount + "," + Templates.Templates.Count);
                    }
                    else
                    {
                        if (streamWriter != null)
                            streamWriter.WriteLine(SentenceCount + "," + Templates.PronedTemplates.Count);
                    }
                }
            }
            return new string[] {template, chunkedSentence, originalSentence};
        }

        #region Constraint Methods

        //Remove PP    returns true if end of Sentence reached
        private bool RemovePpNoVerbDiscrimination_Constraint(Chunk chunk, ref string template, ref string chunkedSentence, ref string originalSentence, ref string pronedSentence)
        {
            if (chunk.Tag == "VP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                Templates.AddTemplate(new Sentence(){ChunkedSentence = chunkedSentence, OriginalSentence = originalSentence, PronedSentence = pronedSentence}, template, TemplateType);

                template = "";
                chunkedSentence = "";
                originalSentence = "";
                pronedSentence = "";
                return true;
            }

            else if (chunk.Tag == "PP")
            {
                Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
                PpVisited = true;
            }

            else if (chunk.Tag == "NP")
            {
                if (PpVisited)
                {
                    Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                    Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
                    PpVisited = false;
                    return false;
                }
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "POSTP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "ADVP")
            {
                Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
            }

            else if (chunk.Tag == "ADJP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }
            return false;
        }
        private bool RemovePpKeepToBeVerbs_Constraint(Chunk chunk, ref string template, ref string chunkedSentence, ref string originalSentence, ref string pronedSentence)
        {
            if (chunk.Tag == "VP")
            {
                if (LanguageResources.PersianToBeVerbs.Contains(chunk.Phrase))
                {
                    Utilizer.AddOriginalPhrase(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                    
                }
                else
                    Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);

                Templates.AddTemplate(new Sentence() { ChunkedSentence = chunkedSentence, OriginalSentence = originalSentence, PronedSentence = pronedSentence }, template, TemplateType);

                template = "";
                chunkedSentence = "";
                originalSentence = "";
                pronedSentence = "";
                return true;
            }

            else if (chunk.Tag == "PP")
            {
                Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
                PpVisited = true;
            }

            else if (chunk.Tag == "NP")
            {
                if (PpVisited)
                {
                    Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                    Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
                    PpVisited = false;
                    return false;
                }
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "POSTP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "ADVP")
            {
                Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
            }

            else if (chunk.Tag == "ADJP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }
            return false;
        }
        private bool RemovePpKeepAllVerbs_Constraint(Chunk chunk, ref string template, ref string chunkedSentence, ref string originalSentence, ref string pronedSentence)
        {
            if (chunk.Tag == "VP")
            {
                Utilizer.AddOriginalPhrase(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                Templates.AddTemplate(new Sentence() { ChunkedSentence = chunkedSentence, OriginalSentence = originalSentence, PronedSentence = pronedSentence }, template, TemplateType);

                template = "";
                chunkedSentence = "";
                originalSentence = "";
                pronedSentence = "";
                return true;
            }

            else if (chunk.Tag == "PP")
            {
                Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
                PpVisited = true;
            }

            else if (chunk.Tag == "NP")
            {
                if (PpVisited)
                {
                    Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                    Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
                    PpVisited = false;
                    return false;
                }
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "POSTP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "ADVP")
            {
                Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
            }

            else if (chunk.Tag == "ADJP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }
            return false;
        }

        //Keep PP
        private bool KeepPpNoVerbDiscrimination_Constraint(Chunk chunk, ref string template, ref string chunkedSentence, ref string originalSentence, ref string pronedSentence)
        {
            if (chunk.Tag == "VP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                Templates.AddTemplate(new Sentence() { ChunkedSentence = chunkedSentence, OriginalSentence = originalSentence, PronedSentence = pronedSentence }, template, TemplateType);

                template = "";
                chunkedSentence = "";
                originalSentence = "";
                pronedSentence = "";
                return true;
            }

            else if (chunk.Tag == "PP")
            {
                Utilizer.AddOriginalPhrase(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "NP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "POSTP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "ADVP")
            {
                Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
            }

            else if (chunk.Tag == "ADJP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }
            return false;
        }
        private bool KeepPpKeepToBeVetbs_Constraint(Chunk chunk, ref string template, ref string chunkedSentence, ref string originalSentence, ref string pronedSentence)
        {
            if (chunk.Tag == "VP")
            {
                if (LanguageResources.PersianToBeVerbs.Contains(chunk.Phrase))
                {
                    Utilizer.AddOriginalPhrase(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                }
                else
                    Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);

                Templates.AddTemplate(new Sentence() { ChunkedSentence = chunkedSentence, OriginalSentence = originalSentence, PronedSentence = pronedSentence }, template, TemplateType);

                template = "";
                chunkedSentence = "";
                originalSentence = "";
                pronedSentence = "";
                return true;
            }

            else if (chunk.Tag == "PP")
            {
                Utilizer.AddOriginalPhrase(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "NP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "POSTP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "ADVP")
            {
                Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
            }

            else if (chunk.Tag == "ADJP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }
            return false;
        }

        private bool KeepPpKeepAllVerbs_Constraint(Chunk chunk, ref string template, ref string chunkedSentence, ref string originalSentence, ref string pronedSentence)
        {
            if (chunk.Tag == "VP")
            {
                Utilizer.AddOriginalPhrase(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
                Templates.AddTemplate(new Sentence() { ChunkedSentence = chunkedSentence, OriginalSentence = originalSentence, PronedSentence = pronedSentence }, template, TemplateType);

                template = "";
                chunkedSentence = "";
                originalSentence = "";
                pronedSentence = "";
                return true;
            }

            else if (chunk.Tag == "PP")
            {
                Utilizer.AddOriginalPhrase(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "NP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "POSTP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }

            else if (chunk.Tag == "ADVP")
            {
                Utilizer.AddChunkToSentence(chunk, ref chunkedSentence);
                Utilizer.AddChunkToOriginalSentence(chunk, ref originalSentence);
            }

            else if (chunk.Tag == "ADJP")
            {
                Utilizer.AddChunkToTemplateAndSentence(chunk, ref template, ref chunkedSentence, ref originalSentence, ref pronedSentence);
            }
            return false;
        }

        #endregion
    }

    public enum TemplateType
    {
        RemovePpNoVerbDiscrimination = 0,
        RemovePpKeepToBeVerbs,
        RemovePpKeepAllVerbs,

        KeepPpNoVerbDiscrimination,
        KeepPpKeepToBeVerbs,
        KeepPpKeepAllVerbs
    }
}
