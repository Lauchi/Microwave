using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.Persistence.UnitTestsSetup;
using Microwave.Queries;

namespace Microwave.Persistence.UnitTests.Querries
{
    [TestClass]
    public class ReadModelRepositoryTests
    {
        [DataTestMethod]
        [PersistenceTypeTest]

        public async Task IdentifiableQuerySaveAndLoad(PersistenceLayerProvider layerProvider)
        {
            var queryRepository = layerProvider.ReadModelRepository;

            var guid = GuidIdentity.Create(Guid.NewGuid());
            var testQuerry = new TestReadModel();
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.Save(testQuerry, guid, 1);

            var querry1 = await queryRepository.Load<TestReadModel>(guid);

            Assert.AreEqual(guid, querry1.Id);
            Assert.AreEqual("Test", querry1.Value.UserName);
            Assert.AreEqual("Jeah", querry1.Value.Strings.First());
        }

        [DataTestMethod]
        [PersistenceTypeTest]

        public async Task LoadWithNullId(PersistenceLayerProvider layerProvider)
        {
            var queryRepository = layerProvider.ReadModelRepository;
            var querry1 = await queryRepository.Load<TestReadModel>(null);

            Assert.IsTrue(querry1.Is<NotFound>());
            Assert.ThrowsException<NotFoundException>(() => querry1.Value);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task IdentifiableQuerySaveAndLoadAll(PersistenceLayerProvider layerProvider)
        {
            var queryRepository = layerProvider.ReadModelRepository;

            var testQuerry = new TestReadModel();
            var testQuerry2 = new TestReadModel();
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            testQuerry2.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.Save(testQuerry, GuidIdentity.Create(), 1);
            await queryRepository.Save(testQuerry2, GuidIdentity.Create(), 1);

            var querry1 = await queryRepository.LoadAll<TestReadModel>();

            Assert.AreEqual(2, querry1.Value.Count());
            Assert.AreEqual("Test", querry1.Value.First().UserName);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task IdentifiableQuerySaveAndLoadAll_UnknownType(PersistenceLayerProvider layerProvider)
        {
            var queryRepository = layerProvider.ReadModelRepository;

            var testQuerry = new TestReadModel();
            var testQuerry2 = new TestReadModel();
            testQuerry.SetVars("Test", new[] {"Jeah", "jeah2"});
            testQuerry2.SetVars("Test", new[] {"Jeah", "jeah2"});
            await queryRepository.Save(testQuerry, GuidIdentity.Create(), 1);
            await queryRepository.Save(testQuerry2, GuidIdentity.Create(), 1);

            var loadAll = await queryRepository.LoadAll<TestReadModel2>();
            Assert.IsTrue(loadAll.Is<NotFound>());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task InsertQuery(PersistenceLayerProvider layerProvider)
        {
            var queryRepository = layerProvider.ReadModelRepository;
            var testQuery = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuery);
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("Test", query.UserName);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task InsertQuery_ConcurrencyProblem(PersistenceLayerProvider layerProvider)
        {
            var queryRepository = layerProvider.ReadModelRepository;
            var testQuery = new TestQuerry { UserName = "Test1"};
            var testQuery2 = new TestQuerry { UserName = "Test2"};
            var save = queryRepository.Save(testQuery);
            var save2 = queryRepository.Save(testQuery2);

            await Task.WhenAll(new List<Task> { save, save2 });
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task UpdateQuery(PersistenceLayerProvider layerProvider)
        {
            var queryRepository = layerProvider.ReadModelRepository;
            await queryRepository.Save(new TestQuerry { UserName = "Test"});
            await queryRepository.Save(new TestQuerry { UserName = "NewName"});
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("NewName", query.UserName);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadTwoTypesOfReadModels_Bug(PersistenceLayerProvider layerProvider)
        {
            var queryRepository = layerProvider.ReadModelRepository;
            var guid2 = GuidIdentity.Create(Guid.NewGuid());
            var testQuery2 = new TestReadModel2();
            testQuery2.SetVars("Test2", new []{ "Jeah", "jeah2"});

            await queryRepository.Save(testQuery2, guid2, 1);

            var loadAll2 = await queryRepository.Load<TestReadModel>(guid2);

            Assert.IsTrue(loadAll2.Is<NotFound>());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task ReadModelNotFoundEceptionHasCorrectT(PersistenceLayerProvider layerProvider)
        {
            var queryRepository = layerProvider.ReadModelRepository;
            var guid2 = GuidIdentity.Create(Guid.NewGuid());
            var result = await queryRepository.Load<TestReadModel>(guid2);

            var notFoundException = Assert.ThrowsException<NotFoundException>(() => result.Value);
            Assert.IsTrue(notFoundException.Message.StartsWith("Could not find TestReadModel"));
        }
    }

    public class TestQuerry : Query
    {
        public string UserName { get; set; }
    }

    public class TestReadModel : ReadModel
    {
        public string UserName { get; private set; }
        public IEnumerable<string> Strings { get; private set; } = new List<string>();

        public void SetVars(string test, IEnumerable<string> str)
        {
            UserName = test;
            Strings = str;
        }

        public override Type GetsCreatedOn { get; }
    }

    public class TestReadModel2 : ReadModel
    {
        public string UserNameAllDifferent { get; private set; }
        public IEnumerable<string> StringsAllDifferent { get; private set; } = new List<string>();

        public void SetVars(string test, IEnumerable<string> str)
        {
            UserNameAllDifferent = test;
            StringsAllDifferent = str;
        }

        public override Type GetsCreatedOn { get; }
    }
}