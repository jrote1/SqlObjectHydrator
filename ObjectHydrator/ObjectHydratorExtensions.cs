using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.DataReaderMapping;

namespace SqlObjectHydrator
{
	public static class ObjectHydratorExtensions
	{
		private static readonly Object StoreMappingLock = new object();
		private static readonly MappingCache MappingCache;
		private static readonly MappingGenerator MappingGenerator;

		static ObjectHydratorExtensions()
		{
			MappingCache = new MappingCache();
			MappingGenerator = new MappingGenerator();
		}

		public static List<T> DataReaderToList<T>( this IDataReader dataReader, ObjectHydratorConfiguration<T> configuration = null ) where T : new()
		{
			configuration = configuration ?? new ObjectHydratorConfiguration<T>();
			var cachedMapping = GetMapping( dataReader, configuration, () => MappingGenerator.GenerateMapping( dataReader, configuration ) );
			return cachedMapping( dataReader, configuration.MappingsActions.Select( x => x.Value ).ToList() );
		}

		private static Func<IDataReader, List<LambdaExpression>, TReturn> GetMapping<T, TReturn>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration, Func<Func<IDataReader, List<LambdaExpression>, TReturn>> generateMapping ) where T : new()
		{
			var containsMapping = ContainsMapping<T, TReturn>( dataReader, configuration, MappingCache );
			if ( !containsMapping )
			{
				lock ( StoreMappingLock )
				{
					if ( !ContainsMapping<T, TReturn>( dataReader, configuration, MappingCache ) )
					{
						MappingCache.StoreMapping( dataReader, configuration, generateMapping() );
					}
				}
			}
			var cachedMapping = MappingCache.GetCachedMapping<T, TReturn>( dataReader, configuration );
			return cachedMapping;
		}

		public static T DataReaderToObject<T>( this IDataReader dataReader, ObjectHydratorConfiguration<T> configuration = null ) where T : new()
		{
			configuration = configuration ?? new ObjectHydratorConfiguration<T>();
			var cachedMapping = GetMapping( dataReader, configuration, () => MappingGenerator.GenerateSingleObjectMapping( dataReader, configuration ) );
			return cachedMapping( dataReader, configuration.MappingsActions.Select( x => x.Value ).ToList() );
		}

		private static bool ContainsMapping<T, TReturn>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration, MappingCache mappingCache ) where T : new()
		{
			return mappingCache.ContainsMapping<T, TReturn>( dataReader, configuration );
		}
	}
}