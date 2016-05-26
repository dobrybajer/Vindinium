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
using Testy;

namespace Test
{
    class Program
    {
        public static Genotype CurrentModel { get; set; } 

        static void Main(string[] args)
        {
            // MutationAddConnecionTest();
            //Genotype();
            //Comp();
            // AddConnectionTest();
            Genotype();
            AddNodeTest();
         
            Comp();
            //CrossoverTest();

        }

        public static void CrossoverTest()
        {
            var NodeGens1 = new List<NodeGenesModel>() {
                new NodeGenesModel { NodeNumber=0, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 3, 4 }  },
                new NodeGenesModel { NodeNumber=1, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 4 }  },
                new NodeGenesModel { NodeNumber=2, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 3 }  },
                new NodeGenesModel { NodeNumber=3, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>() { 0, 4, 2 }, TargetNodes=new HashSet<int>()   },
                new NodeGenesModel { NodeNumber=4, FeedForwardValue=0, Type=NodeType.Output, SourceNodes=new HashSet<int>() { 1 }, TargetNodes=new HashSet<int>() { 3 } }
            };

            var NodeGens2 = new List<NodeGenesModel>()
            {
                 new NodeGenesModel { NodeNumber=0, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 3, 5 }  },
                new NodeGenesModel { NodeNumber=1, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 4 } },
                new NodeGenesModel { NodeNumber=2, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 4, 3} },
                new NodeGenesModel { NodeNumber=3, FeedForwardValue=0, Type=NodeType.Output, SourceNodes=new HashSet<int>() { 0, 5, 2 }, TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=4, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>() { 1, 2 }, TargetNodes=new HashSet<int>() { 5 }  },
                 new NodeGenesModel { NodeNumber=5, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>() { 0, 4 }, TargetNodes=new HashSet<int>() { 3 } },
            };

            var Connectionlist1 = new List<ConnectionGenesModel>()
            {
                new ConnectionGenesModel { InNode=0, OutNode=3, Innovation=1, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.7 },
                new ConnectionGenesModel { InNode=1, OutNode=3, Innovation=2, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=2, OutNode=3, Innovation=3, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=1, OutNode=4, Innovation=4, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.2 },
                new ConnectionGenesModel { InNode=4, OutNode=3, Innovation=5, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.4 },
                new ConnectionGenesModel { InNode=0, OutNode=4, Innovation=8, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 }
            };

            var Connectionlist2 = new List<ConnectionGenesModel>()
            {
                new ConnectionGenesModel { InNode=0, OutNode=3, Innovation=1, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.7 },
                new ConnectionGenesModel { InNode=1, OutNode=3, Innovation=2, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=2, OutNode=3, Innovation=3, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=1, OutNode=4, Innovation=4, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.2 },
                new ConnectionGenesModel { InNode=3, OutNode=4, Innovation=5, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.4 },
                new ConnectionGenesModel { InNode=4, OutNode=5, Innovation=6, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=5, OutNode=3, Innovation=7, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=2, OutNode=4, Innovation=9, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=0, OutNode=5, Innovation=10, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 }
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
         new NodeGenesModel { NodeNumber=0, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 3, 4 }  },
                new NodeGenesModel { NodeNumber=1, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 4 }  },
                new NodeGenesModel { NodeNumber=2, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 3 }  },
                new NodeGenesModel { NodeNumber=3, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>() { 0, 4, 2 }, TargetNodes=new HashSet<int>()   },
                new NodeGenesModel { NodeNumber=4, FeedForwardValue=0, Type=NodeType.Output, SourceNodes=new HashSet<int>() { 1 }, TargetNodes=new HashSet<int>() { 3 } }
            };

            var Connectionlist = new List<ConnectionGenesModel>()
            {
                new ConnectionGenesModel { InNode=0, OutNode=3, Innovation=1, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.7 },
                new ConnectionGenesModel { InNode=1, OutNode=3, Innovation=2, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=2, OutNode=3, Innovation=3, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=1, OutNode=4, Innovation=4, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.2 },
                new ConnectionGenesModel { InNode=4, OutNode=3, Innovation=5, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.4 },
                new ConnectionGenesModel { InNode=0, OutNode=4, Innovation=6, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
               
            };

            var innovationList = new List<Innovations>() ;

            var genotyp1 = new Genotype(Connectionlist, NodeGens);

            var genotypeList = new List<Genotype>()
            {
                 new Genotype { GenomeConnection=new List<ConnectionGenesModel>(Connectionlist), NodeGens= new List<NodeGenesModel>(NodeGens), Value=0 }

            };



            var MutationProvider = new MutationProvider();

            //for (int i = 0; i < 10; i++)
            //{
            //CurrentModel = MutationProvider.MutateAddNode(CurrentModel, ref innovationList);
            CurrentModel = MutationProvider.MutateDeleteConnection(CurrentModel);
            //genotypeList.Add(
            //    new Genotype()
            //    {
            //        GenomeConnection = new List<ConnectionGenesModel>(newGenotyp.GenomeConnection),
            //        NodeGens = new List<NodeGenesModel>(newGenotyp.NodeGens),
            //        Value = 0
            //    }
            //    );
            //Thread.Sleep(100);
            // }

            WriteToFile(genotypeList, "ConnectionTest.txt");
        }

        public static void AddConnectionTest()
        {
            var NodeGens = new List<NodeGenesModel>() {
                new NodeGenesModel { NodeNumber=0, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=1, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=2, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=3, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=4, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=5, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=6, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=7, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=8, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  },
                new NodeGenesModel { NodeNumber=9, FeedForwardValue=0, Type=NodeType.Output, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>()  }
            };

            var Connectionlist = new List<ConnectionGenesModel>()
            {
                new ConnectionGenesModel { InNode=0, OutNode=3, Innovation=1, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.7 },
                new ConnectionGenesModel { InNode=1, OutNode=3, Innovation=2, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=2, OutNode=3, Innovation=3, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=3, OutNode=4, Innovation=4, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.2 },
                new ConnectionGenesModel { InNode=3, OutNode=5, Innovation=5, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.4 },
                new ConnectionGenesModel { InNode=3, OutNode=6, Innovation=6, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=4, OutNode=7, Innovation=7, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=5, OutNode=9, Innovation=8, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=6, OutNode=8, Innovation=9, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=7, OutNode=9, Innovation=10, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 },
                new ConnectionGenesModel { InNode=8, OutNode=9, Innovation=11, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 }

            };

            var innovationList = new List<Innovations>() { new Innovations { InNode = 9, OutNode = 7, InnovationNumber = 0 } };

            // var genotyp1 = new Genotype(Connectionlist, NodeGens);

            var genotypeList = new List<Genotype>()
            {
                 new Genotype { GenomeConnection=new List<ConnectionGenesModel>(Connectionlist), NodeGens= new List<NodeGenesModel>(NodeGens), Value=0 }

            };



            var MutationProvider = new MutationProvider();



            for (int i = 0; i < 10; i++)
            {
                var newGenotyp = MutationProvider.MutateAddConnection(genotypeList[i],ref innovationList);
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

        public static void MutationAddConnecionTest()
        {
            var NodeGens1 = new List<NodeGenesModel>() {
                new NodeGenesModel { NodeNumber=0, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 3, 4 }  },
                new NodeGenesModel { NodeNumber=1, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 4 }  },
                new NodeGenesModel { NodeNumber=2, FeedForwardValue=0, Type=NodeType.Input, SourceNodes=new HashSet<int>(), TargetNodes=new HashSet<int>() { 3 }  },
                new NodeGenesModel { NodeNumber=3, FeedForwardValue=0, Type=NodeType.Hidden, SourceNodes=new HashSet<int>() { 0, 4, 2 }, TargetNodes=new HashSet<int>()   },
                new NodeGenesModel { NodeNumber=4, FeedForwardValue=0, Type=NodeType.Output, SourceNodes=new HashSet<int>() { 1 }, TargetNodes=new HashSet<int>() { 3 } }
            };

            var Connectionlist1 = new List<ConnectionGenesModel>()
            {
                new ConnectionGenesModel { InNode=0, OutNode=3, Innovation=1, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.7 },
                new ConnectionGenesModel { InNode=1, OutNode=3, Innovation=2, IsMutated=false, Status=ConnectionStatus.Disabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=2, OutNode=3, Innovation=3, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.5 },
                new ConnectionGenesModel { InNode=1, OutNode=4, Innovation=4, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.2 },
                new ConnectionGenesModel { InNode=4, OutNode=3, Innovation=5, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.4 },
                new ConnectionGenesModel { InNode=0, OutNode=4, Innovation=6, IsMutated=false, Status=ConnectionStatus.Enabled, Weight=0.6 }
            };


            var innovationList = new List<Innovations>
            {
                new Innovations(1, 0, 3),
                new Innovations(2, 1, 3),
                new Innovations(3, 2, 3),
                new Innovations(4, 1, 4),
                new Innovations(5, 4, 3),
                new Innovations(6, 0, 4)
            };

            var genotype1 = new Genotype { GenomeConnection = new List<ConnectionGenesModel>(Connectionlist1), NodeGens = new List<NodeGenesModel>(NodeGens1), Value = 0 };

            var mutationProvider = new MutationProvider
            {
                RandomGenerator = new MockRandom {ValueToReturn = new[] {2, 4}}
            };
            var newGenotype = mutationProvider.MutateAddConnection(genotype1, ref innovationList);
            WriteToFile(newGenotype, "ttt.txt");
        }

        public static void Comp()
        {
            //var input = MapBoardToNeuralNetworskInput();
            var input = new List<double>() { 1, 1 };

            var nextLevelNodes = new List<Tuple<int, int, double>>();
            var inputNodes = CurrentModel.NodeGens.Where(n => n.Type == NodeType.Input).ToList();

            foreach (var sn in inputNodes)
            {
                sn.FeedForwardValue = input[sn.NodeNumber];
                nextLevelNodes.AddRange(sn.TargetNodes.Select(en => new Tuple<int, int, double>(sn.NodeNumber, en, sn.FeedForwardValue)));
            }

            while (nextLevelNodes.Any())
            {
                var currentLevelNodes = new List<Tuple<int, int, double>>(nextLevelNodes);
                nextLevelNodes.Clear();

                foreach (var en in currentLevelNodes)
                {
                    var node = CurrentModel.GetNodeById(en.Item2);
                    var edge = CurrentModel.GetEnabledConnectionByIds(en.Item1, en.Item2);
                    if (node == null || edge == null) throw new ArgumentOutOfRangeException();

                    node.FeedForwardCount++;
                    node.FeedForwardValue += edge.Weight * en.Item3;
                }

                foreach (var sn in currentLevelNodes.Select(n => n.Item2).Distinct())
                {
                    var node = CurrentModel.GetNodeById(sn);
                    nextLevelNodes.AddRange(from nn in node.TargetNodes where node.FeedForwardCount == node.SourceNodes.Count select new Tuple<int, int, double>(sn, nn, node.FeedForwardValue));
                }
            }

            var outputNodes = CurrentModel.NodeGens.Where(n => n.Type == NodeType.Output).ToList();
            // var output = MapNeuralNetowrkOutputToMove(outputNodes);

            //Console.Out.WriteLine($"Direction: {output}");
            //ClearGenome();
            var o = 0;
            
        }

        public static void Genotype()
        {
            var genotype = new Genotype
            {
                NodeGens = new List<NodeGenesModel>(),
                GenomeConnection = new List<ConnectionGenesModel>()
            };

            // ---- INPUT ----

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 0,
                SourceNodes = new HashSet<int>(),
                TargetNodes = new HashSet<int> { 2 },
                Type = NodeType.Input
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 1,
                SourceNodes = new HashSet<int>(),
                TargetNodes = new HashSet<int> { 3 },
                Type = NodeType.Input
            });
            

            // ---- HIDDEN ----

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 2,
                SourceNodes = new HashSet<int> { 0,3 },
                TargetNodes = new HashSet<int> { 4 },
                Type = NodeType.Hidden
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 3,
                SourceNodes = new HashSet<int> { 1 },
                TargetNodes = new HashSet<int> { 2,4 },
                Type = NodeType.Hidden
            });

         

            // ---- OUTPUT ----

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 4,
                SourceNodes = new HashSet<int> { 2,3},
                TargetNodes = new HashSet<int>(),
                Type = NodeType.Output
            });

         

            // ---- EDGES ----

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 0,
                OutNode = 2,
                Status = ConnectionStatus.Enabled,
                Weight = 1
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode =1,
                OutNode = 3,
                Status = ConnectionStatus.Enabled,
                Weight = 2
            });

            //genotype.GenomeConnection.Add(new ConnectionGenesModel
            //{
            //    InNode = 2,
            //    OutNode = 3,
            //    Status = ConnectionStatus.Enabled,
            //    Weight = 1
            //});

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 3,
                OutNode = 2,
                Status = ConnectionStatus.Enabled,
                Weight = 2
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 2,
                OutNode = 4,
                Status = ConnectionStatus.Enabled,
                Weight = 1
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 3,
                OutNode = 4,
                Status = ConnectionStatus.Enabled,
                Weight = 2
            });


            CurrentModel = genotype;
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
                    line += ", SourceNodes: ";
                    foreach (var it in itm.SourceNodes)
                        line += " " + it;
                    line += ", TargetNodes: ";
                    foreach (var it in itm.TargetNodes)
                        line += " " + it;

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
