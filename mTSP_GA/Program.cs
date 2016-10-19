using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GAF;

namespace mTSP_GA
{

    internal class Program
    {
        // Values for calibration
        private static int NUMBER_OF_GENERATIONS_CALIBRATION = 1000;
        private static int NUMBER_OF_TEST_CASES_CALIBRATION = 100;

        // Value for running a specific instance
        private static int NUMBER_OF_GENERATIONS_STRAT_1 = 300000;
        private static int NUMBER_OF_GENERATIONS_STRAT_2 = 50000;

        private static City DEPOT;
        private static List<City> CITIES;
        private static int NUMBER_OF_DRONES;

        private static void Main(string[] args)
        {
            CITIES = new List<City>();
            if (!true)
            {
                List<string> inputFiles = new List<string>();
                inputFiles.Add("eil51");
                inputFiles.Add("eil76");
                //inputFiles.Add("rat99");

                for (int i = 0; i < inputFiles.Count; i++)
                {
                    string inputFile = inputFiles[i];
                    //string inputFile = "input" + (i + 1).ToString();
                    // Run GA with selected parameters three times: for 2, 3 and 5 travelers

                    // Running for the case that fitness calculates the cost of TSP
                    runGAWithParams(inputFile, 2, false, 1000, 0.2, 0.1, NUMBER_OF_GENERATIONS_STRAT_1);
                    runGAWithParams(inputFile, 3, false, 1000, 0.2, 0.1, NUMBER_OF_GENERATIONS_STRAT_1);
                    runGAWithParams(inputFile, 5, false, 1000, 0.2, 0.1, NUMBER_OF_GENERATIONS_STRAT_1);

                    // Running for the case that fitness calculates the cost of mTSP
                    // using the greedy algorithm
                    runGAWithParams(inputFile, 2, true, 400, 1, 0, NUMBER_OF_GENERATIONS_STRAT_2);
                    runGAWithParams(inputFile, 3, true, 400, 1, 0, NUMBER_OF_GENERATIONS_STRAT_2);
                    runGAWithParams(inputFile, 5, true, 400, 1, 0, NUMBER_OF_GENERATIONS_STRAT_2);
                }
            }
            else
            {
                DEPOT = new City("Depot", 0.0, 0.0);
                CITIES = CreateCities().ToList();

                // 100 test cases runs for both approaches
                calibrateGA(false /* fitnessConsidersPartitions */);
                calibrateGA(true /* fitnessConsidersPartitions */);
            }

            Console.WriteLine("SIMULATION ENDED.");
            Console.ReadLine();
        }

        private static void runGAWithParams(
            string fileName,
            int numberOfDrones,
            bool fitnessConsidersPartitions,
            int population,
            double crossover,
            double mutation,
            int numberOfGenerations)
        {
            NUMBER_OF_DRONES = numberOfDrones;
            var lines = File.ReadAllLines(fileName + ".txt");
            foreach (var line in lines)
            {
                string[] coords = line.Split(' ');
                if (coords[0].Equals("1"))
                {
                    DEPOT = new City("Depot", double.Parse(coords[1]), double.Parse(coords[2]));
                }
                else
                {
                    CITIES.Add(new City("City " + coords[0], double.Parse(coords[1]), double.Parse(coords[2])));
                }
            }

            AlgorithmExecutor executor = new AlgorithmExecutor(
                CITIES, DEPOT, NUMBER_OF_DRONES, numberOfGenerations, population, crossover, mutation,
                    fitnessConsidersPartitions, false /* isCalibration */);
            executor.RunGAInstance();

            string output = "Number of travelers: " +
                numberOfDrones.ToString() + Environment.NewLine +
                "Solution: " +
                AlgorithmExecutor.getSolution().ToString() + Environment.NewLine +
                "Time elapsed: " + AlgorithmExecutor.getTimeElapsed().ToString() + "ms" + Environment.NewLine +
                "Chromosome sequence: " + printChromosome(AlgorithmExecutor.getFittestChromosome()) + Environment.NewLine +
                "Partition points: " + printPartitionPoints(AlgorithmExecutor.getPartitionPoints()) + Environment.NewLine +
                Environment.NewLine;

            File.AppendAllText(fileName + "_solution.txt", output);

            CITIES.Clear();
        }

