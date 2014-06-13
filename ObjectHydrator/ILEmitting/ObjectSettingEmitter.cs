using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Pfz.TypeBuilding;
using SqlObjectHydrator.ClassMapping;

namespace SqlObjectHydrator.ILEmitting
{
	internal static class ObjectSettingEmitter
	{
		public static LocalBuilder Emit( ILGenerator emitter, Type type, Mapping mapping )
		{
			var listType = typeof( List<> ).MakeGenericType( type );
			var resultLocalBuilder = emitter.DeclareLocal( listType );
			emitter.Emit( OpCodes.Newobj, listType.GetConstructors()[ 0 ] );
			emitter.Emit( OpCodes.Stloc, resultLocalBuilder );

			var tempRoot = emitter.DeclareLocal( type );

			LocalBuilder variableTableTypeFunc = null;

			if ( mapping.VariableTableTypes.ContainsKey( type ) )
			{
				variableTableTypeFunc = emitter.DeclareLocal( typeof( Func<IDataRecord, Type> ) );

				emitter.Emit( OpCodes.Ldarg_1 );
				emitter.Emit( OpCodes.Ldc_I4, (int)MappingEnum.VariableTableType );
				emitter.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				emitter.Emit( OpCodes.Castclass, typeof( Dictionary<Type, object> ) );


				emitter.Emit( OpCodes.Ldtoken, type );
				emitter.Emit( OpCodes.Call, typeof( Type ).GetMethod( "GetTypeFromHandle" ) );
				emitter.Emit( OpCodes.Callvirt, typeof( Dictionary<Type, object> ).GetProperty( "Item" ).GetGetMethod() );
				emitter.Emit( OpCodes.Castclass, typeof( Func<IDataRecord, Type> ) );
				emitter.Emit( OpCodes.Stloc, variableTableTypeFunc );
			}


			//Define While Labels
			var whileIf = emitter.DefineLabel();
			var whileStart = emitter.DefineLabel();

			//Start While Loop
			emitter.Emit( OpCodes.Br, whileIf );
			emitter.MarkLabel( whileStart );

			//Create Root Variable
			emitter.Emit( OpCodes.Newobj, type.GetConstructors()[ 0 ] );
			emitter.Emit( OpCodes.Stloc, tempRoot );

			//Get Type
			var typeLocal = emitter.DeclareLocal( typeof( Type ) );

			if ( mapping.VariableTableTypes.ContainsKey( type ) )
			{
				emitter.Emit( OpCodes.Ldloc, variableTableTypeFunc );
				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Callvirt, typeof( Func<IDataRecord, Type> ).GetMethod( "Invoke" ) );
				emitter.Emit( OpCodes.Stloc, typeLocal );
			}
			else
			{
				emitter.Emit( OpCodes.Ldtoken, type );
				emitter.Emit( OpCodes.Call, typeof( Type ).GetMethod( "GetTypeFromHandle" ) );
				emitter.Emit( OpCodes.Stloc, typeLocal );
			}


			var endIf = emitter.DefineLabel();

			emitter.Emit( OpCodes.Ldarg_2 );
			emitter.Emit( OpCodes.Ldloc, typeLocal );
			emitter.Emit( OpCodes.Call, typeof( Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>> ).GetMethod( "ContainsKey" ) );
			emitter.Emit( OpCodes.Brtrue, endIf );

			var objectMethodLocal = emitter.DeclareLocal( typeof( Func<IDataRecord, Dictionary<MappingEnum, object>, object> ) );

			emitter.Emit( OpCodes.Ldloc, typeLocal );
			emitter.Emit( OpCodes.Ldarg_0 );
			emitter.Emit( OpCodes.Ldarg_1 );
			emitter.Emit( OpCodes.Call, typeof( ObjectSettingEmitter ).GetMethod( "GenerateObjectBuilder" ) );
			emitter.Emit( OpCodes.Stloc, objectMethodLocal );

			emitter.Emit( OpCodes.Ldarg_2 );
			emitter.Emit( OpCodes.Ldloc, typeLocal );
			emitter.Emit( OpCodes.Ldloc, objectMethodLocal );
			emitter.Emit( OpCodes.Call, typeof( Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>> ).GetMethod( "Add" ) );

			emitter.MarkLabel( endIf );

			emitter.Emit( OpCodes.Ldarg_2 );
			emitter.Emit( OpCodes.Ldloc, typeLocal );
			emitter.Emit( OpCodes.Call, typeof( Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>> ).GetProperty( "Item" ).GetGetMethod() );
			emitter.Emit( OpCodes.Ldarg_0 );
			emitter.Emit( OpCodes.Ldarg_1 );
			emitter.Emit( OpCodes.Call, typeof( Func<IDataRecord, Dictionary<MappingEnum, object>, object> ).GetMethod( "Invoke" ) );
			emitter.Emit( OpCodes.Castclass, type );
			emitter.Emit( OpCodes.Stloc, tempRoot );


			//Add Root Variable To Result
			emitter.Emit( OpCodes.Ldloc, resultLocalBuilder );
			emitter.Emit( OpCodes.Ldloc, tempRoot );
			emitter.Emit( OpCodes.Callvirt, listType.GetMethod( "Add" ) );

