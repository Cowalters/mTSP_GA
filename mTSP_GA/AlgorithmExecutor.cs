using System;
using System.Collections.Generic;
using System.Diagnostics;
using GAF;
using GAF.Extensions;
using GAF.Operators;

namespace mTSP_GA
{

    class AlgorithmExecutor
    {
        private static int NUMBER_OF_CITIES;
        private static int NUMBER_OF_DRONES;
        private static int NUMBER_OF_GENERATIONS;
        private static int POPULATION_SIZE;
        private static double CROSSOVER_PROBABILITY;
        private static double MUTATION_PROBABILITY;
        private static List<City> CITIES;
        private static City DEPOT;
        private static bool FITNESS_CONSIDERS_PARTITIONS;
        private static bool IS_CALIBRATION;
        private static Stopwatch STOPWATCH = new Stopwatch();
        private static double solution;
        private static double timeElapsed;
        private static int[] partitionPoints;
        private static Chromosome fittestChromosome;

        public AlgorithmExecutor(
            List<City> cities,
            City depot,
            int numberOfDrones,
            int numberOfGenerations,
            int populationSize,
            double crossoverProbability,
            double mutationProbability,
            bool fitnessConsidersPartitions,
            bool isCalibration)
        {
            CITIES = cities;
            DEPOT = depot;
            NUMBER_OF_CITIES = cities.Count;
            NUMBER_OF_DRONES = numberOfDrones;
            NUMBER_OF_GENERATIONS = numberOfGenerations;
            POPULATION_SIZE = populationSize;
            CROSSOVER_PROBABILITY = crossoverProbability;
            MUTATION_PROBABILITY = mutationProbability;
            FITNESS_CONSIDERS_PARTITIONS = fitnessConsidersPartitions;
            IS_CALIBRATION = isCalibration;
        }

        public void RunGAInstance()
        {
            // We can create an empty population as we will be creating the 
            // initial solutions manually.
            var population = new Population();

            // Create the chromosomes.
            for (var p = 0; p < POPULATION_SIZE; p++)
            {

                var chromosome = new Chromosome();
                for (var g = 0; g < NUMBER_OF_CITIES; g++)
                {
                    chromosome.Genes.Add(new Gene(g));
                }
                chromosome.Genes.ShuffleFast();
                population.Solutions.Add(chromosome);
            }

            // Create the elite operator.
            var elite = new Elite(5);

            // Create the crossover operator.
            var crossover = new Crossover(CROSSOVER_PROBABILITY)
            {
                CrossoverType = CrossoverType.DoublePointOrdered
            };

            // Create the mutation operator.
            var mutate = new SwapMutate(MUTATION_PROBABILITY);

            // Create the GA.
            var ga = new GeneticAlgorithm(population, CalculateFitness);

            // Hook up to some useful events.
            ga.OnGenerationComplete += ga_OnGenerationComplete;
            ga.OnRunComplete += ga_OnRunComplete;

            // Add the operators.
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutate);

            // Begin timing.
            STOPWATCH.Restart();

            // Run the GA.
            ga.Run(Terminate);
        }

        public static double getSolution()
        {
            return solution;
        }

        public static double getTimeElapsed()
        {
            return timeElapsed;
        }

        public static int[] getPartitionPoints()
        {
            return partitionPoints;
        }

        public static Chromosome getFittestChromosome()
        {
            return fittestChromosome;
        }

        static void ga_OnRunComplete(object sender, GaEventArgs e)
        {
            var fittest = getFittestChromosome(e.Population);

            // Call the algorithm to generate the mTSP solution.
            var greedy_mTSP = new GreedyAlgorithm(Utils.fittestToCitiesList(CITIES, fittest), DEPOT, NUMBER_OF_DRONES);
            solution = greedy_mTSP.solve().Item1;
            partitionPoints = greedy_mTSP.solve().Item2;
            timeElapsed = STOPWATCH.ElapsedMilliseconds;
            fittestChromosome = fittest;

            Console.WriteLine("Final solution (MinMax cost): {0}", solution);
            Console.WriteLine("Time elapsed: {0} ms", STOPWATCH.ElapsedMilliseconds);
        }

        // Callback for when a generation completes its iteration.
        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            // We print in the console the current results of the algorithm for every 100 generations
            // when not in calibration mode. When calibration, we print only for the last generation.
            if ((!IS_CALIBRATION && e.Generation % 100 == 0) || e.Generation == NUMBER_OF_GENERATIONS)
            {
                var fittest = getFittestChromosome(e.Population);
                var distanceToTravel = 0.0;

                if (!FITNESS_CONSIDERS_PARTITIONS)
                {
                    distanceToTravel = CalculateDistance(fittest);
                }
                else
                {
                    var greedy_mTSP =
                        new GreedyAlgorithm(Utils.fittestToCitiesList(CITIES, fittest), DEPOT, NUMBER_OF_DRONES);
                    distanceToTravel = greedy_mTSP.solve().Item1;
                }

                Console.WriteLine("Generation: {0}, Fitness: {1}, Distance: {2}", e.Generation, fittest.Fitness, distanceToTravel);
            }
        }

        // Returns the chromosome with higher fitness within a population.
        private static Chromosome getFittestChromosome(Population pop)
        {
            double minCost = Double.MaxValue;
            Chromosome fittest = new Chromosome();
            foreach (Chromosome c in pop.GetTopPercent(100))
            {
                double cost = CalculateDistance(c);
                if (cost < minCost)
                {
                    minCost = cost;
                    fittest = c;
                }
            }
            return fittest;
        }

        // Calculates the fitness of a chromosome.
        public static double CalculateFitness(Chromosome chromosome)
        {
            var distanceToTravel = 0.0;
            if (!FITNESS_CONSIDERS_PARTITIONS)
            {
                distanceToTravel = CalculateDistance(chromosome);
            }
            else
            {
                var greedy_mTSP =
                    new GreedyAlgorithm(Utils.fittestToCitiesList(CITIES, chromosome), DEPOT, NUMBER_OF_DRONES);
                distanceToTravel = greedy_mTSP.solve().Item1; 
            }
            return 1 - distanceToTravel / 10000;
        }

        // Calculates the distances involve in traveling through the cities in the sequence
        // contained in a given chromosome. It also includes traveling from and to the depot.
        private static double CalculateDistance(Chromosome chromosome)
        {
            var distanceToTravel = 0.0;
            City previousCity = null;

            // Run through each city in the order specified in the chromosome
            foreach (var gene in chromosome.Genes)
            {
                var currentCity = CITIES[(int)gene.RealValue];

                if (previousCity != null)
                {
                    var distance = previousCity.GetDistanceFromPosition(currentCity.xCoord, currentCity.yCoord);

                    distanceToTravel += distance;
                }
                else
                {
                    distanceToTravel += DEPOT.GetDistanceFromPosition(currentCity.xCoord, currentCity.yCoord);
                }
                previousCity = currentCity;
            }
            // Back to the depot distance
            distanceToTravel += DEPOT.GetDistanceFromPosition(previousCity.xCoord, previousCity.yCoord);

            return distanceToTravel;
        }

        // Criteria for stopping the GA.
        public static bool Terminate(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > NUMBER_OF_GENERATIONS;
        }
    }
}
