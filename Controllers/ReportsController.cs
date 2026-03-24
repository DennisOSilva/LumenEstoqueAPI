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

    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [Authorize]
        [HttpGet("stock")]
        public async Task<ActionResult> GetStockReportAsync()
        {
            var bytes = await _reportService.GetStockReportAsync();
            var fileName = $"stock_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(bytes, contentType, fileName);
        }

        [Authorize]
        [HttpGet("low-stock")]
        public async Task<ActionResult> GetLowStockReportAsync()
        {
            var bytes = await _reportService.GetLowStockReportAsync();
            var fileName = $"low_stock_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, fileName);
        }

        [Authorize]
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
