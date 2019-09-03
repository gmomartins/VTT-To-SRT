using System;
using System.IO;
using System.Text;

namespace VTT_To_SRT
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (string filePath in args)
                {

                    if (Path.GetExtension(filePath).Equals(".vtt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Converting file {filePath}...");
                        ConvertToSrt(filePath);
                    }
                }
            }

            Console.WriteLine("Press Enter to exit...");

            Console.ReadKey();
        }

        static void ConvertToSrt(string filePath)
        {
            try
            {
                string outputFolder = Path.GetDirectoryName(filePath);
                using (StreamReader stream = new StreamReader(filePath))
                {
                    StringBuilder output = new StringBuilder();
                    int lineNumber = 1;
                    while (!stream.EndOfStream)
                    {
                        string line = stream.ReadLine();
                        if (IsTimecode(line))
                        {
                            output.AppendLine(lineNumber.ToString());
                            lineNumber++;
                            line = line.Replace('.', ',');
                            line = DeleteCueSettings(line);
                            output.AppendLine(line);
                            bool foundCaption = false;
                            while (true)
                            {
                                if (stream.EndOfStream)
                                {
                                    if (foundCaption)
                                        break;
                                    else
                                        throw new Exception("Corrupted file: Found timecode without caption");
                                }
                                line = stream.ReadLine();
                                if (String.IsNullOrEmpty(line) || String.IsNullOrWhiteSpace(line))
                                {
                                    output.AppendLine();
                                    break;
                                }
                                foundCaption = true;
                                output.AppendLine(line);
                            }
                        }
                    }
                    string fileName = Path.GetFileNameWithoutExtension(filePath) + ".srt";
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(outputFolder, fileName)))
                        outputFile.Write(output);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static bool IsTimecode(string line)
        {
            return line.Contains("-->");
        }

        static string DeleteCueSettings(string line)
        {
            StringBuilder output = new StringBuilder();
            foreach (char ch in line)
            {
                char chLower = Char.ToLower(ch);
                if (chLower >= 'a' && chLower <= 'z')
                {
                    break;
                }
                output.Append(ch);
            }
            return output.ToString();
        }
    }
}
