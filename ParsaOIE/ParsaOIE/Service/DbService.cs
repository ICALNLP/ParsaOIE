using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RahatCoreNlp.DB;

namespace RahatCoreNlp.Service
{
    
    public class DbService
    {
        public ContentAggregationEntities4 MehrNewsContext { get; set; }

        public DbService()
        {
            MehrNewsContext = new ContentAggregationEntities4();
        }

        public List<MehrNewsContent> AllMehrNews()
        {
            return MehrNewsContext.MehrNewsContents.ToList();
        }

        public List<MehrNewsContent> AllMehrNews(int take )
        {
            return MehrNewsContext.MehrNewsContents.Take(take).ToList();
        }

        public MehrNewsContent GetNewsById(int id)
        {
            return MehrNewsContext.MehrNewsContents.Single(b => b.Id == id);
        }

        public List<MehrNewsContent> GetNews(int startId, int count)
        {
            using (var context = MehrNewsContext)
            {
                // Query for all blogs with names starting with B 
                List<MehrNewsContent> news = (from b in context.MehrNewsContents
                             where b.Title != null && b.Id > startId && b.Id < startId + count
                             select b).ToList();

                // Query for the Blog named ADO.NET Blog 
                //var blog = context.Blogs
                //                .Where(b => b.Name == "ADO.NET Blog")
                //                .FirstOrDefault();
                return news;
            }
        }

        public void SentenceInsert(string [] table/*columns are seperated by tab - each line is a row*/)
        {

            foreach (string row in table)
            {
                string[] columns = row.Split('\t');
                MehrSentence sentence = new MehrSentence();
                sentence.Text = (columns.Count() >= 1) ? columns[0] : "";
                sentence.NewsId = (columns.Count() >= 2) ? Convert.ToInt32(columns[1]) : (int?)null;
                sentence.Tag = (columns.Count() >= 3) ? columns[2] : "";
                MehrNewsContext.MehrSentences.Add(sentence);
            }
            MehrNewsContext.SaveChanges();
        }
        public void RelationInsert(string[] table/*columns are seperated by tab - each line is a row*/)
        {

            foreach (string row in table)
            {
                string[] columns = row.Split('\t');
                MehrRelation relation = new MehrRelation();
                relation.Arg1 = (columns.Count() >= 1) ? columns[0] : "";
                relation.Arg2 = (columns.Count() >= 2) ? columns[1] : "";
                relation.RelationPhrase = (columns.Count() >= 3) ? columns[2] : "";
                relation.Label = (columns.Count() >= 4) ? columns[3] : "";
                relation.SentenceId = (columns.Count() >= 5) ? Convert.ToInt32(columns[4]) : (int?)null;
                MehrNewsContext.MehrRelations.Add(relation);
            }
            MehrNewsContext.SaveChanges();
        }

        public string MakeLabelingFile()
        {
            List<MehrRelation> relations = (from relation in MehrNewsContext.MehrRelations
                        join sentence in MehrNewsContext.MehrSentences on relation.SentenceId equals sentence.Id
                        where relation.Label == ""
                        select relation).ToList();
            string output = "";
            string newsContent = "";
            foreach (MehrRelation relation in relations)
            {
                if (newsContent != relation.MehrSentence.Text)
                {
                    newsContent = relation.MehrSentence.Text;
                    // a new sentence print it.
                    output += "\n" + newsContent + "\n";
                }
                output += "{" + relation.Id + "}(" + relation.Arg1 + " <-> " + relation.Arg2 + " <-> " + relation.RelationPhrase + ")\n";
            }

            return output;
        }

        public void UpdateRelations(int id, string label)
        {
            var result = MehrNewsContext.MehrRelations.SingleOrDefault(b => b.Id == id);
            if (result != null)
            {
                result.Label = label;
                MehrNewsContext.SaveChanges();
            }
        }

        public List<MehrRelation> GetAllUnlabeledRelations()
        {
            List<MehrRelation> relations = (from relation in MehrNewsContext.MehrRelations
                                            join sentence in MehrNewsContext.MehrSentences on relation.SentenceId equals sentence.Id
                                            where relation.Label == ""
                                            select relation).ToList();
            return relations;
        }

        public List<MehrRelation> GetAllRelationsWithLabel()
        {
            List<MehrRelation> relations = (from relation in MehrNewsContext.MehrRelations
                                            join sentence in MehrNewsContext.MehrSentences on relation.SentenceId equals sentence.Id
                                            where (relation.Label == "+" || relation.Label == "-")
                                            select relation).ToList();
            return relations;
        }
    }
}
