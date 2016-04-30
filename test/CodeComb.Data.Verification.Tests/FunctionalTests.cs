using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Entity;
using Xunit;

namespace CodeComb.Data.Verification.Tests
{
    public class FunctionalTests
    {
        public TestContext db;
        public DataVerificationRuleManager dvm;

        public FunctionalTests()
        {
            var collection = new ServiceCollection();

            collection.AddEntityFramework()
                .AddDbContext<TestContext>(x => x.UseInMemoryDatabase())
                .AddInMemoryDatabase();

            collection.AddDataVerification()
                .AddEntityFrameworkStores<TestContext>();

            var services = collection.BuildServiceProvider();

            dvm = services.GetRequiredService<DataVerificationRuleManager>();
            db = services.GetRequiredService<TestContext>();
        }
        
        [Theory]
        [InlineData("Amamiya", "Yuuko", false)]
        [InlineData("Yuuko", "Yuuko", true)]
        [InlineData("Hi", "Yuuko", false)]
        public void equals_tests(string str1, string str2, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.Equal, Expression = str1}
            });

            Assert.Equal(res, dvm.Verify(id, str2).IsSuccess);
        }

        [Fact]
        public void not_equals_tests()
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.NotEqual, Expression = "Amamiya"},
                new Rule { Type = RuleType.NotEqual, Expression = "Yuuko"}
            });

            Assert.True(dvm.Verify(id, "Hello").IsSuccess);
        }

        [Theory]
        [InlineData("Hello", false)]
        [InlineData("", true)]
        [InlineData("  ", true)]
        public void empty_tests(string str, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.Empty }
            });

            Assert.Equal(res, dvm.Verify(id, str).IsSuccess);
        }

        [Theory]
        [InlineData("Hello", true)]
        [InlineData("", false)]
        [InlineData("  ", false)]
        public void not_empty_tests(string str, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.NotEmpty }
            });

            Assert.Equal(res, dvm.Verify(id, str).IsSuccess);
        }

        [Theory]
        [InlineData("Amamiya", true)]
        [InlineData("Yuuko", true)]
        [InlineData("Code Comb", false)]
        public void or_tests(string str, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.Or, NestedRules = new List<Rule>
                {
                    new Rule { Type = RuleType.Equal, Expression = "Amamiya" },
                    new Rule { Type = RuleType.Equal, Expression = "Yuuko" }
                } }
            });
            
            Assert.Equal(res, dvm.Verify(id, str).IsSuccess);
        }

        [Theory]
        [InlineData(10.8, true)]
        [InlineData(10, true)]
        [InlineData(10.0, true)]
        [InlineData(9.999, false)]
        public void gte_tests(object number, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.Gte, Expression = "10" }
            });

            Assert.Equal(res, dvm.Verify(id, number.ToString()).IsSuccess);
        }

        [Theory]
        [InlineData(10.8, true)]
        [InlineData(10, false)]
        [InlineData(10.0, false)]
        [InlineData(9.999, false)]
        public void gt_tests(object number, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.Gt, Expression = "10" }
            });

            Assert.Equal(res, dvm.Verify(id, number.ToString()).IsSuccess);
        }

        [Theory]
        [InlineData(10.8, false)]
        [InlineData(10, true)]
        [InlineData(10.0, true)]
        [InlineData(9.999, true)]
        public void lte_tests(object number, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.Lte, Expression = "10" }
            });

            Assert.Equal(res, dvm.Verify(id, number.ToString()).IsSuccess);
        }

        [Theory]
        [InlineData(10.8, false)]
        [InlineData(10, false)]
        [InlineData(10.0, false)]
        [InlineData(9.999, true)]
        public void lt_tests(object number, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.Lt, Expression = "10" }
            });

            Assert.Equal(res, dvm.Verify(id, number.ToString()).IsSuccess);
        }

        [Theory]
        [InlineData("www.codecomb.com", false)]
        [InlineData("1@codecomb.com", true)]
        [InlineData("@", false)]
        public void regex_tests(string str, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.Regex, Expression = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$" }
            });

            Assert.Equal(res, dvm.Verify(id, str).IsSuccess);
        }

        [Theory]
        [InlineData("单管塔", "100", false)]
        [InlineData("单管塔", "25", false)]
        [InlineData("单管塔", "35", true)]
        [InlineData("仿生树", "25", true)]
        [InlineData("某某塔", "25", false)]
        public void complex_tests(string tower, string height, bool res)
        {
            var id = dvm.Storage.Insert(new List<Rule>
            {
                new Rule { Type = RuleType.Or, NestedRules = new List<Rule>
                {
                    new Rule { Type = RuleType.And, NestedRules = new List<Rule>
                    {
                        new Rule { Type = RuleType.Equal, Expression = "单管塔", ArgumentIndex = 0 },
                        new Rule { Type = RuleType.Gte, Expression = "30", ArgumentIndex = 1 },
                        new Rule { Type = RuleType.Lte, Expression = "50", ArgumentIndex = 1 }
                    } },
                    new Rule { Type = RuleType.And, NestedRules = new List<Rule>
                    {
                        new Rule { Type = RuleType.Equal, Expression = "灯杆景观塔", ArgumentIndex = 0 },
                        new Rule { Type = RuleType.Gte, Expression = "20", ArgumentIndex = 1 },
                        new Rule { Type = RuleType.Lte, Expression = "45", ArgumentIndex = 1 }
                    } },
                    new Rule { Type = RuleType.And, NestedRules = new List<Rule>
                    {
                        new Rule { Type = RuleType.Equal, Expression = "仿生树", ArgumentIndex = 0 },
                        new Rule { Type = RuleType.Gte, Expression = "20", ArgumentIndex = 1 },
                        new Rule { Type = RuleType.Lte, Expression = "30", ArgumentIndex = 1 }
                    } },
                    new Rule { Type = RuleType.And, NestedRules = new List<Rule>
                    {
                        new Rule { Type = RuleType.Equal, Expression = "简易落地塔", ArgumentIndex = 0 },
                        new Rule { Type = RuleType.Gte, Expression = "15", ArgumentIndex = 1 },
                        new Rule { Type = RuleType.Lte, Expression = "35", ArgumentIndex = 1 }
                    } },
                    new Rule { Type = RuleType.And, NestedRules = new List<Rule>
                    {
                        new Rule { Type = RuleType.Equal, Expression = "角钢塔", ArgumentIndex = 0 },
                        new Rule { Type = RuleType.Gte, Expression = "45", ArgumentIndex = 1 },
                        new Rule { Type = RuleType.Lte, Expression = "65", ArgumentIndex = 1 }
                    } }
                } }
            });

            Assert.Equal(res, dvm.Verify(id, tower, height).IsSuccess);
        }
    }
}
