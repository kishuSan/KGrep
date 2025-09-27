using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using codecrafters_grep.src;

internal class KGrep
{
    // input line given by the user
    private string InputLine { get; }
    
    // dynamic array sequence of the parsed patterns
    private List<List<Pattern>> Patterns { get; }

    internal KGrep(string input, string pattern)
    {
        if (input == null || input.Length == 0) throw new ArgumentNullException("Input line is null");
        if (pattern == null || pattern.Length == 0) throw new ArgumentNullException("Pattern is null or empty");

        InputLine = input;
        Patterns = new List<List<Pattern>>();
        Patterns = ParsePattern(pattern); // parse the pattern into a sequence
    }

    private List<List<Pattern>> ParsePattern(string pattern)
    {
        int j = 0;
        int i = 0;
        List<List<Pattern>> patterns = new List<List<Pattern>>();
        while (i < pattern.Length)
        {
            patterns.Add(new List<Pattern>());
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
            else if (pattern[i] == '.')
            {
                curr_pattern.type = PatternType.wildCard;
                curr_pattern.wild = true;
            }
            else if (pattern[i] == '(')
            {
                Stack<char> s1 = new Stack<char>('(');
                int start = i + 1;
                int alternate = 0;
                while(++i < pattern.Length && s1.Count() != 0)
                {
                    if (pattern[i] == '|') alternate = i + 1; // starting of alternate pattern
                    if (pattern[i] == '(') s1.Push('('); // to handle nested groups
                    else if (pattern[i] == ')') s1.Pop();
                }

                if (alternate != 0)
                {
                    patterns[j][0] = ParsePattern(pattern.Substring(start, alternate - start - 1));
                    patterns[j][1] = ParsePattern(pattern.Substring(alternate, i - alternate - 1));  
                }
                else
                {
                    patterns[j] = ParsePattern(pattern.Substring(start, i - start - 1));
                } 
            }
            else
            {
                curr_pattern.type = PatternType.literalChar;
                curr_pattern.CharSet.Add(pattern[i]);
            }
            
            if (i + 1 < pattern.Length)
            {
                i++;
                if (pattern[i] == '*')
                    curr_pattern.type = PatternType.matchZeroOrMore;
                else if (pattern[i] == '?')
                    curr_pattern.type = PatternType.matchZeroOrOne;
                else if (pattern[i] == '+')
                    curr_pattern.type = PatternType.matchOneOrMore;
                else
                    i--;
            }
            i++;
            patterns[j].Add(curr_pattern);
        }
        return patterns;
    }

    internal bool MatchPattern()
    {
        //bool endOfTheString = Patterns.Count > 0 && Patterns.Last().type == PatternType.endOfString;
        bool startOfTheString = Patterns.Count > 0 && Patterns[0].type == PatternType.startOfString;

        if (startOfTheString)
        {
            int pattern_idx = 0, input_idx = 0;
            return Match(ref pattern_idx, ref input_idx);
        }

        for (int i = 0; i <= InputLine.Length; i++)
        {
            int input_idx = i;
            int pattern_idx = 0;
            if (Match(ref pattern_idx, ref input_idx))
                return true;
        }

        return false;
    }

    internal bool Match(ref int pattern_idx, ref int input_idx)
    {
        //base case and endOfString,
        if (pattern_idx == Patterns.Count || (Patterns[pattern_idx].type == PatternType.endOfString && input_idx == InputLine.Length))
            return true;

        if(input_idx >= InputLine.Length)
            return false;

        Pattern curr_p = Patterns[pattern_idx];
        PatternType curr_pt = curr_p.type;

        //startOfString,
        if (curr_pt == PatternType.startOfString && input_idx == 0)
        {
            pattern_idx++;
            return Match(ref pattern_idx, ref input_idx);
        }

        // wildcard
        if(curr_pt == PatternType.wildCard)
        {
            pattern_idx++;
            input_idx++;
            return Match(ref pattern_idx, ref input_idx); 
        }

        //digit, word, literalChar, charSet
        if ((curr_pt == PatternType.digit
            || curr_pt == PatternType.word
            || curr_pt == PatternType.literalChar
            || curr_pt == PatternType.charSet
            ) && curr_p.CharSet.Contains(InputLine[input_idx]))
        {
            pattern_idx++; input_idx++;
            return Match(ref pattern_idx, ref input_idx);
        }

        //nCharSet,
        if (curr_pt == PatternType.nCharSet && !curr_p.CharSet.Contains(InputLine[input_idx]))
        {
            pattern_idx++; input_idx++;
            return Match(ref pattern_idx, ref input_idx);
        }

        //matchOneOrMore,
        if (curr_pt == PatternType.matchOneOrMore)
        {
            return MatchMultiple(ref pattern_idx, ref input_idx, false);
        } 
        
        //matchZeroOrMore,
        if (curr_pt == PatternType.matchZeroOrMore)
        {
            return MatchMultiple(ref pattern_idx, ref input_idx, true);
        }

        //matchZeroOrOne,
        if(curr_pt == PatternType.matchZeroOrOne)
        {
            pattern_idx++;
            int p_idx = pattern_idx;
            int i_idx = input_idx;
            if(Match(ref p_idx, ref i_idx)){
                return true;
            }
            if (curr_p.wild || curr_p.CharSet.Contains(InputLine[input_idx]))
            {
                input_idx++;
                return Match(ref pattern_idx, ref input_idx);
            }
        }

        return false;
    }

    internal bool MatchMultiple(ref int pattern_idx, ref int input_idx, bool star)
    {
        if (star)
        {
            // try to match rest of pattern with zero matches of star 
            int p_idx = pattern_idx + 1;
            int i_idx = input_idx;
            if (Match(ref p_idx, ref i_idx))
                return true;
        }
        Pattern curr_p = Patterns[pattern_idx];
        while (input_idx < InputLine.Length && (curr_p.wild ? true : curr_p.CharSet.Contains(InputLine[input_idx])))
        {
            input_idx++;
            int p_idx = pattern_idx + 1;
            int i_idx = input_idx;
            if (Match(ref p_idx, ref i_idx))
                return true;
        }
        return false;
    }
}
