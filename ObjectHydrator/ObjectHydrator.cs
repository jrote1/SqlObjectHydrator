using SqlObjectHydrator.Caching;
using SqlObjectHydrator.ClassMapping;
using SqlObjectHydrator.ILEmitting;
using System;
using System.Collections.Generic;
using System.Data;

namespace SqlObjectHydrator
{
	public static class ObjectHydrator
	{
		private static object lockObj = new object();

		public static List<T> DataReaderToList<T>( this IDataReader dataReader, Type configuration = null ) where T : new()
		{
			return ObjectHydrator.Run<List<T>>( dataReader, configuration );
		}

		private static T Run<T>( IDataReader dataReader, Type configuration ) where T : new()
		{
			if ( !CacheManager.ContainsMappingCache<T>( dataReader, configuration ) )
			{
				lock ( ObjectHydrator.lockObj )
				{
					if ( !CacheManager.ContainsMappingCache<T>( dataReader, configuration ) )
					{
						CacheManager.GetReaderHashCode( dataReader );
						ClassMapResult classMapResult = new ClassMapResult();
						CacheManager.StoreMappingCache<T>( (Func<MappingCache<T>>)( () => ObjectHydrator.GetMappingCache<T>( dataReader, configuration, out classMapResult ) ), dataReader, configuration );
					}
				}
			}
			return CacheManager.GetMappingCache<T>( dataReader, configuration ).Run( dataReader );
		}

		private static MappingCache<T> GetMappingCache<T>(
		  IDataReader dataReader,
		  Type configuration,
		  out ClassMapResult classMapResult )
		  where T : new()
		{
			classMapResult = ClassMapGenerator.Generate<T>( dataReader, configuration );
			return new MappingCache<T>( MappingGenerator.Generate<T>( dataReader, classMapResult ), FuncMappingsGenerator.Generate( classMapResult.Mappings ) );
		}
	}
}
