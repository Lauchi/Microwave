using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Persistence.UnitTestsSetup.CosmosDb;
using Microwave.Persistence.UnitTestsSetup.InMemory;
using Microwave.Persistence.UnitTestsSetup.MongoDb;

namespace Microwave.Persistence.UnitTests
{
    public class PersistenceTypeTestAttribute : Attribute, ITestDataSource
    {
        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            yield return new object[] { new MongoDbTestSetup() };
            yield return new object[] { new InMemroyTestSetup() };
            yield return new object[] { new CosmosDbTestSetup(),  };
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data != null)
                return string.Format(CultureInfo.CurrentCulture,
                    "Integration - {0} ({1})",
                    methodInfo.Name,
                    string.Join(",",
                        data));

            return null;
        }
    }
}