using System.Collections.Generic;

namespace KnapsackProblem
{
    public class BranchesAndBoundsPlan
    {
        public Dictionary<int, bool> bools = new Dictionary<int, bool>();
        public double MinWeight;
        public double MaxWeight;
        public double MinPrice;
        public double MaxPrice;
    }
}