			//While If
			emitter.MarkLabel( whileIf );
			emitter.Emit( OpCodes.Ldarg_0 );
			emitter.Emit( OpCodes.Callvirt, typeof( IDataReader ).GetMethod( "Read" ) );
			emitter.Emit( OpCodes.Brtrue, whileStart ); //Continue While Loop

			return resultLocalBuilder;
		}

		public static Func<IDataRecord, Dictionary<MappingEnum, object>, object> GenerateObjectBuilder( Type type, IDataRecord dataRecord, Dictionary<MappingEnum, object> mappings )
		{
			var propertyMaps = mappings.ContainsKey( MappingEnum.PropertyMap ) ? mappings[ MappingEnum.PropertyMap ] as Dictionary<PropertyInfo, object> : new Dictionary<PropertyInfo, object>();
			var filteredPropertyMaps = propertyMaps.Where( x => x.Key.DeclaringType == type ).ToList();

			var method = new DynamicMethod( "", typeof( object ), new[]
			{
				typeof( IDataRecord ),
				typeof( Dictionary<MappingEnum, object> )
			}, true );
			var emitter = method.GetILGenerator();


			var localPropertyMapsBuilders = new Dictionary<PropertyInfo, LocalBuilder>();
			if ( filteredPropertyMaps.Count > 0 )
			{
				var propertyMapsLocal = emitter.DeclareLocal( typeof( List<object> ) );

				emitter.Emit( OpCodes.Ldarg_1 );
				emitter.Emit( OpCodes.Ldc_I4, (int)MappingEnum.PropertyMap );
				emitter.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				emitter.Emit( OpCodes.Castclass, typeof( Dictionary<PropertyInfo, object> ) );
				emitter.Emit( OpCodes.Call, typeof( Dictionary<PropertyInfo, object> ).GetProperty( "Values" ).GetGetMethod() );
				emitter.Emit( OpCodes.Call, typeof( Enumerable ).GetMethod( "ToList" ).MakeGenericMethod( typeof( object ) ) );
				emitter.Emit( OpCodes.Stloc, propertyMapsLocal );

				foreach ( var propertyMap in filteredPropertyMaps )
				{
					var localBuilder = emitter.DeclareLocal( propertyMap.Value.GetType() );
					var index = propertyMaps.Keys.ToList().IndexOf( propertyMap.Key );

					emitter.Emit( OpCodes.Ldloc, propertyMapsLocal );
					emitter.Emit( OpCodes.Ldc_I4, index );
					emitter.Emit( OpCodes.Call, typeof( List<object> ).GetProperty( "Item" ).GetGetMethod() );
					emitter.Emit( OpCodes.Castclass, propertyMap.Value.GetType() );
					emitter.Emit( OpCodes.Stloc, localBuilder );
					localPropertyMapsBuilders.Add( propertyMap.Key, localBuilder );
				}
			}

			var objectVariable = emitter.DeclareLocal( type );

			emitter.Emit( OpCodes.Newobj, type.GetConstructors()[ 0 ] );
			emitter.Emit( OpCodes.Stloc, objectVariable );

			for ( var i = 0; i < dataRecord.FieldCount; i++ )
			{
				if ( type == typeof( ExpandoObject ) )
				{
					var valueLocalBuilder = emitter.DeclareLocal( dataRecord.GetFieldType( i ) );
					DataReaderEmitter.GetPropertyValue( emitter, dataRecord.GetFieldType( i ), i, false );
					emitter.Emit( OpCodes.Stloc, valueLocalBuilder );
					ExpandoObjectInteractor.SetExpandoProperty( emitter, objectVariable, dataRecord.GetName( i ), valueLocalBuilder );
				}
				else
				{
					var propertyInfo = type.GetProperty( dataRecord.GetName( i ) );
					if ( propertyInfo != null && propertyInfo.GetActualPropertyType() == dataRecord.GetFieldType( i ) && filteredPropertyMaps.All( x => x.Key != propertyInfo ) )
					{
						emitter.Emit( OpCodes.Ldloc, objectVariable );
						DataReaderEmitter.GetPropertyValue( emitter, propertyInfo.PropertyType, i, propertyInfo.IsNullable() );
						emitter.Emit( OpCodes.Callvirt, propertyInfo.GetSetMethod() );
					}
				}
			}

			foreach ( var propertyMap in filteredPropertyMaps )
			{
				var propertyMapLocal = localPropertyMapsBuilders[ propertyMap.Key ];

				emitter.Emit( OpCodes.Ldloc, objectVariable );
				emitter.Emit( OpCodes.Ldloc, propertyMapLocal );
				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Call, propertyMapLocal.LocalType.GetMethod( "Invoke" ) );
				emitter.Emit( OpCodes.Callvirt, propertyMap.Key.GetSetMethod() ?? propertyMap.Key.GetSetMethod( true ) );
			}

			emitter.Emit( OpCodes.Ldloc, objectVariable );
			emitter.Emit( OpCodes.Ret );

			return (Func<IDataRecord, Dictionary<MappingEnum, object>, object>)method.CreateDelegate( typeof( Func<IDataRecord, Dictionary<MappingEnum, object>, object> ) );
		}
	}
}