using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DddValueObjectEF
{
    public static class DbFunctionsExtensions
    {
        public static bool Equal<T>(this DbFunctions _, T? matchExpression, T? pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Equal)));
        public static bool NotEqual<T>(this DbFunctions _, T? matchExpression, T? pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Equal)));
        public static bool GreaterThan<T>(this DbFunctions _, T matchExpression, T pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThan)));
        public static bool GreaterThanOrEqual<T>(this DbFunctions _, T matchExpression, T pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GreaterThanOrEqual)));
        public static bool LessThan<T>(this DbFunctions _, T matchExpression, T pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThan)));
        public static bool LessThanOrEqual<T>(this DbFunctions _, T matchExpression, T pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(LessThanOrEqual)));
    }

    internal class DbFunctionsExtensionInfo: DbContextOptionsExtensionInfo
    {
        public DbFunctionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => string.Empty;

        public override int GetServiceProviderHashCode() => 0;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;
    }

    public class DbFunctionMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public IEnumerable<IMethodCallTranslator> Translators => new[] { new DbFunctionsTranslator() };
    }

    public class DbFunctionsExtension : IDbContextOptionsExtension
    {
        public DbContextOptionsExtensionInfo Info => new DbFunctionsExtensionInfo(this);

        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IMethodCallTranslatorPlugin, DbFunctionMethodCallTranslatorPlugin>();
        }

        public void Validate(IDbContextOptions options)
        {
        }
    }

    public static class DbFunctionsDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<T> UseCompareDbFunctions<T>(this DbContextOptionsBuilder<T> source) where T : DbContext
        {
            var extension = source.Options.FindExtension<DbFunctionsExtension>() ?? new DbFunctionsExtension();
            ((IDbContextOptionsBuilderInfrastructure)source).AddOrUpdateExtension(extension);
            return source;
        }
    }

    public class DbFunctionsTranslator : IMethodCallTranslator
    {
        public SqlExpression? Translate(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (method.DeclaringType != typeof(DbFunctionsExtensions))
            {
                return null;
            }
            return (method.Name) switch
            {
                nameof(DbFunctionsExtensions.Equal) => ToExpression(ExpressionType.Equal, arguments),
                nameof(DbFunctionsExtensions.NotEqual) => ToExpression(ExpressionType.NotEqual, arguments),
                nameof(DbFunctionsExtensions.GreaterThan) => ToExpression(ExpressionType.GreaterThan, arguments),
                nameof(DbFunctionsExtensions.GreaterThanOrEqual) => ToExpression(ExpressionType.GreaterThanOrEqual, arguments),
                nameof(DbFunctionsExtensions.LessThan) => ToExpression(ExpressionType.LessThan, arguments),
                nameof(DbFunctionsExtensions.LessThanOrEqual) => ToExpression(ExpressionType.LessThanOrEqual, arguments),
                _ => null,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SqlExpression ToExpression(ExpressionType type, IReadOnlyList<SqlExpression> arguments) => new SqlBinaryExpression(
            type,
            arguments[1],
            arguments[2],
            typeof(bool),
            null);
    }
}
