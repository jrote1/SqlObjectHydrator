using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace SqlObjectHydrator.Configuration
{
    internal class ConfigurationValidator
    {
        public const string DuplicatePropertyStringFormat = "The are multiple mappings for property {0}.{1}";
        private const string PropertyValueGetterErrorStringFormat = "Property {0}.{1} has invalid valueGetter. {2}";

        //TODO: Optimize Code
        public ConfigurationValidatorResult ValidateConfiguration<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration ) where T : new()
        {
            var result = new ConfigurationValidatorResult();

            var duplicates = configuration.MappingsActions.GroupBy( x => ExpressionHelpers.GetPropertyExpressionBody( x.Key ) ).Where( x => x.Count() > 1 );
            foreach ( var duplicate in duplicates )
                result.Errors.Add( String.Format( DuplicatePropertyStringFormat, typeof ( T ).FullName, GetPropertyNameFromExpression( duplicate.First() ) ) );

            foreach ( var mappingsAction in configuration.MappingsActions )
            {
                try
                {
                    var testResult = mappingsAction.Value.Compile().DynamicInvoke( dataReader );
                    if ( testResult.GetType() != mappingsAction.Key.ReturnType )
                        result.Errors.Add( String.Format( PropertyValueGetterErrorStringFormat, typeof ( T ).FullName, GetPropertyNameFromExpression( mappingsAction ), "Returns wrong type" ) );
                }
                catch ( Exception exception )
                {
                    result.Errors.Add( String.Format( PropertyValueGetterErrorStringFormat, typeof ( T ).FullName, GetPropertyNameFromExpression( mappingsAction ), exception.InnerException.Message ) );
                }
            }

            return result;
        }

        private string GetPropertyNameFromExpression( KeyValuePair<LambdaExpression, LambdaExpression> expression )
        {
            var body = expression.Key.ToString();
            var propertyNameStart = body.IndexOf( '.' ) + 1;
            return body.Substring( propertyNameStart );
        }
    }
}