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

			var method = new DynamicMethod( "", typeof( List<T> ), new[]
			{
				typeof( IDataReader ),
				typeof( List<LambdaExpression> )
			} );

			var emitter = method.GetILGenerator();

			//Define While Labels
			var whileIf = emitter.DefineLabel();
			var whileStart = emitter.DefineLabel();

			//Declare Variables
			var result = emitter.DeclareLocal( typeof( List<T> ) );
			var tempRoot = emitter.DeclareLocal( typeof( T ) );

			//Create Array With Compiled Maps
			var compiledMaps = CompileMaps( configuration, emitter );

			//Create Result Variable
			emitter.Emit( OpCodes.Newobj, typeof( List<T> ).GetConstructor( new Type[ 0 ] ) );
			emitter.Emit( OpCodes.Stloc, result );

			//Start While Loop
			emitter.Emit( OpCodes.Br, whileIf );
			emitter.MarkLabel( whileStart );

			//Create Root Variable
			emitter.Emit( OpCodes.Newobj, typeof( T ).GetConstructor( new Type[ 0 ] ) );
			emitter.Emit( OpCodes.Stloc, tempRoot );

			//Processes All Properties
			SetProperties( classMap, emitter, compiledMaps, configuration, tempRoot );

			//Add Root Variable To Result
			emitter.Emit( OpCodes.Ldloc, result );
			emitter.Emit( OpCodes.Ldloc, tempRoot );
			emitter.Emit( OpCodes.Callvirt, typeof( List<T> ).GetMethod( "Add" ) );

			//While If
			emitter.MarkLabel( whileIf );
			emitter.Emit( OpCodes.Ldarg_0 );
			emitter.Emit( OpCodes.Callvirt, typeof( IDataReader ).GetMethod( "Read" ) );
			emitter.Emit( OpCodes.Brtrue, whileStart ); //Continue While Loop

			//Return Result
			emitter.Emit( OpCodes.Ldloc, result );
			emitter.Emit( OpCodes.Ret );

			return (Func<IDataReader, List<LambdaExpression>, List<T>>)method.CreateDelegate( typeof( Func<IDataReader, List<LambdaExpression>, List<T>> ) );
		}

		public Func<IDataReader, List<LambdaExpression>, T> GenerateSingleObjectMapping<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration ) where T : new()
		{
			var classMap = new ClassMappingGenerator().GenerateMap( dataReader, configuration );

			var method = new DynamicMethod( "", typeof( T ), new[]
			{
				typeof( IDataReader ),
				typeof( List<LambdaExpression> )
			} );

			var emitter = method.GetILGenerator();

			var result = emitter.DeclareLocal( typeof( T ) );

			var compiledMaps = CompileMaps( configuration, emitter );

			emitter.Emit( OpCodes.Newobj, typeof( T ).GetConstructor( new Type[ 0 ] ) );
			emitter.Emit( OpCodes.Stloc, result );

			SetProperties( classMap, emitter, compiledMaps, configuration, result );

			emitter.Emit( OpCodes.Ldloc, result );
			emitter.Emit( OpCodes.Ret );

			return (Func<IDataReader, List<LambdaExpression>, T>)method.CreateDelegate( typeof( Func<IDataReader, List<LambdaExpression>, T> ) );
		}

		private static LocalBuilder[] CompileMaps<T>( ObjectHydratorConfiguration<T> configuration, ILGenerator emitter ) where T : new()
		{
			var compiledMaps = new LocalBuilder[ configuration.MappingsActions.Count ];
			for ( int index = 0; index < configuration.MappingsActions.Count; index++ )
			{
				compiledMaps[ index ] = emitter.DeclareLocal( configuration.MappingsActions[ index ].Value.Compile().GetType() );
				emitter.Emit( OpCodes.Ldarg_1 );
				emitter.Emit( OpCodes.Ldc_I4, index );
				emitter.Emit( OpCodes.Callvirt, typeof( List<LambdaExpression> ).GetProperty( "Item" ).GetGetMethod() );
				emitter.Emit( OpCodes.Callvirt, typeof( LambdaExpression ).GetMethod( "Compile", new Type[ 0 ] ) );
				emitter.Emit( OpCodes.Castclass, configuration.MappingsActions[ index ].Value.Compile().GetType() );
				emitter.Emit( OpCodes.Stloc, compiledMaps[ index ] );
			}
			return compiledMaps;
		}

		private static void SetProperties<T>( ClassMap classMap, ILGenerator emitter, LocalBuilder[] compiledMaps, ObjectHydratorConfiguration<T> configuration, LocalBuilder localBuilder ) where T : new()
		{
			foreach ( var property in classMap.Propertys )
			{
				if ( property is ClassMap )
				{
					var local = emitter.DeclareLocal( property.Type );
					emitter.Emit( OpCodes.Newobj, property.Type.GetConstructor( new Type[ 0 ] ) );
					emitter.Emit( OpCodes.Stloc, local );
					emitter.Emit( OpCodes.Ldloc, localBuilder );
					emitter.Emit( OpCodes.Ldloc, local );
					emitter.Emit( OpCodes.Callvirt, classMap.Type.GetProperty( property.Name ).GetSetMethod() );
					SetProperties<T>( (ClassMap)property, emitter, compiledMaps, configuration, local );
				}
				else
				{
					if ( property.FieldId.HasValue )
					{
						if ( property.Nullable )
						{
							var ifNotNull = emitter.DefineLabel();
							emitter.Emit( OpCodes.Ldarg_0 );
							emitter.Emit( OpCodes.Ldc_I4, property.FieldId.Value );
							emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( "GetValue" ) );
							emitter.Emit( OpCodes.Ldsfld, typeof( DBNull ).GetField( "Value" ) );
							emitter.Emit( OpCodes.Ceq );
							emitter.Emit( OpCodes.Brtrue, ifNotNull );

							emitter.Emit( OpCodes.Ldloc, localBuilder );
							emitter.Emit( OpCodes.Ldarg_0 );
							emitter.Emit( OpCodes.Ldc_I4, property.FieldId.Value );
							emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( GetDataReaderMethodName( property.Type ) ) );
							emitter.Emit( OpCodes.Newobj, classMap.Type.GetProperty( property.Name ).PropertyType.GetConstructor( new Type[]
							{
								property.Type
							} ) );
							emitter.Emit( OpCodes.Callvirt, typeof( T ).GetProperty( property.Name ).GetSetMethod() );
							emitter.MarkLabel( ifNotNull );
						}
						else
						{
							emitter.Emit( OpCodes.Ldloc, localBuilder );
							emitter.Emit( OpCodes.Ldarg_0 );
							emitter.Emit( OpCodes.Ldc_I4, property.FieldId.Value );
							emitter.Emit( OpCodes.Callvirt, typeof( IDataRecord ).GetMethod( GetDataReaderMethodName( property.Type ) ) );
							emitter.Emit( OpCodes.Callvirt, typeof( T ).GetProperty( property.Name ).GetSetMethod() );
						}
					}
					else
					{
						emitter.Emit( OpCodes.Ldloc, localBuilder );
						emitter.Emit( OpCodes.Ldloc, compiledMaps[ property.ConfigurationMapId.Value ] );
						emitter.Emit( OpCodes.Ldarg_0 );
						emitter.Emit( OpCodes.Callvirt, configuration.MappingsActions[ property.ConfigurationMapId.Value ].Value.Compile().GetType().GetMethod( "Invoke" ) );
						emitter.Emit( OpCodes.Callvirt, classMap.Type.GetProperty( property.Name ).GetSetMethod() );
					}
				}
			}
		}

		private static string GetDataReaderMethodName( Type propertyType )
		{
			if ( propertyType == typeof( Boolean ) )
				return "GetBoolean";
			if ( propertyType == typeof( Byte ) )
				return "GetByte";
			if ( propertyType == typeof( Char ) )
				return "GetChar";
			if ( propertyType == typeof( DateTime ) )
				return "GetDateTime";
			if ( propertyType == typeof( Decimal ) )
				return "GetDecimal";
			if ( propertyType == typeof( Double ) )
				return "GetDouble";
			if ( propertyType == typeof( Single ) )
				return "GetFloat";
			if ( propertyType == typeof( Guid ) )
				return "GetGuid";
			if ( propertyType == typeof( Int16 ) )
				return "GetInt16";
			if ( propertyType == typeof( Int32 ) )
				return "GetInt32";
			if ( propertyType == typeof( Int64 ) )
				return "GetInt64";
			return "GetString";
		}
	}
}