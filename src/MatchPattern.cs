using System;

internal class PatternMatcher
{

    internal bool MatchPattern(string inputLine, string pattern)
    {
        Console.WriteLine(inputLine);
        Console.WriteLine(pattern);
        if (pattern.Length == 1)
        {
             return inputLine.Contains(pattern);
        }
        else if(pattern.Length == 2 && pattern == "\\d")
        {
            foreach(char ch in inputLine)
            {
                if ('0' <= ch - '0' || ch - '0' <= 9)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            throw new ArgumentException($"Unhandled pattern: {pattern}");
        }
        return false;
    }
}
