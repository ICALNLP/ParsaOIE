using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RahatCoreNlp.Data;
using RahatCoreNlp.OieParsa;
using RahatCoreNlp.Seraji;
using RahatCoreNlp.Service;
using RahatCoreNlp.UI;

namespace ParsaOIE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "OieTreePattern file";
            openFileDialog.Filter = "Text|*.txt|All|*.*";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            // first parse input sentence
            string parsedString = ParsPer.Parse(rtbInput.Text);
            List<DependencyParseNode> parsedStructure = DependencyParser.LoadCoNLL09FormatSentence(parsedString);

            RichTextBoxDialog richTextBoxDialog = new RichTextBoxDialog();
            // then match OiePatterns to sentence
            List<OieRelation> relations = OieTreePattern.ExtractRelations(parsedStructure,
                ReadPatternsFile.ReadFromFile( /*"../../OieTreePattern/Patterns.txt"*/ openFileDialog.FileName));
            foreach (OieRelation relation in relations)
            {
                richTextBoxDialog.rtbOutput.Text += string.Format("[{0},{1}] {5:0.000} ({2} <-> {3} <-> {4})\n",
                    relation.matchedPatternLine, relation.matchedPatternIndex, relation.arg1, relation.arg2,
                    relation.rel, relation.confidenceScore);
            }

            richTextBoxDialog.Text = "Relations";
            richTextBoxDialog.Show();

            DependencyGraphViewer dgv = new DependencyGraphViewer();
            dgv.DrawGraph(parsedStructure);
            dgv.Text = "Graph";
            dgv.Show();
        }
    }
}
