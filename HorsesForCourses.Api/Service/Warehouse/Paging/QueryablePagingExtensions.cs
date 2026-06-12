namespace HorsesForCourses.Api.Service.Warehouse.Paging;

public static class QueryablePagingExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PageRequest request)
    {
        if (!query.Expression.ToString().Contains("OrderBy"))
            throw new NoOrderByinPagedQuery();
        int skip = (request.Page - 1) * request.Size;
        return query.Skip(skip).Take(request.Size);
    }
}
