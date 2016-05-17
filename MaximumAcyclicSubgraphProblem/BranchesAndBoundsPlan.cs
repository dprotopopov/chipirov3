using System.Collections.Generic;
using System.Text;

namespace MaximumAcyclicSubgraphProblem
{
    public class BranchesAndBoundsPlan
    {
        public Dictionary<int, bool> bools = new Dictionary<int, bool>();
        public double MinPrice;
        public double MaxPrice;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in bools)
            {
                sb.AppendFormat("{0}:{1} ", pair.Key, pair.Value);
            }
            sb.AppendFormat("MinPrice={0} ", MinPrice);
            sb.AppendFormat("MaxPrice={0} ", MaxPrice);
            return sb.ToString();
        }
    }
}