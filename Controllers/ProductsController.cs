using Asp.Versioning;
using LumenEstoque.Context;
using LumenEstoque.DTOs.Mapping;
using LumenEstoque.DTOs.ProductsDTOs;
using LumenEstoque.Models;
using LumenEstoque.Pagination;
using LumenEstoque.Services.ProductServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;

namespace LumenEstoque.Controllers
{
    [Route("api/v{version:apiVersion}/products")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<PagedList<ProductReadDTO>>> GetAllAsync([FromQuery] ProductParameters productParameters)
        {
            var products = await _productService.GetAllAsync(productParameters);
            return CreatePaginatedResponse(products);
        }

        [Authorize]
        [HttpGet("sku/{sku}")]
        public async Task<ActionResult<ProductReadDTO>> GetBySkuAsync(string sku)
        {
            var product = await _productService.GetBySku(sku);
            return Ok(product);
        }

        [Authorize]
        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<ProductReadDTO>> GetByIdAsync(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(product);
        }

        [Authorize]
        [HttpGet("min-stock")]
        public async Task<ActionResult<IEnumerable<ProductReadDTO>>> GetProductsWithMinStockAsync([FromQuery] ProductParameters productParameters)
        {
            var products = await _productService.GetMinStockAsync(productParameters);
            return Ok(products);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductReadDTO>> CreateAsync(ProductCreateDTO productCreateDTO)
        {
            var createdProduct = await _productService.CreateAsync(productCreateDTO);
            return Created($"/api/v1/products/{createdProduct.Id}", createdProduct);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<ActionResult<ProductReadDTO>> UpdateAsync(int id, ProductUpdateDTO productUpdateDTO)
        {
            var updatedProduct = await _productService.UpdateAsync(id, productUpdateDTO);
            return Ok(updatedProduct);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int:min(1)}/active")]
        public async Task<ActionResult<ProductReadDTO>> UpdateActiveAsync(int id, ProductActiveUpdateDTO productUpdateDTO)
        {
            var updatedProduct = await _productService.UpdateActiveAsync(id, productUpdateDTO);
            return Ok(updatedProduct.IsActive);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var produto = await _productService.DeleteAsync(id);
            return Ok(produto);
        }

        private ActionResult<PagedList<ProductReadDTO>> CreatePaginatedResponse(PagedList<ProductReadDTO> products)
        {
            var metadata = new
            {
                products.TotalCount,
                products.PageSize,
                products.CurrentPage,
                products.TotalPages,
                products.HasNext,
                products.HasPrevious
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(products);
        }
    }
}
