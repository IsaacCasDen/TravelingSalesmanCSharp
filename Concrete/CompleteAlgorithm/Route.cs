
using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelingSalesmanCSharp.Concrete.CompleteAlgorithm {
    public class Route<T>{
        
        List<T> steps;

        public Route(List<T> steps) {
            if (steps==null) throw new NullReferenceException("steps cannot be null");
            this.steps = steps;
        }

        public List<List<T>> proliferate() {
            List<List<T>> values = new List<List<T>>();
            T start = steps[0];
            T finish = steps[steps.Count-1];
            List<T> opts = new List<T>();
            for (int i=1; i<steps.Count-1; i++) {
                T opt = steps[i];
                if (!opts.Contains(opt)) opts.Add(opt);
            }
            opts.Sort();

           List<T> baseList = initList(steps);
           for (int i=0; i<steps.Count; i++) {
                for (int begin=1; begin<steps.Count-1; begin++) {
                    List<T> beginList = copy(baseList);
                    for (int end=begin; end<steps.Count-1; end++) {
                        List<T> endList = copy(beginList);
                        for (int o=0; o<opts.Count; o++) {
                            List<T> optList = copy(endList);

                        }
                    }
                }
                
           }
            return values;
        }

        protected List<T> initList(List<T> source) {
            List<T> values = new List<T>();
            while (values.Count<source.Count) values.Add(default(T));
            values[0]=source[0];
            values[values.Count-1]=source[source.Count-1];
            return values;
        }
        protected List<T> copy(List<T> source) {
            List<T> values = new List<T>();
            for (int i=0; i<source.Count; i++) values.Add(source[i]);
            return values;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //
            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            // TODO: write your implementation of Equals() here
            
            Route<T> other = obj as Route<T>;
            if (other!=null) {
                if (steps.Count!=other.steps.Count) return false;
                for (int i=0; i<steps.Count; i++) {
                    if (!steps[i].Equals(other.steps[i])) return false;
                }

                return true;
            }

            return false;
        }
        
        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return steps.GetHashCode();
        }

    }
}