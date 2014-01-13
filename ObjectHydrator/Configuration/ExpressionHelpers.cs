using System;
using System.Linq.Expressions;

namespace ObjectHydrator.Configuration
{
    internal static class ExpressionHelpers
    {
        internal static string GetPropertyExpressionBody( LambdaExpression x )
        {
            var body = x.ToString().Replace( " ", "" );
            var linqVariableName = body.Substring( 0, body.IndexOf( "=>", StringComparison.Ordinal ) );
            body = body.Replace( linqVariableName, "x" );
            return body;
        }
    }
}