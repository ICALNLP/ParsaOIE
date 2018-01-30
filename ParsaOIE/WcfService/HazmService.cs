using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using WcfService.ServiceReferenceHazmLocal;

namespace WcfService
{
    public static class HazmService
    {
        private static my_dispatcherPortTypeClient  _hazmWebService = new my_dispatcherPortTypeClient();
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
                WcfService.Engine.Start();
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
                portions = Utilizer.SplitUsingUpssalaSentSegmenter(input, safePrtionSize);
            }
            else
            {
                portions = new List<string>() { input };
            }

            for (int i = 0; i < portions.Count; i++)
            {
                // normalize input text
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
                portions = Utilizer.SplitUsingUpssalaSentSegmenter(input, safePrtionSize);
            }
            else
            {
                portions = new List<string>() { input };
            }

            List<string> finalTokens = new List<string>();
            for (int i = 0; i < portions.Count; i++)
            {
                // normalize input text
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
                portions = Utilizer.SplitUsingUpssalaSentSegmenter(input, safePrtionSize);
            }
            else
            {
                portions = new List<string>() { input };
            }

            List<string> finalTokens = new List<string>();
            for (int i = 0; i < portions.Count; i++)
            {
                // normalize input text
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
            return HazmWebService().Stem(input);
        }

        public static string Lemmatize(string input)
        {
            if (input == "")
                return "";
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
                portions = Utilizer.SplitUsingUpssalaSentSegmenter(input, safePrtionSize);
            }
            else
            {
                portions = new List<string>() { input };
            }

            List<string> finalTokens = new List<string>();
            for (int i = 0; i < portions.Count; i++)
            {
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
            return HazmWebService().Chunk(input);
        }

        public static string Parse(string input)
        {
            if (input == "")
                return "";
            return HazmWebService().Parse(input);
        }
    }
}
