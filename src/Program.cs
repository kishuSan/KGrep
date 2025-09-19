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

            var matcher = new PatternMatcher(inputLine);

            if (matcher.StreamMatchPattern(pattern))
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
