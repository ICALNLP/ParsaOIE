using RahatCoreNlp.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using edu.stanford.nlp.semgraph;
using RahatCoreNlp.Data;
using RahatCoreNlp.GraphVizCode;
using RahatCoreNlp.OieConvert;
using RahatCoreNlp.Seraji;
using Stanford.NLP.CoreNLP.CSharp;

namespace RahatCoreNlp.UI
{
    public partial class DependencyGraphViewer : Form
    {
        public DependencyGraphViewer()
        {
            InitializeComponent();
        }

        public class myWord
        {
            public myWord(string _word)
            {
                rect = new RectangleF(-1, -1, -1, -1);
                word = _word;
            }
            public string word;
            public RectangleF rect;
        }

        public Bitmap bmp;
        int arcLevelDistance = 60;
        int betweenWordsDistance = 25;
        Font labelFont = new Font("B Nazanin", 10);
        Graphics DrawSentence(List<myWord> sentnece)
        {
            Font stringFont, numberingFont;
            if (_IsPersian)
                stringFont = new Font("B Nazanin", 12);
            else
                stringFont = new Font("Times New Roman", 14);
            numberingFont = new Font("Times New Roman", 14);
            int totalWidth = 0;
            foreach (myWord myword in sentnece)
            {
                Size size = TextRenderer.MeasureText(myword.word, stringFont);
                myword.rect = new RectangleF(totalWidth, 50, size.Width + 20, size.Height);

                totalWidth += size.Width + betweenWordsDistance;
            }
            Size pictureSize = new Size(totalWidth, 1500);
            bmp = new Bitmap(pictureSize.Width, pictureSize.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Pen yellowPen = new Pen(Color.Yellow, 3);
            int sensize = sentnece.Count();
            for (int p=0;p<sensize;p++)
            {
                myWord myword = sentnece[p];
                g.DrawString(myword.word, stringFont, Brushes.Black, myword.rect);
                RectangleF tempRect = myword.rect;
                tempRect.Y = 0;
                tempRect.Width = 80;
                if (_IsPersian)
                    g.DrawString((sensize - p - 1).ToString(), numberingFont, Brushes.Red, tempRect);
                else
                    g.DrawString(p.ToString(), numberingFont, Brushes.Red, tempRect);
                //g.DrawEllipse(yellowPen, new Rectangle((int)myword.rect.X, (int)myword.rect.Y, (int)myword.rect.Width, (int)myword.rect.Height));
            }
            return g;
        }

        void drawArc(List<myWord> sentnece, string label, int start, int end, Graphics g, int level)
        {
            if (start < end)
                label = ">\n" + label;
            else
                label = "<\n" + label;

            // Create pens.
            Pen redPen = new Pen(Color.Red, 3);
            Pen greenPen = new Pen(Color.Green, 2);

            // Create points that define curve.
            Point point1 = new Point((int)sentnece[start].rect.X + (int)sentnece[start].rect.Width / 2, (int)sentnece[start].rect.Y + (int)sentnece[start].rect.Height);

            Point point2;
            if (start < end)
                point2 = new Point(((int)sentnece[start].rect.X + (int)sentnece[end].rect.X + (int)sentnece[end].rect.Width) / 2, point1.Y + level * arcLevelDistance);
            else
                point2 = new Point(((int)sentnece[end].rect.X + (int)sentnece[start].rect.X + (int)sentnece[start].rect.Width) / 2, point1.Y + level * arcLevelDistance);

            Point point3 = new Point((int)sentnece[end].rect.X + (int)sentnece[end].rect.Width / 2, (int)sentnece[end].rect.Y + (int)sentnece[end].rect.Height);
            Point[] curvePoints = { point1, point2, point3 };

            // Draw curve to screen.
            g.DrawCurve(greenPen, curvePoints);

        }

        // I first draw arcs then draw labels to make sure that labels are readable.
        void drawLables(List<myWord> sentnece, string label, int start, int end, Graphics g, int level)
        {
            if (start < end)
                label = ">\n" + label;
            else
                label = "<\n" + label;

            // Create pens.
            Pen redPen = new Pen(Color.Red, 3);
            Pen greenPen = new Pen(Color.Green, 3);

            // Create points that define curve.
            Point point1 = new Point((int)sentnece[start].rect.X + (int)sentnece[start].rect.Width / 2, (int)sentnece[start].rect.Y + (int)sentnece[start].rect.Height);

            Point point2;
            if (start < end)
                point2 = new Point(((int)sentnece[start].rect.X + (int)sentnece[end].rect.X + (int)sentnece[end].rect.Width) / 2, point1.Y + level * arcLevelDistance);
            else
                point2 = new Point(((int)sentnece[end].rect.X + (int)sentnece[start].rect.X + (int)sentnece[start].rect.Width) / 2, point1.Y + level * arcLevelDistance);

            Point point3 = new Point((int)sentnece[end].rect.X + (int)sentnece[end].rect.Width / 2, (int)sentnece[end].rect.Y + (int)sentnece[end].rect.Height);
            Point[] curvePoints = { point1, point2, point3 };

            Size s = TextRenderer.MeasureText(label, labelFont);
            s.Width += 10;
            RectangleF rectf = new RectangleF(point2.X - 8, point2.Y - 12, s.Width, s.Height);
            g.DrawString(label, labelFont, Brushes.Blue, rectf);
        }

        class myArc
        {
            public myArc(string lable, int start, int end, int level)
            {
                mLabel = lable;
                mStart = start;
                mEnd = end;
                mLevel = level;
            }
            public string mLabel;
            public int mStart, mEnd;
            public int mLevel;
        }


        public void DisplayGraphWithTreeView()
        {
            string delimiter = "~";
            DisplayDepGraphWithTreeView treeViewDialog = new DisplayDepGraphWithTreeView();
            Queue<KeyValuePair<string, int>> queue = new Queue<KeyValuePair<string, int>>();
            Queue<TreeNode> queueTreeView = new Queue<TreeNode>();
            queue.Enqueue(new KeyValuePair<string, int>( _originalParsedStructure[0].UnicodeWord, 0));
            queueTreeView.Enqueue(treeViewDialog.treeView1.Nodes.Add(""));
            while (queue.Count!=0)
            {
                var item = queue.Dequeue();
                var treeNode = queueTreeView.Dequeue();
                treeNode.Text = item.Key + delimiter + _originalParsedStructure[item.Value].UnicodeWord;

                foreach (var dep in _originalParsedStructure[item.Value].Deps)
                {
                    queue.Enqueue(dep);
                    queueTreeView.Enqueue(treeNode.Nodes.Add(""));
                }
            }
            treeViewDialog.Show();
        }

        bool _IsPersian = true;
        private List<DependencyParseNode> _originalParsedStructure = null;
        private List<DependencyParseNode> _collapsedParsedStructure = null;
        private int _displayStatus = 0;   // 0: _originalParsedStructure, 1: _collapsedParsedStructure

       /* public void DrawGraph(List<DependencyParseNode> parsedStructure)
        {
            var bmp = DrawGraph(parsedStructure);
            
        }*/

        public Bitmap GetParsedGraph(List<DependencyParseNode> parsedStructure)
        {
            if (_originalParsedStructure == null)
                _originalParsedStructure = parsedStructure;

            List<myWord> sentnece = new List<myWord>();
            foreach (DependencyParseNode node in parsedStructure)
            {
                sentnece.Add(new myWord(node.UnicodeWord));
            }
            // reversing the sentence to handle Farsi right to left writing
            sentnece.Reverse();
            Graphics graphics = DrawSentence(sentnece);

            // making arcs list. notice that because we reversed the sentence we have to modify the arcs too.
            List<myArc> arcs = new List<myArc>();
            for (int i = 0; i < parsedStructure.Count; i++)
            {
                foreach (KeyValuePair<string, int> node in parsedStructure[i].Deps)
                {
                    arcs.Add(new myArc(/*OieConvertor.TranslateDadeganDepLableToPersian(*/node.Key/*)*/, sentnece.Count - 1 - i, sentnece.Count - 1 - node.Value, -1));
                }
            }

            // computing depth level of arcs and drawing
            int level = 1;
            for (int depth = 1; depth < 30; depth++)
            {
                bool depthVisited = false;
                foreach (myArc arc in arcs)
                {
                    if (Math.Abs(arc.mStart - arc.mEnd) == depth)
                    {
                        depthVisited = true;
                        arc.mLevel = level;
                        drawArc(sentnece, arc.mLabel, arc.mStart, arc.mEnd, graphics, arc.mLevel);
                    }
                }
                if (depthVisited)
                    level++;
            }

            // I first draw arcs then draw labels to make sure that labels are readable.
            level = 1;
            for (int depth = 1; depth < 30; depth++)
            {
                bool depthVisited = false;
                foreach (myArc arc in arcs)
                {
                    if (Math.Abs(arc.mStart - arc.mEnd) == depth)
                    {
                        depthVisited = true;
                        arc.mLevel = level;
                        drawLables(sentnece, arc.mLabel, arc.mStart, arc.mEnd, graphics, arc.mLevel);
                    }
                }
                if (depthVisited)
                    level++;
            }

            graphics.Flush();

            pictureBox1.Image = bmp;
            pictureBox1.Refresh();
            return bmp;
        }/**/

        public Bitmap DrawGraph(List<DependencyParseNode> parsedStructure, bool IsPersian = true, bool useGraphViz = true)
        {
            _IsPersian = IsPersian;

            if (_IsPersian)
            {
                originalDependencyTypeToolStripMenuItem.Text = "Original";
                collapseAndKeepPpDependencyTypeToolStripMenuItem.Text = "Collapse and Keep Pp";
                collapsedAndDropPpDependencyTypeToolStripMenuItem.Text = "Collapsed and Drop Pp";
            }
            else
            {
                originalDependencyTypeToolStripMenuItem.Text = "basic-dependencies";
                collapseAndKeepPpDependencyTypeToolStripMenuItem.Text = "collapsed-dependencies";
                collapsedAndDropPpDependencyTypeToolStripMenuItem.Text = "collapsed-ccprocessed-dependencies";
            }

            if (_originalParsedStructure == null)
                _originalParsedStructure = parsedStructure;

            if (useGraphViz)
            {
                bmp = (Bitmap)MySemanticGraph.RenderImageParsedStructure(parsedStructure, false);
            }
            else
            {
                List<myWord> sentnece = new List<myWord>();
                foreach (DependencyParseNode node in parsedStructure)
                {
                    sentnece.Add(new myWord(node.UnicodeWord));
                }

                // reversing the sentence to handle Farsi right to left writing
                if (IsPersian)
                    sentnece.Reverse();
                Graphics graphics = DrawSentence(sentnece);

                // making arcs list. notice that because we reversed the sentence we have to modify the arcs too.
                List<myArc> arcs = new List<myArc>();
                for (int i = 0; i < parsedStructure.Count; i++)
                {
                    foreach (KeyValuePair<string, int> node in parsedStructure[i].Deps)
                    {
                        if (IsPersian)
                            arcs.Add(new myArc( /*OieConvertor.TranslateDadeganDepLableToPersian(*/
                                node.Key /*)*/, sentnece.Count - 1 - i, sentnece.Count - 1 - node.Value, -1));
                        else
                            arcs.Add(new myArc( /*OieConvertor.TranslateDadeganDepLableToPersian(*/
                                node.Key /*)*/, i, node.Value, -1));
                    }
                }

                // computing depth level of arcs and drawing
                int level = 1;
                for (int depth = 1; depth < 30; depth++)
                {
                    bool depthVisited = false;
                    foreach (myArc arc in arcs)
                    {
                        if (Math.Abs(arc.mStart - arc.mEnd) == depth)
                        {
                            depthVisited = true;
                            arc.mLevel = level;
                            drawArc(sentnece, arc.mLabel, arc.mStart, arc.mEnd, graphics, arc.mLevel);
                        }
                    }
                    if (depthVisited)
                        level++;
                }

                // I first draw arcs then draw labels to make sure that labels are readable.
                level = 1;
                for (int depth = 1; depth < 30; depth++)
                {
                    bool depthVisited = false;
                    foreach (myArc arc in arcs)
                    {
                        if (Math.Abs(arc.mStart - arc.mEnd) == depth)
                        {
                            depthVisited = true;
                            arc.mLevel = level;
                            drawLables(sentnece, arc.mLabel, arc.mStart, arc.mEnd, graphics, arc.mLevel);
                        }
                    }
                    if (depthVisited)
                        level++;
                }

                graphics.Flush();
            }

            pictureBox1.Image = bmp;
            pictureBox1.Refresh();
            bmp.Save("1.bmp");
            return bmp;
        }

        private void btnTreeView_Click(object sender, EventArgs e)
        {
            DisplayGraphWithTreeView();
        }

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBoxDialog richTextBoxDialog = new RichTextBoxDialog();
            richTextBoxDialog.Text = "متن جمله";
            string sentnece = (_displayStatus == 0)
                ? string.Join(" ", _originalParsedStructure.Skip(1).Select(x => x.UnicodeWord).ToArray())
                : string.Join(" ", _collapsedParsedStructure.Skip(1).Select(x => x.UnicodeWord).ToArray());
            richTextBoxDialog.rtbOutput.Text = sentnece;
            richTextBoxDialog.Show();
        }

