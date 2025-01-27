using System.Linq.Expressions;
using System.Reflection;
using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Models;

namespace NetRecord.Utils;

internal static class ExpressionUtils
{
    internal class DictionaryAccessInfo
    {
        public PropertyInfo DictionaryProperty { get; set; }
        public string Key { get; set; }
    }
    
    // Some Claude Generated Magic to check if a dictionary is what's being accessed
    internal static bool IsDictionaryAccess(this Expression<Func<NetRecordTransaction, object>> expression)
    {
        if (expression.Body is UnaryExpression unary) // Handle implicit conversion to object
        {
            return IsDictionaryAccessExpression(unary.Operand);
        }
    
        return IsDictionaryAccessExpression(expression.Body);
    }

    internal static bool IsDictionaryAccessExpression(this Expression expression)
    {
        // Check if the expression is a method call or indexer access
        if (expression is MethodCallExpression methodCall)
        {
            // Check if it's the dictionary's get_Item method
            return methodCall.Method.Name == "get_Item" &&
                   typeof(IDictionary<,>).IsAssignableFrom(
                       methodCall.Object?.Type.GetGenericTypeDefinition());
        }
    
        if (expression is IndexExpression indexExpr)
        {
            // Check if the indexed object is a dictionary
            return typeof(IDictionary<,>).IsAssignableFrom(
                indexExpr.Object?.Type.GetGenericTypeDefinition());
        }

        return false;
    }

    internal static DictionaryAccessInfo GetDictionaryAccessInfo(
        this Expression<Func<NetRecordTransaction, object>> expression)
    {
        var body = expression.Body;
        if (body is UnaryExpression unary)
        {
            body = unary.Operand;
        }

        if (body is MethodCallExpression { Method.Name: "get_Item" } methodCall)
        {
            // Get the dictionary property
            var memberExpr = GetMemberExpression(methodCall.Object);
            if (memberExpr?.Member is PropertyInfo propertyInfo)
            {
                // Get the dictionary key
                var keyConstant = methodCall.Arguments[0] as ConstantExpression;
                if (keyConstant != null)
                {
                    return new DictionaryAccessInfo
                    {
                        DictionaryProperty = propertyInfo,
                        Key = keyConstant.Value?.ToString()
                    };
                }
            }
        }

        throw new NetRecordException("Unrecognized property access expression in file group identifier");
    }
    
    private static MemberExpression GetMemberExpression(Expression expression)
    {
        while (expression != null)
        {
            switch (expression)
            {
                case MemberExpression memberExpr:
                    return memberExpr;
                case UnaryExpression unaryExpr:
                    expression = unaryExpr.Operand;
                    continue;
            }

            break;
        }

        return null;
    }
}