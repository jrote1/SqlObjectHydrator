using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection.Emit;
using SqlObjectHydrator.ClassMapping;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator
{
    internal class MappingGenerator
    {
        public Func<IDataReader, List<LambdaExpression>, List<T>> GenerateMapping<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration ) where T : new()
        {
            var classMap = new ClassMappingGenerator().GenerateMap( dataReader, configuration );

            var method = new DynamicMethod( "", typeof ( List<T> ), new[]
            {
                typeof ( IDataReader ),
                typeof ( List<LambdaExpression> )
            } );

            var emiter = method.GetILGenerator();

            //Define While Labels
            var whileIf = emiter.DefineLabel();
            var whileStart = emiter.DefineLabel();

            //Declare Variables
            var result = emiter.DeclareLocal( typeof ( List<T> ) );
            var tempRoot = emiter.DeclareLocal( typeof ( T ) );

            //Create Array With Compiled Maps
            var compiledMaps = new LocalBuilder[ configuration.MappingsActions.Count ];
            for ( int index = 0; index < configuration.MappingsActions.Count; index++ )
            {
                compiledMaps[ index ] = emiter.DeclareLocal( configuration.MappingsActions[ index ].Value.Compile().GetType() );
                emiter.Emit( OpCodes.Ldarg_1 );
                emiter.Emit( OpCodes.Ldc_I4, index );
                emiter.Emit( OpCodes.Callvirt, typeof ( List<LambdaExpression> ).GetProperty( "Item" ).GetGetMethod() );
                emiter.Emit( OpCodes.Callvirt, typeof ( LambdaExpression ).GetMethod( "Compile", new Type[ 0 ] ) );
                emiter.Emit( OpCodes.Castclass, configuration.MappingsActions[ index ].Value.Compile().GetType() );
                emiter.Emit( OpCodes.Stloc, compiledMaps[ index ] );
            }

            //Create Result Variable
            emiter.Emit( OpCodes.Newobj, typeof ( List<T> ).GetConstructor( new Type[ 0 ] ) );
            emiter.Emit( OpCodes.Stloc, result );

            //Start While Loop
            emiter.Emit( OpCodes.Br, whileIf );
            emiter.MarkLabel( whileStart );

            //Create Root Variable
            emiter.Emit( OpCodes.Newobj, typeof ( T ).GetConstructor( new Type[ 0 ] ) );
            emiter.Emit( OpCodes.Stloc, tempRoot );

            //Processes All Properties
            SetProperties<T>( classMap, emiter, compiledMaps, configuration, tempRoot );

            //Add Root Variable To Result
            emiter.Emit( OpCodes.Ldloc, result );
            emiter.Emit( OpCodes.Ldloc, tempRoot );
            emiter.Emit( OpCodes.Callvirt, typeof ( List<T> ).GetMethod( "Add" ) );

            //While If
            emiter.MarkLabel( whileIf );
            emiter.Emit( OpCodes.Ldarg_0 );
            emiter.Emit( OpCodes.Callvirt, typeof ( IDataReader ).GetMethod( "Read" ) );
            emiter.Emit( OpCodes.Brtrue, whileStart ); //Continue While Loop

            //Return Result
            emiter.Emit( OpCodes.Ldloc, result );
            emiter.Emit( OpCodes.Ret );

            return (Func<IDataReader, List<LambdaExpression>, List<T>>)method.CreateDelegate( typeof ( Func<IDataReader, List<LambdaExpression>, List<T>> ) );
        }

        private static void SetProperties<T>( ClassMap classMap, ILGenerator emiter, LocalBuilder[] compiledMaps, ObjectHydratorConfiguration<T> configuration, LocalBuilder localBuilder ) where T : new()
        {
            foreach ( var property in classMap.Propertys )
            {
                if ( property is ClassMap )
                {
                    var local = emiter.DeclareLocal( property.Type );
                    emiter.Emit( OpCodes.Newobj, property.Type.GetConstructor( new Type[ 0 ] ) );
                    emiter.Emit( OpCodes.Stloc, local );
                    emiter.Emit( OpCodes.Ldloc, localBuilder );
                    emiter.Emit( OpCodes.Ldloc, local );
                    emiter.Emit( OpCodes.Callvirt, classMap.Type.GetProperty( property.Name ).GetSetMethod() );
                    SetProperties<T>( (ClassMap)property, emiter, compiledMaps, configuration, local );
                }
                else
                {
                    if ( property.FieldId.HasValue )
                    {
                        if ( property.Nullable )
                        {
                            var ifNotNull = emiter.DefineLabel();
                            emiter.Emit( OpCodes.Ldarg_0 );
                            emiter.Emit( OpCodes.Ldc_I4, property.FieldId.Value );
                            emiter.Emit( OpCodes.Callvirt, typeof ( IDataRecord ).GetMethod( "GetValue" ) );
                            emiter.Emit( OpCodes.Ldsfld, typeof ( DBNull ).GetField( "Value" ) );
                            emiter.Emit( OpCodes.Ceq );
                            emiter.Emit( OpCodes.Brtrue, ifNotNull );

                            emiter.Emit( OpCodes.Ldloc, localBuilder );
                            emiter.Emit( OpCodes.Ldarg_0 );
                            emiter.Emit( OpCodes.Ldc_I4, property.FieldId.Value );
                            emiter.Emit( OpCodes.Callvirt, typeof ( IDataRecord ).GetMethod( GetDataReaderMethodName( property.Type ) ) );
                            emiter.Emit( OpCodes.Newobj, classMap.Type.GetProperty( property.Name ).PropertyType.GetConstructor( new Type[]
                            {
                                property.Type
                            } ) );
                            emiter.Emit( OpCodes.Callvirt, typeof ( T ).GetProperty( property.Name ).GetSetMethod() );
                            emiter.MarkLabel( ifNotNull );
                        }
                        else
                        {
                            emiter.Emit( OpCodes.Ldloc, localBuilder );
                            emiter.Emit( OpCodes.Ldarg_0 );
                            emiter.Emit( OpCodes.Ldc_I4, property.FieldId.Value );
                            emiter.Emit( OpCodes.Callvirt, typeof ( IDataRecord ).GetMethod( GetDataReaderMethodName( property.Type ) ) );
                            emiter.Emit( OpCodes.Callvirt, typeof ( T ).GetProperty( property.Name ).GetSetMethod() );
                        }
                    }
                    else
                    {
                        emiter.Emit( OpCodes.Ldloc, localBuilder );
                        emiter.Emit( OpCodes.Ldloc, compiledMaps[ property.ConfigurationMapId.Value ] );
                        emiter.Emit( OpCodes.Ldarg_0 );
                        emiter.Emit( OpCodes.Callvirt, configuration.MappingsActions[ property.ConfigurationMapId.Value ].Value.Compile().GetType().GetMethod( "Invoke" ) );
                        emiter.Emit(OpCodes.Callvirt, classMap.Type.GetProperty(property.Name).GetSetMethod());
                    }
                }
            }
        }

        private static string GetDataReaderMethodName( Type propertyType )
        {
            if ( propertyType == typeof ( Boolean ) )
                return "GetBoolean";
            if ( propertyType == typeof ( Byte ) )
                return "GetByte";
            if ( propertyType == typeof ( Char ) )
                return "GetChar";
            if ( propertyType == typeof ( DateTime ) )
                return "GetDateTime";
            if ( propertyType == typeof ( Decimal ) )
                return "GetDecimal";
            if ( propertyType == typeof ( Double ) )
                return "GetDouble";
            if ( propertyType == typeof ( Single ) )
                return "GetFloat";
            if ( propertyType == typeof ( Guid ) )
                return "GetGuid";
            if ( propertyType == typeof ( Int16 ) )
                return "GetInt16";
            if ( propertyType == typeof ( Int32 ) )
                return "GetInt32";
            if ( propertyType == typeof ( Int64 ) )
                return "GetInt64";
            return "GetString";
        }
    }
}