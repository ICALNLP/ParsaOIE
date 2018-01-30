using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RahatCoreNlp.Seraji
{
    public class SentenceSegmenter
    {
        public static string[] GetSegments(string text)
        {
            var utf8WithoutBom = new System.Text.UTF8Encoding(false);
            //            var executionPath = ConfigurationManager.AppSettings["ExecutionPath"];
            var executionPath = File.ReadAllText(@"c:\executionPath.txt");
            var ticks = DateTime.Now.Ticks;
            File.WriteAllText(executionPath + $"../../../../Seraji/SentneceSegmenter/input{ticks}.txt", text, utf8WithoutBom);
            File.Delete(executionPath + $"../../../../Seraji/SentneceSegmenter/output{ticks}.txt");

            var dirPath = Path.GetFullPath(executionPath + "../../../../Seraji/SentneceSegmenter/");
            var strCmdText = "/C cd " + dirPath + $" & perl fa_sent.pl < input{ticks}.txt > output{ticks}.txt & ping 127.0.0.1 -n 2 > nul";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                WorkingDirectory = dirPath,
                Arguments = strCmdText,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process p = System.Diagnostics.Process.Start(startInfo);
            while (!p.HasExited)
            {
                Thread.Sleep(1000);
            }
            string parsedString = File.ReadAllText(executionPath + $"../../../../Seraji/SentneceSegmenter/output{ticks}.txt");

            string [] sentences = parsedString.Split('\n');

            // trim all tokens
            sentences = sentences.Select(s => s.Trim()).ToArray();

            //  remove empty tokens.
            sentences = sentences.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            return sentences;
        }
    }
}
