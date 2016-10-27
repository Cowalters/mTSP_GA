using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GAF;

namespace mTSP_GA
{
    class Utils
    {
        public Utils() { }

        // Generates 20 to 50 random cities.
        public static IEnumerable<City> CreateCities()
        {
            Random rnd = new Random();
            var cityGenerator = new CityGenerator(rnd.Next(20, 51));
            return cityGenerator.generatedCities;
        }

        // Write the results from the GA to the specified file.
        public static void writeResultsToFile(string fileName, int numberOfDrones)
        {
            string output = "Number of travelers: " +
                numberOfDrones.ToString() + Environment.NewLine +
                "Solution: " +
                AlgorithmExecutor.getSolution().ToString() + Environment.NewLine +
                "Time elapsed: " + AlgorithmExecutor.getTimeElapsed().ToString() + "ms" + Environment.NewLine +
                "Chromosome sequence: " + chromosomeToString(AlgorithmExecutor.getFittestChromosome()) + Environment.NewLine +
                "Partition points: " + partitionPointsToString(AlgorithmExecutor.getPartitionPoints()) + Environment.NewLine +
                Environment.NewLine;

            File.AppendAllText(fileName + "_solution.txt", output);
        }

        // Get the results already stored to populate the dictionary.
        public static Dictionary<Tuple<int, double, double>, int> getStoredResults(string outputFile)
        {
            var results = new Dictionary<Tuple<int, double, double>, int>();

            var lines = File.ReadAllLines(outputFile);
            foreach (var line in lines.Take(lines.Count() - 1))
            {
                string[] values = line.Split(' ');
                Tuple<int, double, double> key = new Tuple<int, double, double>(
                    int.Parse(values[0]), double.Parse(values[1]), double.Parse(values[2]));
                results[key] = int.Parse(values[3]);
            }
            return results;
        }

        // After an instance has been completely ran in the calibration mode, this function is
        // called and updates the calibration output file.
        public static void updateCalibrationOutputFile(
            string outputFile,
            Dictionary<Tuple<int, double, double>, int> results)
        {
            string newOutput = "";
            int numberOfTestCases = 0;
            foreach (KeyValuePair<Tuple<int, double, double>, int> entry in results)
            {
                if (results[entry.Key] > 0)
                {
                    numberOfTestCases += entry.Value;
                    Console.WriteLine("({0}, {1}, {2}): {3}",
                        entry.Key.Item1, entry.Key.Item2, entry.Key.Item3, entry.Value);
                    newOutput +=
                    entry.Key.Item1.ToString() + " " +
                    entry.Key.Item2.ToString() + " " +
                    entry.Key.Item3.ToString() + " " +
                    entry.Value.ToString() + Environment.NewLine;
                }
            }
            newOutput += "Number of test cases: " + numberOfTestCases.ToString() + Environment.NewLine;
            File.WriteAllText(outputFile, newOutput);
        }

        // Prints the results from the calibration when all the instances have been run.
        public static void printResultsFromCalibration(Dictionary<Tuple<int, double, double>, int> results)
        {
            foreach (KeyValuePair<Tuple<int, double, double>, int> entry in results)
            {
                if (results[entry.Key] > 0)
                {
                    Console.WriteLine("({0}, {1}, {2}): {3}", entry.Key.Item1, entry.Key.Item2, entry.Key.Item3, entry.Value);
                }
            }
        }

        // Converts a chromosome to its respective sequence of cities' indexes.
        public static string chromosomeToString(Chromosome chromosome)
        {
            string str = "";
            foreach (var gene in chromosome.Genes)
            {
                str += ((int)gene.RealValue).ToString() + " ";
            }
            return str;
        }

        // Converts an array of integers to a string.
        public static string partitionPointsToString(int[] partitionPoints)
        {
            string str = "";
            for (int i = 0; i < partitionPoints.Length; i++)
            {
                str += (partitionPoints[i]).ToString() + " ";
            }
            return str;
        }

        // Converts a chromosome into a list of cities.
        public static List<City> fittestToCitiesList(List<City> cities, Chromosome fittest)
        {
            List<City> TSPSequence = new List<City>();

            foreach (var gene in fittest.Genes)
            {
                TSPSequence.Add(cities[(int)gene.RealValue]);
            }

            return TSPSequence;
        }
    }
}
