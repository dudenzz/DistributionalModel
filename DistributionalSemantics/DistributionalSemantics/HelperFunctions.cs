using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionalSemantics
{
    public static class HelperFunctions
    {
        public static Random rng = new Random();
        public static double rho(Simlex simlexInstance, List<SimpleEntry> calculatedResults, int idx)
        {
            double avg1 = 0;
            double avg2 = 0;
            double avg12 = 0;
            foreach(Entry simLexEntry in simlexInstance.Entries)
            {
                //W kolejnej linii pobierana jest wartość wygenerowanego podobieństwa pomiędzy słowami z aktualnie przetwarzanego Entry z SimLex'a 
                SimpleEntry calculatedEntry = calculatedResults.Where(x => x.w1 == simLexEntry.word1 && x.w2 == simLexEntry.word2).First();
                avg1 += simLexEntry.simLex;
                avg2 += calculatedEntry.result;
                avg12 += simLexEntry.simLex * calculatedEntry.result;
            }   
            avg1 /= simlexInstance.Entries.Count;
            avg2 /= simlexInstance.Entries.Count;
            avg12 /= simlexInstance.Entries.Count;
            double cov = avg12 - (avg1 * avg2);
            double dev1 = 0;
            double dev2 = 0;
            foreach (Entry simLexEntry in simlexInstance.Entries)
            {
                SimpleEntry calculatedEntry = calculatedResults.Where(x => x.w1 == simLexEntry.word1 && x.w2 == simLexEntry.word2).First();
                dev1 += (simLexEntry.simLex - avg1) * (simLexEntry.simLex - avg1);
                dev2 += (calculatedEntry.result - avg2) * (calculatedEntry.result - avg2);
            }
            dev1 /= (simlexInstance.Entries.Count - 1);
            dev2 /= (simlexInstance.Entries.Count - 1);

            dev1 = Math.Sqrt(dev1);
            dev2 = Math.Sqrt(dev2);

            return cov / (dev1 * dev2);
        }
        public static double meanSqError(List<SimpleEntry> SqErrors)
        {
            int n = SqErrors.Count;
            double total = 0;
            for (int i = 0; i < n; i++) total += SqErrors[i].result;
            return total / n;
        }
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
