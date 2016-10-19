using System;
using System.Collections.Generic;

namespace mTSP_GA
{
    class CityGenerator
    {
        public CityGenerator(int nCities)
        {
            possibleNames = new List<string>();
            generatedCities = new List<City>();
            buildDictionary(nCities);
            generateCities(nCities);
        }

        public List<City> generatedCities { get; set; }
        public List<string> possibleNames;

        private void buildDictionary(int nCities)
        {
            for (int i = 0; i < nCities; i++)
            {
                possibleNames.Add("City " + (i + 1).ToString());
            }
        }

        private void generateCities(int nCities)
        {
            Random rnd = new Random();
            for (int i = 0; i < nCities; i++)
            {
                int rndX = rnd.Next(-10000, 10000);
                int rndY = rnd.Next(-10000, 10000);
                generatedCities.Add(new City(generateName(), (double)rndX / 100, (double)rndY / 100));
                Console.WriteLine("City {0}: {1}, {2}", i + 1, (double)rndX / 100, (double)rndY / 100);
            }
        }

        private string generateName()
        {
            Random rnd = new Random();
            if(possibleNames.Count == 0)
            {
                Console.WriteLine("No more cities available.");
                return null;
            }
            int randomInt = rnd.Next(0, possibleNames.Count);
            string generatedName = possibleNames[randomInt];
            possibleNames.Remove(generatedName);
            return generatedName;
        }
    }
}
