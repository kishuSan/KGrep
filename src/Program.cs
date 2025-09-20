using System;
using System.IO;

namespace codecrafters_grep.src
{
    internal class Program // Contains the entry point of the application
    {
        static void Main(string[] args)
        { 
            if (args[0] != "-E")
            {
                Console.WriteLine("Expected first argument to be '-E'");
                Environment.Exit(2);
            }

            string pattern = args[1];
            string inputLine = Console.In.ReadToEnd();
            
            Console.WriteLine(inputLine);
            Console.WriteLine(pattern);

            var matcher = new KGrep(inputLine, pattern);

            if (matcher.MatchPattern())
            {
                Environment.Exit(0);
            }
            else
            {
                Environment.Exit(1);
            }
        }
    }
  }
