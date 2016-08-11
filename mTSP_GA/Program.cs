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

        private static void Main(string[] args)
        {
            List<City> cities = CreateCities().ToList();
            AlgorithmExecutor executor = new AlgorithmExecutor(cities, NUMBER_OF_DRONES, 100, 0.8, 0.02);
            executor.RunGAInstance();
        }

        private static IEnumerable<City> CreateCities()
        {
            var cityGenerator = new CityGenerator(NUMBER_OF_CITIES);
            return cityGenerator.generatedCities;
        }
    }
}
