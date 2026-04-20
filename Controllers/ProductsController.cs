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
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Retorna uma lista paginada de produtos.
        /// </summary>
        /// <remarks>
        /// Requer autenticação. Os parâmetros são passados via query string.
        /// Os metadados de paginação (TotalCount, PageSize, CurrentPage, TotalPages, HasNext, HasPrevious)
        /// são retornados no header <c>X-Pagination</c> da resposta.
        ///
        /// Parâmetros disponíveis:
        ///
        /// | Parâmetro    | Tipo    | Padrão      | Descrição                                              |
        /// |--------------|---------|-------------|--------------------------------------------------------|
        /// | pageNumber   | int     | 1           | Número da página                                       |
        /// | pageSize     | int     | 10          | Quantidade de itens por página                         |
        /// | search       | string  | null        | Busca por nome ou SKU                                  |
        /// | categoryId   | int     | null        | Filtra por categoria                                   |
        /// | lowStock     | bool    | false       | Retorna apenas produtos com estoque abaixo do mínimo   |
        /// | zeroStock    | bool    | false       | Retorna apenas produtos com estoque zerado             |
        /// | active       | bool    | true        | Filtra por status de ativação                          |
        /// | orderBy      | enum    | -           | Campo de ordenação (ex: Name, Price, StockQuantity)    |
        /// | direction    | enum    | Ascending   | Direção da ordenação: Ascending ou Descending          |
        ///
        /// Exemplo de requisição:
        ///
        ///     GET /api/v1/products?pageNumber=1&amp;pageSize=10&amp;search=cabo&amp;categoryId=2&amp;active=true&amp;orderBy=Name&amp;direction=Ascending
        ///
        /// </remarks>
        /// <param name="productParameters">Parâmetros de paginação, filtro e ordenação da consulta.</param>
        /// <returns>Uma lista paginada de produtos.</returns>
        //[Authorize]
        [HttpGet]
        public async Task<ActionResult<PagedList<ProductReadDTO>>> GetAllAsync([FromQuery] ProductParameters productParameters)
        {
            var products = await _productService.GetAllAsync(productParameters);
            return CreatePaginatedResponse(products);
        }

        /// <summary>
        /// Retorna um produto pelo seu SKU.
        /// </summary>
        /// <remarks>
        /// Requer autenticação. O SKU é um identificador alfanumérico único do produto.
        ///
        /// Exemplo de requisição:
        ///
        ///     GET /api/v1/products/sku/PROD-001
        ///
        /// </remarks>
        /// <param name="sku">SKU único do produto.</param>
        /// <returns>O produto correspondente ao <paramref name="sku"/> informado.</returns>
        //[Authorize]
        [HttpGet("sku/{sku}")]
        public async Task<ActionResult<ProductReadDTO>> GetBySkuAsync(string sku)
        {
            var product = await _productService.GetBySku(sku);
            return Ok(product);
        }

        /// <summary>
        /// Retorna um produto pelo seu identificador único.
        /// </summary>
        /// <remarks>
        /// Requer autenticação. O <paramref name="id"/> deve ser um inteiro maior ou igual a 1.
        ///
        /// Exemplo de requisição:
        ///
        ///     GET /api/v1/products/7
        ///
        /// </remarks>
        /// <param name="id">Identificador único do produto. Deve ser maior ou igual a 1.</param>
        /// <returns>O produto correspondente ao <paramref name="id"/> informado.</returns>
        //[Authorize]
        [HttpGet("{id:int:min(1)}")]
        public async Task<ActionResult<ProductReadDTO>> GetByIdAsync(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(product);
        }

        /// <summary>
        /// Retorna os produtos que atingiram o estoque mínimo.
        /// </summary>
        /// <remarks>
        /// Requer autenticação. Retorna apenas produtos cuja quantidade em estoque está
        /// igual ou abaixo do limite mínimo configurado. Suporta paginação e filtros via query string.
        ///
        /// Parâmetros disponíveis:
        ///
        /// | Parâmetro    | Tipo    | Padrão      | Descrição                                              |
        /// |--------------|---------|-------------|--------------------------------------------------------|
        /// | pageNumber   | int     | 1           | Número da página                                       |
        /// | pageSize     | int     | 20          | Quantidade de itens por página                         |
        /// | search       | string  | null        | Busca por nome ou SKU                                  |
        /// | categoryId   | int     | null        | Filtra por categoria                                   |
        /// | zeroStock    | bool    | false       | Retorna apenas produtos com estoque zerado             |
        /// | active       | bool    | true        | Filtra por status de ativação                          |
        /// | orderBy      | enum    | Name        | Campo de ordenação (ex: Name, Price, StockQuantity)    |
        /// | direction    | enum    | Ascending   | Direção da ordenação: Ascending ou Descending          |
        ///
        /// Exemplo de requisição:
        ///
        ///     GET /api/v1/products/min-stock?pageNumber=1&amp;pageSize=20&amp;categoryId=2&amp;active=true
        ///
        /// </remarks>
        /// <param name="productParameters">Parâmetros de paginação e filtro da consulta.</param>
        /// <returns>Lista de produtos com estoque no nível mínimo ou abaixo.</returns>
        //[Authorize]
        [HttpGet("min-stock")]
        public async Task<ActionResult<IEnumerable<ProductReadDTO>>> GetProductsWithMinStockAsync([FromQuery] ProductParameters productParameters)
        {
            var products = await _productService.GetMinStockAsync(productParameters);
            return Ok(products);
        }

        /// <summary>
        /// Cria um novo produto.
        /// </summary>
        /// <remarks>
        /// Requer autenticação e perfil <c>Admin</c>.
        ///
        /// Exemplo de requisição:
        ///
        ///     POST /api/v1/products
        ///     {
        ///         "name": "Cabo HDMI 2m",
        ///         "sku": "PROD-001",
        ///         "price": 49.90,
        ///         "stockQuantity": 100,
        ///         "minStock": 10,
        ///         "categoryId": 2,
        ///         "supplierId": 1
        ///     }
        ///
        /// </remarks>
        /// <param name="productCreateDTO">Dados do produto a ser criado.</param>
        /// <returns>O produto recém-criado com seu identificador gerado.</returns>
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductReadDTO>> CreateAsync(ProductCreateDTO productCreateDTO)
        {
            var createdProduct = await _productService.CreateAsync(productCreateDTO);
            return Created($"/api/v1/products/{createdProduct.Id}", createdProduct);
        }

        /// <summary>
        /// Atualiza os dados de um produto existente.
        /// </summary>
        /// <remarks>
        /// Requer autenticação e perfil <c>Admin</c>. O <paramref name="id"/> deve corresponder
        /// a um produto existente.
        ///
        /// Exemplo de requisição:
        ///
        ///     PUT /api/v1/products?id=7
        ///     {
        ///         "name": "Cabo HDMI 3m",
        ///         "price": 59.90,
        ///         "stockQuantity": 80,
        ///         "minStock": 10,
        ///         "categoryId": 2,
        ///         "supplierId": 1
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Identificador único do produto a ser atualizado.</param>
        /// <param name="productUpdateDTO">Dados atualizados do produto.</param>
        /// <returns>O produto com os dados atualizados.</returns>
        //[Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<ActionResult<ProductReadDTO>> UpdateAsync(int id, ProductUpdateDTO productUpdateDTO)
        {
            var updatedProduct = await _productService.UpdateAsync(id, productUpdateDTO);
            return Ok(updatedProduct);
        }

        /// <summary>
        /// Atualiza o status de ativação de um produto.
        /// </summary>
        /// <remarks>
        /// Requer autenticação e perfil <c>Admin</c>. Permite ativar ou desativar um produto
        /// sem alterar os demais dados. O <paramref name="id"/> deve ser maior ou igual a 1.
        ///
        /// Exemplo de requisição:
        ///
        ///     PATCH /api/v1/products/7/active
        ///     {
        ///         "isActive": false
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Identificador único do produto. Deve ser maior ou igual a 1.</param>
        /// <param name="productUpdateDTO">Objeto contendo o novo valor de <c>IsActive</c>.</param>
        /// <returns>O valor atualizado de <c>IsActive</c> do produto.</returns>
        //[Authorize(Roles = "Admin")]
        [HttpPatch("{id:int:min(1)}/active")]
        public async Task<ActionResult<ProductReadDTO>> UpdateActiveAsync(int id, ProductActiveUpdateDTO productUpdateDTO)
        {
            var updatedProduct = await _productService.UpdateActiveAsync(id, productUpdateDTO);
            return Ok(updatedProduct.IsActive);
        }

        /// <summary>
        /// Remove um produto pelo seu identificador único.
        /// </summary>
        /// <remarks>
        /// Requer autenticação e perfil <c>Admin</c>. A operação é irreversível.
        ///
        ///     DELETE /api/v1/products?id=7
        ///
        /// </remarks>
        /// <param name="id">Identificador único do produto a ser removido.</param>
        /// <returns>O produto que foi removido.</returns>
        //[Authorize(Roles = "Admin")]
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