

using System;

namespace TravelingSalesmanCSharp.Concrete {
    public abstract class Node {
        public object Id {get; protected set;}

        public object Value {get; protected set;}

    }
    public class BinaryNode:Node {
        public BinaryNode(object id, int value) {
            this.Id=id;
            this.Value=value;
        }
    }
    public class PositionNode:Node {

            public PositionNode(object id, Vector position) {
                this.Id = id;
                this.Value = position;
            }
            public PositionNode(string value) {
                string[] info = value.TrimStart('[').TrimEnd(']').Split(' ',1);
                this.Id = info[0];
                this.Value=new Vector(Array.ConvertAll(info[1].Split(','),int.Parse));
            }

            public decimal distance(PositionNode other) {
                return PositionNode.distance(this,other);
            }

            public static decimal distance(PositionNode nodeA, PositionNode nodeB) {
                return ((Vector)nodeA.Value).distance((Vector)nodeB.Value);
            }

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
                    return object.Equals(this.Value,obj) || object.Equals(this.Id,obj);
                }
                
                // TODO: write your implementation of Equals() here
                PositionNode other = obj as PositionNode;
                if (other!=null) {
                    return this.Value.Equals(other.Value);
                }
                return false;
            }
            
            // override object.GetHashCode
            public override int GetHashCode()
            {
                // TODO: write your implementation of GetHashCode() here
                return Value.GetHashCode();
            }

            public override string ToString() {
                string value = string.Format("{0} {1}",this.Id,this.Value);
                return value;
            }
        }
}