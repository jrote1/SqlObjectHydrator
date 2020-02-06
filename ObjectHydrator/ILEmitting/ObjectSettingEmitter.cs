using SqlObjectHydrator.ClassMapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SqlObjectHydrator.ILEmitting
{
	internal static class ObjectSettingEmitter
	{
		public static int TableId = 0;
		private static object safeTypeMapsAddLockObj = new object();

		public static LocalBuilder Emit(
		  ILGenerator emitter,
		  Type type,
		  Mapping mapping,
		  int tableId )
		{
			Type localType = typeof( List<> ).MakeGenericType( type );
			LocalBuilder local1 = emitter.DeclareLocal( localType );
			emitter.Emit( OpCodes.Newobj, localType.GetConstructors()[ 0 ] );
			emitter.Emit( OpCodes.Stloc, local1 );
			LocalBuilder local2 = emitter.DeclareLocal( type );
			LocalBuilder local3 = (LocalBuilder)null;
			if ( mapping.VariableTableTypes.ContainsKey( type ) )
			{
				local3 = emitter.DeclareLocal( typeof( Func<IDataRecord, Type> ) );
				emitter.Emit( OpCodes.Ldarg_1 );
				emitter.Emit( OpCodes.Ldc_I4, 4 );
				emitter.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				emitter.Emit( OpCodes.Castclass, typeof( Dictionary<Type, object> ) );
				emitter.Emit( OpCodes.Ldtoken, type );
				emitter.Emit( OpCodes.Call, typeof( Type ).GetMethod( "GetTypeFromHandle" ) );
				emitter.Emit( OpCodes.Callvirt, typeof( Dictionary<Type, object> ).GetProperty( "Item" ).GetGetMethod() );
				emitter.Emit( OpCodes.Castclass, typeof( Func<IDataRecord, Type> ) );
				emitter.Emit( OpCodes.Stloc, local3 );
			}
			Label label1 = emitter.DefineLabel();
			Label label2 = emitter.DefineLabel();
			emitter.Emit( OpCodes.Br, label1 );
			emitter.MarkLabel( label2 );
			emitter.Emit( OpCodes.Newobj, type.GetConstructors()[ 0 ] );
			emitter.Emit( OpCodes.Stloc, local2 );
			LocalBuilder local4 = emitter.DeclareLocal( typeof( Type ) );
			if ( mapping.VariableTableTypes.ContainsKey( type ) )
			{
				emitter.Emit( OpCodes.Ldloc, local3 );
				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Callvirt, typeof( Func<IDataRecord, Type> ).GetMethod( "Invoke" ) );
				emitter.Emit( OpCodes.Stloc, local4 );
			}
			else
			{
				emitter.Emit( OpCodes.Ldtoken, type );
				emitter.Emit( OpCodes.Call, typeof( Type ).GetMethod( "GetTypeFromHandle" ) );
				emitter.Emit( OpCodes.Stloc, local4 );
			}
			Label label3 = emitter.DefineLabel();
			emitter.Emit( OpCodes.Ldarg_2 );
			emitter.Emit( OpCodes.Ldc_I4, tableId );
			emitter.Emit( OpCodes.Ldloc, local4 );
			emitter.Emit( OpCodes.Call, typeof( ObjectSettingEmitter ).GetMethod( "GetMappingCacheTuple" ) );
			emitter.Emit( OpCodes.Call, typeof( Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>> ).GetMethod( "ContainsKey" ) );
			emitter.Emit( OpCodes.Brtrue, label3 );
			LocalBuilder local5 = emitter.DeclareLocal( typeof( Func<IDataRecord, Dictionary<MappingEnum, object>, object> ) );
			emitter.Emit( OpCodes.Ldloc, local4 );
			emitter.Emit( OpCodes.Ldarg_0 );
			emitter.Emit( OpCodes.Ldarg_1 );
			emitter.Emit( OpCodes.Call, typeof( ObjectSettingEmitter ).GetMethod( "GenerateObjectBuilder" ) );
			emitter.Emit( OpCodes.Stloc, local5 );
			emitter.Emit( OpCodes.Ldarg_2 );
			emitter.Emit( OpCodes.Ldc_I4, tableId );
			emitter.Emit( OpCodes.Ldloc, local4 );
			emitter.Emit( OpCodes.Call, typeof( ObjectSettingEmitter ).GetMethod( "GetMappingCacheTuple" ) );
			emitter.Emit( OpCodes.Ldloc, local5 );
			emitter.Emit( OpCodes.Call, typeof( ObjectSettingEmitter ).GetMethod( "SafeTypeMapsAdd" ) );
			emitter.MarkLabel( label3 );
			emitter.Emit( OpCodes.Ldarg_2 );
			emitter.Emit( OpCodes.Ldc_I4, tableId );
			emitter.Emit( OpCodes.Ldloc, local4 );
			emitter.Emit( OpCodes.Call, typeof( ObjectSettingEmitter ).GetMethod( "GetMappingCacheTuple" ) );
			emitter.Emit( OpCodes.Call, typeof( Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>> ).GetProperty( "Item" ).GetGetMethod() );
			emitter.Emit( OpCodes.Ldarg_0 );
			emitter.Emit( OpCodes.Ldarg_1 );
			emitter.Emit( OpCodes.Call, typeof( Func<IDataRecord, Dictionary<MappingEnum, object>, object> ).GetMethod( "Invoke" ) );
			emitter.Emit( OpCodes.Castclass, type );
			emitter.Emit( OpCodes.Stloc, local2 );
			emitter.Emit( OpCodes.Ldloc, local1 );
			emitter.Emit( OpCodes.Ldloc, local2 );
			emitter.Emit( OpCodes.Callvirt, localType.GetMethod( "Add" ) );
			emitter.MarkLabel( label1 );
			emitter.Emit( OpCodes.Ldarg_0 );
			emitter.Emit( OpCodes.Callvirt, typeof( IDataReader ).GetMethod( "Read" ) );
			emitter.Emit( OpCodes.Brtrue, label2 );
			return local1;
		}

		public static void SafeTypeMapsAdd(
		  Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>> dictionary,
		  Tuple<int, Type> key,
		  Func<IDataRecord, Dictionary<MappingEnum, object>, object> value )
		{
			lock ( ObjectSettingEmitter.safeTypeMapsAddLockObj )
			{
				if ( dictionary.ContainsKey( key ) )
					return;
				dictionary.Add( key, value );
			}
		}

		public static Tuple<int, Type> GetMappingCacheTuple( int tableId, Type type )
		{
			return new Tuple<int, Type>( tableId, type );
		}

		public static Func<IDataRecord, Dictionary<MappingEnum, object>, object> GenerateObjectBuilder(
		  Type type,
		  IDataRecord dataRecord,
		  Dictionary<MappingEnum, object> mappings )
		{
			Dictionary<PropertyInfo, PropertyMap> source = mappings.ContainsKey( MappingEnum.PropertyMap ) ? mappings[ MappingEnum.PropertyMap ] as Dictionary<PropertyInfo, PropertyMap> : new Dictionary<PropertyInfo, PropertyMap>();
			List<KeyValuePair<PropertyInfo, PropertyMap>> list = source.Where<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x => x.Key.DeclaringType.IsAssignableFrom( type ) ) ).ToList<KeyValuePair<PropertyInfo, PropertyMap>>();
			DynamicMethod dynamicMethod = new DynamicMethod( "", typeof( object ), new Type[ 2 ]
			{
		typeof (IDataRecord),
		typeof (Dictionary<MappingEnum, object>)
			}, true );
			ILGenerator ilGenerator1 = dynamicMethod.GetILGenerator();
			Dictionary<PropertyInfo, LocalBuilder> dictionary = new Dictionary<PropertyInfo, LocalBuilder>();
			if ( list.Count > 0 )
			{
				LocalBuilder local1 = ilGenerator1.DeclareLocal( typeof( List<PropertyMap> ) );
				ilGenerator1.Emit( OpCodes.Ldarg_1 );
				ilGenerator1.Emit( OpCodes.Ldc_I4, 3 );
				ilGenerator1.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				ilGenerator1.Emit( OpCodes.Castclass, typeof( Dictionary<PropertyInfo, PropertyMap> ) );
				ilGenerator1.Emit( OpCodes.Call, typeof( Dictionary<PropertyInfo, PropertyMap> ).GetProperty( "Values" ).GetGetMethod() );
				ilGenerator1.Emit( OpCodes.Call, typeof( Enumerable ).GetMethod( "ToList" ).MakeGenericMethod( typeof( PropertyMap ) ) );
				ilGenerator1.Emit( OpCodes.Stloc, local1 );
				foreach ( KeyValuePair<PropertyInfo, PropertyMap> keyValuePair in list.Where<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x => x.Value.PropertyMapType == PropertyMapType.Func ) ) )
				{
					LocalBuilder local2 = ilGenerator1.DeclareLocal( keyValuePair.Value.SetAction.GetType() );
					int num = source.Keys.ToList<PropertyInfo>().IndexOf( keyValuePair.Key );
					ilGenerator1.Emit( OpCodes.Ldloc, local1 );
					ilGenerator1.Emit( OpCodes.Ldc_I4, num );
					ilGenerator1.Emit( OpCodes.Call, typeof( List<PropertyMap> ).GetProperty( "Item" ).GetGetMethod() );
					ilGenerator1.Emit( OpCodes.Call, typeof( PropertyMap ).GetProperty( "SetAction" ).GetGetMethod() );
					ilGenerator1.Emit( OpCodes.Castclass, keyValuePair.Value.SetAction.GetType() );
					ilGenerator1.Emit( OpCodes.Stloc, local2 );
					dictionary.Add( keyValuePair.Key, local2 );
				}
			}
			LocalBuilder localBuilder = ilGenerator1.DeclareLocal( type );
			ilGenerator1.Emit( OpCodes.Newobj, type.GetConstructors()[ 0 ] );
			ilGenerator1.Emit( OpCodes.Stloc, localBuilder );
			for ( int i = 0; i < dataRecord.FieldCount; ++i )
			{
				if ( type == typeof( ExpandoObject ) )
				{
					LocalBuilder local = ilGenerator1.DeclareLocal( dataRecord.GetFieldType( i ) );
					DataReaderEmitter.GetPropertyValue( ilGenerator1, dataRecord.GetFieldType( i ), i, false, (object)null );
					ilGenerator1.Emit( OpCodes.Stloc, local );
					ExpandoObjectInteractor.SetExpandoProperty( ilGenerator1, localBuilder, dataRecord.GetName( i ), local );
				}
				else
				{
					PropertyInfo propertyInfo;
					if ( list.Any<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x =>
					  {
						  if ( x.Value.PropertyMapType == PropertyMapType.ColumnId )
							  return x.Value.ColumnId == i;
						  return false;
					  } ) ) )
						propertyInfo = list.First<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x => x.Value.ColumnId == i ) ).Key;
					else if ( list.Any<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x =>
					  {
						  if ( x.Value.PropertyMapType == PropertyMapType.ColumnName )
							  return x.Value.ColumnName == dataRecord.GetName( i );
						  return false;
					  } ) ) )
						propertyInfo = list.First<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x => x.Value.ColumnName == dataRecord.GetName( i ) ) ).Key;
					else
						propertyInfo = !list.Any<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x => x.Key.Name == dataRecord.GetName( i ) ) ) ? type.GetProperty( dataRecord.GetName( i ) ) : (PropertyInfo)null;
					if ( propertyInfo != (PropertyInfo)null && propertyInfo.GetActualPropertyType() == dataRecord.GetFieldType( i ) && list.Where<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x => x.Value.PropertyMapType == PropertyMapType.Func ) ).All<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x => x.Key != propertyInfo ) ) )
					{
						ilGenerator1.Emit( OpCodes.Ldloc, localBuilder );
						DataReaderEmitter.GetPropertyValue( ilGenerator1, propertyInfo.PropertyType, i, propertyInfo.IsNullable(), (object)null );
						ilGenerator1.Emit( OpCodes.Callvirt, propertyInfo.GetSetMethod() );
					}
				}
			}
			foreach ( KeyValuePair<PropertyInfo, PropertyMap> keyValuePair in list.Where<KeyValuePair<PropertyInfo, PropertyMap>>( (Func<KeyValuePair<PropertyInfo, PropertyMap>, bool>)( x => x.Value.PropertyMapType == PropertyMapType.Func ) ) )
			{
				LocalBuilder local = dictionary[ keyValuePair.Key ];
				ilGenerator1.Emit( OpCodes.Ldloc, localBuilder );
				ilGenerator1.Emit( OpCodes.Ldloc, local );
				ilGenerator1.Emit( OpCodes.Ldarg_0 );
				ilGenerator1.Emit( OpCodes.Call, local.LocalType.GetMethod( "Invoke" ) );
				ILGenerator ilGenerator2 = ilGenerator1;
				OpCode callvirt = OpCodes.Callvirt;
				MethodInfo setMethod = keyValuePair.Key.GetSetMethod();
				if ( (object)setMethod == null )
					setMethod = keyValuePair.Key.GetSetMethod( true );
				ilGenerator2.Emit( callvirt, setMethod );
			}
			ilGenerator1.Emit( OpCodes.Ldloc, localBuilder );
			ilGenerator1.Emit( OpCodes.Ret );
			return (Func<IDataRecord, Dictionary<MappingEnum, object>, object>)dynamicMethod.CreateDelegate( typeof( Func<IDataRecord, Dictionary<MappingEnum, object>, object> ) );
		}
	}
}
