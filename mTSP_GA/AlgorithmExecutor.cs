using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAF;
using GAF.Extensions;
using GAF.Operators;

namespace mTSP_GA
{

    class AlgorithmExecutor
    {
        private static int NUMBER_OF_CITIES;
        private static int NUMBER_OF_DRONES;
        private static int POPULATION_SIZE;
        private static double CROSSOVER_PROBABILITY;
        private static double MUTATION_PROBABILITY;
        private static List<City> CITIES;
        private static City HELIPORT;

        public AlgorithmExecutor(List<City> cities, int numberOfDrones, int populationSize,
            double crossoverProbability, double mutationProbability)
        {
            CITIES = cities;
            NUMBER_OF_CITIES = cities.Count;
            NUMBER_OF_DRONES = numberOfDrones;
            POPULATION_SIZE = populationSize;
            CROSSOVER_PROBABILITY = crossoverProbability;
            MUTATION_PROBABILITY = mutationProbability;
        }

        public void RunGAInstance()
        {
            HELIPORT = new City("Heliport", 0.0, 0.0);

            // Each city can be identified by an integer within the range 0-15
            // our chromosome is a special case as it needs to contain each city 
            // only once. Therefore, our chromosome will contain all the integers
            // between 0 and 15 with no duplicates.

            // We can create an empty population as we will be creating the 
            // initial solutions manually.
            var population = new Population();

            // Create the chromosomes
            for (var p = 0; p < POPULATION_SIZE; p++)
            {

                var chromosome = new Chromosome();
                for (var g = 0; g < NUMBER_OF_CITIES; g++)
                {
                    chromosome.Genes.Add(new Gene(g));
                    //chromosome.Genes.Add(new Gene(new List<int> (g)));
                }
                chromosome.Genes.ShuffleFast();
                population.Solutions.Add(chromosome);
            }

            // Create the elite operator
            var elite = new Elite(5);

            // Create the crossover operator
            var crossover = new Crossover(CROSSOVER_PROBABILITY)
            {
                CrossoverType = CrossoverType.DoublePointOrdered
            };

            // Create the mutation operator
            var mutate = new SwapMutate(MUTATION_PROBABILITY);

            // Create the GA
            var ga = new GeneticAlgorithm(population, CalculateFitness);

            //hook up to some useful events
            ga.OnGenerationComplete += ga_OnGenerationComplete;
            ga.OnRunComplete += ga_OnRunComplete;

            // Add the operators
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutate);

            // Run the GA
            ga.Run(Terminate);

            // Wait for user input to close the program.
            Console.ReadLine();
        }

        static void ga_OnRunComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            foreach (var gene in fittest.Genes)
            {
                Console.WriteLine(CITIES[(int)gene.RealValue].Name);
            }
            // Call the algorithm to generate the mTSP solution.
            // For now:
            // 1) var hillClimbing_mTSP = new HillClimbingAlgorithm(fittest, NUMBER_OF_DRONES);
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            var distanceToTravel = CalculateDistance(fittest);
            Console.WriteLine("Generation: {0}, Fitness: {1}, Distance: {2}", e.Generation, fittest.Fitness, distanceToTravel);
        }

        private static IEnumerable<City> CreateCities()
        {
            var cityGenerator = new CityGenerator(NUMBER_OF_CITIES);
            return cityGenerator.generatedCities;
        }

        public static double CalculateFitness(Chromosome chromosome)
        {
            var distanceToTravel = CalculateDistance(chromosome);
            return 1 - distanceToTravel / 10000;
        }

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
                    distanceToTravel += HELIPORT.GetDistanceFromPosition(currentCity.xCoord, currentCity.yCoord);
                }
                previousCity = currentCity;
            }
            // Back to the heliport distance
            distanceToTravel += HELIPORT.GetDistanceFromPosition(previousCity.xCoord, previousCity.yCoord);

            return distanceToTravel;
        }

        public static bool Terminate(Population population,
            int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > 400;
        }
    }
}
