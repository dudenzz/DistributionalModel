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
            this.values = values;
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
            RESM
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
        public Task<double> compare(Vector toCompareWith, comparisonStyle style)
        {

            TaskCompletionSource<double> tcs = new TaskCompletionSource<double>();
            Task.Run(() =>
                {
                    switch (style)
                    {
                        case comparisonStyle.RESM:
                            int top = 10;
                            int k = 20;
                            var asc_order1 = sortedComponents(this, true, top);
                            var asc_order2 = sortedComponents(toCompareWith, true, top);
                            var desc_order1 = sortedComponents(this, false, top);
                            var desc_order2 = sortedComponents(toCompareWith, false, top);
                            double result1 = 0;
                            double result2 = 0;
                            for (int i = 0; i < top; i++ )
                            {
                                int c1 = asc_order1[i];
                                if(asc_order2.Contains(c1))
                                {
                                    int i1 = i + 1;
                                    int i2 = asc_order2.IndexOf(c1) + 1;
                                    double p1 = Math.Exp(-i1 * k / (double)v_size);
                                    double p2 = Math.Exp(-i2*k/(double)v_size);
                                    result1 += (double)(p1 * p2);
                                }
                                else
                                {
                                    int i1 = i + 1;
                                    int i2 = 300;
                                    double p1 = Math.Exp(-i1 * k / (double)v_size);
                                    double p2 = Math.Exp(-i2 * k / (double)v_size);
                                    result1 += (double)(p1 * p2);
                                }
                                int dc1 = asc_order1[i];
                                if (desc_order2.Contains(c1))
                                {
                                    int di1 = i;
                                    int di2 = desc_order2.IndexOf(c1);
                                    double res = Math.Exp(-di1 * k / v_size) * Math.Exp(-di2 * k / v_size);
                                    result2 += (double)res;
                                }
                                else
                                {
                                    int di1 = i;
                                    int di2 = 300;
                                    double res = Math.Exp(-di1 * k / v_size) * Math.Exp(-di2 * k / v_size);
                                    result2 += (double)res;
                                }
                                
                            }
                            tcs.SetResult((double)(result1+result2));
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
                        
                            var cent1 = Model.centroidsModel.getVector(this.label);
                            var cent2 = Model.centroidsModel.getVector(toCompareWith.label);
                            //double cent_sim1 = Model.sl.findEntry(this.label, toCompareWith.label).simLex / 10;
                            double cent_sim2 = Model.sl.findEntry(this.label, toCompareWith.label).simLex / 10;
                            //double cent_sim1 = Model.sl.findEntry(this.label, toCompareWith.label).concw1 / 10;
                            
                            double cent_sim1 = cosine(this.values, toCompareWith.values, v_size);
                            //double cent_sim2 = cosine(cent1.values, this.values, v_size);
                            //cent_sim1 = cent_sim2;
                            double q1 = (double)(-1*Model.Beta2*(1- cent_sim1- Model.Beta1));
                            double q2 = (double)(-1*Model.Beta2 * (1 - cent_sim2 - Model.Beta1));
                            double r1 = (double)(1 / (1 + Math.Exp(-q1)));
                            double r2 = (double)(1 / (1 + Math.Exp(-q2)));
                            double y_est = cosine(this.values, toCompareWith.values, v_size) + Model.Beta3 * r1;
                            double y = Model.sl.findEntry(this.label, toCompareWith.label).simLex / 10;
                            
                            if(Model.updating)
                                {
                                    Model.update(q1, q2, r1, r2, y_est, y);
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
