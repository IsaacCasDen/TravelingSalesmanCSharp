using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using TravelingSalesmanCSharp.Concrete;
using TravelingSalesmanCSharp.Concrete.CompleteAlgorithm;
using TravelingSalesmanCSharp.Concrete.GreedyAlgorithm;
using TravelingSalesmanCSharp.Concrete.GeneticAlgorithm;

namespace TravelingSalesmanCSharp
{
    class Program
    {

        private static List<Node> readNodes(string path) {
            List<Node> values = new List<Node>();
            int id = 0;

            using (StreamReader sr = new StreamReader(path)) {
                string line;
                while ((line=sr.ReadLine())!=null) {
                    string[] val = line.TrimEnd('\n').Split(',');
                    List<int> pos = new List<int>();
                    foreach (string v in val) {
                        pos.Add(int.Parse(v));
                    }
                    PositionNode n = new PositionNode(id,new Vector(pos));
                    id++;
                    values.Add(n);
                }
            }
            return values;
        }

        private static List<Specimen> readRoutes(string path) {
            List<Specimen> values = new List<Specimen>();
            using (StreamReader sr = new StreamReader(path)) {
                string line;
                while ((line=sr.ReadLine())!=null) {
                    string[] val = line.TrimEnd('\n').Split(',');
                    int id = int.Parse(val[0]);
                    List<PositionNode> nodes = new List<PositionNode>();
                    for (int i = 1; i<val.Length; i++) {
                        string[] _node = val[i].Split(' ');
                        PositionNode node = new PositionNode(_node[0], new Vector(_node[1]));
                        nodes.Add(node);
                    }
                    TravelingSpecimen specimen = new TravelingSpecimen(nodes,id);
                    values.Add(specimen);
                }
            }
            return values;
        }

        // private static List<TravelingSpecimen> calibrateRoutes(List<TravelingSpecimen> list) {
        //     TravelingSpecimen value = list[0];
        //     for (int i=1; i<list.Count; i++) {
        //         TravelingSpecimen item = list[i];
        //         int index=0;
        //         for (; index<item.value.Count; index++) {
        //             if (item.value[index].Equals(value.value[0])) break;
        //         }
        //         if (index<item.value.Count) {
        //             List<Node> route = new List<Node>();
        //             for (int j=index; j<index + item.value.Count; j++) {
        //                 route.Add(item.value[j%item.value.Count]);
        //             }
        //             item.value=route;
        //         }
        //     }
        //     return list;
        // }
        
        static void Main(string[] args)
        {

            string nodeFile = "Graph500.txt";
            string candFile = "export.csv";

            int? nodeCount = null;
            int? candCount = null;

            if (args.Length>0) {
                for (int i=0; i<args.Length; i++) {
                    switch(args[i]) {
                        case "-in":
                            nodeFile = args[++i];
                            break;
                        case "-ic":
                            candFile = args[++i];
                            break;
                        case "-nc":
                            nodeCount = int.Parse(args[++i]);
                            break;
                        case "-cc":
                            candCount = int.Parse(args[++i]);
                            break;
                    }
                }
            }

            List<Node> nodes = null;
            List<Specimen> list = null;

            if (File.Exists(candFile)) {
                list = readRoutes(candFile);
                if (list.Count>0) {
                    Type t = list[0].GetType();
                    if (t == typeof(TravelingSpecimen)) {
                        GeneticAlgorithm<TravelingSpecimen> alg2 = new GeneticAlgorithm<TravelingSpecimen>();
                        alg2.beginTests(list,0);
                    } else if (t == typeof(BinarySpecimen)) {

                    }
                }
            }   else if (File.Exists(nodeFile)) {
                nodes = readNodes(nodeFile);
                if (nodes.Count>0) {
                    Type t = nodes[0].GetType();
                    if (t == typeof(PositionNode)) {
                        GreedyAlgorithm alg1 = new GreedyAlgorithm(nodes.Cast<PositionNode>().ToList());
                        if (alg1.execute()) {
                            alg1.exportRoutes(candFile);
                        }
                    }
                    if (File.Exists(candFile)) {
                        list = readRoutes(candFile);
                    }
                    if (list!=null && list.Count>0) {
                        t = list[0].GetType();
                        if (t == typeof(TravelingSpecimen)) {
                            GeneticAlgorithm<TravelingSpecimen> alg2 = new GeneticAlgorithm<TravelingSpecimen>();
                            alg2.beginTests(list,candCount);
                        } else if (t == typeof(BinarySpecimen)) {
                            GeneticAlgorithm<BinarySpecimen> alg2 = new GeneticAlgorithm<BinarySpecimen>();
                            alg2.beginTests(list,candCount);
                        }
                    } else {
                        if (t == typeof(PositionNode)) {
                            GeneticAlgorithm<TravelingSpecimen> alg2 = new GeneticAlgorithm<TravelingSpecimen>();
                            alg2.beginTests(nodes,true,nodeCount);
                        } else if (t == typeof(BinaryNode)) {
                            GeneticAlgorithm<BinarySpecimen> alg2 = new GeneticAlgorithm<BinarySpecimen>();
                            alg2.beginTests(nodes,false,nodeCount);
                        }
                    }
                }
            } else {
                        Console.WriteLine(string.Format("Please specify existing file containing nodes or routes to optimize"));
            }
        }
    }
}
