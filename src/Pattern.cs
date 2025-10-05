using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_grep.src
{
    internal class Pattern
    {
        //introduce an enum for the following types
        internal PatternType type; // d, w, lc, cs, ncs
        internal HashSet<char> CharSet;
        internal bool wild;
        internal int backrefIndex; // for backreference type
        internal List<List<Pattern>> subPatterns { get; set; } // for alternate type
        // 0 row -> first alternate
        // 1 row -> second alternate
        // ...

        internal Pattern()
        {
            CharSet = new HashSet<char>();
            type = PatternType.literalChar;
            subPatterns = new List<List<Pattern>>();
        }
    }

    public enum PatternType
    {
        digit,
        word,
        literalChar,
        charSet,
        nCharSet,
        matchZeroOrOne,
        matchZeroOrMore,
        matchOneOrMore,
        startOfString,
        endOfString,
        wildCard,
        group,
        alternate,
        backref,
    }
}
