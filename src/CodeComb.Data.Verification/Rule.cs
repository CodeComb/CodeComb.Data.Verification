using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CodeComb.Data.Verification
{
    public class Rule
    {
        public RuleType Type { get; set; }

        public string Expression { get; set; }

        public int ArgumentIndex { get; set; } = 0;

        public ICollection<Rule> NestedRules { get; set; } = new List<Rule>();
    }
}
