
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace TravelingSalesmanCSharp.Concrete.GeneticAlgorithm
{
    public class GeneticAlgorithm<T> where T:Specimen {
        static readonly int DEFAULT_CANDIDATE_COUNT = 16;

        List<Specimen> Candidates {get;set;}

        public int MaxCandidates {get; protected set;}
        
        List<Node> Nodes {get;set;} = new List<Node>();

        Random random;

        public int MaxThreads {get; protected set;} = Math.Max(Environment.ProcessorCount,1);
        
        public int GenCount {get; protected set;} = 0;

        Worker<Tuple<Specimen,Specimen>,Specimen[]>[] workers;

        Worker<Tuple<string,string,int,string,string>,bool> dataWorker;

        Worker<Tuple<string,string>, bool> logWorker;

        public GeneticAlgorithm() {
            random = new Random(0);
        }
        public GeneticAlgorithm(int seed) {
            random = new Random(seed);
        }
        
        public void beginTests(List<Node> nodes, bool uniqueNodes, int? candCount = null, int? candSize = null) {
            if (!candCount.HasValue) candCount = DEFAULT_CANDIDATE_COUNT;
            if (!initNodes(nodes)) return;
            if (!initCandidates(candCount.Value, uniqueNodes, candSize)) return;
        }
        public void beginTests(List<Specimen> cand, int? candCount = null) {
            // if (!candCount.HasValue) candCount = DEFAULT_CANDIDATE_COUNT;
            initCandidates(cand);
            
        }

        private bool initNodes(List<Node> nodes) {
            if (nodes == null || nodes.Count == 0) return false;
            this.Nodes = nodes;
            return true;
        }
        private bool initCandidates(int count, bool uniqueNodes, int? size = null) {
            if (this.Nodes==null || this.Nodes.Count>0) return false;
            this.Candidates = new List<Specimen>();
            
            return true;
        }

        // private void initCandidates(List<Specimen> _cand = null,List<Node[]> _nodes = null, int count = 512)
        // {
        //     if (_cand!=null) {
        //         _cand.Sort();
        //         candidates = _cand;
        //         return;
        //     }
        //     if (_nodes == null) {
        //         for (int i=0; i<count; i++) {
        //             List<Node> cand = nodes.ToArray().ToList();
        //             List<Node> vals = new List<Node>();

        //             int index = 0;
        //             Node start = nodes[index];
        //             vals.Add(start);
        //             cand.RemoveAt(index);

        //             while (cand.Count>0) {
        //                 index = random.Next(0,cand.Count);
        //                 vals.Add(cand[index]);
        //                 cand.RemoveAt(index);
        //             }

        //             vals.Add(start);
        //             candidates.Add(createCandidate(vals.ToArray()));
        //         }
        //     } else {
        //         foreach (Node[] value in _nodes) {
        //             candidates.Add(createCandidate(value));
        //         }
        //     }
        // }

        private bool initCandidates(List<Specimen> cand, int? newMaxCount = null) {
            if (cand == null || cand.Count == 0) return false;
            this.Candidates = cand;
            if (newMaxCount.HasValue) this.MaxCandidates = newMaxCount.Value;
            else this.MaxCandidates = cand.Count;
            return true;
        }
        // public void runTests(List<Node> _nodes = null, List<Specimen> _cand = null, int _nodeCount = 16, int _candCount = 16) {
        public void runTests() {
            // initNodes(_nodes,_nodeCount);
            // initCandidates(_cand,null,_candCount);
            initWorkers();

            string templateOutputPath = Path.Combine("output","output{0}{1}");
            string templateOutputExt = ".txt";
            string templateDataExt = ".csv";

            int index = 0;

            index=0;
            while (File.Exists(string.Format(templateOutputPath,index,templateOutputExt))) {
                index++;
            }
            string outputPath = string.Format(templateOutputPath,index,templateOutputExt);
            string dataPath = string.Format(templateOutputPath,index,"");

            Console.WriteLine("Beginning Tests");
            string output_header = "Generation\tMax Fit\tMin Fit\tAverage Fit\tCandidates\tTime\n";
            string data_header = "Generation,Id,Fitness,Mutation Count,Parent A,Parent B";

            index = 0;
            foreach (Node n in Candidates[0].Values) {
                data_header += string.Format(",Value {0}",index);
                index++;
            }
            data_header += "\n";

            string output = "";

            foreach (Specimen c in Candidates) {
                output += string.Format("{0},{1},{2},{3},{4},{5},",GenCount,c.Id,c.Fit,c.MutationCount,c.ParentIdA,c.ParentIdA);
                output += string.Join(",",c.Values);
                output += "\n";
            }
            
            logWorker.addJobs(new Tuple<string,string>(outputPath,output_header));
            dataWorker.addJobs(new Tuple<string, string, int, string, string>(dataPath,templateDataExt,GenCount,data_header,output));
            
            decimal? maxFit = null;
            int sameCount = 0;
            int lastImproved = 0;
            Console.WriteLine(output_header);
            Tuple<decimal,decimal,decimal> results = null;
            while (sameCount<100000) {
                GenCount++;
                results = createCandidates();
                output = "";
                foreach (Specimen c in Candidates) {
                    output += string.Format("{0},{1},{2},{3},{4},{5},",GenCount,c.Id,c.Fit,c.MutationCount,c.ParentIdA,c.ParentIdB);
                    output += string.Join(",",c.Values);
                    output += "\n";
                }
                dataWorker.addJobs(new Tuple<string, string, int, string, string>(dataPath,templateDataExt,GenCount,data_header,output));
                if (!maxFit.HasValue || results.Item1<maxFit.Value) {
                    lastImproved = GenCount;
                    bool display = (!maxFit.HasValue || Math.Round(results.Item1,6)<Math.Round(maxFit.Value,6));
                    maxFit = results.Item1;
                    sameCount=0;
                    if (display) {
                        output = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",GenCount,Math.Round(maxFit.Value,6),Math.Round(results.Item2,6),Math.Round(results.Item3,6),Candidates.Count,DateTime.Now);
                        logWorker.addJobs(new Tuple<string,string>(outputPath,output + "\n"));
                        Console.WriteLine(output);
                    }
                } else if (results.Item1 == maxFit) {
                    sameCount++;
                }

                if (GenCount%10000==0) {
                    output = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",GenCount,Math.Round(maxFit.Value,6),Math.Round(results.Item2,6),Math.Round(results.Item3,6),Candidates.Count,DateTime.Now);
                    logWorker.addJobs(new Tuple<string,string>(outputPath,output + "\n"));
                    Console.WriteLine(output);
                }
            }

            output = "";
            foreach (Specimen c in Candidates) {
                output += string.Format("{0},{1},{2},{3},{4},{5},",GenCount,c.Id,c.Fit,c.MutationCount,c.ParentIdA,c.ParentIdB);
                output += string.Join(",",c.Values);
                output += "\n";
            }

            dataWorker.addJobs(new Tuple<string, string, int, string, string>(dataPath,templateDataExt,GenCount,data_header,output));

            if (results!=null) {
                output = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",GenCount,Math.Round(maxFit.Value,6),Math.Round(results.Item2,6),Math.Round(results.Item3,6),Candidates.Count,DateTime.Now);
                logWorker.addJobs(new Tuple<string,string>(outputPath,output + "\n"));
                Console.WriteLine(output);
            }


            output = "Best Candidates:";
            for (int i=0; i<3 && i<Candidates.Count; i++) {
                output += string.Format("\nRank {0}\t{1}",i+1,Candidates[i]);
            }

            logWorker.addJobs(new Tuple<string,string>(outputPath,output));
            Console.WriteLine(output);

            completeTests();
        }

        private void completeTests() {
            foreach (var worker in workers) {
                while (worker.JobRunning) Thread.Sleep(500);
                worker.stop();
            }

            while (logWorker.JobRunning) Thread.Sleep(500);
            logWorker.stop();

            while (dataWorker.JobRunning) Thread.Sleep(500);
            dataWorker.stop();
        }
        private bool writeCandidates(Tuple<string,string,int,string,string> data) {
            string dataPath = data.Item1;
            string dataExt = data.Item2;
            int generation = data.Item3;
            string header = data.Item4;
            string value = data.Item5;

            string path = string.Format("{0}_Gen{1}{2}",dataPath,generation,dataExt);
            if (File.Exists(path)) File.Delete(path);
            using (StreamWriter swData = new StreamWriter(path)) {
                swData.Write(header);
                swData.Write(value);
            }
            return true;
        }

        private bool writeLog(Tuple<string,string> data) {
            string outputPath = data.Item1;
            string value = data.Item2;

            using (StreamWriter swOutput = new StreamWriter(outputPath,true)) {
                swOutput.Write(value);
            }
            return true;
        }
        private void initWorkers()
        {
            workers = new Worker<Tuple<Specimen,Specimen>,Specimen[]>[MaxThreads];
            for (int i=0; i<MaxThreads; i++) {
                Worker<Tuple<Specimen,Specimen>,Specimen[]> w = 
                    new Worker<Tuple<Specimen, Specimen>, Specimen[]>(combine);
                workers[i] = w;
                w.start();
            }

            logWorker = new Worker<Tuple<string, string>, bool>(writeLog);
            logWorker.start();

            dataWorker = new Worker<Tuple<string, string, int, string, string>, bool>(writeCandidates);
            dataWorker.start();
        }

        private Tuple<decimal, decimal, decimal> createCandidates()
        {
            int keepCount = Candidates.Count;
            List<Specimen> cand = new List<Specimen>();
            decimal[] oldFit = (from value in Candidates select value.Fit).ToArray();
            List<Tuple<Specimen,Specimen>> jobs = new List<Tuple<Specimen, Specimen>>();

            for (int i=0; i<Candidates.Count-1; i++) {
                Tuple<Specimen,Specimen> job = 
                    new Tuple<Specimen, Specimen>(Candidates[i],Candidates[i+1]);
                jobs.Add(job);
            }

            if (workers==null) {
                foreach (Tuple<Specimen,Specimen> job in jobs) {
                    Specimen[] results = combine(job);
                    cand.AddRange(results);
                }
            } else {
                int splitCount = workers.Length;
                int amount = jobs.Count/splitCount;
                int remainder = jobs.Count-(amount*splitCount);
                int[] amounts = new int[splitCount];

                for (int i=0; i<amounts.Length; i++) {
                    amounts[i] = amount;
                    if (remainder>0) {
                        amounts[i]++;
                        remainder--;
                    }
                } 

                List<List<Tuple<Specimen,Specimen>>> _jobs = new List<List<Tuple<Specimen, Specimen>>>();
               for (int i=0; i<amounts.Length; i++) {
                    List<Tuple<Specimen,Specimen>> __jobs = new List<Tuple<Specimen, Specimen>>();
                    for (int j=0; j<amounts[i]; j++) {
                        Tuple<Specimen,Specimen> k = jobs[0];
                        jobs.RemoveAt(0);
                        __jobs.Add(k);
                    }
                    
                    if (jobs.Count>0) {
                        __jobs.Add(jobs[0]);
                    } 
                    _jobs.Add(__jobs);
                }


                for (int i=0; i<workers.Length; i++) {
                    workers[i].addJobs(_jobs[i]);
                }   

                bool running;
                do {
                    running = false;
                    foreach (Worker<Tuple<Specimen,Specimen>,Specimen[]> worker in workers) {
                        if (worker.JobRunning) {
                            running=true;
                            Thread.Sleep(500);
                            break;
                        }
                    }
                } while(running);

                foreach (Worker<Tuple<Specimen,Specimen>,Specimen[]> worker in workers) {
                    while (worker.HasResults) {
                        List<Specimen[]> results = worker.GetResults();
                        if (results!=null) {
                            foreach(Specimen[] r in results) {
                                cand.AddRange(r);
                            }
                        } else {
                            throw new Exception("Error getting results");
                        }
                    }
                }
            }

            Specimen[] keep = (from value in cand where !Candidates.Contains(value) select value).ToArray();
            Candidates.AddRange(keep);
            Candidates.Sort();

            decimal[] fit = (from value in Candidates select value.Fit).ToArray();
            Candidates = (List<Specimen>)Candidates.Take(keepCount).ToList();

            Tuple<decimal,decimal,decimal> result = new Tuple<decimal, decimal, decimal>(
                fit[0],fit[fit.Length-1],fit.Average()-oldFit.Average()
            );
            return result;
        }

        private Specimen[] combine(Tuple<Specimen, Specimen> job)
        {
            List<Specimen> values = new List<Specimen>();
            Specimen[] children;
            children = job.Item1.combine(random, job.Item2);
            values.AddRange(children);

            children = job.Item2.combine(random, job.Item1);
            values.AddRange(children);

            return values.ToArray();
        }

        

        private Specimen createCandidate(Node[] value)
        {
            T specimen = (T)Activator.CreateInstance(typeof(T),value);
            return specimen;
        }

        // private void initNodes(List<Node> _nodes = null, int dimensions = 2, int count = 16)
        // {
        //     if (_nodes != null) {
        //         nodes = _nodes;
        //         return;
        //     }

        //     int min = 0;
        //     int max = 100;

        //     for (int i=min; i<max; i++) {
        //         List<int> pos = new List<int>();
        //         for (int j=0; j<dimensions; j++) {
        //             pos.Add(random.Next(min,max));
        //         }
        //         nodes.Add(new SpatialNode("Node" + i.ToString(), new Vector(pos)));
        //     }
        // }

        // private List<Specimen> readCand(string path, int gen) {
        //     List<Specimen> values = new List<Specimen>();
        //     genCount = gen;
        //     using (StreamReader sr = new StreamReader(path)) {
        //         string line;
        //         sr.ReadLine(); // header
        //         while ((line=sr.ReadLine())!=null) {
        //             string[] val = line.TrimEnd('\n').Split(',');
        //             if (int.Parse(val[0])==gen) {
        //                 object id = val[1];
        //                 decimal fitness = decimal.Parse(val[2]);
        //                 int mutationCount = int.Parse(val[3]);
        //                 object parentIdA = val[4];
        //                 object parentIdB = val[5];
        //                 List<SpatialNode> list = new List<SpatialNode>();
        //                 for (int i=6; i<val.Length; i++) {
        //                     list.Add(new SpatialNode(val[i]));
        //                 }
        //                 T specimen = (T)Activator.CreateInstance(typeof(T),list.ToArray(),id,parentIdA,parentIdB,mutationCount);
        //                 values.Add(specimen);
        //             }
        //         }
        //     }
        //     return values;
        // }

        
    }
}