        private void cONNLFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBoxDialog richTextBoxDialog = new RichTextBoxDialog();
            richTextBoxDialog.Text = "جمله با فرمت CoNLL";
            string conllString = (_displayStatus == 0)
                ? OieConvertor.ConvertParsedStructure2ConllString(_originalParsedStructure)
                : OieConvertor.ConvertParsedStructure2ConllString(_collapsedParsedStructure);
            richTextBoxDialog.rtbOutput.Text = conllString;
            richTextBoxDialog.Show();
        }
        private void originalDependencyTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _displayStatus = 0;
            DrawGraph(_originalParsedStructure, _IsPersian);
        }
        private void collapseAndKeepPpDependencyTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _displayStatus = 1;
            _collapsedParsedStructure = new List<DependencyParseNode>();
            if (_IsPersian)
            {
                _collapsedParsedStructure = _originalParsedStructure.Select(item => item.Clone()).ToList();
                ParsPer.CollapseDependencies(_collapsedParsedStructure, ParsPer.PersianDependencyType.CollapsedKeepPp);
            }
            else
            {
                string sentence = string.Join(" ", _originalParsedStructure.Skip(1).Select(x => x.UnicodeWord).ToArray());
                _collapsedParsedStructure = MyStanfordCoreNLP.Parse(sentence,true,
                    MyStanfordCoreNLP.StanfordDependencyType.collapsed_dependencies);
            }
            DrawGraph(_collapsedParsedStructure, _IsPersian);
        }

        private void collapsedAndDropPpDependencyTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _displayStatus = 1;
            _collapsedParsedStructure = new List<DependencyParseNode>();
            if (_IsPersian)
            {
                _collapsedParsedStructure = _originalParsedStructure.Select(item => item.Clone()).ToList();
                ParsPer.CollapseDependencies(_collapsedParsedStructure, ParsPer.PersianDependencyType.CollapsedDropPp);
                DrawGraph(_collapsedParsedStructure, _IsPersian);
            }
            else
            {
                string sentence = string.Join(" ", _originalParsedStructure.Skip(1).Select(x => x.UnicodeWord).ToArray());
                _collapsedParsedStructure = MyStanfordCoreNLP.Parse(sentence,true,
                    MyStanfordCoreNLP.StanfordDependencyType.collapsed_ccprocessed_dependencies);
            }
            DrawGraph(_collapsedParsedStructure, _IsPersian);
        }

        private void ToolStripFarsi_Click(object sender, EventArgs e)
        {
            DrawGraph(_originalParsedStructure);
        }

        private void ToolStripEnglish_Click(object sender, EventArgs e)
        {
            DrawGraph(_originalParsedStructure, false);
        }

        private void ToolStripBreakLoopsTrue_Click(object sender, EventArgs e)
        {
            if (!_IsPersian && _displayStatus == 1)
            {
                string sentence = string.Join(" ", _originalParsedStructure.Skip(1).Select(x => x.UnicodeWord).ToArray());
                _collapsedParsedStructure = MyStanfordCoreNLP.Parse(sentence, true,
                    MyStanfordCoreNLP.StanfordDependencyType.collapsed_ccprocessed_dependencies);
                DrawGraph(_collapsedParsedStructure, _IsPersian);
            }
        }

        private void ToolStripBreakLoopsFalse_Click(object sender, EventArgs e)
        {
            if (!_IsPersian && _displayStatus == 1)
            {
                string sentence = string.Join(" ", _originalParsedStructure.Skip(1).Select(x => x.UnicodeWord).ToArray());
                _collapsedParsedStructure = MyStanfordCoreNLP.Parse(sentence, false,
                    MyStanfordCoreNLP.StanfordDependencyType.collapsed_ccprocessed_dependencies);
                DrawGraph(_collapsedParsedStructure, _IsPersian);
            }
        }

        private void graphViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DrawGraph(_originalParsedStructure, _IsPersian, true);
        }

        private void lineViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DrawGraph(_originalParsedStructure, _IsPersian, false);
        }

        private void saveImageAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bmp == null)
            {
                MessageBox.Show("No Image Exists.");
                return;
            }
            SaveFileDialog savefile = new SaveFileDialog();
            // set a default file name
            savefile.FileName = "unknown.png";
            // set filters - this can be done in properties as well
            savefile.Filter = "Image files (*.png)|*.png|All files (*.*)|*.*";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                bmp.Save(savefile.FileName);
            }
        }
    }
}
