using RahatCoreNlp.Data;
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
    public class TagPer
    {
        // input is a tokenized sentence. output is corresponding PosTags 
        public static string[] GetTags(string[] sentenceTokens)
        {
            StringBuilder fileContent = new StringBuilder();
            foreach (string token in sentenceTokens)
            {
                fileContent.Append(token + "\n");
            }

            var utf8WithoutBom = new System.Text.UTF8Encoding(false);
            //            var executionPath = ConfigurationManager.AppSettings["ExecutionPath"];
            var executionPath = File.ReadAllText(@"c:\executionPath.txt");
            var ticks = DateTime.Now.Ticks;
            File.WriteAllText(executionPath + $"../../../../Seraji/PosTagger/input{ticks}.txt", fileContent.ToString(), utf8WithoutBom);
            File.Delete(executionPath + $"../../../../Seraji/PosTagger/output{ticks}.txt");

            // run command. first I change directory to YaraParser. then run command. then wait for 3 second for user to read the command prompet.
            var dirPath = Path.GetFullPath(executionPath + "../../../../Seraji/PosTagger/");
            var strCmdText = "/C cd " + dirPath + $" & hunpos-tag.exe model_TagPer < input{ticks}.txt > output{ticks}.txt & ping 127.0.0.1 -n 2 > nul";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                WorkingDirectory = dirPath,
                Arguments = strCmdText,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process p = Process.Start(startInfo);
            while (!p.HasExited)
            {
                Thread.Sleep(1000);
            }
            string parsedString = File.ReadAllText(executionPath + $"../../../../Seraji/PosTagger/output{ticks}.txt");
            var output = parsedString.Split('\n').ToList();
            // trim all tokens
            output = output.Select(s => s.Trim()).ToList();

            //  remove empty tokens.
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return output.ToArray();
        }
    }
}
