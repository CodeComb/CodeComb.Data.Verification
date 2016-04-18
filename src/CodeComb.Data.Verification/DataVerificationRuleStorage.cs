using System;
using System.Collections.Generic;

namespace CodeComb.Data.Verification
{
    public abstract class DataVerificationRuleStorage
    {
        public abstract Guid Insert(ICollection<Rule> Rules);

        public abstract ICollection<Rule> Get(Guid Id);

        public abstract void Remove(Guid Id);

        public abstract void Update(ICollection<Rule> Rules, Guid id);
    }
}
