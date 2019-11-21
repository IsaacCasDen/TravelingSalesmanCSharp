
using System;
using System.Collections;
using System.Collections.Generic;

namespace TravelingSalesmanCSharp.Concrete.CompleteAlgorithm {

    public class CompleteAlgorithm {

        List<Node> nodes;
        List<List<Node>> routes = new List<List<Node>>();

        public CompleteAlgorithm(List<Node> nodes) {
            this.nodes = nodes;
            generateRoutes();
        }

        private void generateRoutes() {
            
        }
        private void generateRoute() {

        }
        private decimal testRoute(Tuple<int,List<PositionNode>> info) {
            
            decimal distance = 0;
            for (int i=0; i<info.Item2.Count-1; i++) {
                distance += info.Item2[i].distance(info.Item2[i+2]);
            }

            return distance;

        }
    }

}