# mTSP_GA

C# project using GAF for solving instances of the Multiple Traveling Salesman Problem (mTSP).

There are two heuristics created and used for solving the problem. The first one (strategy 1) uses a GA that evaluates the best TSP solution and, after the specified number of generations, uses a greedy algorithm with the best chromosome to solve the problem for multiple salesmen. The second one (strategy 2) evaluates the best mTSP solution (using the greedy algorithm for that purpose).

There are two main modes for this application:
- Calibration mode, which runs 100 instances and stores the best set of the GA parameters found for both strategies in the `output_strat1.txt` and `output_strat2.txt`
- Solving mode, which runs sample instances (eil51, eil76 and rat99 are given as samples) with hard-coded parameters, also storing the results found in a text file

The sample instances were retrieved from the TSPLIB.
