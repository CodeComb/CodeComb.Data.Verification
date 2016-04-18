using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace CodeComb.Data.Verification.EntityFramework
{
    public class DataVerificationRule
    {
        public Guid Id { get; set; }

        public string RuleJson { get; set; }

        [NotMapped]
        private ICollection<Verification.Rule> ruleObject;

        [NotMapped]
        public ICollection<Verification.Rule> RuleObject
        {
            get
            {
                if (ruleObject == null)
                    ruleObject = JsonConvert.DeserializeObject<ICollection<Verification.Rule>>(RuleJson);
                return ruleObject;
            }
            set
            {
                ruleObject = value;
            }
        }

        public void SaveRules()
        {
            RuleJson = JsonConvert.SerializeObject(ruleObject);
        }
    }
}
