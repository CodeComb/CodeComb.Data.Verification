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

        public VerifyResult Verify(Guid RuleId, params string[] Text)
        {
            var rules = Storage.Get(RuleId);
            var verifyInfo = new VerifyResult { IsSuccess = true };
            foreach (var x in rules)
            {
                var result = LiftTest(x, Text);
                if (!result.IsSuccess)
                {
                    verifyInfo.Information += result.Information;
                    verifyInfo.FailedRules.AddRange(result.FailedRules);
                    verifyInfo.IsSuccess = false;
                    break;
                }
            }
            return verifyInfo;
        }

        private VerifyResult LiftTest(Rule r, params string[] Text)
        {
            bool flag;
            if (r.ArgumentIndex >= Text.Length)
                return new VerifyResult { IsSuccess = false, Information = "校验时索引超出最大参数值\r\n" };
            switch (r.Type)
            {
                case RuleType.And:
                    flag = true;
                    foreach (var x in r.NestedRules)
                    {
                        var result = LiftTest(x, Text);
                        if (!result.IsSuccess)
                        {
                            flag = false;
                            result.FailedRules.Add(x);
                            return new VerifyResult { IsSuccess = false, Information = result.Information + "该字段未能通过且逻辑校验\r\n", FailedRules = result.FailedRules };
                        }
                    }
                    return new VerifyResult { IsSuccess = true };
                case RuleType.Or:
                    flag = false;
                    var results = new List<VerifyResult>();
                    foreach (var x in r.NestedRules)
                    {
                        var result = LiftTest(x, Text);
                        results.Add(result);
                        if (result.IsSuccess)
                        {
                            flag = true;
                            return new VerifyResult { IsSuccess = true };
                        }
                    }
                    var information = "";
                    var rules = new List<Rule>();
                    foreach(var res in results)
                    {
                        information += res.Information;
                        rules.AddRange(res.FailedRules);
                    }
                    information += "该字段未能通过或逻辑校验\r\n";
                    return new VerifyResult { IsSuccess = false, Information = information, FailedRules = rules };
                case RuleType.Not:
                    flag = true;
                    foreach (var x in r.NestedRules)
                    {
                        var result = LiftTest(x, Text);
                        if (result.IsSuccess)
                        {
                            flag = false;
                            return new VerifyResult { IsSuccess = false, Information = "该字段未能通过非逻辑校验\r\n", FailedRules = new List<Rule> { r } };
                        }
                    }
                    return new VerifyResult { IsSuccess = true };
                case RuleType.Empty:
                    if (string.IsNullOrWhiteSpace(Text[r.ArgumentIndex]))
                        return new VerifyResult { IsSuccess = true };
                    else
                        return new VerifyResult { IsSuccess = false, Information = "该字段未能通过空逻辑校验\r\n", FailedRules = new List<Rule> { r } };
                case RuleType.NotEmpty:
                    if (!string.IsNullOrWhiteSpace(Text[r.ArgumentIndex]))
                        return new VerifyResult { IsSuccess = true };
                    else
                        return new VerifyResult { IsSuccess = false, Information = "该字段未能通过非空逻辑校验\r\n", FailedRules = new List<Rule> { r } };
                case RuleType.Regex:
                    var regex = new Regex(r.Expression);
                    if (regex.IsMatch(Text[r.ArgumentIndex]))
                        return new VerifyResult { IsSuccess = true };
                    else
                        return new VerifyResult { IsSuccess = false, Information = "该字段未能通过正则表达式校验\r\n", FailedRules = new List<Rule> { r } };
                case RuleType.Equal:
                    if (Text[r.ArgumentIndex] == r.Expression)
                        return new VerifyResult { IsSuccess = true };
                    else
                        return new VerifyResult { IsSuccess = false, Information = "该字段未能通过相等校验\r\n", FailedRules = new List<Rule> { r } };
                case RuleType.NotEqual:
                    if (Text[r.ArgumentIndex] != r.Expression)
                        return new VerifyResult { IsSuccess = true };
                    else
                        return new VerifyResult { IsSuccess = false, Information = "该字段未能通过不相等校验\r\n", FailedRules = new List<Rule> { r } };
                case RuleType.Gte:
                    if (Text[r.ArgumentIndex].IndexOf('.') > 0 || r.Expression.IndexOf('.') > 0)
                    {
                        var a = Convert.ToDouble(Text[r.ArgumentIndex]);
                        var b = Convert.ToDouble(r.Expression);
                        flag = a >= b;
                    }
                    else
                    {
                        var a = Convert.ToInt64(Text[r.ArgumentIndex]);
                        var b = Convert.ToInt64(r.Expression);
                        flag = a >= b;
                    }
                    if (flag)
                        return new VerifyResult { IsSuccess = true };
                    else
                        return new VerifyResult { IsSuccess = false, Information = "该字段未能通过大于等于校验\r\n", FailedRules = new List<Rule> { r } };
                case RuleType.Gt:
                    if (Text[r.ArgumentIndex].IndexOf('.') > 0 || r.Expression.IndexOf('.') > 0)
                    {
                        var a = Convert.ToDouble(Text[r.ArgumentIndex]);
                        var b = Convert.ToDouble(r.Expression);
                        flag = a > b;
                    }
                    else
                    {
                        var a = Convert.ToInt64(Text[r.ArgumentIndex]);
                        var b = Convert.ToInt64(r.Expression);
                        flag = a > b;
                    }
                    if (flag)
                        return new VerifyResult { IsSuccess = true };
                    else
                        return new VerifyResult { IsSuccess = false, Information = "该字段未能通过大于校验\r\n", FailedRules = new List<Rule> { r } };
                case RuleType.Lte:
                    if (Text[r.ArgumentIndex].IndexOf('.') > 0 || r.Expression.IndexOf('.') > 0)
                    {
                        var a = Convert.ToDouble(Text[r.ArgumentIndex]);
                        var b = Convert.ToDouble(r.Expression);
                        flag = a <= b;
                    }
                    else
                    {
                        var a = Convert.ToInt64(Text[r.ArgumentIndex]);
                        var b = Convert.ToInt64(r.Expression);
                        flag = a <= b;
                    }
                    if (flag)
                        return new VerifyResult { IsSuccess = true };
                    else
                        return new VerifyResult { IsSuccess = false, Information = "该字段未能通过小于等于校验\r\n", FailedRules = new List<Rule> { r } };
                case RuleType.Lt:
                    if (Text[r.ArgumentIndex].IndexOf('.') > 0 || r.Expression.IndexOf('.') > 0)
                    {
                        var a = Convert.ToDouble(Text[r.ArgumentIndex]);
                        var b = Convert.ToDouble(r.Expression);
                        flag = a < b;
                    }
                    else
                    {
                        var a = Convert.ToInt64(Text[r.ArgumentIndex]);
                        var b = Convert.ToInt64(r.Expression);
                        flag = a < b;
                    }
                    if (flag)
                        return new VerifyResult { IsSuccess = true };
                    else
                        return new VerifyResult { IsSuccess = false, Information = "该字段未能通过小于校验\r\n", FailedRules = new List<Rule> { r } };
                default:
                    return new VerifyResult { Information = "未知错误", IsSuccess = false };
            }
        }
    }
}
