
using System;
using System.Threading;
using System.Collections.Generic;

namespace TravelingSalesmanCSharp.Concrete {
public class Worker<T1,T2>
    {

        public bool IsRunning {get; private set;} = false;
        Func<T1,T2> Function {get;set;}

        object jobLock = new Object();
        List<List<T1>> Jobs {get;set;} = new List<List<T1>>();
        
        public bool HasResults {
            get {
                bool value;
                lock(resultLock) {
                    value = Results.Count>0;
                }
                return value;
            }
        }
        object resultLock = new Object();
        List<List<T2>> Results {get;set;} = new List<List<T2>>();

        int JobsCompleted {get;set;} = 0;

        object jobrunningLock = new Object();
        public bool JobRunning {
            get {
                bool value;
                lock(jobrunningLock) {
                    value = _JobRunning;
                }
                return value;
            }
            private set {
                lock(jobrunningLock) {
                    _JobRunning = value;
                }
            }
        }
        private bool _JobRunning = false; 

        Thread thread;

        public Worker(Func<T1,T2> function)
        {
            this.Function = function;
            thread = new Thread(new ThreadStart(run));
        }

        public void start() {
            if (!IsRunning) {
                IsRunning = true;
                thread.Start();
            }
        }

        public void stop() {
            IsRunning=false;
        }

        private void run() {
            bool hasJobs = false;
            while (IsRunning) {
                lock(jobLock) {
                    hasJobs = Jobs.Count>0;
                }
                if (hasJobs) {
                    JobRunning=true;
                    lock(jobLock) {
                        while (Jobs.Count>0) {
                            
                            List<T1> job = Jobs[0];
                            List<T2> result = new List<T2>();
                            Jobs.RemoveAt(0);
                            foreach(T1 j in job) {
                                result.Add(this.Function(j));
                            }
                            lock(resultLock) {
                                Results.Add(result);
                                JobsCompleted += 1;
                            }
                        }
                        JobRunning=false;
                    }
                } else {
                    Thread.Sleep(500);
                }
            }
        }

        
        public void addJobs(List<T1> jobs) {
            lock (jobLock) {
                this.Jobs.Add(jobs);
                JobRunning=true;
            }
        }
        public void addJobs(T1 job) {
            lock (jobLock) {
                List<T1> _job = new List<T1>(1);
                _job.Add(job);
                this.Jobs.Add(_job);
                JobRunning=true;
            }
        }

        public List<T2> GetResults() {
            if (Results.Count==0) return null;
            List<T2> value = null;
            lock(resultLock) {
                value = Results[0];
                Results.RemoveAt(0);
            }
            return value;
        }
        
    }
}