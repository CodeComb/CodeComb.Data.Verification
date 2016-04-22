using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeComb.Data.Verification
{
    public class VerifyResult
    {
        public bool IsSuccess { get; set; }

        public string Information { get; set; } = "";

        public List<Rule> FailedRules { get; set; } = new List<Rule>();
    }
}
