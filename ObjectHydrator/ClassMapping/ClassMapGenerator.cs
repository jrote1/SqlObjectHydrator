using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator.ClassMapping
{
	internal static class ClassMapGenerator
	{
		public static ClassMapResult Generate<T>( IDataReader dataReader, Type configurationType = null ) where T : new()
		{
			var mappings = new Mapping();
			if ( configurationType != null )
			{
				var configuration = (IObjectHydratorConfiguration)Activator.CreateInstance( configurationType );
				configuration.Mapping( mappings );
			}

			var classMapResult = new ClassMapResult
			{
				Mappings = mappings
			};

			var baseType = ( typeof( T ).IsGenericType && typeof( T ).GetGenericTypeDefinition() == typeof( List<> ) ) ? typeof( T ).GetGenericArguments()[ 0 ] : typeof( T );
			if ( !mappings.TableMaps.ContainsValue( baseType ) )
				mappings.TableMaps.Add( 0, baseType );

			mappings.TableMaps = mappings.TableMaps.OrderBy( x => x.Key ).ToDictionary( x => x.Key, x => x.Value );

			return classMapResult;
		}
	}
}