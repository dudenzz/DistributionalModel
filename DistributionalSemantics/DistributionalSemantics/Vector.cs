using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionalSemantics
{
    public class Vector
    {
        bool context = false;

        public bool Context
        {
            get { return context; }
            set { context = value; }
        }
        private Vector[] contextVectors;

        public Vector[] ContextVectors
        {
            get { return contextVectors; }
            set { contextVectors = value; }
        }

        double[] values;

        public double[] Values
        {
            get { return values; }
            set { values = value; }
        }
        string label;

        public string Label
        {
            get { return label; }
            set { label = value; }
        }

        int v_size;

        public int V_size
        {
            get { return v_size; }
            set { v_size = value; }
        }
        public static Vector dummy(int size)
        {
            double[] vals = new double[size];
            for(int i = 0; i<size; i++)
                vals[i] = 0;
            Vector v = new Vector(vals, "dummy");
            return v;
        }

        public static int RecognizeVSize(string line)
        {
            int v_size = 0;
            string[] tokens = line.Split(new char[] { ' ', '\t'});
            double tmp;
            foreach (string token in tokens) if (double.TryParse(token.Replace('.',','), out tmp)) v_size++;
            return v_size;
        }
        public Vector(double[] values, string label)
        {
            this.values = new double[values.Length];
            values.CopyTo(this.values, 0);
            this.label = label;
        }
        
        public override string ToString()
        {
            return this.label;
        }
        public Vector(string line, int size)
        {
            values = new double[size];
            v_size = size;
            string[] tokens = line.Split(new char[] { ' ', '\t' });
            label = tokens[0];
            for (int i = 1; i <= size; i++)
            {
                string tok = tokens[i];
                values[i - 1] = (double)double.Parse(tokens[i].Replace('.',','), NumberStyles.AllowExponent | NumberStyles.Float);
            }
        }
        public enum comparisonStyle
        {
            Cosine,
            CosineHR,
            RESM,
            APSyn
        }
        #region sorting components
        public int findMax(List<double> values, List<int> removed)
        {
            int res = 0;
            double max = values[0];
            for (int i = 0; i < values.Count; i++)
            {

                if (removed.Contains(i)) continue;

                if (values[i] > max)
                {
                    max = values[i];
                    res = i;
                }
            }
            return res;
        }
        public int findMin(List<double> values, List<int> removed)
        {
            int res = 0;
            double min = values[0];
            for (int i = 0; i < values.Count; i++)
            {
                if(removed.Contains(i)) continue;
                if (values[i] < min)
                {
                    min = values[i];
                    res = i;
                }
            }
            return res;
        }
        List<int> sortedComponents(Vector v, bool ascending, int top)
        {
            List<int> result = new List<int>();
            List<double> values = v.values.ToList();
            for (int i = 0; i < values.Count; i++ )
            {
                int toRem;
                if (ascending) toRem = findMin(values, result);
                else toRem = findMax(values, result);
                result.Add(toRem); 
            }
            return result.Take(top).ToList();
        }
        #endregion
        public double this[int i] { get { return values[i]; }
            set { this.values[i] = value; }
        }
        public static double norm(Vector v)
        {
            double l1 = 0;
            for (int i = 0; i < v.v_size; i++)
            {
                l1 += v.values[i] * v.values[i];
            }
            return Math.Sqrt(l1);
        }
        public static double cosine(Vector v1, Vector v2)
        {
            double v1v2 = 0;
            double l1 = 0;
            double l2 = 0;
            if(v1.v_size != v2.v_size) throw new Exception();
            for(int i = 0; i<v1.v_size; i++)
            {
                v1v2 += v1.values[i] * v2.values[i];
                l1 += v1.values[i] * v1.values[i];
                l2 += v2.values[i] * v2.values[i];
            }
            return v1v2 / (Math.Sqrt(l1) * Math.Sqrt(l2));
        }
        public static Vector operator *(Vector v1, double val)
        {
            Vector v = new Vector(v1.Values, v1.label);
            v.context = v1.context;
            v.contextVectors = v1.contextVectors;
            v.v_size = v1.v_size;
            for (int i = 0; i < v1.v_size; i++)
                v.values[i] *= val;
            return v;
        }
        public static Vector operator /(Vector v1, double val)
        {
            Vector v = new Vector(v1.Values, v1.label);
            v.context = v1.context;
            v.contextVectors = v1.contextVectors;
            v.v_size = v1.v_size;
            for (int i = 0; i < v1.v_size; i++)
                v.values[i] /= val;
            return v;
        }
        public static Vector operator +(Vector v1, Vector v2)
        {
            Vector v = new Vector(v1.Values, v1.label);
            v.v_size = v1.v_size;
            for (int i = 0; i < v1.v_size; i++)
                v.values[i] = v1.values[i] + v2.values[i];
            return v;
        }
        public Task<double> compare(Vector toCompareWith, comparisonStyle style)
        {

            TaskCompletionSource<double> tcs = new TaskCompletionSource<double>();
            Task.Run(() =>
                {
                    switch (style)
                    {
                        case comparisonStyle.APSyn:
                            int apsyntop = 300;
                            var apsynasc_order1 = sortedComponents(this, false, apsyntop);
                            var apsynasc_order2 = sortedComponents(toCompareWith, false, apsyntop);
                            double r = 0;
                            for (int i = 0; i < apsyntop; i++)
                            {

                                int c1 = apsynasc_order1[i];
                                if (apsynasc_order2.Contains(c1))
                                {

                                    int i1 = i + 1;
                                    int i2 = apsynasc_order2.IndexOf(c1) + 1;

                                    r += 1/((i1 + i2) / (double)2.0);
                                }
                            }
                            tcs.SetResult(r);
                            break;
                        case comparisonStyle.RESM:
                            int top = 150;
                            int k = 2;
                            var asc_order1 = sortedComponents(this, true, top);
                            var asc_order2 = sortedComponents(toCompareWith, true, top);
                            var desc_order1 = sortedComponents(this, false, top);
                            var desc_order2 = sortedComponents(toCompareWith, false, top);
                            var cent1resm = Model.centroidsModel.getVector(this.label);
                            
                            List<int>[] ctxAsc = null;
                            List<int>[] ctxDsc = null;
                            if(context)
                            {
                                ctxAsc = new List<int>[contextVectors.Length];
                                ctxDsc = new List<int>[contextVectors.Length];
                                for (int i = 0; i < contextVectors.Length; i++)
                                {
                                    ctxAsc[i] = sortedComponents(contextVectors[i], true, top);
                                    ctxDsc[i] = sortedComponents(contextVectors[i], false, top);
                                }
                            }
                            double result1 = 0;
                            double result2 = 0;
                            double h1 = 1;
                            double h2 = 1;
                            for (int i = 0; i < top; i++ )
                            {
                                double hp1 = 1;
                                double hp2 = 1;
                                int c1 = asc_order1[i];
                                if(asc_order2.Contains(c1))
                                {
                                    
                                    int i1 = i + 1;
                                    int i2 = asc_order2.IndexOf(c1) + 1;
                                    
                                    double p1 = Math.Exp(-i1 * k / (double)v_size);
                                    double p2 = Math.Exp(-i2*k/(double)v_size);
                                     
                                    //result1 += (double)(p1 * p2);
                                     
                                    result1 += this.values[i] * toCompareWith.values[i];// - Math.Pow(cent1resm.values[i],0.144); 
                                }
                                
                                int dc1 = desc_order1[i];
                                if (desc_order2.Contains(dc1))
                                {
                                    
                                    int di1 = i;
                                    int di2 = desc_order2.IndexOf(c1);
                                    
                                    double pi1 = Math.Exp(-di1 * k / (double)v_size);
                                    double pi2 = Math.Exp(-di2 *k/(double)v_size);
                                    //result2 += (double)(pi1 * pi2);
                                    
                                    result2 += this.values[i] * toCompareWith.values[i];// - Math.Pow(cent1resm.values[i], 0.144); 
                                }
                                if(context)
                                {
                                    for (int j = 0; j < contextVectors.Length; j++ )
                                    {
                                        if (ctxAsc[j].Contains(c1))
                                        {
                                            int dpi1 = i;
                                            int dpi2 = ctxAsc[j].IndexOf(i);
                                            double ppi1 = Math.Exp(-dpi1 * k / (double)v_size);
                                            double ppi2 = Math.Exp(-dpi2 * k / (double)v_size);
                                            
                                            //hp1 += (double) ppi1 * ppi2;// - Math.Pow(cent1resm.values[i], 0.144);
                                            hp1 *= this.values[i] * contextVectors[j].values[i]; 
                                      
                                        }
                                        if (ctxDsc[j].Contains(c1))
                                        {
                                            int dpni1 = i;
                                            int dpni2 = ctxDsc[j].IndexOf(i);
                                            double pnpi1 = Math.Exp(-dpni1 * k / (double)v_size);
                                            double pnpi2 = Math.Exp(-dpni2 * k / (double)v_size);

                                            //hp2 += (double)pnpi1 * pnpi2;// - Math.Pow(cent1resm.values[i], 0.144);
                                            hp2 *= this.values[i] * contextVectors[j].values[i];
                                        }
                                    }
                                }

                                h1 *= hp1;
                                h2 *= hp2;
                            }
                            h1 /= v_size;
                            h2 /= v_size;
                            tcs.SetResult((double)(result1/h1+result2/h2));
                            break;
                        case comparisonStyle.CosineHR:
                            /*
                            var cent1 = Model.centroidsModel.getVector(this.label);
                            var cent2 = Model.centroidsModel.getVector(toCompareWith.label);
                            double q1 = 0;
                            double q2 = 0;
                            double r1 = cosine(this.values, toCompareWith.values, v_size);
                            double r2 = cosine(cent1.values, this.values, v_size);
                            double y_est = Model.Beta1 * Math.Pow(r1, Model.Beta2) - Model.Beta3 * Math.Pow(r2, Model.Beta4);    
                            double y = Model.sl.findEntry(this.label, toCompareWith.label).simLex / 10;
                            
                            if(Model.updating)
                                {
                                    Model.update(q1, q2, r1, r2, y_est, y);
                                    Model.msqe += (y - y_est) * (y - y_est) / 2;
                                }
                            tcs.SetResult(y_est);
                            */
                            
                            Vector cent1 = null; 
                            Vector cent2 = null;
                            try
                            {
                            cent1 = Model.centroidsModel.getVector(this.label);
                            }
                            catch
                            {
                                cent1 = Vector.dummy(300);}
                            try
                            {
                                cent2 = Model.centroidsModel.getVector(toCompareWith.label);
                            }
                            catch
                            { cent2 = Vector.dummy(300); }
                            
                            ////double cent_sim1 = Model.sl.findEntry(this.label, toCompareWith.label).simLex / 10;
                            //double cent_sim2 = Model.sl.findEntry(this.label, toCompareWith.label).simLex / 10;
                            //double cent_sim1 = Model.sl.findEntry(this.label, toCompareWith.label).concw1 / 10;
                            
                            double cent_sim1 = cosine(this.values, cent1.values, v_size);
                            //double cent_sim2 = cosine(cent1.values, this.values, v_size);
                            //cent_sim1 = cent_sim2;
                            double q1 = (double)(-1*Model.Beta2*(1- cent_sim1- Model.Beta1));
                            //double q2 = (double)(-1*Model.Beta2 * (1 - cent_sim2 - Model.Beta1));
                            double r1 = (double)(1 / (1 + Math.Exp(-q1)));
                            //double r2 = (double)(1 / (1 + Math.Exp(-q2)));
                            double y_est = cosine(this.values, toCompareWith.values, v_size) - Math.Pow(cent_sim1, 0.12);
                            double y = Model.sl.findEntry(this.label, toCompareWith.label).simLex / 10;
                            
                            if(Model.updating)
                                {
                                    //Model.update(q1, q2, r1, r2, y_est, y);
                                    Model.msqe += (y - y_est) * (y - y_est) / 2;
                                }
                            tcs.SetResult(y_est);
                            
                            /*
                             * 
                             * model z różnicą pomiędzy aktywacją centroidów
                             *
                            var cent1 = Model.centroidsModel.getVector(this.label);
                            var cent2 = Model.centroidsModel.getVector(toCompareWith.label);
                            double cent_sim1 = cosine(cent1.values, this.values, v_size);
                            double cent_sim2 = cosine(cent2.values, toCompareWith.values, v_size);
                            double q1 = (double)(-1*Model.Beta2*(1- cent_sim1- Model.Beta1));
                            double q2 = (double)(-1*Model.Beta2 * (1 - cent_sim2 - Model.Beta1));
                            double r1 = (double)(1 / (1 + Math.Exp(-q1)));
                            double r2 = (double)(1 / (1 + Math.Exp(-q2)));
                            double y_est = cosine(this.values, toCompareWith.values, v_size) + (double)Model.Beta3*(r1 - r2);
                            double y = Model.sl.findEntry(this.label, toCompareWith.label).simLex / 10;
                            if(Model.updating)
                                {
                                    Model.update(q1, q2, r1, r2, y_est, y);
                                }
                            tcs.SetResult(y_est);
                              
                             */
                            /*
                            
                             * B1 = 0.24
                             * B2 = 0.14
                             * B3 = 700
                             
                            double actual_val = Model.sl.findEntry(this.label, toCompareWith.label).simLex;
                            tcs.SetResult((double)(c + 0.14*Math.Abs(act1-act2)));
                            
                             */
                             break;
                        case comparisonStyle.Cosine:
                        default:
                            tcs.SetResult(cosine(this.values, toCompareWith.values, v_size));
                            break;
                    }
                });
            return tcs.Task;
        }
        double cosine(double[] v1, double[] v2, int size)
        {
            double t = 0;
            double l1 = 0;
            double l2 = 0;
            for (int i = 0; i < v_size; i++)
            {
                t += v1[i] * v2[i];
                l1 +=  v1[i] * v1[i];
                l2 += v2[i] * v2[i];
            }
            return (double)(t / (Math.Sqrt(l1) * Math.Sqrt(l2)));
        }
        double euclidean(double[] v1, double [] v2, int size)
        {
            double t = 0;
            for (int i = 0; i < v_size; i++)
            {
                t += (v1[i] - v2[i]) * (v1[i] - v2[i]);
            }
            return Math.Sqrt(t);

        }
    }
  
}