        private static void calibrateGA(bool fitnessConsidersPartitions)
        {
            double bestCost = double.MaxValue;
            int bestPop = -1;
            double bestMut = -1.0, bestCross = -1.0;
            int currPop;
            double currMut, currCross;

            var results = new Dictionary<Tuple<int, double, double>, int>();

            //var lastLine = File.ReadAllLines("output.txt").Last();
            //string[] valuesFromLastLine = lastLine.Split(' ');
            //Console.WriteLine("NUMBER OF TESTS READ: {0}", int.Parse(valuesFromLastLine[4]));
            //Console.ReadLine();

            string outputFile = "";
            if (!fitnessConsidersPartitions)
            {
                outputFile = "output_strat1.txt";
            }
            else
            {
                outputFile = "output_strat2.txt";
            }

            var lines = File.ReadAllLines(outputFile);
            foreach (var line in lines.Take(lines.Count() - 1))
            {
                string[] values = line.Split(' ');
                Tuple<int, double, double> key = new Tuple<int, double, double>(
                    int.Parse(values[0]), double.Parse(values[1]), double.Parse(values[2]));
                results[key] = int.Parse(values[3]);
            }

            for (int i = 0; i < NUMBER_OF_TEST_CASES_CALIBRATION; i++)
            {
                bestCost = double.MaxValue;
                CITIES = CreateCities().ToList();

                Random rnd = new Random();
                NUMBER_OF_DRONES = rnd.Next(2, 8);

                for (int j = 0; j < 5; j++)
                {
                    currMut = j * 0.05;
                    for (int k = 0; k < 6; k++)
                    {
                        currCross = k * 0.2;
                        for (int m = 1; m <= 4; m++)
                        {
                            currPop = m * 50;
                            Console.WriteLine("Running test case {0}", i + 1);
                            AlgorithmExecutor executor = new AlgorithmExecutor(
                                CITIES, DEPOT, NUMBER_OF_DRONES, NUMBER_OF_GENERATIONS_CALIBRATION,
                                    currPop, currCross, currMut, fitnessConsidersPartitions, true /* isCalibration */);
                            executor.RunGAInstance();
                            double currCost = AlgorithmExecutor.getSolution();

                            if (currCost < bestCost)
                            {
                                bestCost = currCost;
                                bestMut = currMut;
                                bestCross = currCross;
                                bestPop = currPop;
                            }
                        }
                    }
                }

                Tuple<int, double, double> currKey = new Tuple<int, double, double>(bestPop, bestCross, bestMut);
                if (!results.ContainsKey(currKey))
                    results[currKey] = 1;
                else results[currKey]++;

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
            foreach (KeyValuePair<Tuple<int, double, double>, int> entry in results)
            {
                if (results[entry.Key] > 0)
                {
                    Console.WriteLine("({0}, {1}, {2}): {3}", entry.Key.Item1, entry.Key.Item2, entry.Key.Item3, entry.Value);
                }
            }
        }

        private static IEnumerable<City> CreateCities()
        {
            Random rnd = new Random();
            var cityGenerator = new CityGenerator(rnd.Next(20, 51));
            return cityGenerator.generatedCities;
        }

        private static string printChromosome(Chromosome chromosome)
        {
            string str = "";
            foreach (var gene in chromosome.Genes)
            {
                str += ((int)gene.RealValue).ToString() + " ";
            }
            return str;
        }

        private static string printPartitionPoints(int[] partitionPoints)
        {
            string str = "";
            for (int i = 0; i < partitionPoints.Length; i++)
            {
                str += (partitionPoints[i]).ToString() + " ";
            }
            return str;
        }
    }
}
