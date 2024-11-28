using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Models.Sellers;
using SteamDigiSellerBot.Network.Models.DTO;
using SteamDigiSellerBot.Tests.Integration.Helpers;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Tests.Integration
{
    [TestFixture]
    public sealed class SellersControllerTests
    {
        private DigisellerWebApplicationFactory<Program> _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new DigisellerWebApplicationFactory<Program> ();
        }

        [Test]
        public async Task SellersController_List()
        {
            using var scope = _factory.Services.CreateScope();            
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var client = _factory.CreateClient();

            SellerDto dto = new SellerDto(){
                Login = TestContext.CurrentContext.Random.GetString(),
                Password = TestContext.CurrentContext.Random.GetString(),
                RentDays = TestContext.CurrentContext.Random.NextByte()
            };

            var response = await client.PostAsJsonAsync<SellerDto>("/sellers", dto);
            response.EnsureSuccessStatusCode();

            var sellerResponse = await response.Content.ReadFromJsonAsync<SellersCreateResponse>();
            
            var listResponse = await client.GetAsync("/sellers/");

            listResponse.EnsureSuccessStatusCode();
            var sellerListResponse = await listResponse.Content.ReadFromJsonAsync<SellersListResponse>();
            Assert.That(sellerListResponse.HasError, Is.EqualTo(false));
            Assert.That(sellerListResponse.Sellers.Count, Is.GreaterThan(0));

            var seller = context.Sellers.First(s=>s.Id == sellerResponse.Seller.Id);            
            context.Sellers.Remove(seller);
            context.SaveChanges();
            var user = await userManager.FindByIdAsync(seller.AspNetUserId);
            await userManager.DeleteAsync(user);     
        }

        [Test]
        public async Task SellersController_Get()
        {
            using var scope = _factory.Services.CreateScope();            
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var client = _factory.CreateClient();

            SellerDto dto = new SellerDto(){
                Login = TestContext.CurrentContext.Random.GetString(),
                Password = TestContext.CurrentContext.Random.GetString(),
                RentDays = TestContext.CurrentContext.Random.NextByte()
            };

            var response = await client.PostAsJsonAsync<SellerDto>("/sellers", dto);
            response.EnsureSuccessStatusCode();

            var sellerResponse = await response.Content.ReadFromJsonAsync<SellersCreateResponse>();
            
            var getResponse = await client.GetAsync("/sellers/" + sellerResponse.Seller.Id);
            getResponse.EnsureSuccessStatusCode();
            Assert.That(sellerResponse.HasError, Is.EqualTo(false)); 

            var seller = context.Sellers.First(s=>s.Id == sellerResponse.Seller.Id);            
            context.Sellers.Remove(seller);
            context.SaveChanges();
            var user = await userManager.FindByIdAsync(seller.AspNetUserId);
            await userManager.DeleteAsync(user);     
        }

        [Test]
        public async Task SellersController_Create()
        {
            using var scope = _factory.Services.CreateScope();            
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var client = _factory.CreateClient();

            SellerDto dto = new SellerDto(){
                Login = TestContext.CurrentContext.Random.GetString(),
                Password = TestContext.CurrentContext.Random.GetString(),
                RentDays = TestContext.CurrentContext.Random.NextByte()
            };

            var response = await client.PostAsJsonAsync<SellerDto>("/sellers", dto);
            response.EnsureSuccessStatusCode();

            var sellerResponse = await response.Content.ReadFromJsonAsync<SellersCreateResponse>();
            
            Assert.That(sellerResponse.HasError, Is.EqualTo(false)); 
            Assert.That(sellerResponse.Seller.Login, Is.EqualTo(dto.Login));
            Assert.That(sellerResponse.Seller.RentDays, Is.EqualTo(dto.RentDays));

            var seller = context.Sellers.First(s=>s.Id == sellerResponse.Seller.Id);            
            context.Sellers.Remove(seller);
            context.SaveChanges();
            var user = await userManager.FindByIdAsync(seller.AspNetUserId);
            await userManager.DeleteAsync(user);     
        }

        [Test]
        public async Task SellersController_Update()
        {
            using var scope = _factory.Services.CreateScope();            
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var client = _factory.CreateClient();

            SellerDto dto = new SellerDto(){
                Login = TestContext.CurrentContext.Random.GetString(),
                Password = TestContext.CurrentContext.Random.GetString()
            };

            var creatResponse = await client.PostAsJsonAsync<SellerDto>("/sellers", dto);
            creatResponse.EnsureSuccessStatusCode();

            var sellerCreateResponse = await creatResponse.Content.ReadFromJsonAsync<SellersCreateResponse>();

            dto = sellerCreateResponse.Seller;
            dto.Login = TestContext.CurrentContext.Random.GetString();
            dto.RentDays = TestContext.CurrentContext.Random.NextByte();

            var updateResponse = await client.PutAsJsonAsync<SellerDto>("/sellers", dto);
            updateResponse.EnsureSuccessStatusCode();

            var sellerResponse = await updateResponse.Content.ReadFromJsonAsync<SellersUpdateResponse>();

            Assert.That(sellerResponse.HasError, Is.EqualTo(false));
            Assert.That(sellerResponse.Seller.Login, Is.EqualTo(dto.Login));
            Assert.That(sellerResponse.Seller.RentDays, Is.EqualTo(dto.RentDays));

            var seller = context.Sellers.First(s=>s.Id == sellerResponse.Seller.Id);            
            context.Sellers.Remove(seller);
            context.SaveChanges();
            var user = await userManager.FindByIdAsync(seller.AspNetUserId);
            await userManager.DeleteAsync(user);    
        }

        [Test]
        public async Task SellersController_Delete()
        {
            var client = _factory.CreateClient();

            SellerDto dto = new SellerDto(){
                Login = TestContext.CurrentContext.Random.GetString(),
                Password = TestContext.CurrentContext.Random.GetString()
            };

            var creatResponse = await client.PostAsJsonAsync<SellerDto>("/sellers", dto);
            creatResponse.EnsureSuccessStatusCode();

            var sellerCreateResponse = await creatResponse.Content.ReadFromJsonAsync<SellersCreateResponse>();        
            int id = sellerCreateResponse.Seller.Id;

            var deleteResponse = await client.DeleteAsync("/sellers/" + id.ToString());
            deleteResponse.EnsureSuccessStatusCode();

            var sellerResponse = await deleteResponse.Content.ReadFromJsonAsync<SellersDeleteResponse>();

            Assert.That(sellerResponse.HasError, Is.EqualTo(false));   
        }
    }
}