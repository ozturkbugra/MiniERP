namespace MiniERP.Application.Features.Customer.Queries.GetCustomerById
{
    public sealed record GetCustomerByIdQueryResponse(
    Guid Id,
    string Name,
    string? TaxDepartment,
    string? TaxNumber,
    string? Phone,
    string? Email,
    string? Address,
    string Type);
}
