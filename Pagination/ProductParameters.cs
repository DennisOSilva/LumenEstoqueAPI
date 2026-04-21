using LumenEstoque.Enums;

namespace LumenEstoque.Pagination;

public class ProductParameters : QueryStringParameters
{
    public string? search { get; set; }
    public int? categoryId { get; set; }
    public int? supplierId { get; set; }
    public bool lowStock { get; set; } = false;
    public bool zeroStock { get; set; } = false;
    public bool active { get; set; } = true;
    public ProductOrderBy orderBy { get; set; }
    public OrderDirection direction { get; set; } = OrderDirection.Ascending;
}
