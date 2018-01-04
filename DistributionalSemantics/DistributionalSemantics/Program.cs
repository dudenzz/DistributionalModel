using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionalSemantics
{
    class Program
    {
        static void Main(string[] args)
        {

            Model m = new Model("D://glove_vec//paragram_filtered//paragram_filtered_vecs.txt");
            m = m.cos_retrofit("D://glove_vec//ppdb_synonyms.txt", "D://glove_vec//wordnet_antonyms.txt", 20);
            //m.saveModel("D://glove_vec//glove_mrksic//glove_retro_vecs.txt");
            Model.loadCentroids("D://glove_vec//glove_mrksic//centroids_retro10");
            RegressionSet sl = new RegressionSet("D://SimLex-999//SimLex-999.txt");
            Model.sl = sl;
            ClassificationSet esl = new ClassificationSet("D://glove_vec//esl", ClassificationSet.SetType.ESL);
            ClassificationSet toefl = new ClassificationSet("D://glove_vec//toefl", ClassificationSet.SetType.TOEFL);
            //var dict = HelperFunctions.generateDictionary(new List<TestSet> { sl999, esl, toefl });
            
            
            Model.compStyle = Vector.comparisonStyle.Cosine;
            Classifier cEsl = new Classifier(esl, m);
            Classifier cToefl = new Classifier(toefl, m);
            var r0 = m.generateEntriesForComparison(sl);
            double acc = cEsl.accuracy();
            double acc2 = cToefl.accuracy();
            double rho = HelperFunctions.rho(sl, r0, 0);
            //double rho = 0;
            //double acc = 0;
            //double acc2 = 0;
            System.Console.WriteLine("ESL: {0}\nTOEFL: {1}\nSimLex999: {2}", acc, acc2, rho);
            Console.ReadKey();
          
            /*
            
            Model.loadCentroids("D://glove_vec/centroids5");
            Model.sl = sl;
            
            Model.compStyle = Vector.comparisonStyle.Cosine;
            var r0 = m.generateEntriesForComparison(sl);
            var result0 = HelperFunctions.rho(sl, r0, 0);
            Console.WriteLine(result0);
            StreamWriter sw = new StreamWriter("D://results.txt");
            Model.compStyle = Vector.comparisonStyle.CosineHR;
            */
            /*for (int i = 0; i < 100 ; i++)
            {
                //Model.Beta1 = i/10.0;
                if (i % 50 == 0) Console.WriteLine(i);
                var r = m.generateEntriesForComparison(sl);
                var result = HelperFunctions.rho(sl, r, 0);
                Model.msqe /= 999.0;
                sw.WriteLine("{0}\t{1:F6}\tBeta1:{2}\tBeta2:{3}\tBeta3:{4}\tMSQE: {5}",i/100.0,result,Model.Beta1,Model.Beta2,Model.Beta3, Model.msqe);
                Model.msqe = 0;
                sl.shuffle();
                Model.resetBatch();
            }
            sw.Close();*/
             
            /*
            int remainder = 1029 + 420;
            Stopwatch s = new Stopwatch();
            //Model centroids = new Model(m.Vectors.Length, m.Vectors[0].V_size);
            //remainder = m.Vectors.Length;
            Model centroids = new Model(1029 + 420, m.Vectors[0].V_size);
            s.Start();
            foreach (Vector v in m.Vectors)
            {
                if (sl.Entries.Select(x => x.word1).Contains(v.Label) || 
                    sl.Entries.Select(x => x.word2).Contains(v.Label) ||
                    esl.Questions.Select(x => x.Answer).Contains(v.Label) ||
                    esl.Questions.Select(x => x.Possibilities[0]).Contains(v.Label) ||
                    esl.Questions.Select(x => x.Possibilities[1]).Contains(v.Label) ||
                    esl.Questions.Select(x => x.Possibilities[2]).Contains(v.Label) ||
                    esl.Questions.Select(x => x.Possibilities[3]).Contains(v.Label) ||
                    toefl.Questions.Select(x => x.Answer).Contains(v.Label) ||
                    toefl.Questions.Select(x => x.Possibilities[0]).Contains(v.Label) ||
                    toefl.Questions.Select(x => x.Possibilities[1]).Contains(v.Label) ||
                    toefl.Questions.Select(x => x.Possibilities[2]).Contains(v.Label) ||
                    toefl.Questions.Select(x => x.Possibilities[3]).Contains(v.Label) 
                    
                    )
                {
                    
                    var t = m.findNMostSimilar(v, 10);
                    var c = m.calculateCentroid(v.Label, t);
                    Console.WriteLine("{0}/{1} : {2:F5}%\t {3:D2}:{4:D2}:{5:D2}", remainder, 1029+420, 100 * (1 - (remainder / (1029.0+420.0))), s.Elapsed.Hours, s.Elapsed.Minutes, s.Elapsed.Seconds);
                    remainder--;
                    
                    centroids.Vectors[remainder] = c;
                }
            }
            
            centroids.saveModel("D://glove_vec//centroids_original5");
             */
            
            
            //var entries = m.generateEntriesForComparison(sl);

            //double res = HelperFunctions.rho(sl, entries, 0);
            //System.Console.WriteLine("{0}", res);

            //Model m = new Model("../../../data/nmrksic/counter-fitting.git/trunk/word_vectors/");
        
             
        }
    }
}
