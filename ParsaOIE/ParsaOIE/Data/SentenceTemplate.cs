using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RahatCoreNlp.Service;

namespace RahatCoreNlp.Data
{

    public class ExtractedTemplates
    {
        public ExtractedTemplates()
        {
            Templates = new Dictionary<string, Template>();
            PronedTemplates = new Dictionary<string, Template>();
        }
        public Dictionary<string, Template> Templates { get; set; }    // string is templatePattern  
        public Dictionary<string, Template> PronedTemplates { get; set; }

        public int AddTemplate(Sentence sentence, string templatePattern, TemplateType templateType)
        {
            TemplateGenerator.SentenceCount++;
            if (TemplateGenerator.PronningThreshold == 0)
            {
                if (Templates.ContainsKey(templatePattern))
                {
                    Templates[templatePattern].Sentences.Add(sentence);
                    return -1;
                }
                Templates.Add(templatePattern, new Template()
                {
                    Sentences = new List<Sentence>()
                    {
                        sentence
                    },
                    TemplatePattern = templatePattern,
                    TemplateType = templateType
                });
                return 1;
            }

            else if (TemplateGenerator.PronningThreshold > 0)
            {
                if (Templates[templatePattern].Sentences.Count >= TemplateGenerator.PronningThreshold)
                {
                    if (PronedTemplates.ContainsKey(templatePattern))
                    {
                        PronedTemplates[templatePattern].Sentences.Add(sentence);
                        return -1;
                    }
                    PronedTemplates.Add(templatePattern, new Template()
                    {
                        Sentences = new List<Sentence>()
                        {
                            sentence
                        },
                        TemplatePattern = templatePattern,
                        TemplateType = templateType
                    });
                    return 1;
                }
                else
                {
                    TemplateGenerator.SentenceProned++;
                }

            }
            return -1;
        }

        public void WriteToFile()
        {
            string path = string.Format("{0}_{1}_{2}_TemplatesHistogram.txt", TemplateGenerator.CorpusSize, TemplateGenerator.TemplateType, TemplateGenerator.PronningThreshold);
            StreamWriter streamWriter = new StreamWriter(path);
            int i = 0;
            if (TemplateGenerator.PronningThreshold == 0)
            {
                foreach (var template in Templates.OrderByDescending(x => x.Value.Sentences.Count))
                {
                    streamWriter.WriteLine(++i + "," + template.Value.Sentences.Count);
                }
            }
            else
            {
                foreach (var template in PronedTemplates.OrderByDescending(x => x.Value.Sentences.Count))
                {
                    streamWriter.WriteLine(++i + "," + template.Value.Sentences.Count);
                }
            }
            streamWriter.Close();
        }
    }

    public class Template
    {
        public Template()
        {
            Sentences = new List<Sentence>();
        }       
        public string TemplatePattern { get; set; }
        public List<Sentence> Sentences { get; set; }
        public TemplateType TemplateType { get; set; }
    }

    public class Sentence
    {
        public string OriginalSentence { get; set; }
        public string ChunkedSentence { get; set; }
        public string PronedSentence { get; set; }
        public int NewsId { get; set; }

    }
}
