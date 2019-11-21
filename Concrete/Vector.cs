
using Common;

using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelingSalesmanCSharp.Concrete {
public class Vector {

        public int Dimension {
            get {
                return position.Count;
            }
        }

        public readonly List<int> position;

        public Vector(List<int> position) {
            if (position==null) throw new NullReferenceException("position cannot be null");
            this.position = position;
        }
        public Vector(int[] position) {
            if (position==null) throw new NullReferenceException("position cannot be null");
            this.position = position.ToList();
        }
        public Vector(string position) {
            if (position==null) throw new NullReferenceException("position cannot be null");
            int[] pos =Array.ConvertAll(position.TrimStart('[').TrimEnd(']').Trim('\n').Split(';'),int.Parse);
            this.position = pos.ToList();
        }

        public decimal distance(Vector other) {
            return Vector.distance(this,other);
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
            
            if (obj == null) {
                return false;
            } else if (GetType() != obj.GetType())
            {
                return false;
            }
            
            // TODO: write your implementation of Equals() here
            Vector other = obj as Vector;
            if (other!=null) {
                return this.position.SequenceEqual(other.position);
            }

            return false;
        }
        
        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return this.position.GetHashCode();
        }

        public static decimal distance(Vector vec1, Vector vec2) {
            decimal value = 0;
            decimal val = 0;
            int size = Math.Min(vec1.Dimension,vec2.Dimension);

            for (int i=0; i<size; i++) {
                val += Common.Common.Pow(vec1.position[i]-vec2.position[i],2);
            }
            value = Common.Common.Sqrt(Math.Abs(val));

            return value;
        }

        public override string ToString() {
            string value = "[{0}]";
            value = string.Format(value,string.Join(';',position));
            return value;
        }
    }
}