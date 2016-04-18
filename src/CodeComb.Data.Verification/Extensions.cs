using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using CodeComb.Data.Verification;
using CodeComb.Data.Verification.EntityFramework;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DataVerificationBuilder
    {
        public IServiceCollection services;
    }

    public static class Extensions
    {
        public static DataVerificationBuilder AddDataVerification(this IServiceCollection self)
        {
            var dvb = new DataVerificationBuilder { services = self };
            dvb.services.AddScoped<DataVerificationRuleManager>();
            return dvb;
        }

        public static DataVerificationBuilder AddEntityFrameworkStores<TContext>(this DataVerificationBuilder self)
            where TContext : DbContext, IDataVerificationDbContext
        {
            self.services.AddScoped<DataVerificationRuleStorage, EFDataVerificationRuleStorage<TContext>>();
            return self;
        }
    }
}
