using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlObjectHydrator.Caching
{
	internal static class CacheManager
	{
		internal static readonly Dictionary<int, object> MappingCaches;

		static CacheManager()
		{
			MappingCaches = new Dictionary<int, object>();
		}

		public static void StoreMappingCache<T>( Func<MappingCache<T>> mappingCache, IDataReader dataReader, Type configurationType )
		{
			var cacheHashCode = GetCacheHashCode<T>( dataReader, configurationType );
			MappingCaches.Add( cacheHashCode, mappingCache() );
		}

		public static int GetCacheHashCode<T>( IDataReader dataReader, Type configurationType )
		{
			var result = typeof( T ).GetHashCode() + GetReaderHashCode( dataReader );
			if ( configurationType != null )
			{
				result += configurationType.GetHashCode();
			}
			return result;
		}

		public static bool ContainsMappingCache<T>( IDataReader dataReader, Type configurationType )
		{
			return MappingCaches.ContainsKey( GetCacheHashCode<T>( dataReader, configurationType ) );
		}

		public static MappingCache<T> GetMappingCache<T>( IDataReader dataReader, Type configurationType )
		{
			return (MappingCache<T>)MappingCaches.FirstOrDefault( x => x.Key == GetCacheHashCode<T>( dataReader, configurationType ) ).Value;
		}

		public static int GetReaderHashCode( IDataReader dataReader )
		{
			var hashcode = 0;
			for ( var i = 0; i < dataReader.FieldCount; i++ )
			{
				hashcode += dataReader.GetFieldType( i ).GetHashCode();
				hashcode += dataReader.GetName( i ).GetHashCode();
			}
			return hashcode;
		}
	}
}