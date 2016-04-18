using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using CodeComb.Data.Verification.EntityFramework;

namespace CodeComb.Data.Verification.Tests
{
    public class TestContext : DbContext, IDataVerificationDbContext
    {
        public DbSet<DataVerificationRule> DataVerificationRules { get; set; }
    }
}
