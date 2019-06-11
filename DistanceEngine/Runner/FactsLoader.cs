using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Distance.Engine.Runner
{
    class FactsLoader
    {
        public static IEnumerable<T> Load<T>(string pcapPath, string filter, string[] fields, Func<string[], T> creator)
        {
            return RunShark(pcapPath, filter, fields).Select(arg => creator(arg.Split(Separator)));
        }

        public static IEnumerable<object> Load(string pcapPath, string filter, string[] fields, Func<string[], object> creator)
        {
            return RunShark(pcapPath, filter, fields).Select(arg => creator(arg.Split(Separator)));
        }

        public static IEnumerable<string> RunShark(string inputfile, string filter, params string[] fields)
        {
            var fieldString = String.Join(" -e ", fields);
            var arguments = $"-r {inputfile} -Y {filter} -T fields -e {fieldString}";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "tshark",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            while (true)
            {
                string line = process.StandardOutput.ReadLine();
                if (line != null)
                {
                    yield return line;
                }
                else
                {
                    break;
                }
            }

            process.WaitForExit();
        }

        public static readonly Char Separator = '\t';
    }
}
