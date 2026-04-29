using Asp.Versioning;
using LumenEstoque.Pagination;
using LumenEstoque.Services.ReportServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LumenEstoque.Controllers
{
    [Route("api/v{version:apiVersion}/report")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Gera e retorna o relatório geral de estoque em formato Excel.
        /// </summary>
        /// <remarks>
        /// Requer autenticação. O arquivo gerado contém a situação atual de todos os produtos em estoque.
        /// O nome do arquivo segue o padrão <c>stock_dd-MM-yyyy_HH-mm-ss.xlsx</c>.
        ///
        /// Exemplo de requisição:
        ///
        ///     GET /api/v1/report/stock
        ///
        /// </remarks>
        /// <returns>Arquivo <c>.xlsx</c> com o relatório de estoque.</returns>
        //[Authorize]
        [HttpGet("stock")]
        public async Task<ActionResult> GetStockReportAsync()
        {
            var bytes = await _reportService.GetStockReportAsync();
            var fileName = $"stock_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, fileName);
        }

        /// <summary>
        /// Gera e retorna o relatório de produtos com estoque baixo em formato Excel.
        /// </summary>
        /// <remarks>
        /// Requer autenticação. O arquivo gerado contém apenas os produtos cuja quantidade em estoque
        /// está igual ou abaixo do limite mínimo configurado.
        /// O nome do arquivo segue o padrão <c>low_stock_dd-MM-yyyy_HH-mm-ss.xlsx</c>.
        ///
        /// Exemplo de requisição:
        ///
        ///     GET /api/v1/report/low-stock
        ///
        /// </remarks>
        /// <returns>Arquivo <c>.xlsx</c> com o relatório de produtos com estoque baixo.</returns>
        //[Authorize]
        [HttpGet("low-stock")]
        public async Task<ActionResult> GetLowStockReportAsync()
        {
            var bytes = await _reportService.GetLowStockReportAsync();
            var fileName = $"low_stock_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, fileName);
        }

        /// <summary>
        /// Gera e retorna o relatório de movimentações de estoque em um período em formato Excel.
        /// </summary>
        /// <remarks>
        /// Requer autenticação. Os parâmetros de período são passados via query string.
        /// O arquivo gerado contém todas as entradas e saídas de estoque dentro do intervalo informado.
        /// O nome do arquivo segue o padrão <c>stock_movements_dd-MM-yyyy_HH-mm-ss.xlsx</c>.
        ///
        /// Exemplo de requisição:
        ///
        ///     GET /api/v1/report/movements?startDate=2025-01-01&amp;endDate=2025-03-31
        ///
        /// </remarks>
        /// <param name="reportParameters">Parâmetros de período para filtragem das movimentações.</param>
        /// <returns>Arquivo <c>.xlsx</c> com o relatório de movimentações no período informado.</returns>
        //[Authorize]
        [HttpGet("movements")]
        public async Task<ActionResult> GetStockMovementsReportAsync([FromQuery] ReportParameters reportParameters)
        {
            var bytes = await _reportService.GetFromPeriodAsync(reportParameters);
            var fileName = $"stock_movements_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, fileName);
        }
    }
}   