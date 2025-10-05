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
    private List<Pattern> Tokens { get; }

    // to store the backref matches
    private List<List<string>> groups; 

    // global stack to maintain the groups, it also helps to prevent unwanted parsing of groups.
    Stack<char> groupcheck;

    internal KGrep(string input, string pattern)
    {
        if (input == null || input.Length == 0) throw new ArgumentNullException("Input line is null");
        if (pattern == null || pattern.Length == 0) throw new ArgumentNullException("Pattern is null or empty");

        InputLine = input;
        groups = new List<List<string>>();
        groupcheck = new Stack<char>();
        Tokens = new List<Pattern>();
        int idx = 0;
        Tokens = Tokenizer(pattern, ref idx); // parse the pattern into a sequence
    }

    public void PrintPatternWrapper()
    {
        Console.WriteLine("Printing the parsed tokens from the given inputed pattern");
        PrintPattern(Tokens, 0, 0);
    }

    private void PrintPattern(List<Pattern> tokens, int depth, int idx)
    {
        foreach (Pattern p in tokens)
        {
            Console.Write(string.Concat(Enumerable.Repeat("  ", depth * 4)) + p.type + " " + (p.wild ? "wild" : "") + " " + string.Join("", p.CharSet) + ", ");
            if (p.subPatterns.Count > 0)
            {
                int i = 0;
                foreach (List<Pattern> sub in p.subPatterns)
                {
                Console.WriteLine();
                    Console.WriteLine(
                        string.Concat(Enumerable.Repeat("=>", depth * 4)) + 
                        " subPattern " + 
                        new string('0', depth) + 
                        (idx * 10 + i++)
                    );
                    PrintPattern(sub, depth + 1, i-1);
                }
            }
        }
    }

    private List<Pattern> Tokenizer(string pattern, ref int i)
    {
        List<Pattern> tokens = new List<Pattern>();
        while (i < pattern.Length)
        {
            Pattern curr_pattern = new Pattern();
            if (pattern[i] == '(')
            {
                // group start
                // ?can also add the group number to store the backref
                i++;
                groupcheck.Push('(');
                int curr_size_group = groupcheck.Count();
                curr_pattern.type = PatternType.group;
                curr_pattern.subPatterns.Add(Tokenizer(pattern, ref i));
                tokens.Add(curr_pattern);
                if(groupcheck.Count() == curr_size_group)
                {
                    throw new ArgumentException("Unmatched opening parenthesis at position " + (i - 1));
                }
                if (i >= pattern.Length){
                    return tokens;
                }
                else continue;
            }
            if (pattern[i] == ')')
            {
                if(groupcheck.Count() == 0)
                {
                    throw new ArgumentException("Unmatched closing parenthesis at position " + i);
                }
                // group end
                i++;
                groupcheck.Pop();
                return tokens;
            }
            if (pattern[i] == '|')
            {
                // alternate
                i++;
                curr_pattern.type = PatternType.alternate;
                curr_pattern.subPatterns.Add(new List<Pattern>(tokens));
                List<Pattern> alternate = Tokenizer(pattern, ref i);
                if (alternate[0].type == PatternType.alternate)
                {
                    foreach (List<Pattern> x in alternate[0].subPatterns)
                    curr_pattern.subPatterns.Add(x);
                }
                else
                {
                    curr_pattern.subPatterns.Add(alternate);
                }
                tokens.Clear();
                tokens.Add(curr_pattern);
                if (i >= pattern.Length) return tokens;
            }
            if (pattern[i] == '^')
            {
                // start of the string
                curr_pattern.type = PatternType.startOfString;
            }
            else if (pattern[i] == '$')
            {
                // end of the string
                curr_pattern.type = PatternType.endOfString;
            }
            else if (pattern[i] == '\\')
            {
                // either the esc char of to match digit, word, space or literal char
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
                // match the charset
                curr_pattern.type = PatternType.charSet;
                if (i + 1 < pattern.Length && pattern[i + 1] == '^')
                {
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
                // wildcard
                curr_pattern.type = PatternType.wildCard;
                curr_pattern.wild = true;
            }
            else
            {
                // literal char
                curr_pattern.type = PatternType.literalChar;
                curr_pattern.CharSet.Add(pattern[i]);
            }

            if (i + 1 < pattern.Length)
            {
                // to check the muiltiple match operators
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
            tokens.Add(curr_pattern);
        }
        return tokens;
    }

    internal bool MatchPattern()
    {
        //bool endOfTheString = Tokens.Count > 0 && Tokens.Last().type == PatternType.endOfString;
        bool startOfTheString = Tokens.Count > 0 && Tokens[0].type == PatternType.startOfString;

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
        if (pattern_idx == Tokens.Count || (Tokens[pattern_idx].type == PatternType.endOfString && input_idx == InputLine.Length))
            return true;

        if (input_idx >= InputLine.Length)
            return false;

        Pattern curr_p = Tokens[pattern_idx];
        PatternType curr_pt = curr_p.type;

        //startOfString,
        if (curr_pt == PatternType.startOfString && input_idx == 0)
        {
            pattern_idx++;
            return Match(ref pattern_idx, ref input_idx);
        }

        // wildcard
        if (curr_pt == PatternType.wildCard)
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
        if (curr_pt == PatternType.matchZeroOrOne)
        {
            pattern_idx++;
            int p_idx = pattern_idx;
            int i_idx = input_idx;
            if (Match(ref p_idx, ref i_idx))
            {
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
        Pattern curr_p = Tokens[pattern_idx];
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

    //helper methods
    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private bool IsWord(char c)
    {
        return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || IsDigit(c) || c == '_';
    }
}