using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Models.Sellers;
using SteamDigiSellerBot.Network.Models.DTO;
using SteamDigiSellerBot.Services.Interfaces;

namespace SteamDigiSellerBot.Controllers
{
    [ApiController]
    [Authorize]
    public class SellersController : Controller
    {
        private readonly ISellersService _sellersService;

        public SellersController(ISellersService sellersService)
        {
            _sellersService = sellersService;
        }

        [HttpGet]
        [Route("sellers/")]
        public async Task<SellersListResponse> List()
        {
            var response = new SellersListResponse();
            try
            {
                response.Sellers = await _sellersService.GetSellers();            
            }
            catch(Exception ex)
            {
                response.HasError = true;
                response.ErrorText = ex.Message;
            }
            return response;
        }

        [HttpGet]
        [Route("sellers/{id}")]
        public async Task<SellersGetResponse> Get([FromRoute] int id)
        {
            var response = new SellersGetResponse();
            try
            {
                response.Seller = await _sellersService.GetSeller(id);            
            }
            catch(Exception ex)
            {
                response.HasError = true;
                response.ErrorText = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("sellers")]
        public async Task<SellersCreateResponse> Create([FromBody] SellerDto sellerDto)
        {
            var response = new SellersCreateResponse();
            try
            {
                response.Seller = await _sellersService.AddSeller(sellerDto);            
            }
            catch(Exception ex)
            {
                response.HasError = true;
                response.ErrorText = ex.Message;
            }
            return response;
        }

        [HttpPut]
        [Route("sellers")]
        public async Task<SellersUpdateResponse> Update([FromBody] SellerDto sellerDto)
        {
            var response = new SellersUpdateResponse();
            try
            {
                response.Seller = await _sellersService.UpdateSeller(sellerDto);            
            }
            catch(Exception ex)
            {
                response.HasError = true;
                response.ErrorText = ex.Message;
            }
            return response;
        }

        [HttpDelete]
        [Route("sellers/{id}")]
        public async Task<SellersDeleteResponse> Delete([FromRoute] int id)
        {
            var response = new SellersDeleteResponse();
            try
            {
                await _sellersService.DeleteSeller(id);            
            }
            catch(Exception ex)
            {
                response.HasError = true;
                response.ErrorText = ex.Message;
            }
            return response;
        }
    }
}