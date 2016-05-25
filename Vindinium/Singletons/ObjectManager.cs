using System.IO;
using Newtonsoft.Json;

namespace vindinium.Singletons
{
    public static class ObjectManager
    {
        /// <summary>
        /// Writes the given object instance to a Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="fileName">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToJsonFile<T>(string fileName, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite);
                var path = Parameters.DefaultPathToWrittenFiles + fileName;
                writer = new StreamWriter(path, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                writer?.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="fileName">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the Json file.</returns>
        public static T ReadFromJsonFile<T>(string fileName) where T : new()
        {
            TextReader reader = null;
            try
            {
                var path = Parameters.DefaultPathToWrittenFiles + fileName;
                reader = new StreamReader(path);
                var fileContents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContents);
            }
            finally
            {
                reader?.Close();
            }
        }

        public static bool FileExist(int generationNumber, int populationCount, string map, string activationFunction, uint turns)
        {
            return File.Exists(Parameters.DefaultPathToWrittenFiles + "generation" + generationNumber + "_populationCount" + populationCount + "_" + map + "_activationFunction" + activationFunction + "_turns" + turns + ".txt");
        }

        public static void WriteGenerationToFile<T>(T objectToWrite, int generationNumber, int populationCount, string map, string activationFunction, uint turns) where T : new()
        {
            WriteToJsonFile("generation" + generationNumber + "_populationCount" + populationCount + "_" + map + "_activationFunction" + activationFunction + "_turns" + turns + ".txt", objectToWrite);
        }

        public static T ReadGenerationFromFile<T>(int generationNumber, int populationCount, string map, string activationFunction, uint turns) where T : new()
        {
            return ReadFromJsonFile<T>("generation" + generationNumber + "_populationCount" + populationCount + "_" + map + "_activationFunction" + activationFunction + "_turns" + turns + ".txt");
        }
    }
}
