
using System;
using System.Collections.Generic;
using System.Linq;

using TravelingSalesmanCSharp.Concrete;

namespace TravelingSalesmanCSharp.Concrete.GeneticAlgorithm {
    public abstract class Specimen:IComparable {
        protected int getNewId() {
            int new_id = next_id;
            next_id++;
            return new_id;
        }
        private static int next_id = 0;

        public object Id {
                get {
                    return _id;
                }
                protected set {
                    if (value.GetType() == typeof(int) && ((int)value)>next_id) {
                        next_id = ((int)value)+1;
                    }
                    _id = value;
                }
            }
            private object _id;
            public object ParentIdA {get; protected set;}
            public object ParentIdB {get; protected set;}
            public int MutationCount {get;protected set;}

            public decimal Fit {
                get {
                    if (!fit.HasValue) fit = _fit();
                    return fit.Value;
                }
                protected set {
                    fit = value;
                }
            }
            private decimal? fit;
            protected abstract decimal _fit();

            public List<Node> Values {get; protected set;}

            protected Specimen(List<Node> values, object id = null, object parentIdA =null, object parentIdB = null, int mutationCount = 0) {
                this.Values=values;
                this.ParentIdA=parentIdA;
                this.ParentIdB=parentIdB;
                this.MutationCount=mutationCount;
                this.Fit = _fit();
            }

            public int CompareTo(object obj)
            {   
                if (obj==null) return -1;
                else if (obj.GetType() == this.GetType()) return this.Fit.CompareTo(((TravelingSpecimen)obj).Fit);
                else return Fit.CompareTo(obj);
            }

            public abstract Specimen[] combine(Random random, Specimen other);

            public abstract override string ToString();

            // public abstract Specimen Create(Random random, int nodeCount);
            // public abstract Specimen Create(Random random, Specimen template);
    }

    public class BinarySpecimen : Specimen
    {

        int target;

        public BinarySpecimen(List<BinaryNode> values, object id = null, object parentIdA = null, object parentIdB = null, int mutationCount = 0) : base(values.Cast<Node>().ToList(), id, parentIdA, parentIdB, mutationCount)
        {
            init();
        }

        public BinarySpecimen(BinaryNode[] values, object id = null, object parentIdA = null, object parentIdB = null, int mutationCount = 0) : base(values.Cast<Node>().ToList(), id, parentIdA, parentIdB, mutationCount)
        {
            init();
        }

        private void init() {
            target = Values.Count;
        }

        public override Specimen[] combine(Random random, Specimen other) {
            return (Specimen[])combine(random,(BinarySpecimen)other);
        }
        public BinarySpecimen[] combine(Random random, BinarySpecimen other)
        {
            List<int> ind = new List<int>();
            BinaryNode[] newValue = (BinaryNode[])this.Values.ToArray();

            bool hasMutation = random.NextDouble()<0.02;
            int? mutInd = null;
            int mutCount = Math.Max(this.MutationCount,other.MutationCount);
            if (hasMutation) {
                mutInd = random.Next(0,newValue.Length);
                mutCount++;
            }

            for (int i=0; i<newValue.Length/2; i++) {
                int? pos = null;
                while (!pos.HasValue || ind.Contains(pos.Value)) {
                    pos = random.Next(0,newValue.Length);
                }
                ind.Add(pos.Value);
            }

            if (hasMutation) {
                for (int i=0; i<newValue.Length; i++) {
                    if (ind.Contains(i)) {
                        newValue[i] = (BinaryNode)other.Values[i];
                    }
                    if (mutInd.HasValue && mutInd.Value == i) {
                        newValue[i] = new BinaryNode(newValue[i].Id,((int)newValue[i].Value==0)?1:0);
                    }
                }
            } else {
                for (int i=0; i<newValue.Length; i++) {
                    if (ind.Contains(i)) {
                        newValue[i] = (BinaryNode)other.Values[i];
                    }
                }
            }

            List<BinarySpecimen> children = new List<BinarySpecimen>();
            children.Add(new BinarySpecimen(newValue,null,this.Id,other.Id,mutCount));
            if (hasMutation) {
                    children.AddRange(combine(random, other));
                }
            return children.ToArray();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Values.GetHashCode();
        }

        public override string ToString()
        {
            string value = string.Format("Id: {0} [{1}]",this.Id,string.Join("",this.Values));

            return value;
        }

        protected override decimal _fit()
        {
            int value = 0;

            for (int i=0; i<this.Values.Count; i++) {
                if (((int)this.Values[i].Value)==1) value++;
            }

            return value;
        }

        // public override Specimen Create(Random random, int nodeCount) {
        //     BinaryNode[] nodes = new BinaryNode[nodeCount];
        //     for (int i=0; i<nodes.Length; i++) nodes[i] = new BinaryNode(i,random.Next(0,2));
        //     Specimen specimen = new BinarySpecimen(nodes,null,null,null,0);
        //     return specimen;
        // }
        // public override Specimen Create(Random random, Specimen template) {
        //     BinaryNode[] nodes = new BinaryNode[template.Values.Count];
        //     for (int i=0; i<nodes.Length; i++) nodes[i] = new BinaryNode(i,random.Next(0,2));
        //     Specimen specimen = new BinarySpecimen(nodes,null,null,null,0);
        //     return specimen;
        // }
    }
    public class TravelingSpecimen:Specimen {

            public List<PositionNode> value;

