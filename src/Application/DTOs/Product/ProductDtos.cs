namespace Application.DTOs.Product;

public record CreateProductRequest(string ProductName);

public record UpdateProductRequest(string ProductName);

public record ProductResponse(
    int Id,
    string ProductName,
    string CreatedBy,
    DateTime CreatedOn,
    string? ModifiedBy,
    DateTime? ModifiedOn
);

public record ProductWithItemsResponse(
    int Id,
    string ProductName,
    string CreatedBy,
    DateTime CreatedOn,
    string? ModifiedBy,
    DateTime? ModifiedOn,
    IEnumerable<ItemSummary> Items
);

public record ItemSummary(int Id, int Quantity);

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
