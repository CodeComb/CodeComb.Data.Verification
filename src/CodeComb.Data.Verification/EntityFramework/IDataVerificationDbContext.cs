using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;

namespace CodeComb.Data.Verification.EntityFramework
{
    public interface IDataVerificationDbContext
    {
        DbSet<DataVerificationRule> DataVerificationRules { get; set; }
        int SaveChanges();
    }
}
