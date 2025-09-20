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
            if (pattern[i] == '^')
            {
                curr_pattern.type = PatternType.startOfString;
            }
            else if (pattern[i] == '$')
            {
                curr_pattern.type = PatternType.endOfString;
            }
            else if (pattern[i] == '\\')
            {
                if (i + 1 >= pattern.Length)
                {
                    throw new ArgumentException("Dangling escape at end of pattern");
                }

                char esc = pattern[++i];
                if (esc == 'd')
                {
                    curr_pattern.type = PatternType.digit;
                    for (char x = '0'; x <= '9'; x++) curr_pattern.CharSet.Add(x);
                }
                else if (esc == 'w')
                {
                    curr_pattern.type = PatternType.word;
                    for (char x = '0'; x <= '9'; x++) curr_pattern.CharSet.Add(x);
                    for (char x = 'a'; x <= 'z'; x++) curr_pattern.CharSet.Add(x);
                    for (char x = 'A'; x <= 'Z'; x++) curr_pattern.CharSet.Add(x);
                    curr_pattern.CharSet.Add('_');
                }
                else
                {
                    curr_pattern.type = PatternType.literalChar; // literal char
                    curr_pattern.CharSet.Add(esc);
                }
            }
            else if (pattern[i] == '[')
            {
                curr_pattern.type = PatternType.charSet;
                if (i + 1 < pattern.Length && pattern[i + 1] == '^') {
                    curr_pattern.type = PatternType.nCharSet;
                    curr_pattern.negated = true;
                    i++;
                }

                i++;
                while (i < pattern.Length && pattern[i] != ']')
                {
                    curr_pattern.CharSet.Add(pattern[i++]);
                }

                if (i == pattern.Length)
                {
                    throw new ArgumentException("Invalid Pattern: " + pattern);
                }
            }
            else
            {
                curr_pattern.type = PatternType.literalChar;
                curr_pattern.CharSet.Add(pattern[i]);
            }
            i++;
            Patterns.Add(curr_pattern);
        }
    }

    internal bool MatchPattern()
    {
        bool endOfTheString = Patterns.Count > 0 && Patterns.Last().type == PatternType.endOfString;
        bool startOfTheString = Patterns.Count > 0 && Patterns[0].type == PatternType.startOfString;

        int patternOffset = 0;
        int inputOffset = 0;

        if (startOfTheString)
        {
            patternOffset = 1;
            if (Match(inputOffset, patternOffset))
            {
                if (!endOfTheString) return true;
                //means end of the string matching regex is present
                if(InputLine.Length == Patterns.Count-2) return true;
            }
            return false;
        }

        for (int i = 0; i <= InputLine.Length; i++)
        {
            if (Match(i, patternOffset))
            {
                if (endOfTheString)
                {
                    if (i + Patterns.Count - patternOffset - 1 == InputLine.Length) return true;
                    else continue;
                }
                return true;
            }
        }

        return false;
    }

    internal bool Match(int inputoffset, int patternOffset)
    {
        int end = Patterns.Count - (Patterns.Last().type == PatternType.endOfString ? 1 : 0);
        for(int i = patternOffset; i < end; i++, inputoffset++)
        {
            if (inputoffset >= InputLine.Length) return false;

            char ch = InputLine[inputoffset];
            if (Patterns[i].negated)
            {
                if (Patterns[i].CharSet.Contains(ch)) return false;
            }
            else if(!Patterns[i].CharSet.Contains(ch)) return false;
        }
        //if(i == Patterns.Count) return true;
        return true;
    }
}
