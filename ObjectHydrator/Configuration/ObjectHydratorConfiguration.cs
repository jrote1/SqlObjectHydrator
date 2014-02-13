using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace SqlObjectHydrator.Configuration
{
    public class ObjectHydratorConfiguration<T> where T : new()
    {
        internal readonly List<KeyValuePair<LambdaExpression, LambdaExpression>> MappingsActions;

        public ObjectHydratorConfiguration()
        {
            MappingsActions = new List<KeyValuePair<LambdaExpression, LambdaExpression>>();
        }

        public ObjectHydratorConfiguration<T> Mapping<TResult>( Expression<Func<T, TResult>> destinationProperty, Expression<Func<IDataRecord, TResult>> valueGetter )
        {
            MappingsActions.Add( new KeyValuePair<LambdaExpression, LambdaExpression>( destinationProperty, valueGetter ) );
            _hashCodeCache = null;
            _hashCodeCache = GetHashCode();
            return this;
        }

        private int? _hashCodeCache;

        public override int GetHashCode()
        {
            if ( _hashCodeCache != null )
                return _hashCodeCache.Value;

            var result = 0;

            foreach ( var mappingsAction in MappingsActions )
            {
                result += ExpressionHelpers.GetPropertyExpressionBody( mappingsAction.Key ).GetHashCode();
                result += ExpressionHelpers.GetPropertyExpressionBody( mappingsAction.Value ).GetHashCode();
            }

            return (int)( _hashCodeCache = result );
        }

        public override bool Equals( object obj )
        {
            return GetHashCode() == obj.GetHashCode();
        }

        public static bool operator ==( ObjectHydratorConfiguration<T> x, object y )
        {
            return x.Equals( y );
        }

        public static bool operator !=( ObjectHydratorConfiguration<T> x, object y )
        {
            return !x.Equals( y );
        }
    }
}