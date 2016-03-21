using System.Collections.Generic;

namespace TravelingSalesmanProblem
{
    public class BranchesAndBoundsPlan
    {
        public Dictionary<int, bool> bools = new Dictionary<int, bool>();
        public int MaxCount;
        public double MaxPrice;
        public int MinCount;
        public double MinPrice;
    }
}