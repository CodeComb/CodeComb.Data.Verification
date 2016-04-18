using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.Extensions.PlatformAbstractions;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Newtonsoft.Json;

namespace CodeComb.Data.Verification
{
    public class DataVerificationRuleManager
    {
        public DataVerificationRuleStorage Storage;

        public DataVerificationRuleManager(DataVerificationRuleStorage Storage)
        {
            this.Storage = Storage;
        }

        public bool Verify(Guid RuleId, params string[] Text)
        {
            var rules = Storage.Get(RuleId);
            var flag = true;
            foreach (var x in rules)
            {
                if (!LiftTest(x, Text))
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        private bool LiftTest(Rule r, params string[] Text)
        {
            bool flag;
            if (r.ArgumentIndex >= Text.Length)
                return false;
            switch (r.Type)
            {
                case RuleType.And:
                    flag = true;
                    foreach (var x in r.NestedRules)
                    {
                        if (!LiftTest(x, Text))
                        {
                            flag = false;
                            break;
                        }
                    }
                    return flag;
                case RuleType.Or:
                    flag = false;
                    foreach (var x in r.NestedRules)
                    {
                        if (LiftTest(x, Text))
                        {
                            flag = true;
                            break;
                        }
                    }
                    return flag;
                case RuleType.Not:
                    flag = true;
                    foreach (var x in r.NestedRules)
                    {
                        if (LiftTest(x, Text))
                        {
                            flag = false;
                            break;
                        }
                    }
                    return flag;
                case RuleType.Empty:
                    return string.IsNullOrWhiteSpace(Text[r.ArgumentIndex]);
                case RuleType.NotEmpty:
                    return !string.IsNullOrWhiteSpace(Text[r.ArgumentIndex]);
                case RuleType.Regex:
                    var regex = new Regex(r.Expression);
                    return regex.IsMatch(Text[r.ArgumentIndex]);
                case RuleType.Equal:
                    return Text[r.ArgumentIndex] == r.Expression;
                case RuleType.NotEqual:
                    return Text[r.ArgumentIndex] != r.Expression;
                case RuleType.Gte:
                    if (Text[r.ArgumentIndex].IndexOf('.') > 0 || r.Expression.IndexOf('.') > 0)
                    {
                        var a = Convert.ToDouble(Text[r.ArgumentIndex]);
                        var b = Convert.ToDouble(r.Expression);
                        return a >= b;
                    }
                    else
                    {
                        var a = Convert.ToInt64(Text[r.ArgumentIndex]);
                        var b = Convert.ToInt64(r.Expression);
                        return a >= b;
                    }
                case RuleType.Gt:
                    if (Text[r.ArgumentIndex].IndexOf('.') > 0 || r.Expression.IndexOf('.') > 0)
                    {
                        var a = Convert.ToDouble(Text[r.ArgumentIndex]);
                        var b = Convert.ToDouble(r.Expression);
                        return a > b;
                    }
                    else
                    {
                        var a = Convert.ToInt64(Text[r.ArgumentIndex]);
                        var b = Convert.ToInt64(r.Expression);
                        return a > b;
                    }
                case RuleType.Lte:
                    if (Text[r.ArgumentIndex].IndexOf('.') > 0 || r.Expression.IndexOf('.') > 0)
                    {
                        var a = Convert.ToDouble(Text[r.ArgumentIndex]);
                        var b = Convert.ToDouble(r.Expression);
                        return a <= b;
                    }
                    else
                    {
                        var a = Convert.ToInt64(Text[r.ArgumentIndex]);
                        var b = Convert.ToInt64(r.Expression);
                        return a <= b;
                    }
                case RuleType.Lt:
                    if (Text[r.ArgumentIndex].IndexOf('.') > 0 || r.Expression.IndexOf('.') > 0)
                    {
                        var a = Convert.ToDouble(Text[r.ArgumentIndex]);
                        var b = Convert.ToDouble(r.Expression);
                        return a < b;
                    }
                    else
                    {
                        var a = Convert.ToInt64(Text[r.ArgumentIndex]);
                        var b = Convert.ToInt64(r.Expression);
                        return a < b;
                    }
                default:
                    return false;
            }
        }
    }
}
