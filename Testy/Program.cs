using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vindinium;
using vindinium.NEAT.Mutation;
using vindinium.NEAT;
using vindinium.NEAT.Crossover;
using System.Threading;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            AddConnectionTest();
            AddNodeTest();
            CrossoverTest();

        }

        public static void CrossoverTest()
        {
            var NodeGens1 = new List<NodeGenesModel>() {
                new NodeGenesModel { NodeNumber=1, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 4, 5 }  },
                new NodeGenesModel { NodeNumber=2, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 5 }  },
                new NodeGenesModel { NodeNumber=3, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 4 }  },
                new NodeGenesModel { NodeNumber=4, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>() { 1, 5, 3 }, TargetNodes=new HashSet<int>()   },
                new NodeGenesModel { NodeNumber=5, FeedForwardValue=0, Type=NodeType.Output, SourceNodes=new HashSet<int>() { 2 }, TargetNodes=new HashSet<int>() { 4 } }
            };

            var NodeGens2 = new List<NodeGenesModel>()
            {
                 new NodeGenesModel { NodeNumber=1, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 4, 6 }  },
                new NodeGenesModel { NodeNumber=2, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 5 } },
                new NodeGenesModel { NodeNumber=3, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 5, 4} },
                new NodeGenesModel { NodeNumber=4, FeedForwardValue=0, Type=NodeType.Output, SourceNodes=new HashSet<int>() { 1, 6, 3 }, TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=5, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>() { 2, 3 }, TargetNodes=new HashSet<int>() { 6 }  },
                 new NodeGenesModel { NodeNumber=6, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>() { 1, 5 }, TargetNodes=new HashSet<int>() { 4 } },
            };

            var Connectionlist1 = new List<ConnectionGenesModel>()
            {
                new ConnectionGenesModel { InNode=1, OutNode=4, Innovation=1, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.7 },
                new ConnectionGenesModel { InNode=2, OutNode=4, Innovation=2, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=3, OutNode=4, Innovation=3, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=2, OutNode=5, Innovation=4, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.2 },
                new ConnectionGenesModel { InNode=5, OutNode=4, Innovation=5, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.4 },
                new ConnectionGenesModel { InNode=1, OutNode=5, Innovation=8, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 }
            };

            var Connectionlist2 = new List<ConnectionGenesModel>()
            {
                new ConnectionGenesModel { InNode=1, OutNode=4, Innovation=1, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.7 },
                new ConnectionGenesModel { InNode=2, OutNode=4, Innovation=2, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=3, OutNode=4, Innovation=3, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=2, OutNode=5, Innovation=4, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.2 },
                new ConnectionGenesModel { InNode=4, OutNode=5, Innovation=5, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.4 },
                new ConnectionGenesModel { InNode=5, OutNode=6, Innovation=6, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=6, OutNode=4, Innovation=7, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=3, OutNode=5, Innovation=9, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=1, OutNode=6, Innovation=10, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 }
            };

            var innovationList = new List<Innovations>();

            var genotype1 = new Genotype { GenomeConnection = new List<ConnectionGenesModel>(Connectionlist1), NodeGens = new List<NodeGenesModel>(NodeGens1), Value = 0 };

            var genotype2 = new Genotype { GenomeConnection = new List<ConnectionGenesModel>(Connectionlist2), NodeGens = new List<NodeGenesModel>(NodeGens2), Value = 0 };

            var CorrelationProvider = new CorrelationProvider();
            var CrossoverProvider = new CrossoverProvider(CorrelationProvider);
            var newGenotype = CrossoverProvider.CrossoverGenotype(genotype1, genotype2);

            WriteToFile(newGenotype, "Crossover.txt");
        }

        public static void AddNodeTest()
        {
            var NodeGens = new List<NodeGenesModel>() {
                new NodeGenesModel { NodeNumber=1, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=2, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=3, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=4, FeedForwardValue=0, Type=NodeType.Output, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=5, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  }
            };

            var Connectionlist = new List<ConnectionGenesModel>()
            {
                new ConnectionGenesModel { InNode=1, OutNode=4, Innovation=1, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.7 },
                new ConnectionGenesModel { InNode=2, OutNode=4, Innovation=2, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=3, OutNode=4, Innovation=3, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=2, OutNode=5, Innovation=4, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.2 },
                new ConnectionGenesModel { InNode=5, OutNode=4, Innovation=5, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.4 },
                new ConnectionGenesModel { InNode=1, OutNode=5, Innovation=6, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=4, OutNode=5, Innovation=7, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
            };

            var innovationList = new List<Innovations>();

            var genotyp1 = new Genotype(Connectionlist, NodeGens);

            var genotypeList = new List<Genotype>()
            {
                 new Genotype { GenomeConnection=new List<ConnectionGenesModel>(Connectionlist), NodeGens= new List<NodeGenesModel>(NodeGens), Value=0 }

            };



            var MutationProvider = new MutationProvider();

            for (int i = 0; i < 10; i++)
            {
                var newGenotyp = MutationProvider.MutateAddNode(genotypeList[i], innovationList);
                genotypeList.Add(
                    new Genotype()
                    {
                        GenomeConnection = new List<ConnectionGenesModel>(newGenotyp.GenomeConnection),
                        NodeGens = new List<NodeGenesModel>(newGenotyp.NodeGens),
                        Value = 0
                    }
                    );
                Thread.Sleep(100);
            }

            WriteToFile(genotypeList, "ConnectionTest.txt");
        }

        public static void AddConnectionTest()
        {
            var NodeGens = new List<NodeGenesModel>() {
                new NodeGenesModel { NodeNumber=1, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=2, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=3, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=4, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=5, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=6, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=7, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=8, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=9, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=10, FeedForwardValue=0, Type=NodeType.Output, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  }
            };

            var Connectionlist = new List<ConnectionGenesModel>()
            {
                new ConnectionGenesModel { InNode=1, OutNode=4, Innovation=1, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.7 },
                new ConnectionGenesModel { InNode=2, OutNode=4, Innovation=2, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=3, OutNode=4, Innovation=3, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=4, OutNode=5, Innovation=4, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.2 },
                new ConnectionGenesModel { InNode=4, OutNode=6, Innovation=5, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.4 },
                new ConnectionGenesModel { InNode=4, OutNode=7, Innovation=6, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=5, OutNode=8, Innovation=7, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=6, OutNode=10, Innovation=8, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=7, OutNode=9, Innovation=9, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=8, OutNode=10, Innovation=10, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=9, OutNode=10, Innovation=11, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 }

            };

            var innovationList = new List<Innovations>();

            // var genotyp1 = new Genotype(Connectionlist, NodeGens);

            var genotypeList = new List<Genotype>()
            {
                 new Genotype { GenomeConnection=new List<ConnectionGenesModel>(Connectionlist), NodeGens= new List<NodeGenesModel>(NodeGens), Value=0 }

            };



            var MutationProvider = new MutationProvider();



            for (int i = 0; i < 10; i++)
            {
                var newGenotyp = MutationProvider.MutateAddConnection(genotypeList[i], innovationList);
                genotypeList.Add(
                    new Genotype()
                    {
                        GenomeConnection = new List<ConnectionGenesModel>(newGenotyp.GenomeConnection),
                        NodeGens = new List<NodeGenesModel>(newGenotyp.NodeGens),
                        Value = 0
                    }
                    );
                Thread.Sleep(100);
            }

            WriteToFile(genotypeList, "NodeTest.txt");
        }

        public static void WriteToFile(List<Genotype> genotype, string nameFile)
        {
            List<string> lines = new List<string>();
            foreach (var el in genotype)
            {
                lines.Add("Nodes:");
                foreach (var itm in el.NodeGens)
                {
                    var line = "Number: " + itm.NodeNumber + ", Typ: " + itm.Type.ToString();
                    lines.Add(line);
                }
                lines.Add("Connections: ");
                foreach (var itm in el.GenomeConnection)
                {
                    var line = "Innovation: " + itm.Innovation + ", InNode: " + itm.InNode + ", OutNode: " + itm.OutNode + ", weight: " + itm.Weight + ", status: " + itm.Status;
                    lines.Add(line);
                }
                lines.Add("\n");

            }

            File.WriteAllLines(nameFile, lines.ToArray());
        }

        public static void WriteToFile(Genotype genotype, string nameFile)
        {
            List<string> lines = new List<string>();

            lines.Add("Nodes:");
            foreach (var itm in genotype.NodeGens)
            {
                var line = "Number: " + itm.NodeNumber + ", Typ: " + itm.Type.ToString();
                lines.Add(line);
            }
            lines.Add("Connections: ");
            foreach (var itm in genotype.GenomeConnection)
            {
                var line = "Innovation: " + itm.Innovation + ", InNode: " + itm.InNode + ", OutNode: " + itm.OutNode + ", weight: " + itm.Weight + ", status: " + itm.Status;
                lines.Add(line);
            }
            lines.Add("\n");



            File.WriteAllLines(nameFile, lines.ToArray());
        }
    }
}
