using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using System.Reflection;

namespace Laaud_UWP.Util.SQLiteExtensions
{
    class SqliteLikeTranslator : IMethodCallTranslator
    {
        public SqliteLikeTranslator()
        {
        }

        private static readonly MethodInfo _methodInfo
            = typeof(StringExtensions).GetRuntimeMethod(nameof(StringExtensions.ContainsIgnoreCase), new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo _concat
            = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            return ReferenceEquals(methodCallExpression.Method, _methodInfo)
                ? new LikeExpression(
                    methodCallExpression.Arguments[0],
                    Expression.Add(
                        methodCallExpression.Arguments[1],
                        Expression.Constant("%", typeof(string)),
                    _concat))
                : null;
        }
    }
}