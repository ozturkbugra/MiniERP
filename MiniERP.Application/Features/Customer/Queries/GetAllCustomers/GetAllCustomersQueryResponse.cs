namespace MiniERP.Application.Features.Customer.Queries.GetAllCustomers
{
    public sealed record GetAllCustomersQueryResponse(
    Guid Id,
    string Name,
    string? TaxDepartment,
    string? TaxNumber,
    string? Phone,
    string? Email,
    string? Address,
    string Type);
}
