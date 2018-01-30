using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace WcfService
{
    public static class Engine
    {
        public static void Start()
        {
            var utf8WithoutBom = new System.Text.UTF8Encoding(false);
            //            var executionPath = ConfigurationManager.AppSettings["ExecutionPath"];
            var executionPath = File.ReadAllText(@"c:\executionPath.txt");

            var dirPath = Path.GetFullPath(executionPath + "../../../../HazmServerSide/");

            var strCmdText = "/C cd " + dirPath + " & python HazmWebServer.py & ping 127.0.0.1 -n 2 > nul";

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
        }
    }
}
