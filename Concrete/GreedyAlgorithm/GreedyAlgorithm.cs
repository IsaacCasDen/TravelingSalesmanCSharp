
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TravelingSalesmanCSharp.Concrete.GreedyAlgorithm {
    public class GreedyAlgorithm {
        public class Test {
            public decimal? DistanceTraveled {get; protected internal set;} = null;
            public List<PositionNode> Visited {get; protected internal set;} = new List<PositionNode>();
            public List<PositionNode> Nodes {get; protected internal set;} = new List<PositionNode>();
        }

        Worker<Tuple<int,Test>, decimal>[] workers;

        public List<Test> Tests {get; private set;} = new List<Test>();
        

        public GreedyAlgorithm(List<PositionNode> nodes) {
            init(nodes);
        }
        public bool execute() {
            return executeThreaded();
        }
        public void exportRoutes(string filename) {
            using (StreamWriter sr = new StreamWriter(filename)) {
                for (int i=0; i<Tests.Count; i++) {
                    Test test = Tests[i];
                    string output = string.Format("{0},{1}",i,string.Join(",",test.Visited));
                    sr.WriteLine(output);
                }
            }
        }

        private void init(List<PositionNode> nodes)
        {
            for (int i=0; i<nodes.Count; i++) {
                Test test = new Test();
                for (int j=i; j<nodes.Count+i; j++) {
                    test.Nodes.Add(nodes[j%nodes.Count]);
                }
                Tests.Add(test);
            }
            initWorkers();
        }

        private void initWorkers() {
            int procCount = Environment.ProcessorCount;
            workers = new Worker<Tuple<int,Test>, decimal>[procCount];
            for (int i=0; i<workers.Length; i++) {
                Worker<Tuple<int,Test>, decimal> w = new Worker<Tuple<int,Test>, decimal>(executeTest);
                workers[i] = w;
                w.start();
            }
        }
        
        private void executeUnthreaded() {

            decimal? distance = null;
            int? pos = null;

            for (int i=0; i<Tests.Count; i++) {
                decimal dist = executeTest(new Tuple<int,Test>(i,Tests[i]));
                Console.Write("\nTotal Distance Traveled on Test {0}:\t{1}",i,dist);
                if (!distance.HasValue||dist<distance) {
                    distance = dist;
                    pos = i;
                    Console.WriteLine("\t***New Best***");
                } 
                else {
                    Console.WriteLine();
                }
                // Thread.Sleep(5000);
            }

            Console.WriteLine("\n--- Best Result at Test {0} with Total Distance Traveled: {1} ---",pos.Value,distance.Value);
        }
        private bool executeThreaded() {
            int testsPerW = Tests.Count/workers.Length;
            int remaining = Tests.Count - (testsPerW*workers.Length);
            int testIndex = 0;

            for (int i=0; i<workers.Length; i++) {
                List<Tuple<int, Test>> tests = new List<Tuple<int,Test>>();
                int j=testIndex;
                for (; j<testsPerW+testIndex; j++) {
                    tests.Add(new Tuple<int, Test>(j,Tests[j]));
                }
                if (remaining>0) {
                    tests.Add(new Tuple<int, Test>(j,Tests[j]));
                    remaining--;
                    j++;
                }
                workers[i].addJobs(tests);
                testIndex=j;
            }

            bool running = false;
            do {
                running = false;
                for (int i=0; i<workers.Length; i++) {
                    if (workers[i].JobRunning) {
                        running = true;
                        Thread.Sleep(500);
                    }
                }
            } while (running);
            
            Test test = Tests[0];
            for (int i=1; i<Tests.Count; i++) {
                Test item = Tests[i];
                int index = 0;
                for (; index<item.Visited.Count; index++) {
                    if (item.Visited[index].Equals(test.Visited[0])) {
                        break;
                    }
                }
                if (index<item.Visited.Count) {
                    List<PositionNode> route = new List<PositionNode>();
                    for (int j=index; j<index+item.Visited.Count; j++) {
                        int v = j%item.Visited.Count;
                        if ((j%item.Visited.Count)!=0) {
                            route.Add(item.Visited[j%item.Visited.Count]);
                        }
                    }
                    route.Add(route[0]);
                    item.Visited=route;
                }
            }



            decimal? distance = null;
            int? pos = null;

            for (int i=0; i<Tests.Count; i++) {
                if (!distance.HasValue || Tests[i].DistanceTraveled<distance) {
                    distance = Tests[i].DistanceTraveled;
                    pos = i;
                }
            }

            for (int i=0; i<Tests.Count; i++) {
                Console.Write(string.Format("\nTest {0}\tTotal Distance: {1}",i,Tests[i].DistanceTraveled));
                if (i==pos.Value) {
                    Console.Write("\t*** Best Result ***");
                }
            }

            Console.WriteLine(string.Format("Best Result: Route {0}\tTotal Distance: {1}",pos.Value,Tests[pos.Value].DistanceTraveled));
            Console.WriteLine(string.Format("{0}\n{1}",Tests[pos.Value].Visited[0],string.Join(",",Tests[pos.Value].Visited)));

            return true;
        }
        private decimal executeTest(Tuple<int,Test> test) {
            PositionNode currentNode = test.Item2.Nodes[0];
            test.Item2.Nodes.Remove(currentNode);
            test.Item2.Visited.Add(currentNode);
            decimal dist = 0;
            test.Item2.DistanceTraveled = dist;

            while (test.Item2.Nodes.Count>0) {
                // Console.WriteLine(string.Format("Traveled to {0}\tDistance: {1}\tDistance Traveled: {2}",currentNode, dist, test.DistanceTraveled));
                decimal? shortest = null;
                PositionNode closest=null;

                for (int i=0; i<test.Item2.Nodes.Count; i++) {
                    decimal distance = currentNode.distance(test.Item2.Nodes[i]);
                    if (!shortest.HasValue || distance<shortest) {
                        shortest = distance;
                        closest = test.Item2.Nodes[i];
                    }
                }

                if (shortest.HasValue) {
                    dist=shortest.Value;
                    test.Item2.DistanceTraveled += shortest;
                    currentNode=closest;
                    test.Item2.Visited.Add(currentNode);
                    test.Item2.Nodes.Remove(currentNode);
                    
                } else {
                    throw new Exception("Unable to locate closest Node");
                }   
            }
            
            test.Item2.DistanceTraveled += test.Item2.Visited[test.Item2.Visited.Count-1].distance(test.Item2.Visited[0]);
            test.Item2.Visited.Add(test.Item2.Visited[0]);

            Console.WriteLine(string.Format("Route {0}\tTotal Distance Traveled: {1}",test.Item1, test.Item2.DistanceTraveled));

            return test.Item2.DistanceTraveled.Value;
        }

    }
}