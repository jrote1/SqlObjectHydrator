using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator.DataReaderMapping
{
    internal class MappingCache
    {
        internal static Dictionary<Int32, Object> InternalMappingCache;

        static MappingCache()
        {
            InternalMappingCache = new Dictionary<Int32, Object>();
        }

        public bool ContainsMapping<T,TReturn>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration ) where T : new()
        {
            return InternalMappingCache.ContainsKey( GetKey<T,TReturn>( dataReader, configuration ) );
        }

        public Func<IDataReader, List<LambdaExpression>, TReturn> GetCachedMapping<T,TReturn>(IDataReader dataReader, ObjectHydratorConfiguration<T> configuration) where T : new()
        {
            var key = GetKey<T,TReturn>( dataReader, configuration );
            if ( InternalMappingCache.ContainsKey( key ) )
                return (Func<IDataReader, List<LambdaExpression>, TReturn>)InternalMappingCache[key];
            return null;
        }

        public void StoreMapping<T,TReturn>(IDataReader dataReader, ObjectHydratorConfiguration<T> configuration, Func<IDataReader, List<LambdaExpression>, TReturn> mapping) where T : new()
        {
            InternalMappingCache.Add( GetKey<T,TReturn>( dataReader, configuration ), mapping );
        }

        private static Int32 GetKey<T,TReturn>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration ) where T : new()
        {
            return typeof(TReturn).GetHashCode() + GetDataReaderHashCode( dataReader ) + configuration.GetHashCode();
        }

        private static Int32 GetDataReaderHashCode( IDataReader dataReader )
        {
            var result = 0;
            for ( var i = 0; i < dataReader.FieldCount; i++ )
            {
                result += dataReader.GetName( i ).GetHashCode();
                result += dataReader.GetFieldType( i ).GetHashCode();
            }
            return result;
        }
    }
}