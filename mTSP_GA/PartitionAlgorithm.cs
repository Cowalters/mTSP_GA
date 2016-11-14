using System;
using System.Collections.Generic;

namespace mTSP_GA
{
    public abstract class PartitionAlgorithm
    {
        protected List<City> cities;
        protected City depot;
        protected int numberOfDrones;

        public PartitionAlgorithm(List<City> TSPSequence, City depot, int numberOfDrones)
        {
            cities = TSPSequence;
            this.depot = depot;
            this.numberOfDrones = numberOfDrones;
        }

        // Method that returns a double containing the highest cost of the best solution and an 
        // array containing the partition points that provided it.
        public virtual Tuple<double, int[]> solve()
        {
            return new Tuple<double, int[]>(0, new int[0]);
        }

        // Method to initialize the array with the partition points.
        public virtual void initializePartitions(int[] partitionPoints) { }
    }
}
