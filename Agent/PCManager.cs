using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public  class PCManager
    {
        private readonly string baseDirectory;
        private PCManager() 
        {
            this.baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        private static PCManager instance;

        private static object instanceLock = new object();

        public static PCManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        lock (instanceLock)
                        {
                            instance = new PCManager();
                        }
                    }
                }

                return instance;
            }
        }

        public string Combine(string path)
        {
            return Path.Combine(baseDirectory, path);
        }

        public string GetBaseDirectory => this.baseDirectory;
    }
}
