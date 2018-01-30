using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceModel.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using RahatCoreNlp.Data;
using RahatCoreNlp.Seraji;
using RahatCoreNlp.Utility;

namespace WcfService
{
    public static class Utilizer
    {
        public static string RemoveTroublesomeCharacters(string inString)
        {
            if (inString == null) return null;

            StringBuilder newString = new StringBuilder();
            char ch;

            for (int i = 0; i < inString.Length; i++)
            {

                ch = inString[i];
                // remove any characters outside the valid UTF-8 range as well as all control characters
                // except tabs and new lines
                //if ((ch < 0x00FD && ch > 0x001F) || ch == '\t' || ch == '\n' || ch == '\r')
                //if using .NET version prior to 4, use above logic
                if (XmlConvert.IsXmlChar(ch)) //this method is new in .NET 4
                {
                    newString.Append(ch);
                }
            }
            return newString.ToString();

        }

        public static int GiveNumOfTokensInTextExcludePuncuations(string text)
        {
            // in this function we try to count 
            string[] tokens = TagPer.GetTags(Tokenizer.GetTokens(text));
            int count = 0;
            foreach (var token in tokens)
            {
                if (!token.Contains("\tDELM"))
                    count++;
            }
            return count;
        }

        public static List<string> Split(string str, int chunkSize)
        {
            var words = new List<string>();

            for (int i = 0; i < str.Length; i += chunkSize)
                if (str.Length - i >= chunkSize) words.Add(str.Substring(i, chunkSize));
                else words.Add(str.Substring(i, str.Length - i));

            return words;
        }

        public static List<string> SplitUsingUpssalaSentSegmenter(string str, int chunkSize)
        {
            // since Hazm webServer can not handle big size texts, I implemented this function to
            // first split the text using ParsPer sentence spliter (which can handle big texts). 
            // then return the segemnts for furture processing like Hazm normalization and tokenization

            string[] sentences = SentenceSegmenter.GetSegments(str);

            List<string> temp = new List<string>();
            for (int i = 0; i < sentences.Length; i++)
            {
                if (sentences[i].Length >= chunkSize)
                {
                    // to break very very large sentences!!!
                    int breakPoint = sentences[i].Length/2;
                    temp.Add(sentences[i].Substring(0,breakPoint));
                    temp.Add(sentences[i].Substring(breakPoint, sentences[i].Length - breakPoint));
                }
                else
                {
                    temp.Add(sentences[i]);
                }
            }
            sentences = temp.ToArray();

            List<string> segments = new List<string>();

            string currentSegment = "";
            int sentenceCounter = 0;
            while (true)
            {
                if (sentenceCounter == sentences.Length)
                {
                    // the final peice of text is in currentSegment. Dont forget to add it to segments.
                    segments.Add(currentSegment);
                    break;
                }

                if (currentSegment.Length + sentences[sentenceCounter].Length <= chunkSize)
                {
                    currentSegment += sentences[sentenceCounter] + " ";
                    sentenceCounter++;
                }
                else
                {
                    segments.Add(currentSegment);
                    currentSegment = "";
                }
            }
            return segments;
        }


        public static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }

        public static void AddChunkToTemplateAndSentence(Chunk chunk, ref string template, ref string chunkedSentence , ref string originalSentence, ref string pronedSentence)
        {
            template += chunk.Tag.AddLessThanGreaterThan();
            chunkedSentence += " [" + chunk.Phrase.Trim() + " " + chunk.Tag + "]";
            originalSentence += chunk.Phrase.Trim() + " " + chunk.OutOfChunkPhrase + " ";
            pronedSentence += " [" + chunk.Phrase.Trim() + " " + chunk.Tag + "]";
        }

        public static void AddChunkToSentence(Chunk chunk, ref string sentence)
        {
            sentence += " [" + chunk.Phrase.Trim() + " " + chunk.Tag + "]";
        }

        public static void AddChunkToOriginalSentence(Chunk chunk, ref string originalSentence)
        {
            originalSentence += chunk.Phrase.Trim() + " " + chunk.OutOfChunkPhrase + " " ;
        }

        public static void AddOriginalPhrase(Chunk chunk, ref string template, ref string chunkSentence, ref string originalSentence, ref string pronedSentence)
        {
            template += chunk.Phrase.AddLessThanGreaterThan();
            chunkSentence += " [" + chunk.Phrase.Trim() + " " + chunk.Tag + "]";
            originalSentence += chunk.Phrase.Trim() + " " + chunk.OutOfChunkPhrase + " ";
            pronedSentence += " [" + chunk.Phrase.Trim() + " " + chunk.Tag + "]";
        }

        public static List<string> ParsTemplate(string template) // for example: <NP><NP><است>
        {
            List<string> results = new List<string>();
            string regexKeyValue = @"<[^>]+>";
            MatchCollection matchCollection = Regex.Matches(template, regexKeyValue);
            foreach (Match match in matchCollection)
            {
                results.Add(match.Value.Substring(1, match.Value.Length - 2));  // remove first and last character
            }
            return results;
        }

        public static List<Chunk> ParsChenckedSentence(string chenckedSentence) // for example: [نوروز NP] [آیینی بس ارجمند و ستودنی NP] [است VP]
        {
            List<Chunk> results = new List<Chunk>();
            string regexKeyValue = @"\[[^\]]+\]";
            MatchCollection matchCollection = Regex.Matches(chenckedSentence, regexKeyValue);
            foreach (Match match in matchCollection)
            {
                string temp = match.Value.Substring(1, match.Value.Length - 2); //remove brackets
                string[] splited = temp.Split();
                string tag = splited[splited.Length - 1];
                string phrase = String.Join(" ", splited, 0, splited.Length - 1);
                Chunk phrasetag = new Chunk
                {
                    Phrase = phrase
                    ,
                    Tag = tag
                    ,
                    OutOfChunkPhrase = ""
                };
                results.Add(phrasetag);  // remove first and last character
            }
            return results;
        }

        public static List<int> ParsListIndex(string text)
        {
            List<int> result = new List<int>();
            List<string> tokens = text.Split(',').ToList();
            tokens = tokens.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            foreach (string token in tokens)
            {
                result.Add(Int32.Parse(token));
            }
            return result;
        } 
    }
}
