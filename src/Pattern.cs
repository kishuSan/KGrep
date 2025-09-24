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
        internal bool negated;
        internal bool wild;
        //internal char literal;

        internal Pattern()
        {
            CharSet = new HashSet<char>();
            type = PatternType.literalChar;
            negated = false;
            //literal = '\0';
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
    }
}
