using SqlObjectHydrator.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlObjectHydrator.ClassMapping
{
	internal static class ClassMapGenerator
	{
		public static ClassMapResult Generate<T>(
		  IDataReader dataReader,
		  Type configurationType = null )
		  where T : new()
		{
			Mapping mapping = new Mapping();
			if ( configurationType != (Type)null )
				( (IObjectHydratorConfiguration)Activator.CreateInstance( configurationType ) ).Mapping( (IMapping)mapping );
			ClassMapResult classMapResult = new ClassMapResult()
			{
				Mappings = mapping
			};
			Type type = !typeof( T ).IsGenericType || !( typeof( T ).GetGenericTypeDefinition() == typeof( List<> ) ) ? typeof( T ) : typeof( T ).GetGenericArguments()[ 0 ];
			if ( !mapping.TableMaps.ContainsValue( type ) )
				mapping.TableMaps.Add( 0, type );
			mapping.TableMaps = mapping.TableMaps.OrderBy<KeyValuePair<int, Type>, int>( (Func<KeyValuePair<int, Type>, int>)( x => x.Key ) ).ToDictionary<KeyValuePair<int, Type>, int, Type>( (Func<KeyValuePair<int, Type>, int>)( x => x.Key ), (Func<KeyValuePair<int, Type>, Type>)( x => x.Value ) );
			return classMapResult;
		}
	}
}
