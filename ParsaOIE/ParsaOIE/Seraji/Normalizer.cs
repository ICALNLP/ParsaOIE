using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;


namespace RahatCoreNlp.Seraji
{
    public class Normalizer
    {
        public static string Normalize(string text)
        {
            text = text.Replace("\r\n", "\n").Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
            //            var executionPath = ConfigurationManager.AppSettings["ExecutionPath"];
            var executionPath = File.ReadAllText(@"c:\executionPath.txt");

            var utf8WithoutBom = new UTF8Encoding(false);

            var ticks = DateTime.Now.Ticks;

            File.WriteAllText(executionPath + $"../../../../Seraji/Normalizer/input{ticks}.txt", text, utf8WithoutBom);

            File.Delete(executionPath + $"../../../../Seraji/Normalizer/output{ticks}.txt");

            var dirPath = Path.GetFullPath(executionPath + "../../../../Seraji/Normalizer/");
            string strCmdText = "/C cd " + dirPath + $" & ruby pre_per.rb input{ticks}.txt > output{ticks}.txt & ping 127.0.0.1 -n 2 > nul";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                WorkingDirectory = dirPath,
                Arguments = strCmdText,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            
            using (Process p = Process.Start(startInfo))
            {
                try
                {
                    p.WaitForExit();
                }
                catch (Exception e)
                {

                    throw e;
                }
            }

            string parsedString = File.ReadAllText(executionPath + $"../../../../Seraji/Normalizer/output{ticks}.txt");
            // remove \r\n from end of file!
            parsedString = parsedString.Replace("\r\n", "\n").Replace("\r", "").Replace("\n", "");
            return parsedString;
        }
    }
}
