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
    public class PerStem
    {
        public static string Stem(string text)
        {
            var utf8WithoutBom = new UTF8Encoding(false);
            //            var executionPath = ConfigurationManager.AppSettings["ExecutionPath"];
            var executionPath = File.ReadAllText(@"c:\executionPath.txt");
            var ticks = DateTime.Now.Ticks;
            File.WriteAllText(executionPath + $"../../../../Seraji/PerStem/input{ticks}.txt", text, utf8WithoutBom);
            File.Delete(executionPath + $"../../../../Seraji/PerStem/output{ticks}.txt");

            var dirPath = Path.GetFullPath(executionPath + "../../../../Seraji/PerStem/");
            var strCmdText = "/C cd " + dirPath + $" & perl perstem.pl < input{ticks}.txt > output{ticks}.txt & ping 127.0.0.1 -n 2 > nul";
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
            string parsedString = File.ReadAllText(executionPath + $"../../../../Seraji/PerStem/output{ticks}.txt");
            // remove \r\n from end of file!
            parsedString = parsedString.Substring(0, parsedString.Length - 2);
            return parsedString;
        }
    }
}
