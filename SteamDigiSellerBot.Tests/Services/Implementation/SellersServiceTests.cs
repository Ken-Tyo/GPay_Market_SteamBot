using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Models.DTO;
using SteamDigiSellerBot.Services.Implementation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Tests.Services.Implementation
{
    [TestFixture]
    public sealed class SellersServiceTests
    {
        private Mock<ISellerRepository> _sellerRepositoryMock;
        private Mock<UserManager<User>> _userManagerMock;
        private SellersService _sellerService;

        [SetUp]
        public void SetUp()
        {
            _sellerRepositoryMock = new Mock<ISellerRepository>();
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _sellerService = new SellersService(_sellerRepositoryMock.Object, _userManagerMock.Object);

            _userManagerMock.Setup(manager => manager.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);
            
            _userManagerMock.Setup(manager => manager.AddPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(manager => manager.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User(){});
            
            _userManagerMock.Setup(manager => manager.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(manager => manager.RemovePasswordAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(manager => manager.DeleteAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _sellerRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Seller>(), CancellationToken.None));

            _sellerRepositoryMock.Setup(repo => repo.ListAsync(CancellationToken.None))
                .ReturnsAsync(new List<Seller>());

            _sellerRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Seller(){
                    Permissions = new SellerPermissions()
                });

            _sellerRepositoryMock.Setup(repo => repo.Updatesync(It.IsAny<Seller>(), CancellationToken.None));

            _sellerRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<Seller>(), CancellationToken.None));
        }

        [Test]
        public async Task Sellers_List()
        {
            await _sellerService.GetSellers();

            _sellerRepositoryMock.Verify(repo => repo.ListAsync(CancellationToken.None),Times.Once);
        }

        [Test]
        public async Task Sellers_Get()
        {
            var id = TestContext.CurrentContext.Random.NextByte();
            await _sellerService.GetSeller(id);

            _sellerRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()),Times.Once);
            _userManagerMock.Verify(manager => manager.FindByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Sellers_Create()
        {
            var seller = new SellerDto(){
                Login = TestContext.CurrentContext.Random.GetString()
            };

            var sellerResult = await _sellerService.AddSeller(seller);

            _userManagerMock.Verify(manager => manager.CreateAsync(It.IsAny<User>()),Times.Once);
            _userManagerMock.Verify(manager => manager.AddPasswordAsync(It.IsAny<User>(), It.IsAny<string>()),Times.Once);
            _userManagerMock.Verify(manager => manager.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _sellerRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Seller>(), CancellationToken.None), Times.Once);
            _sellerRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _userManagerMock.Verify(manager => manager.FindByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Sellers_Update()
        {
            var seller = new SellerDto(){
                Id = TestContext.CurrentContext.Random.NextByte(),
                Login = TestContext.CurrentContext.Random.GetString(),
                Password = TestContext.CurrentContext.Random.GetString()
            };

            var sellerResult = await _sellerService.UpdateSeller(seller);

            _userManagerMock.Verify(manager => manager.FindByIdAsync(It.IsAny<string>()), Times.Exactly(2));
            _userManagerMock.Verify(manager => manager.UpdateAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(manager => manager.RemovePasswordAsync(It.IsAny<User>()),Times.Once);
            _userManagerMock.Verify(manager => manager.AddPasswordAsync(It.IsAny<User>(), It.IsAny<string>()),Times.Once);
            _sellerRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()),Times.Exactly(2));
            _sellerRepositoryMock.Verify(repo => repo.Updatesync(It.IsAny<Seller>(), CancellationToken.None),Times.Once);            
        }

        [Test]
        public async Task Sellers_Delete()
        {
            var id = TestContext.CurrentContext.Random.NextByte();
            await _sellerService.DeleteSeller(id);

            _sellerRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()),Times.Once);
            _userManagerMock.Verify(manager => manager.FindByIdAsync(It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(manager => manager.DeleteAsync(It.IsAny<User>()), Times.Once);
            _sellerRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Seller>(), CancellationToken.None),Times.Once);         
        }
    }
}