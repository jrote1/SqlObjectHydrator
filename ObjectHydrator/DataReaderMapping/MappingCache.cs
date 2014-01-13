using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using ObjectHydrator.Configuration;

namespace ObjectHydrator.DataReaderMapping
{
    internal class MappingCache
    {
        private static readonly Dictionary<Int32, Object> InternalMappingCache;

        static MappingCache()
        {
            InternalMappingCache = new Dictionary<Int32, Object>();
        }

        public bool ContainsMapping<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration ) where T : new()
        {
            return InternalMappingCache.ContainsKey( GetKey( dataReader, configuration ) );
        }

        public Func<IDataReader, List<LambdaExpression>, List<T>> GetCachedMapping<T>(IDataReader dataReader, ObjectHydratorConfiguration<T> configuration) where T : new()
        {
            var key = GetKey( dataReader, configuration );
            if ( InternalMappingCache.ContainsKey( key ) )
                return (Func<IDataReader, List<LambdaExpression>, List<T>>)InternalMappingCache[key];
            return null;
        }

        public void StoreMapping<T>(IDataReader dataReader, ObjectHydratorConfiguration<T> configuration, Func<IDataReader, List<LambdaExpression>, List<T>> mapping) where T : new()
        {
            InternalMappingCache.Add( GetKey( dataReader, configuration ), mapping );
        }

        private static Int32 GetKey<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration ) where T : new()
        {
            return GetDataReaderHashCode( dataReader ) + configuration.GetHashCode();
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