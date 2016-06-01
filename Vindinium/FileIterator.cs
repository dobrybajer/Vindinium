using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using vindinium.NEAT;
using ObjectManager = vindinium.Singletons.ObjectManager;

namespace vindinium
{
    public class FileIterator
    {
        private Dictionary<string, List<string>> filesPerMapDictionary = new Dictionary<string, List<string>>
        {
            {"m1", new List<string>() },
            {"m2", new List<string>() },
            {"m3", new List<string>() },
            {"m4", new List<string>() },
            {"m5", new List<string>() },
            {"m6", new List<string>() },
        };
        public Dictionary<string, List<string>> Interate(string trainingResultsPath)
        {
            var files = Directory.GetFiles(trainingResultsPath, "*.txt", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var parts = file.Split('_');
                filesPerMapDictionary[parts[2]].Add(file);
            }

            return filesPerMapDictionary;
        }
    }

    public class BestGenerationBuilder
    {
        public Dictionary<string, Genotype> FilesPerMapDictionary = new Dictionary<string, Genotype>
        {
            {"m1", null },
            {"m2", null },
            {"m3", null },
            {"m4", null },
            {"m5", null },
            {"m6", null },
        };

        private readonly FileIterator _fileIterator = new FileIterator();

        public void Build()
        {
            var filesPerMap = _fileIterator.Interate("..\\..\\CreatedObjects");
            foreach (var key in filesPerMap.Keys)
            {
                var genotypesList = new List<Genotype>();
                foreach (var filePath in filesPerMap[key])
                    genotypesList.AddRange(ObjectManager.ReadFromJsonFileWithoutDefaultPath<List<Genotype>>(filePath));
                if (genotypesList.Any())
                    FilesPerMapDictionary[key] = genotypesList.OrderBy(g => g.Value).Last();
            }
        }
    }
}
