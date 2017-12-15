using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionalSemantics
{
    class Model
    {
        List<Vector> vectors;
        int v_size = 0;
        public Model(string filename)
        {
            vectors = new List<Vector>();
            StreamReader sr = new StreamReader(filename);
            string line;
            bool known_size = false;
            while((line = sr.ReadLine()) != null)
            {
             
                if(!known_size)
                    v_size = Vector.RecognizeVSize(line);
                vectors.Add(new Vector(line,v_size));
            }
        }
    }
}
