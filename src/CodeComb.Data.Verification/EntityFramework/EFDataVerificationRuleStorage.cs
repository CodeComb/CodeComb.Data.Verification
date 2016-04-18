using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace CodeComb.Data.Verification.EntityFramework
{
    public class EFDataVerificationRuleStorage<TContext> : DataVerificationRuleStorage
        where TContext: DbContext, IDataVerificationDbContext
    {
        private TContext db;

        public EFDataVerificationRuleStorage(IServiceProvider services)
        {
            db = services.GetRequiredService<TContext>();
        }

        public override ICollection<Rule> Get(Guid Id)
        {
            return db.DataVerificationRules
                .Where(x => x.Id == Id)
                .Single()
                .RuleObject;
        }

        public override Guid Insert(ICollection<Rule> Rules)
        {
            var rule = new DataVerificationRule
            {
                RuleJson = Newtonsoft.Json.JsonConvert.SerializeObject(Rules)
            };
            db.DataVerificationRules.Add(rule);
            db.SaveChanges();
            return rule.Id;
        }

        public override void Remove(Guid Id)
        {
            db.DataVerificationRules.Remove(db.DataVerificationRules.Single(x => x.Id == Id));
            db.SaveChanges();
        }

        public override void Update(ICollection<Rule> Rules, Guid Id)
        {
            var rule = db.DataVerificationRules.Single(x => x.Id == Id);
            rule.RuleJson = Newtonsoft.Json.JsonConvert.SerializeObject(Rules);
            db.SaveChanges();
        }
    }
}
