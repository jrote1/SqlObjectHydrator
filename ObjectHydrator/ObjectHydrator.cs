using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SqlObjectHydrator.Caching;
using SqlObjectHydrator.ClassMapping;
using SqlObjectHydrator.ILEmitting;

namespace SqlObjectHydrator
{
	public static class ObjectHydrator
	{
		public static List<T> DataReaderToList<T>( this IDataReader dataReader, Type configuration = null ) where T : new()
		{
			return Run<List<T>>( dataReader, configuration );
		}

		private static object lockObj = new object();

		private static T Run<T>( IDataReader dataReader, Type configuration ) where T : new()
		{
			if ( !CacheManager.ContainsMappingCache<T>( dataReader, configuration ) )
			{
				lock ( lockObj )
				{
					if ( !CacheManager.ContainsMappingCache<T>( dataReader, configuration ) )
					{
						var hashcode = CacheManager.GetReaderHashCode( dataReader );
						var classMapResult = new ClassMapResult();
						CacheManager.StoreMappingCache( () => GetMappingCache<T>( dataReader, configuration, out classMapResult ), dataReader, configuration );
					}
				}
			}
			return CacheManager.GetMappingCache<T>( dataReader, configuration ).Run( dataReader );
		}

		private static MappingCache<T> GetMappingCache<T>( IDataReader dataReader, Type configuration, out ClassMapResult classMapResult ) where T : new()
		{
			classMapResult = ClassMapGenerator.Generate<T>( dataReader, configuration );
			var func = MappingGenerator.Generate<T>( dataReader, classMapResult );
			return new MappingCache<T>( func, FuncMappingsGenerator.Generate( classMapResult.Mappings ) );
		}
	}

	internal static class FuncMappingsGenerator
	{
		public static Dictionary<MappingEnum, object> Generate( Mapping mappings )
		{
			return new Dictionary<MappingEnum, object>
			{
				{
					MappingEnum.DictionaryJoin, mappings.DictionaryTableJoins.Select( x => new KeyValuePair<object, object>( x.Condition, x.Destination ) ).ToList()
				},
				{
					MappingEnum.Join, mappings.Joins.Select( x => x.Value ).ToList()
				},
				{
					MappingEnum.TableJoin, mappings.TableJoins.Select( x => new KeyValuePair<object, object>( x.Value.Key, x.Value.Value ) ).ToList()
				},
				{
					MappingEnum.PropertyMap, mappings.PropertyMaps.ToDictionary( x=>x.Key,x=>x.Value.Value )
				}
			};
		}
	}
}