using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RahatCoreNlp.Data;
using RahatCoreNlp.UI;
using RahatCoreNlp.Utility;

namespace RahatCoreNlp.Service
{
    class TemplatePackage
    {
        public int sentenceCounter = 0;
        public Sentence sentence = new Sentence();
        public Template[] templates = new Template[TemplateGenerator.TemplateTypeCount];
        public int MostGeneralTemplateMatchedIdx = -1;
    }

    class TemplateHierarchyGenerator
    {
        public static int CorpusSize = 0;
        public static int StartIdx = 0;
        int[] results = new int[TemplateGenerator.TemplateTypeCount + 1];

        public Dictionary<string, TemplatePackage> sentenceTemplateHierarchy { get; set; }    // string is Original Sentence  

        ExtractedTemplates[] LoadedTemplatesHierarchy = new ExtractedTemplates[TemplateGenerator.TemplateTypeCount];

        public TemplateHierarchyGenerator()
        {
            sentenceTemplateHierarchy = new Dictionary<string, TemplatePackage>();
        }

        private int LoadTemplateHierarchyFromFile()
        {
            for (int level = 0; level < TemplateGenerator.TemplateTypeCount; level++)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                TemplateType templatType = (TemplateType) level;
                openFileDialog.Title = "Load Template Hierarchy level: " + templatType.ToString();
                openFileDialog.FileName = "*"+templatType.ToString() +"*Display*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    DisplayTree displayTree = new DisplayTree();
                    LoadedTemplatesHierarchy[level] = displayTree.LoadFromFile(openFileDialog.FileName);
                }
                else
                {
                    return -1;
                }
            }
            return 0;
        }

        public void ExtractTemplateHierarchy(int testCorpusSize, int startIdx)
        {
            CorpusSize = testCorpusSize;
            StartIdx = startIdx;

            if (LoadTemplateHierarchyFromFile() == -1)
                return;

            for (int level = TemplateGenerator.TemplateTypeCount; level >= 0 ; level--)
            {
                TemplateGenerator templateGenerator = new TemplateGenerator();
                int corpusSize2 = testCorpusSize;
                int pronningThreshlod2 = 0;
                TemplateType constraintType2 = (TemplateType) level;
                ExtractedTemplates extractedTemplates = templateGenerator.ExtractTemplatesWithPronning(corpusSize2,
                    constraintType2, pronningThreshlod2, startIdx);

                foreach (KeyValuePair<string, Template> template in extractedTemplates.Templates)
                {
                    foreach (Sentence sentence in template.Value.Sentences)
                    {
                        if (sentenceTemplateHierarchy.ContainsKey(sentence.OriginalSentence))
                        {
                            sentenceTemplateHierarchy[sentence.OriginalSentence].templates[level] = template.Value;
                            sentenceTemplateHierarchy[sentence.OriginalSentence].sentenceCounter++;
                            if (sentenceTemplateHierarchy[sentence.OriginalSentence].MostGeneralTemplateMatchedIdx == -1
                                && LoadedTemplatesHierarchy[level].Templates.ContainsKey(template.Key))
                                sentenceTemplateHierarchy[sentence.OriginalSentence].MostGeneralTemplateMatchedIdx =
                                    level;
                        }
                        else
                        {
                            TemplatePackage package = new TemplatePackage();
                            package.sentence = sentence;
                            package.sentenceCounter++;
                            if (LoadedTemplatesHierarchy[level].Templates.ContainsKey(template.Key))
                                package.MostGeneralTemplateMatchedIdx = level;
                            package.templates[level] = template.Value;
                            sentenceTemplateHierarchy.Add(sentence.OriginalSentence, package);
                        }
                    }
                }

            }

            foreach (var templatePackage in sentenceTemplateHierarchy)
            {

                templatePackage.Value.sentenceCounter /= TemplateGenerator.TemplateTypeCount;  // because we have considered each sentece {TemplateTypeCount} times.
                results[templatePackage.Value.MostGeneralTemplateMatchedIdx + 1]++;
            }

            TreeViewDialog trd = new TreeViewDialog();
            TreeNode resul = trd.treeView1.Nodes.Add("Results:");
            resul.Nodes.Add("sentence count->" + sentenceTemplateHierarchy.Values.Count);
            TreeNode count = trd.treeView1.Nodes.Add("Count:");

            string[] msg = new string[7] { "بدون قالب", "حذف گروههای حرف اضافه، حذف افعال"
                , "حذف گروههای حرف اضافه، افعال اسنادی", "حذف گروههای حرف اضافه، حفظ همه افعال"
                , "حفظ گروههای حرف اضافه، حذف همه افعال", "حفظ گروههای حرف اضافه، حفظ افعال اسنادی"
                , "حفظ گروههای حرف اضافه، حفظ همه افعال" };
            double sum = 0;
            for (int i = 0; i < 7; i++)
            {
                sum += results[i];
                count.Nodes.Add(String.Format("{0}({1})->{2}", i, msg[i], results[i]));
            }

            TreeNode perc = trd.treeView1.Nodes.Add("Percentage:");
            double[] percentages = new double[TemplateGenerator.TemplateTypeCount + 1];
            for (int i = 0; i < 7; i++)
            {
                percentages[i] = (results[i]*100)/sum;
                perc.Nodes.Add(String.Format("{0}({1})->{2}", i, msg[i], percentages[i]));
            }

            TreeNode Matched = trd.treeView1.Nodes.Add("Matched:");
            TreeNode UnMatched = trd.treeView1.Nodes.Add("UnMatched:");
            for (int i = 0; i < TemplateGenerator.TemplateTypeCount; i++)
            {
                Matched.Nodes.Add(((TemplateType) i).ToString());
            }

            foreach (var templatePackage in sentenceTemplateHierarchy)
            {
                TreeNode temp;
                if (templatePackage.Value.MostGeneralTemplateMatchedIdx != -1)
                    temp =
                        Matched.Nodes[templatePackage.Value.MostGeneralTemplateMatchedIdx].Nodes.Add(templatePackage.Key);
                else
                    temp = UnMatched.Nodes.Add(templatePackage.Key);
                temp.Nodes.Add("SentenceCount: " + templatePackage.Value.sentenceCounter);
                temp.Nodes.Add("Matched Index: " + templatePackage.Value.MostGeneralTemplateMatchedIdx);
                temp.Nodes.Add("Chunked: " + templatePackage.Value.sentence.ChunkedSentence);
                for (int i = 0; i < TemplateGenerator.TemplateTypeCount; i++)
                {
                    temp.Nodes.Add(string.Format("[{0}]    {1}", i, templatePackage.Value.templates[i].TemplatePattern));
                }
            }

            string path = string.Format("corpusSize_{0}_StartIdx_{1}_TemplateHierarchy.txt", TemplateHierarchyGenerator.CorpusSize, TemplateHierarchyGenerator.StartIdx);
            DisplayTree.SaveTree(trd.treeView1, path);
            trd.Show();
        }
    }
}
