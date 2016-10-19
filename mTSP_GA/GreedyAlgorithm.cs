using System;
using System.Collections.Generic;

namespace mTSP_GA
{
    class GreedyAlgorithm
    {
        private List<City> cities;
        private City depot;
        private int numberOfDrones; 

        public GreedyAlgorithm(List<City> TSPSequence, City depot, int numberOfDrones)
        {
            cities = TSPSequence;
            this.depot = depot;
            this.numberOfDrones = numberOfDrones;
        }

        public Tuple<double, int[]> solve()
        {
            int[] partitionPoints = new int[numberOfDrones - 1];
            initializePartitions(partitionPoints);

            int numberOfIterations = numberOfDrones*cities.Count;
            Tuple<int, double, double[]> currentDroneAndCost = calculateAllCosts(partitionPoints);
            int currDrone = currentDroneAndCost.Item1;
            double bestCost = currentDroneAndCost.Item2;
            double[] currCosts = currentDroneAndCost.Item3;
            int[] bestPartition = partitionPoints;

            //double cost = 0;
            //int bestClimber;
            //bool goUp = false;

            while (numberOfIterations > 0)
            {
                if(currDrone == 0)
                {
                    if (partitionPoints[0] > 0)
                    {
                        partitionPoints[0]--;
                    }
                }
                else if (currDrone == numberOfDrones - 1)
                {
                    if (partitionPoints[partitionPoints.Length - 1] < cities.Count - 2)
                    {
                        partitionPoints[partitionPoints.Length - 1]++;
                    }
                }
                else
                {
                    if (currCosts[currDrone + 1] > currCosts[currDrone - 1])
                    {
                        partitionPoints[currDrone - 1]++;
                    }
                    else
                    {
                        partitionPoints[currDrone]--;
                    }
                }

                currentDroneAndCost = calculateAllCosts(partitionPoints);
                currDrone = currentDroneAndCost.Item1;
                currCosts = currentDroneAndCost.Item3;
                if (currentDroneAndCost.Item2 < bestCost)
                {
                    Array.Copy(partitionPoints, bestPartition, partitionPoints.Length);
                    bestCost = currentDroneAndCost.Item2;
                }

                //bestClimber = -1;
                //for (int i = 0; i < partitionPoints.Length; ++i)
                //{
                //    if (partitionPoints[i] > 0
                //        && (i == 0 || partitionPoints[i] > partitionPoints[i - 1] + 1))
                //    {
                //        partitionPoints[i]--;
                //        double currentCost = calculateAllCosts(partitionPoints);
                //        if (currentCost < bestCost)
                //        {
                //            bestCost = currentCost;
                //            bestClimber = i;
                //            goUp = true;
                //            Console.WriteLine("BIZUROU");
                //        }
                //        partitionPoints[i]++;
                //    }
                //    if (partitionPoints[i] < cities.Count - 2
                //        && (i == partitionPoints.Length - 1 || partitionPoints[i] + 1 < partitionPoints[i + 1]))
                //    {
                //        partitionPoints[i]++;
                //        double currentCost = calculateAllCosts(partitionPoints);
                //        if (currentCost < bestCost)
                //        {
                //            bestCost = currentCost;
                //            bestClimber = i;
                //            goUp = false;
                //            Console.WriteLine("BIZUROU");
                //        }
                //        partitionPoints[i]--;
                //    }
                //}
                //if (bestClimber >= 0)
                //{
                //    Console.WriteLine("CLIMBOU: {0}", partitionPoints[bestClimber]);
                //    partitionPoints[bestClimber] += (goUp ? 1 : -1);
                //    Console.WriteLine("PARA: {0}", partitionPoints[bestClimber]);
                //}
                //cost = bestCost;
                numberOfIterations--;
            }

            //Console.WriteLine("PARTITIONS");

            for (int i = 0; i < partitionPoints.Length; ++i)
            {
                //Console.WriteLine("Partition {0}: {1}", i + 1, partitionPoints[i]); 
            }

            //Console.WriteLine("Final solution (MinMax cost): {0}", bestCost);

            return new Tuple<double, int[]> (bestCost, bestPartition);
        }

        private void initializePartitions(int[] partitionPoints)
        {
            for (int i = 1; i <= numberOfDrones - 1; ++i)
            {
                partitionPoints[i - 1] = (i * cities.Count / numberOfDrones - 1);
            }
        }

        private Tuple<int, double, double[]> calculateAllCosts(int[] partitionPoints)
        {
            double[] costForDrone = new double[numberOfDrones];

            int droneId = 0;
            int nextPartition = partitionPoints[droneId];
            City previousCity = depot;
            for (int i = 0; i < cities.Count; ++i)
            {
                if (i != nextPartition)
                {
                    costForDrone[droneId] += cities[i].GetDistanceFromPosition(previousCity.xCoord, previousCity.yCoord);
                    previousCity = cities[i];
                }
                else
                {
                    costForDrone[droneId] += cities[i].GetDistanceFromPosition(previousCity.xCoord, previousCity.yCoord);
                    costForDrone[droneId] += depot.GetDistanceFromPosition(cities[i].xCoord, cities[i].yCoord);
                    droneId++;

                    if (droneId >= numberOfDrones - 1)
                        nextPartition = cities.Count - 1;
                    else nextPartition = partitionPoints[droneId];
                    previousCity = depot;
                }
            }

            for (int i = 0; i < costForDrone.Length; ++i)
            {
                //Console.WriteLine("Cost for drone {0}: {1}", i + 1, costForDrone[i]);
            }

            return maxDroneCost(costForDrone);
        }

        private Tuple<int, double, double[]> maxDroneCost(double[] costForDrone)
        {
            int maxDrone = -1;
            double maxCost = 0;
            for (int i = 0; i < costForDrone.Length; ++i)
            {
                if (costForDrone[i] > maxCost)
                {
                    maxCost = costForDrone[i];
                    maxDrone = i;
                }
            }
            return new Tuple<int, double, double[]> (maxDrone, maxCost, costForDrone);
        }
    }
}
