using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// This class provides a fast property path evaluator that can evaluate property paths on objects.
/// It is designed to replace BindingEvaluator, which is slower but more robust.
/// </summary>
public static class FastPropertyPathEvaluator
{
    private static readonly ConcurrentDictionary<(Type Type, string Path), Func<object, object>> _delegateCache =
        new ConcurrentDictionary<(Type, string), Func<object, object>>();

    public static object GetValue(object source, string path)
    {
        if (source == null) return null;
        if (string.IsNullOrEmpty(path)) return source;

        var type = source.GetType();
        var key = (type, path);

        var evaluator = _delegateCache.GetOrAdd(key, k => CompilePath(k.Type, k.Path));
        return evaluator(source);
    }

    private static Func<object, object> CompilePath(Type type, string path)
    {
        var parameter = Expression.Parameter(typeof(object), "obj");
        Expression currentExpr = Expression.Convert(parameter, type);

        var parts = path.Split('.');

        foreach (var part in parts)
        {
            int openBracketIdx = part.IndexOf('[');

            if (openBracketIdx != -1)
            {
                // Extract and process the property name before the bracket (if one exists)
                string propName = part.Substring(0, openBracketIdx);
                if (!string.IsNullOrEmpty(propName))
                {
                    currentExpr = GetPropertyExpression(currentExpr, propName);
                    if (currentExpr == null) return _ => null;
                }

                // Loop through brackets (handles nested or multi-dimensional indexers)
                int currentIdx = openBracketIdx;
                while (currentIdx != -1)
                {
                    int closeBracketIdx = part.IndexOf(']', currentIdx);
                    if (closeBracketIdx == -1) return _ => null;

                    string indexStr = part.Substring(currentIdx + 1, closeBracketIdx - currentIdx - 1);

                    // Determine if the index is an Integer or a String Key and route accordingly
                    if (int.TryParse(indexStr, out int intIndex))
                    {
                        // Way 2: Integer indexing
                        currentExpr = GetIntegerIndexerExpression(currentExpr, intIndex);
                    }
                    else
                    {
                        // Way 3: String indexing
                        currentExpr = GetStringIndexerExpression(currentExpr, indexStr);
                    }

                    if (currentExpr == null) return _ => null;

                    currentIdx = part.IndexOf('[', closeBracketIdx);
                }
            }
            else
            {
                // Way 1: Standard property path without brackets
                currentExpr = GetPropertyExpression(currentExpr, part);
                if (currentExpr == null) return _ => null;
            }
        }

        var conversion = Expression.Convert(currentExpr, typeof(object));
        return Expression.Lambda<Func<object, object>>(conversion, parameter).Compile();
    }

    /// <summary>
    /// Way 1: Compiles access to a standard, dot-separated property.
    /// </summary>
    private static Expression GetPropertyExpression(Expression expression, string propertyName)
    {
        var prop = expression.Type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop == null) return null;

        return Expression.Property(expression, prop);
    }

    /// <summary>
    /// Way 2: Compiles access to a numeric indexer (Arrays or List/Collections).
    /// </summary>
    private static Expression GetIntegerIndexerExpression(Expression expression, int index)
    {
        var indexExpr = Expression.Constant(index);

        if (expression.Type.IsArray)
        {
            return Expression.ArrayIndex(expression, indexExpr);
        }

        var indexerProp = expression.Type.GetProperty("Item", new[] { typeof(int) });
        if (indexerProp == null) return null;

        return Expression.Property(expression, indexerProp, indexExpr);
    }

    /// <summary>
    /// Way 3: Compiles access to a string indexer (Dictionaries or custom Registries).
    /// </summary>
    private static Expression GetStringIndexerExpression(Expression expression, string rawKey)
    {
        string cleanKey = rawKey.Trim('"', '\'');
        var indexExpr = Expression.Constant(cleanKey);

        var indexerProp = expression.Type.GetProperty("Item", new[] { typeof(string) });
        if (indexerProp == null) return null;

        return Expression.Property(expression, indexerProp, indexExpr);
    }
}