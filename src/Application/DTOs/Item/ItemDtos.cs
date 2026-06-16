namespace Application.DTOs.Item;

public record CreateItemRequest(int ProductId, int Quantity);

public record UpdateItemRequest(int Quantity);

public record ItemResponse(int Id, int ProductId, int Quantity);
