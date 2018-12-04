using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Searcher
{
    public class Results
    {
        public int lineNumber { get; set; }
        public string path { get; set; }
        public string wordMatch { get; set; }
        public int index { get; set; }

        public Results(int lineNum, string path, string word, int index)
        {
            lineNumber = lineNum;
            this.path = path;
            wordMatch = word;
            this.index = index;
        }

        override
        public string ToString()
        {
            string word = "Word: " + wordMatch;
            string line = "Line: " + lineNumber;
            string position = "Position: " + index;
            string file = "File: " + path;
            word = word.PadRight(30, ' ');
            line = line.PadRight(20, ' ');
            position = position.PadRight(30, ' ');

            string output = string.Format("{0} | {1} | {2} | {3}", word, line, position, file);
            return output;
            //return "Word: " + wordMatch + " | line: " + lineNumber + " | Position: " + index +  " | file: " + path;
        }
    }
}
