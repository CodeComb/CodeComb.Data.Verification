using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeComb.Data.Verification
{
    public enum RuleType
    {
        And,
        Or,
        Not,
        Equal,
        NotEqual,
        Gte,
        Gt,
        Lte,
        Lt,
        Empty,
        NotEmpty,
        Regex
    }
}
