using System.Linq.Expressions;

namespace Api.Common.Extensions;


public static class QueryableExtension
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
        => condition ? query.Where(predicate) : query;
}


