using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Results;
using Mongo2Go;
using MongoDB.Driver;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class ReadModelRepositoryTests
    {
        [TestMethod]
        public async Task IdentifiableQuerySaveAndLoad()
        {
            var runner = MongoDbRunner.Start("IdentifiableQuerySaveAndLoad");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("IdentifiableQuerySaveAndLoad");
            client.DropDatabase("IdentifiableQuerySaveAndLoad");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database));

            var guid = Guid.NewGuid();
            var testQuerry = new TestReadModel();
            testQuerry.SetVars("Test", guid, new[] {"Jeah", "jeah2"});
            await queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuerry, guid, 1));

            var querry1 = (await queryRepository.Load<TestReadModel>(guid)).Value;

            Assert.AreEqual(guid, querry1.Id);
            Assert.AreEqual("Test", querry1.ReadModel.UserName);
            Assert.AreEqual("Jeah", querry1.ReadModel.Strings.First());

            runner.Dispose();
        }

        [TestMethod]
        public async Task InsertQuery()
        {
            var runner = MongoDbRunner.Start("InsertQuery");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("InsertQuery");
            client.DropDatabase("InsertQuery");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database));
            var testQuery = new TestQuerry { UserName = "Test"};
            await queryRepository.Save(testQuery);
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("Test", query.UserName);

            runner.Dispose();
        }

        [TestMethod]
        public async Task InsertQuery_ConcurrencyProblem()
        {
            var runner = MongoDbRunner.Start("InsertQuery_ConcurrencyProblem");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("InsertQuery_ConcurrencyProblem");
            client.DropDatabase("InsertQuery_ConcurrencyProblem");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database));
            var testQuery = new TestQuerry { UserName = "Test1"};
            var testQuery2 = new TestQuerry { UserName = "Test2"};
            var save = queryRepository.Save(testQuery);
            var save2 = queryRepository.Save(testQuery2);

            await Task.WhenAll(new List<Task> { save, save2});

            runner.Dispose();
        }

        [TestMethod]
        public async Task InsertIDQuery_ConcurrencyProblem()
        {
            var runner = MongoDbRunner.Start("InsertIDQuery_ConcurrencyProblem");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("InsertIDQuery_ConcurrencyProblem");
            client.DropDatabase("InsertIDQuery_ConcurrencyProblem");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database));
            Guid guid = Guid.NewGuid();
            var testQuery = new TestReadModel();
            testQuery.SetVars("Test1", guid, new []{ "Jeah", "jeah2"});
            var testQuery2 = new TestReadModel();
            testQuery2.SetVars("Test2", guid, new []{ "Jeah", "jeah2"});

            var save = queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery, guid, 1));
            var save2 = queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery2, guid, 2));

            await Task.WhenAll(new List<Task<Result>> { save, save2 });

            var resultOfLoad = await queryRepository.Load<TestReadModel>(guid);
            Assert.AreEqual(2, resultOfLoad.Value.Version);

            runner.Dispose();
        }

        [TestMethod]
        public async Task UpdateQuery()
        {
            var runner = MongoDbRunner.Start("UpdateQuery");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("UpdateQuery");
            client.DropDatabase("UpdateQuery");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database));
            await queryRepository.Save(new TestQuerry { UserName = "Test"});
            await queryRepository.Save(new TestQuerry { UserName = "NewName"});
            var query = (await queryRepository.Load<TestQuerry>()).Value;

            Assert.AreEqual("NewName", query.UserName);

            runner.Dispose();
        }

        [TestMethod]
        public async Task LoadAllReadModels()
        {
            var runner = MongoDbRunner.Start("LoadAllReadModels");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("LoadAllReadModels");
            client.DropDatabase("LoadAllReadModels");

            var queryRepository = new ReadModelRepository(new ReadModelDatabase(database));
            Guid guid = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            var testQuery = new TestReadModel();
            testQuery.SetVars("Test1", guid, new []{ "Jeah", "jeah2"});
            var testQuery2 = new TestReadModel();
            testQuery2.SetVars("Test2", guid2, new []{ "Jeah", "jeah2"});

            await queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery, guid, 1));
            await queryRepository.Save(new ReadModelWrapper<TestReadModel>(testQuery2, guid2, 1));

            var loadAll = await queryRepository.LoadAll<TestReadModel>();
            var readModelWrappers = loadAll.Value.ToList();

            Assert.AreEqual(2, readModelWrappers.Count);
            Assert.AreEqual(testQuery.UserName, readModelWrappers[0].ReadModel.UserName);
            Assert.AreEqual(testQuery2.UserName, readModelWrappers[1].ReadModel.UserName);

            runner.Dispose();
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

        public void SetVars(string test, Guid guid, IEnumerable<string> str)
        {
            UserName = test;
            Strings = str;
        }
    }
}