using System;
using System.ComponentModel;
using System.ComponentModel.Design;

internal class PatternMatcher
{
    string? InputLine { get; set; }
    string MatchInput { get; set; }
    internal PatternMatcher(string ip = null)
    {
        InputLine = ip;
    }

    internal bool StreamMatchPattern(string pattern)
    {
        if(InputLine == null)
        {
            throw new ArgumentException("Input line is null");
        }
        if(pattern == null || pattern.Length == 0)
        {
            throw new ArgumentException("Pattern is null or empty");
        }

        int i = 0;
        int j = 0;
        while(i < pattern.Length && j < InputLine.Length)
        {
            string ParsedPattern = "";
            if (pattern[i] == '[')
            {
                while (pattern[i] != ']' && i < pattern.Length)
                {
                    ParsedPattern += pattern[i++];
                }
                // handle case where there is no closing ']'
            }
            else if (pattern[i] == '\\')
            {
                ParsedPattern += pattern[i++];
                if (i < pattern.Length) ParsedPattern += pattern[i++];
            }
            else ParsedPattern += pattern[i++];

            MatchInput = InputLine[j].ToString();
            
            Console.WriteLine("Matched Input: " + MatchInput);
            Console.WriteLine("Parsed Pattern: " + ParsedPattern);
            
            while (!MatchPattern(ParsedPattern) && ++j < InputLine.Length)
            {
                MatchInput = InputLine[j].ToString();
                Console.WriteLine("Matched Input: " + MatchInput);
                Console.WriteLine("Parsed Pattern: " + ParsedPattern);
            }

            j++;
        }
        if (i < pattern.Length) return false;
        return true;
    }

    internal bool MatchPattern(string pattern)
    {
        switch (pattern[0])
        {
        case '\\':
            if(pattern == "\\d")
            {
                return MatchDigit();
            }
            else if (pattern == "\\w")
            {
                return MatchWord();
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
        foreach (char ch in MatchInput)
        {
            if (ch == pattern) return true;
        }
        return false;
    }

    private bool MatchWord()
    {
        foreach (char ch in MatchInput)
        {
            //Console.WriteLine("Char: " + ch + " Value: " + (int)ch);
            if (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ('0' <= ch && ch <= '9') || ch == '_') 
                return true;
        }
        return false;
    }
    private bool MatchDigit()
    {
        foreach(char ch in MatchInput)
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
        foreach(char ch in MatchInput)
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
        foreach(char ch in MatchInput)
        {
            if(!charset.Contains(ch))
            {
                return true;
            }
        }
        return false;
    }
}
