using System.Linq.Expressions;
using System.Reflection;
using NetRecord.Utils.Exceptions;

namespace NetRecord.Utils.Extensions;

public static class ExpressionExtensions
{
    // Edited version of this stack overflow
    // https://stackoverflow.com/questions/17115634/get-propertyinfo-of-a-parameter-passed-as-lambda-expression
    public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> propertyLambda)
    {
        MemberExpression Exp = null;

        //this line is necessary, because sometimes the expression comes in as Convert(originalexpression)
        if (propertyLambda.Body is UnaryExpression)
        {
            var UnExp = (UnaryExpression)propertyLambda.Body;
            if (UnExp.Operand is MemberExpression operand)
            {
                Exp = operand;
            }
            else
                throw new NetRecordException(
                    $"Lambda {propertyLambda} is not a property expression"
                );
        }
        else if (propertyLambda.Body is MemberExpression body)
        {
            Exp = body;
        }
        else
        {
            throw new NetRecordException($"Lambda {propertyLambda} is not a property expression");
        }

        return (PropertyInfo)Exp.Member;
    }
}
