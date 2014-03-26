using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator.ClassMapping
{
	internal  static class ClassMapGenerator
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

			var tableMaps = mappings.TableMaps.OrderBy( x => x.Key ).ToDictionary( x => x.Key, x => x.Value );

			var currentTableId = 0;

			do
			{
				if ( tableMaps.ContainsKey( currentTableId ) )
				{
					var tableType = tableMaps[ currentTableId ];
					var classMap = new ClassMap( tableType, currentTableId );
					
					for ( var i = 0; i < dataReader.FieldCount; i++ )
					{
						if ( tableType != typeof( ExpandoObject ) )
						{
							var propertyInfo = tableType.GetProperty( dataReader.GetName( i ) );
							if ( propertyInfo != null && propertyInfo.GetActualPropertyType() == dataReader.GetFieldType( i ) )
								classMap.Properties.Add( new PropertyMap( propertyInfo, i )
								{
									Nullable = propertyInfo.IsNullable()
								} );
						}
						else
						{
							classMap.Properties.Add( new ExpandoPropertyMap( dataReader.GetName( i ).Replace( " ", "" ), dataReader.GetFieldType( i ), i ) );
						}
					}

					var tempTableData = new List<Dictionary<int, object>>();
					while ( dataReader.Read() )
					{
						var data = new Dictionary<int, object>();
						for ( var i = 0; i < dataReader.FieldCount; i++ )
						{
							data.Add( i, dataReader.GetValue( i ) );
						}
						tempTableData.Add( data );
					}
					classMapResult.TempDataStorage.Add( tempTableData );
					classMapResult.ClassMaps.Add( classMap );
					
				}
				currentTableId++;
			} while ( dataReader.NextResult() );
			return classMapResult;
		}
	}
}