using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SqlObjectHydrator.ClassMapping;

namespace SqlObjectHydrator.ILEmitting
{
	internal static class ObjectSettingEmitter
	{
		public static LocalBuilder Emit( ILGenerator emitter, ClassMap classMap, Mapping mapping )
		{
			var listType = typeof( List<> ).MakeGenericType( classMap.Type );
			var resultLocalBuilder = emitter.DeclareLocal( listType );
			emitter.Emit( OpCodes.Newobj, listType.GetConstructors()[ 0 ] );
			emitter.Emit( OpCodes.Stloc, resultLocalBuilder );

			var tempRoot = emitter.DeclareLocal( classMap.Type );

			var propertyMapsList = mapping.PropertyMaps.ToList();
			var propertyMapLocals = new Dictionary<PropertyInfo,LocalBuilder>();
			var propertyMaps = emitter.DeclareLocal( typeof( List<object> ) );
			emitter.Emit( OpCodes.Ldarg_1 );
			emitter.Emit( OpCodes.Ldc_I4, (int)MappingEnum.PropertyMap );
			emitter.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
			emitter.Emit( OpCodes.Castclass, typeof( List<object> ) );
			emitter.Emit( OpCodes.Stloc, propertyMaps );

			foreach ( var propertyMap in propertyMapsList )
			{
				if ( propertyMap.Value.Key == classMap.Type )
				{
					var localBuilder = emitter.DeclareLocal( propertyMap.Value.Value.GetType() );
					emitter.Emit( OpCodes.Ldloc, propertyMaps );
					emitter.Emit( OpCodes.Ldc_I4, propertyMapsList.IndexOf( propertyMap ) );
					emitter.Emit( OpCodes.Call, typeof(List<object> ).GetProperty( "Item" ).GetGetMethod() );
					emitter.Emit( OpCodes.Castclass, localBuilder.LocalType );
					emitter.Emit( OpCodes.Stloc, localBuilder );

					propertyMapLocals.Add( propertyMap.Key,localBuilder );
				}
			}


			//Define While Labels
			var whileIf = emitter.DefineLabel();
			var whileStart = emitter.DefineLabel();

			//Start While Loop
			emitter.Emit( OpCodes.Br, whileIf );
			emitter.MarkLabel( whileStart );

			//Create Root Variable
			emitter.Emit( OpCodes.Newobj, classMap.Type.GetConstructors()[ 0 ] );
			emitter.Emit( OpCodes.Stloc, tempRoot );

			//Processes All Properties


			if ( classMap.Type == typeof( ExpandoObject ) )
			{
				foreach ( var property in classMap.Properties.Cast<ExpandoPropertyMap>() )
				{
					var tempVariable = emitter.DeclareLocal( property.Type );
					DataReaderEmitter.GetPropertyValue( emitter, property.Type, property.FieldId, false );
					emitter.Emit( OpCodes.Stloc, tempVariable );
					ExpandoObjectInteractor.SetExpandoProperty( emitter, tempRoot, property.Name, tempVariable );
				}
			}
			else
			{
				foreach ( var property in classMap.Properties.Cast<PropertyMap>() )
				{
					if ( propertyMapsList.All( x => x.Key != property.PropertyInfo ) )
					{
						emitter.Emit( OpCodes.Ldloc, tempRoot );
						DataReaderEmitter.GetPropertyValue( emitter, property.PropertyInfo.PropertyType, property.FieldId, property.Nullable );
						emitter.Emit( OpCodes.Callvirt, property.PropertyInfo.GetSetMethod() );
					}
				}
			}

			foreach ( var propertyMapLocal in propertyMapLocals )
			{
				emitter.Emit( OpCodes.Ldloc, tempRoot );
				emitter.Emit( OpCodes.Ldloc, propertyMapLocal.Value );
				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Call, propertyMapLocal.Value.LocalType.GetMethod( "Invoke" ) );
				emitter.Emit( OpCodes.Call, propertyMapLocal.Key.GetSetMethod() );
			}

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
	}
}