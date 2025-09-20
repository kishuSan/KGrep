using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using codecrafters_grep.src;

internal class KGrep
{
    // input line given by the user
    private string InputLine { get; }
    
    // dynamic array sequence of the parsed patterns
    private List<Pattern> Patterns { get; }

    internal KGrep(string input, string pattern)
    {
        if (input == null || input.Length == 0) throw new ArgumentNullException("Input line is null");
        if (pattern == null || pattern.Length == 0) throw new ArgumentNullException("Pattern is null or empty");

        InputLine = input;
        Patterns = new List<Pattern>();
        ParsePattern(pattern); // parse the pattern into a sequence
    }

    private void ParsePattern(string pattern)
    {
        int i = 0;
        while (i < pattern.Length)
        {
            Pattern curr_pattern = new Pattern();
            if (pattern[i] == '\\')
            {
                if (i + 1 >= pattern.Length)
                {
                    throw new ArgumentException("Dangling escape at end of pattern");
                }

                char esc = pattern[++i];
                if (esc == 'd')
                {
                    curr_pattern.type = "d";
                    for (char x = '0'; x <= '9'; x++)
                        curr_pattern.CharSet.Add(x);
                }
                else if (esc == 'w')
                {
                    curr_pattern.type = "w";
                    for (char x = '0'; x <= '9'; x++)
                        curr_pattern.CharSet.Add(x);
                    for (char x = 'a'; x <= 'z'; x++)
                        curr_pattern.CharSet.Add(x);
                    for (char x = 'A'; x <= 'Z'; x++)
                        curr_pattern.CharSet.Add(x);
                    curr_pattern.CharSet.Add('_');
                }
                else
                {
                    curr_pattern.type = "lc"; // literal char
                    curr_pattern.CharSet.Add(esc);
                }
                i++;
            }
            else if (pattern[i] == '[') 
            {
                curr_pattern.type = "cs";
                if (i+1 < pattern.Length && pattern[i + 1] == '^') {
                    curr_pattern.type = "ncs";
                    curr_pattern.negated = true;
                    i++;
                }

                i++;
                while (i < pattern.Length && pattern[i] != ']')
                {
                    curr_pattern.CharSet.Add(pattern[i++]);
                }

                if(i == pattern.Length)
                {
                    throw new ArgumentException("Invalid Pattern: " + pattern);
                }
                i++;
            }
            else
            {
                curr_pattern.type = "lc";
                curr_pattern.CharSet.Add(pattern[i++]);
            }
            Patterns.Add(curr_pattern);
        }
    }

    internal bool MatchPattern()
    {
        for (int i = 0; i < InputLine.Length; i++)
        {
            // iterate through every char input and then start matching the pattern from there assuming that it is the starting of the string 
            if (Match(i))
            {
                return true;
            }
        }
        return false;
    }

    internal bool Match(int start)
    {
        //might have to remove this check in future;
        if(InputLine.Length - start < Patterns.Count) return false;

        int i = 0;
        for(i = 0; i < Patterns.Count && start < InputLine.Length; i++, start++)
        {
            if (Patterns[i].negated)
            {
                if (Patterns[i].CharSet.Contains(InputLine[start])) return false;
            }
            else if(!Patterns[i].CharSet.Contains(InputLine[start])) return false;
        }
        if(i == Patterns.Count) return true;
        return false;
    }

    //internal bool MatchPattern(string pattern)
    //{
    //    switch (pattern[0])
    //    {
    //    case '\\':
    //        if(pattern == "\\d")
    //        {
    //            if ('0' <= MatchInput && MatchInput <= '9') return true;
    //            return false;
    //            //return MatchDigit();
    //        }
    //        else if (pattern == "\\w")
    //        {
    //            if (('a' <= MatchInput && MatchInput <= 'z') 
    //                    || ('A' <= MatchInput && MatchInput <= 'Z') 
    //                    || ('0' <= MatchInput && MatchInput <= '9') 
    //                    || MatchInput == '_') return true;
    //                //return MatchWord();
    //            return false;
    //        }
    //        else
    //        {
    //            throw new ArgumentException($"Unhandled pattern: {pattern}");
    //        }
    //    case '[':
    //        return MatchCharSet(pattern);
    //    default:
    //        if(pattern.Length == 1)
    //        {
    //                if (MatchInput == pattern) return true;
    //                return false;
    //                //return MatchSingleChar(pattern[0]);
    //        }
    //        else
    //        {
    //            throw new ArgumentException($"Unhandled pattern: {pattern}");
    //        }
    //    }
    //}

    //private bool MatchSingleChar(char pattern)
    //{
    //    foreach (char ch in MatchInput)
    //    {
    //        if (ch == pattern) return true;
    //    }
    //    return false;
    //}

    //private bool MatchWord()
    //{
    //    foreach (char ch in MatchInput)
    //    {
    //        //Console.WriteLine("Char: " + ch + " Value: " + (int)ch);
    //        if (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ('0' <= ch && ch <= '9') || ch == '_') 
    //            return true;
    //    }
    //    return false;
    //}
    //private bool MatchDigit()
    //{
    //    foreach(char ch in MatchInput)
    //    {
    //        //int val = ch - '0';
    //        //Console.WriteLine("Char: " + ch + " Value: " + val);
    //        if ('0' <= ch && ch <= '9')
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //private bool MatchCharSet(string pattern)
    //{
    //    HashSet<char> charset = new HashSet<char>();
    //    int start = pattern[1] == '^' ? 2 : 1;
    //    for(int i = start; i < pattern.Length-1; i++)
    //    {
    //        charset.Add(pattern[i]);
    //    }
    //    return start == 1 ? MatchPositiveCharSet(charset) : MatchNegativeCharSet(charset);
    //}
    //private bool MatchPositiveCharSet(HashSet<char> charset)
    //{
    //    foreach(char ch in MatchInput)
    //    {
    //        if(charset.Contains(ch))
    //        {
    //            return true;
    //        }
    //    }
    //    return false; 
    //}
    //private bool MatchNegativeCharSet(HashSet<char> charset)
    //{
    //    foreach(char ch in MatchInput)
    //    {
    //        if(!charset.Contains(ch))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
}
