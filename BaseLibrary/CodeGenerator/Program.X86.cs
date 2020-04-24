using System;
using System.Diagnostics;
using System.IO;
using Larpx.ResourceSpider.BaseLibrary.Extension;

namespace Larpx.ResourceSpider.BaseLibrary.CodeGenerator.X86
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 1)
                {
                    args = Json.Parser.Parse<string[]>(File.ReadAllText(args[0]));
                    if (args.Length >= 4) CodeGenerator.Program.X86(args);
                }
            }
            catch (Exception error)
            {
                Messages.Add(error);
            }
            finally { Messages.Open(); }
        }
    }
}
