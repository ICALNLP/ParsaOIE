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
    public class Tokenizer
    {
        public static string[] GetTokens(string text)
        {
            var utf8WithoutBom = new UTF8Encoding(false);
            //            var executionPath = ConfigurationManager.AppSettings["ExecutionPath"];
            var executionPath = File.ReadAllText(@"c:\executionPath.txt");
            var ticks = DateTime.Now.Ticks;
            File.WriteAllText(executionPath + $"../../../../Seraji/Tokenizer/input{ticks}.txt", text, utf8WithoutBom);
            File.Delete(executionPath + $"../../../../Seraji/Tokenizer/output{ticks}.txt");

            var dirPath = Path.GetFullPath(executionPath + "../../../../Seraji/Tokenizer/");
            var strCmdText = "/C cd " + dirPath + $" & perl fa_tok.pl < input{ticks}.txt > output{ticks}.txt & ping 127.0.0.1 -n 2 > nul";

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
            string parsedString = File.ReadAllText(executionPath + $"../../../../Seraji/Tokenizer/output{ticks}.txt");
            // removing last item from array if it's empty.
            List<string> list = parsedString.Split(' ').ToList();

            // trim all tokens
            list = list.Select(s => s.Trim()).ToList();

            //  remove empty tokens.
            list = list.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return list.ToArray();
        }
    }
}
