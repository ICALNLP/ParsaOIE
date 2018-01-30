using System;
using System.Collections.Generic;
using System.Linq;
using RahatCoreNlp.HazmWebReference;
using RahatCoreNlp.Utility;
using RahatCoreNlp.WebReference;

namespace RahatCoreNlp.Service
{
    public static class HazmService
    {
        public static bool UseWebReference = true;     // true: use web reference/ false: use local reference to hazm library
        private static ParsaWebService parsaWebService = new ParsaWebService();
        private static my_dispatcherPortTypeClient _hazmWebService;
        static int safePrtionSize = 5000; // to split big strings before calling webservice
        public static my_dispatcherPortTypeClient HazmWebService()
        {
            if (_hazmWebService == null)
            {
                _hazmWebService = new my_dispatcherPortTypeClient();
            }
            try
            {
                _hazmWebService.Normalizer("سلام");
            }
            catch (Exception e)
            {
                Hazm.Engine.Start();
            }
            return _hazmWebService;
        }

        public static string Normalizer(string input)
        {
            if (input == "" || input == null)
                return "";

            input = Utilizer.RemoveTroublesomeCharacters(input);

            // split string based on size
            // 5000 character is a safe size for hazm websevice
            List<string> portions;
            if (input.Length > safePrtionSize)
            {
                portions = Utility.Utilizer.SplitUsingUpssalaSentSegmenter(input, safePrtionSize);
            }
            else
            {
                portions = new List<string>() { input };
            }

            for (int i = 0; i < portions.Count; i++)
            {
                // normalize input text
                if (UseWebReference)
                    portions[i] = parsaWebService.Hazm_Normalizer(portions[i]);
                else
                    portions[i] = HazmWebService().Normalizer(portions[i]);
            }

            string output = portions.Aggregate((x, y) => x + y);

            return output;
        }

        public static string[] SentenceTokenizer(string input)
        {
            if (input == "" || input == null)
            {
                string[] a = new string[0];
                return a;
            }

            input = Utilizer.RemoveTroublesomeCharacters(input);

            // split string based on size
            // 5000 character is a safe size for hazm websevice
            List<string> portions;
            if (input.Length > safePrtionSize)
            {
                portions = Utility.Utilizer.SplitUsingUpssalaSentSegmenter(input, safePrtionSize);
            }
            else
            {
                portions = new List<string>() { input };
            }

            List<string> finalTokens = new List<string>();
            for (int i = 0; i < portions.Count; i++)
            {
                // normalize input text
                if (UseWebReference)
                    finalTokens.AddRange(parsaWebService.Hazm_SentenceTokenizer(portions[i]));
                else
                    finalTokens.AddRange(HazmWebService().SentenceTokenizer(portions[i]));
            }

            // trim all tokens
            finalTokens = finalTokens.Select(s => s.Trim()).ToList();

            //  remove empty tokens.
            finalTokens = finalTokens.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return finalTokens.ToArray();
        }

        public static string[] WordTokenizer(string input)
        {
            if (input == "")
            {
                string[] a = new string[0];
                return a;
            }

            input = Utilizer.RemoveTroublesomeCharacters(input);

            // split string based on size
            // 5000 character is a safe size for hazm websevice
            List<string> portions;
            if (input.Length > safePrtionSize)
            {
                portions = Utility.Utilizer.SplitUsingUpssalaSentSegmenter(input, safePrtionSize);
            }
            else
            {
                portions = new List<string>() { input };
            }

            List<string> finalTokens = new List<string>();
            for (int i = 0; i < portions.Count; i++)
            {
                // normalize input text
                if (UseWebReference)
                    finalTokens.AddRange(parsaWebService.Hazm_WordTokenizer(portions[i]));
                else
                    finalTokens.AddRange(HazmWebService().WordTokenizer(portions[i]));
            }

            // trim all tokens
            finalTokens = finalTokens.Select(s => s.Trim()).ToList();

            //  remove empty tokens.
            finalTokens = finalTokens.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return finalTokens.ToArray();
        }

        public static string Stem(string input)
        {
            if (input == "")
                return "";
            if (UseWebReference)
                return parsaWebService.Hazm_Stemmer(input);
            return HazmWebService().Stem(input);
        }

        public static string Lemmatize(string input)
        {
            if (input == "")
                return "";
            if (UseWebReference)
                return parsaWebService.Hazm_Lemmatizer(input);
            return HazmWebService().Lemmatize(input);
        }

        public static string[] PosTag(string input)
        {
            if (input == "")
            {
                string[] a = new string[0];
                return a;
            }

            input = Utilizer.RemoveTroublesomeCharacters(input);

            // split string based on size
            // 5000 character is a safe size for hazm websevice
            List<string> portions;
            if (input.Length > safePrtionSize)
            {
                portions = Utility.Utilizer.SplitUsingUpssalaSentSegmenter(input, safePrtionSize);
            }
            else
            {
                portions = new List<string>() { input };
            }

            List<string> finalTokens = new List<string>();
            for (int i = 0; i < portions.Count; i++)
            {
                if (UseWebReference)
                    finalTokens.AddRange(parsaWebService.Hazm_PosTagger(portions[i]));
                else
                    finalTokens.AddRange(HazmWebService().PosTag(portions[i]));
            }

            // trim all tokens
            finalTokens = finalTokens.Select(s => s.Trim()).ToList();

            //  remove empty tokens.
            finalTokens = finalTokens.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return finalTokens.ToArray();
        }

        public static string Chunk(string input)
        {
            if (input == "")
                return "";
            if (UseWebReference)
                return parsaWebService.Hazm_RawChunker(input);
            return HazmWebService().Chunk(input);
        }

        public static string Parse(string input)
        {
            if (input == "")
                return "";
            if (UseWebReference)
                return parsaWebService.Hazm_RawParser(input);
            return HazmWebService().Parse(input);
        }
    }
}
