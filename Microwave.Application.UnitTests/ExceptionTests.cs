using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Exceptions;
using Microwave.Domain;
using Microwave.EventStores.Ports;
using Microwave.Queries;

namespace Microwave.Application.UnitTests
{
    [TestClass]
    public class ExceptionTests
    {
        [TestMethod]
        public void NotFound()
        {
            var readModelWrapper = new ReadModelResult<ReadModelTest>(new ReadModelTest(), GuidIdentity.Create());
            var notFoundException = new NotFoundException(readModelWrapper.GetType(), "TheId");
            Assert.AreEqual("Could not find ReadModelTest with ID TheId", notFoundException.Message);
        }

        [TestMethod]
        public void NotFound_EventStoreResult()
        {
            var eventStoreResult = new EventStoreResult<TestClass>(new TestClass(), 10);
            var notFoundException = new NotFoundException(eventStoreResult.GetType(), "TheId");
            Assert.AreEqual("Could not find TestClass with ID TheId", notFoundException.Message);
        }
    }

    public class TestClass
    {
    }

    public class ReadModelTest : ReadModel
    {
        public override Type GetsCreatedOn { get; }
    }
}