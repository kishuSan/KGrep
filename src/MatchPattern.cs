using System;

internal class PatternMatcher
{
    string InputLine { get; set; }
    internal PatternMatcher(string ip = "")
    {
        InputLine = ip;
    }
    internal bool MatchPattern(string pattern)
    {
        Console.WriteLine(InputLine);
        Console.WriteLine(pattern);

        if(InputLine == null)
        {
            throw new ArgumentException("Input line is null");
        }

        switch (pattern[0])
        {
        case '\\':
            if(pattern == "\\d")
            {
                return MatchDigit(pattern);
            }
            else if (pattern == "\\w")
            {
                return MatchWord(pattern);
            }
            else
            {
                throw new ArgumentException($"Unhandled pattern: {pattern}");
            }
        case '[':
            return MatchCharSet(pattern);
        default:
            if(pattern.Length == 1)
            {
                return MatchSingleChar(pattern[0]);
            }
            else
            {
                throw new ArgumentException($"Unhandled pattern: {pattern}");
            }
        }
    }

    private bool MatchSingleChar(char pattern)
    {
        foreach (char ch in InputLine)
        {
            if (ch == pattern) return true;
        }
        return false;
    }

    private bool MatchWord(string pattern)
    {
        foreach (char ch in InputLine)
        {
            //Console.WriteLine("Char: " + ch + " Value: " + (int)ch);
            if (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ('0' <= ch && ch <= '9') || ch == '_') 
                return true;
        }
        return false;
    }
    private bool MatchDigit(string pattern)
    {
        foreach(char ch in InputLine)
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
    private bool MatchCharSet(string pattern)
    {
        HashSet<char> charset = new HashSet<char>();
        int start = pattern[1] == '^' ? 2 : 1;
        for(int i = start; i < pattern.Length-1; i++)
        {
            charset.Add(pattern[i]);
        }
        return start == 1 ? MatchPositiveCharSet(charset) : MatchNegativeCharSet(charset);
    }
    private bool MatchPositiveCharSet(HashSet<char> charset)
    {
        foreach(char ch in InputLine)
        {
            if(charset.Contains(ch))
            {
                return true;
            }
        }
        return false; 
    }
    private bool MatchNegativeCharSet(HashSet<char> charset)
    {
        foreach(char ch in InputLine)
        {
            if(!charset.Contains(ch))
            {
                return true;
            }
        }
        return false;
    }
}
