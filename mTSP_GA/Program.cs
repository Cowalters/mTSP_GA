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

    internal class Program
    {
        private static int NUMBER_OF_CITIES = 16;
        private static int NUMBER_OF_DRONES = 4;
        private static List<City> _cities;
        private static City heliport;

        private static void Main(string[] args)
        {
            heliport = new City("Heliport", 0.0, 0.0);
            //get our cities
            _cities = CreateCities().ToList();

            //Each city can be identified by an integer within the range 0-15
            //our chromosome is a special case as it needs to contain each city 
            //only once. Therefore, our chromosome will contain all the integers
            //between 0 and 15 with no duplicates.

            //We can create an empty population as we will be creating the 
            //initial solutions manually.
            var population = new Population();

            //create the chromosomes
            for (var p = 0; p < 100; p++)
            {

                var chromosome = new Chromosome();
                for (var g = 0; g < NUMBER_OF_CITIES; g++)
                {
                    //chromosome.Genes.Add(new Gene(g));
                    chromosome.Genes.Add(new Gene(new List<int> (g)));
                }
                chromosome.Genes.ShuffleFast();
                population.Solutions.Add(chromosome);
            }

            //create the elite operator
            var elite = new Elite(5);

            //create the crossover operator
            var crossover = new Crossover(0.8)
            {
                CrossoverType = CrossoverType.DoublePointOrdered
            };

            //create the mutation operator
            var mutate = new SwapMutate(0.02);

            //create the GA
            var ga = new GeneticAlgorithm(population, CalculateFitness);

            //hook up to some useful events
            ga.OnGenerationComplete += ga_OnGenerationComplete;
            ga.OnRunComplete += ga_OnRunComplete;

            //add the operators
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutate);

            //run the GA
            ga.Run(Terminate);
            Console.ReadLine();
        }

        static void ga_OnRunComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            foreach (var gene in fittest.Genes)
            {
                Console.WriteLine(_cities[(int)gene.RealValue].Name);
            }
            // Call the each algorithm to generate solution.
            // For now:
            // 1) var hillClimbing_mTSP = new HillClimbingAlgorithm(fittest, NUMBER_OF_DRONES);
            // 2) var multiChromosomeTechnique_mTSP = new MultiChromosomeTechnique(fittest, NUMBER_OF_DRONES);
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

            //run through each city in the order specified in the chromosome
            foreach (var gene in chromosome.Genes)
            {
                var currentCity = _cities[(int)gene.RealValue];

                if (previousCity != null)
                {
                    var distance = previousCity.GetDistanceFromPosition(currentCity.xCoord, currentCity.yCoord);

                    distanceToTravel += distance;
                }
                else
                {
                    distanceToTravel += heliport.GetDistanceFromPosition(currentCity.xCoord, currentCity.yCoord);
                }
                previousCity = currentCity;
            }
            // Back to the heliport distance
            distanceToTravel += heliport.GetDistanceFromPosition(previousCity.xCoord, previousCity.yCoord);

            return distanceToTravel;
        }

        public static bool Terminate(Population population,
            int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > 400;
        }
    }
}
