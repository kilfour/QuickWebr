namespace HorsesForCourses.Api.Service.Warehouse.Paging;

public sealed record PageRequest(int PageNumber = 1, int PageSize = 25)
{
    public int Page => PageNumber < 1 ? 1 : PageNumber;
    public int Size => PageSize is < 1 ? 1 : (PageSize > 25 ? 25 : PageSize);
}