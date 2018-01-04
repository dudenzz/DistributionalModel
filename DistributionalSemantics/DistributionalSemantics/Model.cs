using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionalSemantics
{
    public class Model
    {
        Vector[] vectors;
        public static Model centroidsModel;
        public static double Beta1 = 0.1;
        public static double Beta2 = 18;
        public static double Beta3 = 0.3;
        public static double Beta4 = 0.4;
        public static double LR1 = 0.1;
        public static double LR2 = 0.1;
        public static double LR3 = 0.1;
        public static double LR4 = 0.1;
        public static bool updating = false;
        public static double msqe = 0;
        public static Vector.comparisonStyle compStyle = Vector.comparisonStyle.Cosine;
        public static RegressionSet sl;
        static int batch_size = 10;
        static int counter = 0;
        static double db1 = 0;
        static double db2 = 0;
        static double db3 = 0;
        static double db4 = 0;
        public static void loadCentroids(string filename)
        {
            centroidsModel = new Model(filename);
        }

        public Model retrofit(string synonymsFilename, int iterations)
        {
            Model newModel = new Model(vectors.Length,v_size);
            this.vectors.CopyTo(newModel.vectors, 0);
            newModel.index = new Dictionary<string, int>();
            foreach(var tuple in this.index)
            {
                newModel.index[tuple.Key] = tuple.Value;
            }
            string[] syns = File.ReadAllLines(synonymsFilename);
            for (int iter = 0; iter < iterations; iter++ )
                foreach (string line in syns)
                {
                    string b_token = line.Split(' ')[0].Trim();
                    if (!index.Keys.Contains(b_token)) continue;
                    string[] tokens = line.Split(' ').Skip(1).ToArray();
                    List<string> extTokens = new List<string>();
                    foreach (string token in tokens) if (index.Keys.Contains(token)) extTokens.Add(token);
                    tokens = extTokens.ToArray();
                    int num = tokens.Length;
                    if (num == 0) continue;
                    Vector nVector = new Vector(newModel.vectors[index[b_token]].Values, b_token) * num;

                    foreach (string token in tokens)
                    {
                        nVector += newModel.vectors[index[token]];
                    }
                    newModel.vectors[index[b_token]] = nVector / (2 * num);
                }
            return newModel;
        }
        public double partialDerivative(Vector v1, Vector v2, int component)
        {
            return v2[component] / (Vector.norm(v1) * Vector.norm(v2)) - Vector.cosine(v1, v2) * v1[component] / (Math.Pow(Vector.norm(v1),2));
        }
        public Model cos_retrofit(string synonymsFilename, string antonymsFilename, int iterations)
        {
            Model newModel = new Model(vectors.Length, v_size);
            this.vectors.CopyTo(newModel.vectors, 0);
            newModel.index = new Dictionary<string, int>();
            foreach (var tuple in this.index)
            {
                newModel.index[tuple.Key] = tuple.Value;
            }
            string[] syns = File.ReadAllLines(synonymsFilename);
            string[] ants = File.ReadAllLines(antonymsFilename);
            Stopwatch SW = new Stopwatch(); 
            for (int iter = 0; iter < iterations; iter++)
            {
                SW.Start();
                Console.WriteLine("Running iteration : {0}", iter);
                foreach (string line in syns)
                {
                    string b_token = line.Split(' ')[0].Trim();
                    if (!index.Keys.Contains(b_token)) continue;
                    string[] synonymTokens = line.Split(' ').Skip(1).ToArray();
                    List<string> extSynTokens = new List<string>();
                    foreach (string token in synonymTokens) if (index.Keys.Contains(token)) extSynTokens.Add(token);
                    synonymTokens = extSynTokens.ToArray();
                    int num = synonymTokens.Length;
                    if (num == 0) continue;
                    Vector oVector = new Vector(this.vectors[index[b_token]].Values, b_token);
                    Vector nVector = new Vector(newModel.vectors[index[b_token]].Values, b_token);
                    nVector.V_size = v_size;
                    oVector.V_size = v_size;
                    double cosineON = Vector.cosine(nVector, oVector);
                    for (int i = 0; i < nVector.V_size; i++)
                    {
                        double sN = nVector[i];
                        sN -= -2 * partialDerivative(nVector, oVector, i);
                        sN -= 2 * cosineON * partialDerivative(oVector, nVector, i);
                        foreach (string token in synonymTokens)
                        {

                            Vector sVector = newModel.vectors[index[token]];
                            double cosSN = Vector.cosine(nVector, sVector);
                            sN -= 2 * cosSN * partialDerivative(nVector, sVector, i) * 100;
                            //sN -= -1.74 * partialDerivative(nVector, sVector, i) * 100;
                            sN -= -2 * partialDerivative(nVector, sVector, i) * 100;
                        }
                        nVector[i] = sN;
                        double difference = nVector[i] - oVector[i];


                    }
                    foreach (string token in synonymTokens)
                    {
                        Vector sVector = newModel.vectors[index[token]];

                        double c1 = Vector.cosine(sVector, oVector);
                        double c2 = Vector.cosine(sVector, nVector);
                        double c3 = Vector.cosine(oVector, nVector);

                    }
                    newModel.vectors[index[b_token]] = nVector;
                }
                foreach (string line in ants)
                {
                    string b_token = line.Split(' ')[0].Trim();
                    if (!index.Keys.Contains(b_token)) continue;
                    string[] synonymTokens = line.Split(' ').Skip(1).ToArray();
                    List<string> extSynTokens = new List<string>();
                    foreach (string token in synonymTokens) if (index.Keys.Contains(token)) extSynTokens.Add(token);
                    synonymTokens = extSynTokens.ToArray();
                    int num = synonymTokens.Length;
                    if (num == 0) continue;
                    Vector oVector = new Vector(this.vectors[index[b_token]].Values, b_token);
                    Vector nVector = new Vector(newModel.vectors[index[b_token]].Values, b_token);
                    nVector.V_size = v_size;
                    oVector.V_size = v_size;
                    double cosineON = Vector.cosine(nVector, oVector);
                    for (int i = 0; i < nVector.V_size; i++)
                    {
                        double sN = nVector[i];
                        sN -= -2 * partialDerivative(nVector, oVector, i);
                        sN -= 2 * cosineON * partialDerivative(oVector, nVector, i);
                        foreach (string token in synonymTokens)
                        {

                            Vector sVector = newModel.vectors[index[token]];
                            double cosSN = Vector.cosine(nVector, sVector);
                            sN -= 2 * cosSN * partialDerivative(nVector, sVector, i) * 100;
                            sN -= -0 * partialDerivative(nVector, sVector, i) * 100;
                            //sN -= -0.46 * partialDerivative(nVector, sVector, i) * 100;
                        }
                        nVector[i] = sN;
                        double difference = nVector[i] - oVector[i];


                    }
                    foreach (string token in synonymTokens)
                    {
                        Vector sVector = newModel.vectors[index[token]];

                        double c1 = Vector.cosine(sVector, oVector);
                        double c2 = Vector.cosine(sVector, nVector);
                        double c3 = Vector.cosine(oVector, nVector);

                    }
                    newModel.vectors[index[b_token]] = nVector;
                }
                SW.Stop();
                Console.WriteLine("Elapsed: {0}:{1}", SW.Elapsed.Seconds, SW.Elapsed.Milliseconds);
                SW.Reset();
            }
            return newModel;
            
        }
        public static void resetBatch()
        {
            db1 = 0;
            db2 = 0;
            db3 = 0;
            db4 = 0;
            counter = 0;
        }
        public static void update(double q1, double q2, double r1, double r2, double y_est, double y)
        {
            counter += 1;
            //db1 = LR1 * (y_est - y) * Math.Pow(r1, Beta2);
            //db2 = LR2 * (y_est - y) * Beta1 * Math.Pow(r1, Beta2) * Math.Log(r1);
            //db3 = -LR3 * (y_est - y) * Math.Pow(r2, Beta3);
            //db4 = -LR4 * (y_est - y) * Beta3 * Math.Pow(r2, Beta4) * Math.Log(r2);
            
            //model z pojedynczym elementem typu sigmoid
            
            db1 = LR1 * (y_est - y) * Beta2 * Beta3 * (r1 * (1 - r1));
            db2 = LR2 * (y_est - y) * Beta3 * (r1 * (1 - r1) * q1 / (-Beta2));
            db3 = LR3 * (y_est - y) * r1;
            
             //* model z aktywacją przy różnicy centroidów
            //double db1  = LR1 * (y_est - y) * Beta2 * Beta3 * (r1 * (1 - r1) - r2 * (1 - r2));
            
            //double db2 = LR2 * (y_est - y) * Beta3 * (r1 * (1 - r1) * (q1 / (-Beta2)) - r2 * (1 - r2) * (q2 / (-Beta2)));
            
            //double db3 = LR3 * (y_est - y) * (r1 - r2);
            
            //if (counter % 10 == 0)
            //{
                Beta1 -= db1;
                Beta2 -= db2;
                Beta3 -= db3;
                Beta4 -= db4;
                db1 = 0;
                db2 = 0;
                db3 = 0;
            //}
            
        }
        internal Vector[] Vectors
        {
            get { return vectors; }
            set { vectors = value; }
        }
        int v_size = 0;
        Dictionary<string, int> index;
        public Vector getVector(string label)
        {
            foreach (Vector v in vectors)
                if (v.Label == label) return v;
            return null;
        }

        public Model(int l, int size)
        {
            vectors = new Vector[l];
            v_size = size;
            for (int i = 0; i < l; i++)
                vectors[i] = Vector.dummy(size);
        }
        public Model(string filename)
        {
            index = new Dictionary<string, int>();
            List<Vector> vs = new List<Vector>();
            StreamReader sr = new StreamReader(filename);
            string line;
            bool known_size = false;
            int iterator = 0;
            while((line = sr.ReadLine()) != null)
            {

                if (!known_size)
                {
                    v_size = Vector.RecognizeVSize(line);
                    known_size = true;
                }
                
                Vector v = new Vector(line,v_size);
                vs.Add(v);
                index.Add(v.Label, iterator);
                iterator++;
            }
            vectors = vs.ToArray();
        }
        public async Task<double> compare(string a, string b)
        {
            return await vectors[index[a]].compare(vectors[index[b]],Model.compStyle);
        }
        public async Task<double> compare(string a, string b, string[] context)
        {
            vectors[index[a]].Context = true;
            Vector[] vcs = new Vector[context.Length];
            for (int i = 0; i < context.Length; i++)
                vcs[i] = vectors[index[context[i]]];
            vectors[index[a]].ContextVectors = vcs;
            return await vectors[index[a]].compare(vectors[index[b]], Model.compStyle);
        }
        public void saveModel(string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            StringBuilder sb = new StringBuilder();
            foreach(Vector v in vectors)
            {
                sb.Clear();
                sb.Append(v.Label);
                foreach (double val in v.Values)
                {
                    sb.Append("\t");
                    sb.AppendFormat("{0:F6}",val);
                }
                sw.WriteLine(sb.ToString());
            }
            sw.Close();
        }
        public Vector calculateCentroid(string label, List<Vector> mostSimilar)
        {
            double[] vec = new double[v_size];
            for(int i = 0; i<v_size; i++)
            {
                vec[i] = 0;
                for(int j = 0; j<mostSimilar.Count; j++)
                {
                    vec[i] += mostSimilar[j].Values[i];
                }
                vec[i] /= mostSimilar.Count;
            }
            return new Vector(vec, label);
        }
        public List<Vector> findNMostSimilar(Vector a, int n)
        {
            Vector[] vs = new Vector[n];
            double[] sims = new double[n];
            int additional = 0;
            for(int i = 0; i<n; i++)
            {
                if(vectors[i] == a) {
                    vs[i] = vectors[n];
                    additional++;
                }
                else
                {
                    vs[i] = vectors[i];
                }
                var task = compare(vs[i].Label,a.Label);
                task.Wait();
                sims[i] = task.GetAwaiter().GetResult();
            }
            swaperoo(n, ref sims, ref vs);
            for(int i = n + additional; i<vectors.Length; i++)
            {
                if (vectors[i] == a) continue;
                var task = compare(vectors[i].Label, a.Label);
                task.Wait();
                double current = task.GetAwaiter().GetResult();
                if(current > sims[n-1])
                {
                    sims[n - 1] = current;
                    vs[n - 1] = vectors[i];
                    swaperoo(n, ref sims, ref vs);
                }
            }
            return vs.ToList();
        }
        public void swaperoo(int n, ref double[] sims, ref Vector[] vs)
        {
            double min = sims[0];
            int idx = 0;
            for (int i = 0; i < n; i++)
            {
                if (sims[i] < min)
                {
                    min = sims[i];
                    idx = i;
                }
            }
            double tmpF = sims[idx];
            Vector tmpV = vs[idx];
            sims[idx] = sims[n-1];
            vs[idx] = vs[n-1];
            sims[n - 1] = tmpF;
            vs[n - 1] = tmpV;
        }
        public List<SimpleEntry> generateEntriesForComparison(RegressionSet input)
        {
            List<SimpleEntry> entries = new List<SimpleEntry>();
            int i = 0;
            foreach(Entry entry in input.Entries)
            {
                System.Console.WriteLine(i++);
                //Console.WriteLine(i++);
                var t = compare(entry.word1, entry.word2);
                double result = 0.5;
                try
                {
                    t.Wait();
                    result = t.GetAwaiter().GetResult();
                }
                catch
                {

                }
                entries.Add(new SimpleEntry(entry.word1, entry.word2, result));
            }
            return entries;
        }
    }
}
