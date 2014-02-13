using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator.ClassMapping
{
    internal class ClassMappingGenerator
    {
        public ClassMap GenerateMap<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration ) where T : new()
        {
            var properties = typeof ( T ).GetProperties();

            var result = new ClassMap
            {
                Type = typeof ( T ),
                Properties = new List<PropertyMap>()
            };

            for ( var i = 0; i < dataReader.FieldCount; i++ )
            {
                var fieldName = dataReader.GetName( i );
                var fieldType = dataReader.GetFieldType( i );
                if ( ContainsProperty( properties, fieldName, fieldType ) )
                    result.Properties.Add( new PropertyMap
                    {
                        Name = fieldName,
                        FieldId = i,
                        Type = fieldType,
                        Nullable = IsPropertyNullable( properties.Single( x => x.Name == fieldName ) )
                    } );
            }
            foreach ( var source in configuration.MappingsActions )
            {
                var currentClass = result;
                var parts = source.Key.ToString().Replace( ")", "" ).Split( '.' );
                for ( int i = 1; i < parts.Length; i++ )
                {
                    if ( ( i + 1 ) == parts.Length )
                    {
                        if ( currentClass.Properties.Any( x => x.Name == parts[ i ] ) )
                            currentClass.Properties.RemoveAll( x => x.Name == parts[ i ] );
                        currentClass.Properties.Add( new PropertyMap
                        {
                            Name = parts[ i ],
                            ConfigurationMapId = configuration.MappingsActions.IndexOf( source ),
                            Type = source.Key.ReturnType
                        } );
                    }
                    else
                    {
                        if ( currentClass.Properties.Any( x => x.Name == parts[ i ] && x.GetType() == typeof ( ClassMap ) ) )
                            currentClass = currentClass.Properties.Single( x => x.Name == parts[ i ] ) as ClassMap;
                        else
                        {
                            var classMap = new ClassMap
                            {
                                Name = parts[ i ],
                                Type = currentClass.Type.GetProperty( parts[ i ] ).PropertyType,
                                Properties = new List<PropertyMap>()
                            };
                            currentClass.Properties.Add( classMap );
                            currentClass = classMap;
                        }
                    }
                }
            }

            return result;
        }


        private static bool ContainsProperty( PropertyInfo[] properties, string fieldName, Type fieldType )
        {
            if ( properties.All( x => x.Name != fieldName ) )
                return false;
            var property = properties.Single( x => x.Name == fieldName );
            var actualType = property.PropertyType;
            if ( IsPropertyNullable( property ) )
                actualType = property.PropertyType.GetGenericArguments()[ 0 ];
            return actualType == fieldType;
        }

        private static bool IsPropertyNullable( PropertyInfo property )
        {
            if ( !property.PropertyType.IsGenericType )
                return false;
            return property.PropertyType.GetGenericTypeDefinition() == typeof ( Nullable<> );
        }
    }
}