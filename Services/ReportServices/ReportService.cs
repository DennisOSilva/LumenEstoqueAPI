using ClosedXML.Excel;
using LumenEstoque.Context;
using LumenEstoque.Enums;
using LumenEstoque.Pagination;
using Microsoft.EntityFrameworkCore;

namespace LumenEstoque.Services.ReportServices;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    // ════════════════════════════════════════════════════════
    //  RELATÓRIO — ESTOQUE COMPLETO
    // ════════════════════════════════════════════════════════

    public async Task<byte[]> GetStockReportAsync()
    {
        var products = await _context.Products.ToListAsync();

        var headers = new[] { "SKU", "Nome", "Quantidade", "Preço", "Valor Total" };

        return BuildWorkbook("Estoque", headers, products.Count, (sheet, i) =>
        {
            var p = products[i];
            sheet.Cell(i + 2, 1).Value = p.Sku;
            sheet.Cell(i + 2, 2).Value = p.Name;
            sheet.Cell(i + 2, 3).Value = p.StockQuantity;
            sheet.Cell(i + 2, 4).Value = p.Price;
            sheet.Cell(i + 2, 5).Value = p.StockQuantity * p.Price;
        });
    }

    // ════════════════════════════════════════════════════════
    //  RELATÓRIO — ESTOQUE CRÍTICO
    // ════════════════════════════════════════════════════════

    public async Task<byte[]> GetLowStockReportAsync()
    {
        var products = await _context.Products
            .Where(p => p.StockQuantity <= p.MinStock)
            .ToListAsync();

        var headers = new[] { "SKU", "Nome", "Quantidade", "Preço", "Valor Total" };

        return BuildWorkbook("Estoque Crítico", headers, products.Count, (sheet, i) =>
        {
            var p = products[i];
            sheet.Cell(i + 2, 1).Value = p.Sku;
            sheet.Cell(i + 2, 2).Value = p.Name;
            sheet.Cell(i + 2, 3).Value = p.StockQuantity;
            sheet.Cell(i + 2, 4).Value = p.Price;
            sheet.Cell(i + 2, 5).Value = p.StockQuantity * p.Price;
        });
    }

    // ════════════════════════════════════════════════════════
    //  RELATÓRIO — MOVIMENTAÇÕES POR PERÍODO
    // ════════════════════════════════════════════════════════

    public async Task<byte[]> GetFromPeriodAsync(ReportParameters reportParameters)
    {
        var movements = await _context.Movements
            .Where(p => p.CreatedAt.Date >= reportParameters.from
                     && p.CreatedAt.Date <= reportParameters.to)
            .Include(p => p.Product)
            .ToListAsync();

        var headers = new[]
        {
            "Data", "Produto", "Quantidade",
            "Qtd. Anterior", "Qtd. Nova",
            "Tipo", "Motivo"
        };

        return BuildWorkbook("Movimentações", headers, movements.Count, (sheet, i) =>
        {
            var m = movements[i];
            sheet.Cell(i + 2, 1).Value = m.CreatedAt;
            sheet.Cell(i + 2, 2).Value = m.Product?.Name;
            sheet.Cell(i + 2, 3).Value = m.Quantity;
            sheet.Cell(i + 2, 4).Value = m.PreviousStock;
            sheet.Cell(i + 2, 5).Value = m.NewStock;
            sheet.Cell(i + 2, 6).Value = m.Type switch
            {
                MovementType.Entry => "Entrada",
                MovementType.Exit => "Saída",
                MovementType.Adjust => "Ajuste",
                _ => "Não informado"
            };
            sheet.Cell(i + 2, 7).Value = m.Reason;
        });
    }

    // ════════════════════════════════════════════════════════
    //  HELPER — MONTA O WORKBOOK
    // ════════════════════════════════════════════════════════

    private static byte[] BuildWorkbook(
        string sheetName,
        string[] headers,
        int rowCount,
        Action<IXLWorksheet, int> fillRow)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);

        // Cabeçalho
        for (int col = 0; col < headers.Length; col++)
        {
            var cell = sheet.Cell(1, col + 1);
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1A1A2E");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Dados
        for (int i = 0; i < rowCount; i++)
        {
            fillRow(sheet, i);

            // Zebra
            if (i % 2 == 1)
            {
                sheet.Row(i + 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
            }
        }

        // Largura automática
        sheet.Columns().AdjustToContents();

        // Borda na tabela inteira
        var tableRange = sheet.Range(1, 1, rowCount + 1, headers.Length);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}