            public TravelingSpecimen(PositionNode[] values, object id = null, object parentIdA =null, object parentIdB = null, int mutationCount = 0):
                base(values.Cast<Node>().ToList(),id,parentIdA,parentIdB,mutationCount) {
            }
            public TravelingSpecimen(List<PositionNode> values, object id = null, object parentIdA =null, object parentIdB = null, int mutationCount = 0):
                base(values.Cast<Node>().ToList(),id,parentIdA,parentIdB,mutationCount) {
            }

            protected override decimal _fit() {
                decimal fit = this.distance();
                return fit;
        }

            protected decimal distance() {
                decimal value = 0;

                for (int i=0; i<this.value.Count-1; i++) {
                    Vector vec1 = (Vector)this.value[i].Value;
                    Vector vec2 = (Vector)this.value[i+1].Value;
                    value += vec1.distance(vec2);
                }

                return value;
            }

            public override Specimen[] combine(Random random, Specimen other) {
                if (GetType().Equals(other.GetType())) {
                    return combine(random,(TravelingSpecimen)other);
                } else {
                    throw new ArgumentOutOfRangeException("Incompatible specimen argument");
                }
            }
            public TravelingSpecimen[] combine(Random random, TravelingSpecimen other) {
                List<TravelingSpecimen> children = new List<TravelingSpecimen>();

                int min = 1;
                int max = this.value.Count-1;

                List<PositionNode> newValue = this.value.ToArray().ToList();

                bool hasMutation = random.NextDouble()<0.06;
                int mutCount = Math.Max(this.MutationCount,other.MutationCount);

                if (hasMutation) {
                    int mutInd1 = random.Next(min,max);
                    int? mutInd2 = null;
                    while (!mutInd2.HasValue || mutInd2.Value==mutInd1) {
                        mutInd2 = random.Next(min,max);
                    }
                    PositionNode v1 = newValue[mutInd1];
                    PositionNode v2 = newValue[mutInd2.Value];

                    newValue[mutInd1] = v2;
                    newValue[mutInd2.Value] = v1;

                    mutCount++;
                }

                List<int> indices = new List<int>();

                while (indices.Count<newValue.Count/2) {
                    int? ind = null;
                    while (!ind.HasValue || newValue[0].Equals(other.value[ind.Value]) || indices.Contains(ind.Value)) {
                        ind = random.Next(min,max);
                    }
                    indices.Add(ind.Value);
                }


                int i=0;
                
                for (i=0; i<indices.Count; i++) {
                    PositionNode a=null;
                    PositionNode b=null;
                    bool found = false;
                    int? ind = null;
                    for (int j=1; j<newValue.Count-1; j++) {
                        if (newValue[j] != null) {
                            if (newValue[j].Equals(other.value[indices[i]])) {
                                ind = j;
                                break;
                            } else {
                                if (!found) {
                                    a = newValue[j]; 
                                    b = other.value[indices[i]];
                                    if (a.Equals(b)) {
                                        Console.WriteLine("Huh...");
                                        found = true;
                                    }
                                }
                            }
                        }
                    }
                    if (!ind.HasValue) {
                        throw new Exception(string.Format("Error: {0} not in {1}",other.value[indices[i]],string.Join(",",newValue)));
                    } else {
                        newValue[ind.Value] = null;
                    }
                }

                indices.Sort();
                indices.Reverse();

                i=0;
                while (i<indices.Count) {
                    int ind = indices[i];
                    int j = ind+1;
                    while (j<newValue.Count) {
                        if (newValue[j]==null) {
                            newValue.RemoveAt(j);
                        } else {
                            j++;
                        }
                    }
                    newValue.Insert(ind,other.value[ind]);
                    i++;
                }

                i=0;
                while (i<newValue.Count) {
                    if (newValue[i]==null)
                        newValue.RemoveAt(i);
                    else
                        i++;
                }

                foreach (PositionNode val in this.value) {
                    if (!newValue.Contains(val)) {
                        string output = string.Format("Error: Missing Value {0}\n{1}\n{2}",val,this.value.ToString(),newValue.ToString());
                        throw new Exception(output);
                    }
                }

                children.Add(new TravelingSpecimen(newValue,null,this.Id,other.Id,mutCount));
                if (hasMutation) {
                    children.AddRange(combine(random, other));
                }
                return children.ToArray();
            }

            public override string ToString() {
                string value = string.Format("Id: {0} Mutation Count: {1} Parent A: {2} Parent B: {3} Values: [{4}]",
                    this.Id,this.MutationCount,this.ParentIdA,this.ParentIdB,string.Join(",",this.value));
                return value;
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
                
                if (obj == null)
                {
                    return false;
                } else if (GetType() != obj.GetType()) {
                    return this.value.Equals(obj);
                }
                
                // TODO: write your implementation of Equals() here
                return this.value.Equals(((TravelingSpecimen)obj).value);
            }
            
            // override object.GetHashCode
            public override int GetHashCode()
            {
                // TODO: write your implementation of GetHashCode() here
                return this.value.GetHashCode();
            }

        // public override Specimen Create(Random random, int nodeCount) {
        //     PositionNode[] nodes = new PositionNode[nodeCount];

        //     // for (int i=0; i<nodes.Length; i++) nodes[i] = new PositionNode(i,random.Next(0,2));
        //     Specimen specimen = new TravelingSpecimen(nodes,null,null,null,0);
        //     return specimen;
        // }
        // public override Specimen Create(Random random, Specimen template) {
        //     PositionNode[] nodes = new PositionNode[template.Values.Count];

        //     // for (int i=0; i<nodes.Length; i++) nodes[i] = new PositionNode(i,random.Next(0,2));
        //     Specimen specimen = new TravelingSpecimen(nodes,null,null,null,0);
        //     return specimen;
        // }
        }
    }