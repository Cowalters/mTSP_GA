using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mTSP_GA
{

    internal class Program
    {
        // Values for calibration.
        private static int NUMBER_OF_GENERATIONS_CALIBRATION_STRAT_1 = 1000;
        private static int NUMBER_OF_GENERATIONS_CALIBRATION_STRAT_2 = 200;
        private static int NUMBER_OF_TEST_CASES_CALIBRATION = 100;

        // Value for running a specific instance.
        private static int NUMBER_OF_GENERATIONS_STRAT_1 = 300000;
        private static int NUMBER_OF_GENERATIONS_STRAT_2 = 50000;

        // Variables that are part of a mTSP instance.
        private static City DEPOT;
        private static List<City> CITIES;
        private static int NUMBER_OF_DRONES;

        private static void Main(string[] args)
        {
            CITIES = new List<City>();

            Console.WriteLine("To run the calibration mode (100 random instances), press 'C' or 'c' and then ENTER.");
            Console.WriteLine("The results will then be stored as 'output_strat1.txt' and 'output_strat2.txt'.");
            Console.WriteLine("Otherwise, press just ENTER and the application will run the 3 sample instances.");

            // Read user input.
            string input = Console.ReadLine();

            if (input != "c" && input != "C")
            {
                List<string> inputFiles = new List<string>();
                // Add the input files.
                inputFiles.Add("eil51");
                inputFiles.Add("eil76");
                inputFiles.Add("rat99");

                for (int i = 0; i < inputFiles.Count; i++)
                {
                    string inputFile = inputFiles[i];

                    // Run GA with selected parameters three times: for 2, 3 and 5 travelers.

                    // Running for the case that fitness calculates the cost of TSP.
                    runGAWithParams(inputFile, 2, false, 200, 0.6, 0.1, NUMBER_OF_GENERATIONS_STRAT_1);
                    runGAWithParams(inputFile, 3, false, 200, 0.6, 0.1, NUMBER_OF_GENERATIONS_STRAT_1);
                    runGAWithParams(inputFile, 5, false, 200, 0.6, 0.1, NUMBER_OF_GENERATIONS_STRAT_1);

                    // Running for the case that fitness calculates the cost of mTSP
                    // using the greedy algorithm.
                    runGAWithParams(inputFile, 2, true, 200, 1, 0, NUMBER_OF_GENERATIONS_STRAT_2);
                    runGAWithParams(inputFile, 3, true, 200, 1, 0, NUMBER_OF_GENERATIONS_STRAT_2);
                    runGAWithParams(inputFile, 5, true, 200, 1, 0, NUMBER_OF_GENERATIONS_STRAT_2);
                }
            }
            else
            {
                DEPOT = new City("Depot", 0.0, 0.0);
                CITIES = Utils.CreateCities().ToList();

                // 100 test cases runs for both approaches.
                calibrateGA(false /* fitnessConsidersPartitions */, NUMBER_OF_GENERATIONS_CALIBRATION_STRAT_1);
                calibrateGA(true /* fitnessConsidersPartitions */, NUMBER_OF_GENERATIONS_CALIBRATION_STRAT_2);
            }

            Console.WriteLine("SIMULATION ENDED.");
            Console.ReadLine();
        }

        // Runs the GA with the given parameters.
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

            // Write the results from the simulation with the given number of drones
            // to the specified file.
            Utils.writeResultsToFile(fileName, numberOfDrones);

            CITIES.Clear();
        }

        // Runs several mTSP instances for several set of parameters. For each instance,
        // the set of parameters that provided the best solution is stored along with the number
        // of times it provided the best solution.
        private static void calibrateGA(bool fitnessConsidersPartitions, int numberOfGenerations)
        {
            double bestCost = double.MaxValue;
            int bestPop = -1;
            double bestMut = -1.0, bestCross = -1.0;
            int currPop;
            double currMut, currCross;

            var results = new Dictionary<Tuple<int, double, double>, int>();

            string outputFile = !fitnessConsidersPartitions ? "output_strat1.txt" : "output_strat2.txt";

            // Initializing the dictionary with the results stored from previous simulations.
            results = Utils.getStoredResults(outputFile);

            // Running the instances.
            for (int i = 0; i < NUMBER_OF_TEST_CASES_CALIBRATION; i++)
            {
                // Initializing values for costs and generating cities.
                bestCost = double.MaxValue;
                CITIES = Utils.CreateCities().ToList();

                // Generating a random value between 2 and 7 for the number of drones.
                Random rnd = new Random();
                NUMBER_OF_DRONES = rnd.Next(2, 8);

                // j is the step multiplier for the mutation values.
                for (int j = 0; j < 5; j++)
                {
                    currMut = j * 0.05;
                    //k is the step multiplier for the crossover values.
                    for (int k = 0; k < 6; k++)
                    {
                        currCross = k * 0.2;
                        // m is the step multiplier for the population values.
                        for (int m = 1; m <= 4; m++)
                        {
                            currPop = m * 50;

                            // With the parameters already set, we finally run the GA.
                            Console.WriteLine("Running test case {0}", i + 1);
                            AlgorithmExecutor executor = new AlgorithmExecutor(
                                CITIES, DEPOT, NUMBER_OF_DRONES, numberOfGenerations,
                                    currPop, currCross, currMut, fitnessConsidersPartitions, true /* isCalibration */);
                            executor.RunGAInstance();
                            
                            // Since the operation above is synchronous, we can access here the solution.
                            double currCost = AlgorithmExecutor.getSolution();

                            // If the current set of parameters provided the best solution so far,
                            // we keep it stored.
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

                // Assigning the best solution found for the current instance to its
                // corresponding set of parameters.
                Tuple<int, double, double> currKey = new Tuple<int, double, double>(bestPop, bestCross, bestMut);
                if (!results.ContainsKey(currKey))
                    results[currKey] = 1;
                else results[currKey]++;

                // Updating the calibration output file with the newfound result.
                Utils.updateCalibrationOutputFile(outputFile, results);
            }

            // After the calibration, print the results to the console.
            Utils.printResultsFromCalibration(results);
        }
    }
}
