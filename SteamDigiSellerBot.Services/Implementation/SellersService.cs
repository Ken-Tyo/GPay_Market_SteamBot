using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Models.DTO;
using SteamDigiSellerBot.Services.Interfaces;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class SellersService: ISellersService
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly UserManager<User> _userManager;

        public SellersService(ISellerRepository sellerRepository, UserManager<User> userManager)
        {
            _sellerRepository = sellerRepository;
            _userManager = userManager;
        }

        public async Task<IReadOnlyList<SellerDto>>  GetSellers(){
            var sellers = await _sellerRepository.ListAsync();
            var userIds = sellers.Select(s=>s.AspNetUserId).ToList();
            
            var users = _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .OrderBy( u=> u.UserName)
                .ToList();

            return users
                .Select(u=> GenerateDto(sellers.First(s => s.AspNetUserId == u.Id), u))
                .ToList();
        }

        public async Task<SellerDto> GetSeller(int id)
        {
            var seller = await _sellerRepository.GetByIdAsync(id);
            var user = await _userManager.FindByIdAsync(seller.AspNetUserId);

            return GenerateDto(seller, user);
        }

        public async Task<SellerDto> AddSeller(SellerDto sellerDto)
        {
            User user = new User(){
                UserName = sellerDto.Login
            };

            var identityCreateResult = await _userManager.CreateAsync(user);

            if (!identityCreateResult.Succeeded)
            {
                throw new ArgumentException(identityCreateResult.Errors.First().Description);
            }
            var identityPasswordResult = await _userManager.AddPasswordAsync(user, sellerDto.Password);

            if (!identityPasswordResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                throw new ArgumentException(identityPasswordResult.Errors.First().Description);
            }

            var seller = new Seller(){
                AspNetUserId = user.Id,
                RentDays = sellerDto.RentDays,
                ItemsLimit = sellerDto.ItemsLimit,
                Blocked = sellerDto.Blocked,
                Comments = sellerDto.Comments,
                Permissions = new SellerPermissions(){
                    DigisellerItems = sellerDto.PermissionDigisellerItems,
                    KFGItems = sellerDto.PermissionKFGItems,
                    FuryPayItems = sellerDto.PermissionFuryPayItems,
                    ItemsHierarchy = sellerDto.PermissionItemsHierarchy,
                    OneTimeBots = sellerDto.PermissionOneTimeBots,
                    OrderSessionCreation = sellerDto.PermissionOrderSessionCreation,
                    ItemsMultiregion = sellerDto.PermissionItemsMultiregion,
                    DirectBotsDeposit = sellerDto.PermissionDirectBotsDeposit,
                    BotsLimitsParsing = sellerDto.PermissionBotsLimitsParsing,
                    DigisellerItemsGeneration = sellerDto.PermissionDigisellerItemsGeneration,
                    SteamPointsAutoDelivery = sellerDto.PermissionSteamPointsAutoDelivery
                }
            };

            await _sellerRepository.AddAsync(seller);

            var resultSeller = await _sellerRepository.GetByIdAsync(seller.Id);
            var resultUser = await _userManager.FindByIdAsync(seller.AspNetUserId);
            return GenerateDto(resultSeller, resultUser);
        }

        public async Task<SellerDto> UpdateSeller(SellerDto sellerDto)
        {
            var user = await _userManager.FindByIdAsync(sellerDto.UserId);

            if (sellerDto.Login != user.UserName)
            {
                user.UserName = sellerDto.Login;
                var identityUpdateResult = await _userManager.UpdateAsync(user);
                if (!identityUpdateResult.Succeeded)
                {
                    throw new ArgumentException(identityUpdateResult.Errors.First().Description);
                }
            }

            if (!string.IsNullOrWhiteSpace(sellerDto.Password))
            {
                var identityRemovePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!identityRemovePasswordResult.Succeeded)
                {
                    throw new ArgumentException(identityRemovePasswordResult.Errors.First().Description);
                }

                var identitySetPasswordResult = await _userManager.AddPasswordAsync(user, sellerDto.Password);
                if (!identitySetPasswordResult.Succeeded)
                {
                    throw new ArgumentException(identitySetPasswordResult.Errors.First().Description);
                }
            }

            if (!sellerDto.Id.HasValue)
                throw new ArgumentException("Id is null");

            var seller = await _sellerRepository.GetByIdAsync(sellerDto.Id.Value);

            seller.AspNetUser = null;
            seller.AspNetUserId = user.Id;
            seller.RentDays = sellerDto.RentDays;
            seller.ItemsLimit = sellerDto.ItemsLimit;
            seller.Blocked = sellerDto.Blocked;
            seller.Comments = sellerDto.Comments;
            seller.Permissions.DigisellerItems = sellerDto.PermissionDigisellerItems;
            seller.Permissions.KFGItems = sellerDto.PermissionKFGItems;
            seller.Permissions.FuryPayItems = sellerDto.PermissionFuryPayItems;
            seller.Permissions.ItemsHierarchy = sellerDto.PermissionItemsHierarchy;
            seller.Permissions.OneTimeBots = sellerDto.PermissionOneTimeBots;
            seller.Permissions.OrderSessionCreation = sellerDto.PermissionOrderSessionCreation;
            seller.Permissions.ItemsMultiregion = sellerDto.PermissionItemsMultiregion;
            seller.Permissions.DirectBotsDeposit = sellerDto.PermissionDirectBotsDeposit;
            seller.Permissions.BotsLimitsParsing = sellerDto.PermissionBotsLimitsParsing;
            seller.Permissions.DigisellerItemsGeneration = sellerDto.PermissionDigisellerItemsGeneration;
            seller.Permissions.SteamPointsAutoDelivery = sellerDto.PermissionSteamPointsAutoDelivery;

            await _sellerRepository.Updatesync(seller);

            var resultSeller = await _sellerRepository.GetByIdAsync(seller.Id);
            var resultUser = await _userManager.FindByIdAsync(seller.AspNetUserId);
            return GenerateDto(resultSeller, resultUser);
        }

        public async Task DeleteSeller(int id)
        {
            var seller = await _sellerRepository.GetByIdAsync(id);
            await _sellerRepository.DeleteAsync(seller);
            var user = await _userManager.FindByIdAsync(seller.AspNetUserId);

            var identityDeleteResult  = await _userManager.DeleteAsync(user);
            if (!identityDeleteResult.Succeeded)
            {
                throw new ArgumentException(identityDeleteResult.Errors.First().Description);
            }
        }

        private SellerDto GenerateDto(Seller seller, User user)
        {
            return new SellerDto(){
                Id = seller.Id,
                UserId = user.Id,
                Login = user.UserName,
                Password = string.Empty,
                RentDays = seller.RentDays,
                ItemsLimit = seller.ItemsLimit,
                Blocked = seller.Blocked,
                Comments = seller.Comments,
                PermissionDigisellerItems = seller.Permissions.DigisellerItems,
                PermissionKFGItems = seller.Permissions.KFGItems,
                PermissionFuryPayItems = seller.Permissions.FuryPayItems,
                PermissionItemsHierarchy = seller.Permissions.ItemsHierarchy,
                PermissionOneTimeBots = seller.Permissions.OneTimeBots,
                PermissionOrderSessionCreation = seller.Permissions.OrderSessionCreation,
                PermissionItemsMultiregion = seller.Permissions.ItemsMultiregion,
                PermissionDirectBotsDeposit = seller.Permissions.DirectBotsDeposit,
                PermissionBotsLimitsParsing = seller.Permissions.BotsLimitsParsing,
                PermissionDigisellerItemsGeneration = seller.Permissions.DigisellerItemsGeneration,
                PermissionSteamPointsAutoDelivery = seller.Permissions.SteamPointsAutoDelivery
            };
        }
    }
}