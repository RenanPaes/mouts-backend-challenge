using System.Linq.Expressions;
using System.Reflection;

namespace Ambev.DeveloperEvaluation.ORM.Extensions;

/// <summary>
/// Helpers for applying dynamic ordering to an <see cref="IQueryable{T}"/> based on the
/// <c>_order</c> convention described in the general API definitions
/// (e.g. "saleDate desc, saleNumber asc").
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies a comma-separated ordering expression to the query. Unknown fields are ignored.
    /// When no valid ordering is provided, <paramref name="defaultProperty"/> is used descending.
    /// </summary>
    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> source, string? order, string defaultProperty)
    {
        var clauses = ParseClauses(order);
        if (clauses.Count == 0)
            clauses.Add((defaultProperty, true));

        IOrderedQueryable<T>? ordered = null;
        foreach (var (field, descending) in clauses)
        {
            var property = ResolveProperty<T>(field);
            if (property == null)
                continue;

            ordered = ordered == null
                ? ApplyOrder(source, property, descending, first: true)
                : ApplyOrder(ordered, property, descending, first: false);
        }

        return ordered ?? source;
    }

    private static List<(string Field, bool Descending)> ParseClauses(string? order)
    {
        var result = new List<(string, bool)>();
        if (string.IsNullOrWhiteSpace(order))
            return result;

        foreach (var token in order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = token.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0];
            var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            result.Add((field, descending));
        }

        return result;
    }

    private static PropertyInfo? ResolveProperty<T>(string field)
    {
        return typeof(T).GetProperty(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    }

    private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, PropertyInfo property, bool descending, bool first)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var lambda = Expression.Lambda(propertyAccess, parameter);

        var methodName = (first, descending) switch
        {
            (true, true) => "OrderByDescending",
            (true, false) => "OrderBy",
            (false, true) => "ThenByDescending",
            (false, false) => "ThenBy"
        };

        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IOrderedQueryable<T>)method.Invoke(null, new object[] { source, lambda })!;
    }
}
