using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CleanFilenames
{
    class Program
    {
        static int Main(string[] args)
        {
            int iRequired = 1;
            string sPattern = "";
            string sReplace = "";
            string sPath = Environment.CurrentDirectory;
            bool bTest = false;
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "/s":
                        sPattern = args[++i];
                        Console.WriteLine("Debug: sPattern set to " + sPattern);
                        iRequired--;
                        break;
                    case "/r":
                        sReplace = args[++i];
                        Console.WriteLine("Debug: sReplace set to " + sReplace);
                        break;
                    case "/p":
                        sPath = args[++i];
                        Console.WriteLine("Debug: sPath set to " + sPath);
                        break;
                    case "/t":
                        bTest = true;
                        Console.WriteLine("Debug: bTest set to true");
                        break;
                    default:
                        Console.WriteLine("Invalid input: " + args[i] + "!");
                        Program.Usage();
                        return 1;
                }
            }

            if(iRequired != 0)
            {
                Program.Usage();
                return 1;
            }

            if (bTest)
                Console.WriteLine("[Test Mode - no files will be renamed!]");

            Console.WriteLine("Processing files in:\n" + sPath + "...");

            Regex oRegex = new Regex(sPattern);
            int iTouched = 0;
            DirectoryInfo di = new DirectoryInfo(sPath);
            FileInfo[] fi = di.GetFiles();
            foreach (FileInfo fiTemp in fi)
            {
                Console.Write(fiTemp.Name + ": ");

                if (oRegex.IsMatch(fiTemp.Name))
                {
                    Console.WriteLine("Match!");

                    string sNewName = oRegex.Replace(fiTemp.Name, sReplace);
                    sNewName = Path.Combine(sPath, sNewName);
                    int iUniqueCounter = 0;
                    while (File.Exists(sNewName)) //Append ".nn" to the filename
                    {
                        iUniqueCounter++;
                        string sTempName = string.Concat(Path.GetFileNameWithoutExtension(sNewName),
                                                          ".",
                                                          iUniqueCounter.ToString("D2"),
                                                          fiTemp.Extension);
                        sTempName = Path.Combine(sPath, sTempName);
                        if (!File.Exists(sTempName))
                            sNewName = sTempName;
                    }
                    Console.WriteLine("***RENAME***:\n\t" + Path.GetFileName(sNewName));
                    iTouched++;
                    if (bTest)
                        Console.WriteLine("[Test Mode - no files are being renamed!]");
                    else
                    {
                        try
                        {
                            fiTemp.MoveTo(sNewName);
                        }
                        catch (Exception e)
                        {
                            Console.Write("Error updating file: " + e.Message);
                        }
                    }
                }
                else
                    Console.WriteLine("OK");
            }

            Console.WriteLine("Operations complete:\n\tTotal Files:" + fi.GetLength(0) + "\n\tRenamed:" + iTouched + "\n\tUntouched: " + (fi.GetLength(0) - iTouched));
            return 0;
        }

        static void Usage()
        { 
            Console.WriteLine("Use this tool to rename files where specified patterns are replaced.");
            Console.WriteLine("\nUsage: cleanfilenames </s <search>> [/r <replace>] [/p <path>] [/t]\n\tWhere:\n");
            Console.WriteLine("\t<search>\t- RegEx search pattern to find in filenames");
            Console.WriteLine("\t<replace>\t- RegEx replacement string (use \"$[1,2...n]\" to substitute search groups)");
            Console.WriteLine("\t<path>\t\t- Path to clean files");
            Console.WriteLine("\t/t\t\t- Tests the RegEx expression (i.e. no files are modified)");
            Console.WriteLine("Example: the following will remove all non-English characters from files in the current folder:");
            Console.WriteLine("\t cleanfilenames /s \\p{Lo} /r \"\"");
        }
    }
}
