using Bakalauras.Domain.Models;
using Bakalauras.Persistence.Repositories;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Bakalauras.Persistence;

namespace Bakalauras.UnitTests
{
    public class NodeRepositoryTests
    {
        private AppDbContext _dbContext;
        private NodeRepository _nodeRepository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _dbContext = new AppDbContext(options);
            _nodeRepository = new NodeRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetById_ReturnsNode_WhenExists()
        {
            var nodeId = Guid.NewGuid();
            var node = new Node { Id = nodeId, Name = "Test Node" };
            _dbContext.Nodes.Add(node);
            await _dbContext.SaveChangesAsync();

            var result = await _nodeRepository.GetByIdAsync(nodeId);

            Assert.NotNull(result);
            Assert.AreEqual(nodeId, result?.Id);
        }

        [Test]
        public async Task GetById_ReturnsNull_WhenNotFound()
        {
            var nonExistentNodeId = Guid.NewGuid();

            var result = await _nodeRepository.GetByIdAsync(nonExistentNodeId);

            Assert.IsNull(result);
        }

        [Test]
        public async Task AddNode_AddsNode_WhenValid()
        {
            var node = new Node { Name = "New Node" };

            var result = await _nodeRepository.AddAsync(node);

            Assert.NotNull(result);
            Assert.AreEqual("New Node", result.Name);
        }

        [Test]
        public async Task DeleteNode_RemovesNode_WhenExists()
        {
            var nodeId = Guid.NewGuid();
            var node = new Node { Id = nodeId, Name = "Node To Delete" };
            _dbContext.Nodes.Add(node);
            await _dbContext.SaveChangesAsync();

            await _nodeRepository.DeleteAsync(nodeId);

            var result = await _nodeRepository.GetByIdAsync(nodeId);
            Assert.IsNull(result);
        }
    }
}
