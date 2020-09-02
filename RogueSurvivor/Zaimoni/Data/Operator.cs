using System.Collections.Generic;

#nullable enable

namespace Zaimoni.Data
{
    public interface Operator<in SRC,out OP> where OP:class
    {
        bool IsLegal();
        OP? Bind(SRC src);
    }

    internal interface BackwardPlan<OP>
    {
      List<OP>? prequel();
      Dictionary<OP,int>? backward();
    }
}
