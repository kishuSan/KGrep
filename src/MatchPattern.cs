using System;

internal class PatternMatcher
{

    internal bool MatchPattern(string inputLine, string pattern)
    {
        Console.WriteLine(inputLine);
        Console.WriteLine(pattern);
        if (pattern.Length >= 2 && pattern[0] == '[')
        {
            HashSet<char> charset = new HashSet<char>();
            for(int i = 1; i < pattern.Length-1; i++)
            {
                charset.Add(pattern[i]);
            }
            foreach(char ch in inputLine)
            {
                if(charset.Contains(ch))
                {
                    return true;
                }
            }
            return false;
        }
        else if(pattern == "\\d")
        {
            foreach(char ch in inputLine)
            {
                //int val = ch - '0';
                //Console.WriteLine("Char: " + ch + " Value: " + val);
                if ('0' <= ch && ch <= '9')
                {
                    return true;
                }
            }
            return false;
        }
        else if(pattern == "\\w")
        {
            foreach (char ch in inputLine)
            {
                //Console.WriteLine("Char: " + ch + " Value: " + (int)ch);
                if (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ('0' <= ch && ch <= '9') || ch == '_') 
                    return true;
            }
            return false;
        }
        else
        {
            throw new ArgumentException($"Unhandled pattern: {pattern}");
        }
    }
}
