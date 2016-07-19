using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mTSP_GA
{
    class CityGenerator
    {
        public CityGenerator(int nCities)
        {
            possibleNames = new List<string>();
            generatedCities = new List<City>();
            buildDictionary();
            generateCities(nCities);
        }

        public List<City> generatedCities { get; set; }
        public List<string> possibleNames;

        private void buildDictionary()
        {
            possibleNames.Add("Curitiba1");
            possibleNames.Add("Curitiba2");
            possibleNames.Add("Curitiba3");
            possibleNames.Add("Curitiba4");
            possibleNames.Add("Curitiba5");
            possibleNames.Add("Curitiba6");
            possibleNames.Add("Curitiba7");
            possibleNames.Add("Curitiba8");
            possibleNames.Add("Curitiba9");
            possibleNames.Add("Curitiba10");
            possibleNames.Add("Curitiba11");
            possibleNames.Add("Curitiba12");
            possibleNames.Add("Curitiba13");
            possibleNames.Add("Curitiba14");
            possibleNames.Add("Curitiba15");
            possibleNames.Add("Curitiba16");
        }

        private void generateCities(int nCities)
        {
            Random rnd = new Random();
            for (int i = 0; i < nCities; i++)
            {
                int rndX = rnd.Next(-20000, 20000);
                int rndY = rnd.Next(-20000, 20000);
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
