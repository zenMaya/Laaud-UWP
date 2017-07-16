using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Laaud_UWP.Util.SQLiteExtensions
{
    class LaaudSqliteCompositeMethodCallTranslator : SqliteCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] methodCallTranslators =
        {
            new SqliteLikeTranslator()
        };

        public LaaudSqliteCompositeMethodCallTranslator(ILogger<SqliteCompositeMethodCallTranslator> logger) : base(logger)
        {
            AddTranslators(methodCallTranslators);
        }
    }
}
