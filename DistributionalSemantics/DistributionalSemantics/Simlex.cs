using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionalSemantics
{
    public struct SimpleEntry
    {
        public string w1;
        public string w2;
        public double result;

        public SimpleEntry(string w1, string w2, double result)
        {
            this.w1 = w1;
            this.w2 = w2;
            this.result = result;
        }
        public override string ToString()
        {
            return w1 + " " + w2 + " " + result.ToString();
        }
    }
    public struct Entry
    {
        public string word1;
        public string word2;
        public string POS;
        public double simLex;
        public double concw1;
        public double concw2;
        public double concQ;
        public double Assoc;
        public double simAssoc;
        public double SD;

        public Entry(string line)
        {
            string[] tokens = line.Split('\t');
            word1 = tokens[0];
            word2 = tokens[1];
            POS = tokens[2];
            simLex = double.Parse(tokens[3].Replace('.', ','));
            concw1 = double.Parse(tokens[4].Replace('.', ','));
            concw2 = double.Parse(tokens[5].Replace('.', ','));
            concQ = double.Parse(tokens[6].Replace('.', ','));
            Assoc = double.Parse(tokens[7].Replace('.', ','));
            simAssoc = double.Parse(tokens[8].Replace('.', ','));
            SD = double.Parse(tokens[9].Replace('.', ','));
        }
    }
    public class RegressionSet : TestSet
    {
        public static Random rng = new Random();
        
        List<Entry> entries;

        internal List<Entry> Entries
        {
            get { return entries; }
            set { entries = value; }
        }
        public void shuffle()
        {
            HelperFunctions.Shuffle<Entry>(entries);
        }
        public Entry findEntry(string word1, string word2)
        {
            foreach (Entry e in Entries)
                if (e.word1 == word1 && e.word2 == word2) return e;
            return new Entry();
        }
        public RegressionSet(string filename)
        {
            entries = new List<Entry>();
            string[] entriesStrings = File.ReadAllLines(filename);
            for (int i = 1; i < entriesStrings.Length; i++) entries.Add(new Entry(entriesStrings[i]));
        }
    }
}
