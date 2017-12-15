using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionalSemantics
{
    class Vector
    {
        float[] values;

        public float[] Values
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

        public static int RecognizeVSize(string line)
        {
            int v_size = 0;
            string[] tokens = line.Split(new char[] { ' ', '\t'});
            float tmp;
            foreach (string token in tokens) if (float.TryParse(token, out tmp)) v_size++;
            return v_size;
        }
        public Vector(string line, int size)
        {
            values = new float[size];
            v_size = size;
            string[] tokens = line.Split(new char[] { ' ', '\t' });
            label = tokens[0];
            for (int i = 1; i <= size; i++) values[i - 1] = float.Parse(tokens[i]);
        }

        public Task<float> compare(Vector toCompareWith)
        {
            TaskCompletionSource<float> tcs = new TaskCompletionSource<float>();
            Task.Run(() =>
                {
                    float t = 0;
                    float l1 = 0;
                    float l2 = 0;
                    for (int i = 0; i < v_size; i++)
                    {
                        t += values[i] * toCompareWith.values[i];
                        l1 += values[i] * values[i];
                        l2 += toCompareWith.values[i] * toCompareWith.values[i];
                    }
                    tcs.SetResult((float)(t / (Math.Sqrt(l1) * Math.Sqrt(l2))));
                });
            return tcs.Task;
        }
    }
}
