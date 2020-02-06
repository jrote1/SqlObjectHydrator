using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlObjectHydrator.Caching
{
	internal static class CacheManager
	{
		internal static readonly Dictionary<int, object> MappingCaches = new Dictionary<int, object>();

		public static void StoreMappingCache<T>(
		  Func<MappingCache<T>> mappingCache,
		  IDataReader dataReader,
		  Type configurationType )
		{
			int cacheHashCode = CacheManager.GetCacheHashCode<T>( dataReader, configurationType );
			CacheManager.MappingCaches.Add( cacheHashCode, (object)mappingCache() );
		}

		public static int GetCacheHashCode<T>( IDataReader dataReader, Type configurationType )
		{
			int num = typeof( T ).GetHashCode() + CacheManager.GetReaderHashCode( dataReader );
			if ( configurationType != (Type)null )
				num += configurationType.GetHashCode();
			return num;
		}

		public static bool ContainsMappingCache<T>( IDataReader dataReader, Type configurationType )
		{
			return CacheManager.MappingCaches.ContainsKey( CacheManager.GetCacheHashCode<T>( dataReader, configurationType ) );
		}

		public static MappingCache<T> GetMappingCache<T>(
		  IDataReader dataReader,
		  Type configurationType )
		{
			return (MappingCache<T>)CacheManager.MappingCaches.FirstOrDefault<KeyValuePair<int, object>>( (Func<KeyValuePair<int, object>, bool>)( x => x.Key == CacheManager.GetCacheHashCode<T>( dataReader, configurationType ) ) ).Value;
		}

		public static int GetReaderHashCode( IDataReader dataReader )
		{
			int num = 0;
			for ( int i = 0; i < dataReader.FieldCount; ++i )
				num = num + dataReader.GetFieldType( i ).GetHashCode() + dataReader.GetName( i ).GetHashCode();
			return num;
		}
	}
}
