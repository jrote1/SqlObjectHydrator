using SqlObjectHydrator.ClassMapping;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.Configuration.ExpressionGenerators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SqlObjectHydrator.ILEmitting
{
	internal class MappingGenerator
	{
		public static void NextResult( IDataReader dataReader )
		{
			dataReader.NextResult();
		}

		public static Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T> Generate<T>(
		  IDataReader dataReader,
		  ClassMapResult classMapResult )
		  where T : new()
		{
			Type baseType = !typeof( T ).IsGenericType || !( typeof( T ).GetGenericTypeDefinition() == typeof( List<> ) ) ? typeof( T ) : typeof( T ).GetGenericArguments()[ 0 ];
			bool flag = typeof( T ).IsGenericType && typeof( T ).GetGenericTypeDefinition() == typeof( List<> );
			DynamicMethod dynamicMethod = new DynamicMethod( "", typeof( T ), new Type[ 3 ]
			{
		typeof (IDataReader),
		typeof (Dictionary<MappingEnum, object>),
		typeof (Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>)
			}, true );
			ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
			Dictionary<TableJoinMap, KeyValuePair<LocalBuilder, LocalBuilder>> dictionary1 = new Dictionary<TableJoinMap, KeyValuePair<LocalBuilder, LocalBuilder>>();
			Dictionary<KeyValuePair<Type, Type>, LocalBuilder> dictionary2 = new Dictionary<KeyValuePair<Type, Type>, LocalBuilder>();
			Dictionary<KeyValuePair<Type, int>, KeyValuePair<LocalBuilder, LocalBuilder>> dictionary3 = new Dictionary<KeyValuePair<Type, int>, KeyValuePair<LocalBuilder, LocalBuilder>>();
			if ( classMapResult.Mappings.TableJoins.Count > 0 )
			{
				List<TableJoinMap> list = classMapResult.Mappings.TableJoins.ToList<TableJoinMap>();
				LocalBuilder local1 = ilGenerator.DeclareLocal( typeof( List<TableJoinMap> ) );
				ilGenerator.Emit( OpCodes.Ldarg_1 );
				ilGenerator.Emit( OpCodes.Ldc_I4, 0 );
				ilGenerator.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				ilGenerator.Emit( OpCodes.Castclass, typeof( List<TableJoinMap> ) );
				ilGenerator.Emit( OpCodes.Stloc, local1 );
				foreach ( TableJoinMap tableJoin in classMapResult.Mappings.TableJoins )
				{
					LocalBuilder localBuilder = ilGenerator.DeclareLocal( tableJoin.CanJoin.GetType() );
					LocalBuilder local2 = ilGenerator.DeclareLocal( tableJoin.ListSet.GetType() );
					LocalBuilder local3 = ilGenerator.DeclareLocal( typeof( TableJoinMap ) );
					int num = list.IndexOf( tableJoin );
					ilGenerator.Emit( OpCodes.Ldloc, local1 );
					ilGenerator.Emit( OpCodes.Ldc_I4, num );
					ilGenerator.Emit( OpCodes.Call, typeof( List<TableJoinMap> ).GetProperty( "Item" ).GetGetMethod() );
					ilGenerator.Emit( OpCodes.Stloc, local3 );
					ilGenerator.Emit( OpCodes.Ldloc, local3 );
					ilGenerator.Emit( OpCodes.Call, typeof( TableJoinMap ).GetProperty( "CanJoin" ).GetGetMethod() );
					ilGenerator.Emit( OpCodes.Castclass, tableJoin.CanJoin.GetType() );
					ilGenerator.Emit( OpCodes.Stloc, localBuilder );
					ilGenerator.Emit( OpCodes.Ldloc, local3 );
					ilGenerator.Emit( OpCodes.Call, typeof( TableJoinMap ).GetProperty( "ListSet" ).GetGetMethod() );
					ilGenerator.Emit( OpCodes.Castclass, tableJoin.ListSet.GetType() );
					ilGenerator.Emit( OpCodes.Stloc, local2 );
					dictionary1.Add( tableJoin, new KeyValuePair<LocalBuilder, LocalBuilder>( localBuilder, local2 ) );
				}
			}
			if ( classMapResult.Mappings.Joins.Count > 0 )
			{
				List<KeyValuePair<KeyValuePair<Type, Type>, object>> list = classMapResult.Mappings.Joins.ToList<KeyValuePair<KeyValuePair<Type, Type>, object>>();
				LocalBuilder local1 = ilGenerator.DeclareLocal( typeof( List<object> ) );
				ilGenerator.Emit( OpCodes.Ldarg_1 );
				ilGenerator.Emit( OpCodes.Ldc_I4, 1 );
				ilGenerator.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				ilGenerator.Emit( OpCodes.Castclass, typeof( List<object> ) );
				ilGenerator.Emit( OpCodes.Stloc, local1 );
				foreach ( KeyValuePair<KeyValuePair<Type, Type>, object> keyValuePair in list )
				{
					LocalBuilder local2 = ilGenerator.DeclareLocal( keyValuePair.Value.GetType() );
					int num = list.IndexOf( keyValuePair );
					ilGenerator.Emit( OpCodes.Ldloc, local1 );
					ilGenerator.Emit( OpCodes.Ldc_I4, num );
					ilGenerator.Emit( OpCodes.Call, typeof( List<object> ).GetProperty( "Item" ).GetGetMethod() );
					ilGenerator.Emit( OpCodes.Castclass, keyValuePair.Value.GetType() );
					ilGenerator.Emit( OpCodes.Stloc, local2 );
					dictionary2.Add( keyValuePair.Key, local2 );
				}
			}
			if ( classMapResult.Mappings.DictionaryTableJoins.Count > 0 )
			{
				LocalBuilder local1 = ilGenerator.DeclareLocal( typeof( List<KeyValuePair<object, object>> ) );
				ilGenerator.Emit( OpCodes.Ldarg_1 );
				ilGenerator.Emit( OpCodes.Ldc_I4, 2 );
				ilGenerator.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				ilGenerator.Emit( OpCodes.Castclass, typeof( List<KeyValuePair<object, object>> ) );
				ilGenerator.Emit( OpCodes.Stloc, local1 );
				foreach ( DictionaryTableJoin dictionaryTableJoin in classMapResult.Mappings.DictionaryTableJoins )
				{
					LocalBuilder localBuilder = ilGenerator.DeclareLocal( dictionaryTableJoin.Condition.GetType() );
					LocalBuilder local2 = ilGenerator.DeclareLocal( typeof( Action<,> ).MakeGenericType( dictionaryTableJoin.ParentTableType, typeof( List<ExpandoObject> ) ) );
					LocalBuilder local3 = ilGenerator.DeclareLocal( typeof( KeyValuePair<object, object> ) );
					int num = classMapResult.Mappings.DictionaryTableJoins.IndexOf( dictionaryTableJoin );
					ilGenerator.Emit( OpCodes.Ldloc, local1 );
					ilGenerator.Emit( OpCodes.Ldc_I4, num );
					ilGenerator.Emit( OpCodes.Call, typeof( List<KeyValuePair<object, object>> ).GetProperty( "Item" ).GetGetMethod() );
					ilGenerator.Emit( OpCodes.Stloc, local3 );
					ilGenerator.Emit( OpCodes.Ldloc, local3 );
					ilGenerator.Emit( OpCodes.Ldfld, typeof( KeyValuePair<object, object> ).GetField( "key", BindingFlags.Instance | BindingFlags.NonPublic ) );
					ilGenerator.Emit( OpCodes.Castclass, dictionaryTableJoin.Condition.GetType() );
					ilGenerator.Emit( OpCodes.Stloc, localBuilder );
					ilGenerator.Emit( OpCodes.Ldloc, local3 );
					ilGenerator.Emit( OpCodes.Ldfld, typeof( KeyValuePair<object, object> ).GetField( "value", BindingFlags.Instance | BindingFlags.NonPublic ) );
					ilGenerator.Emit( OpCodes.Castclass, dictionaryTableJoin.Destination.GetType() );
					ilGenerator.Emit( OpCodes.Ldstr, dictionaryTableJoin.KeyColumn );
					ilGenerator.Emit( OpCodes.Ldstr, dictionaryTableJoin.ValueColumn );
					ilGenerator.Emit( OpCodes.Call, typeof( DynamicToDictionaryGenerator ).GetMethod( nameof( Generate ) ).MakeGenericMethod( dictionaryTableJoin.ParentTableType, dictionaryTableJoin.KeyType, dictionaryTableJoin.ValueType ) );
					ilGenerator.Emit( OpCodes.Stloc, local2 );
					dictionary3.Add( new KeyValuePair<Type, int>( dictionaryTableJoin.ParentTableType, dictionaryTableJoin.ChildTable ), new KeyValuePair<LocalBuilder, LocalBuilder>( localBuilder, local2 ) );
				}
			}
			Dictionary<KeyValuePair<Type, int>, LocalBuilder> source = new Dictionary<KeyValuePair<Type, int>, LocalBuilder>();
			int num1 = classMapResult.Mappings.TableMaps.Select<KeyValuePair<int, Type>, int>( (Func<KeyValuePair<int, Type>, int>)( x => x.Key ) ).OrderBy<int, int>( (Func<int, int>)( x => x ) ).Last<int>();
			for ( int index = 0; index <= num1; ++index )
			{
				if ( classMapResult.Mappings.TableMaps.ContainsKey( index ) )
				{
					Type tableMap = classMapResult.Mappings.TableMaps[ index ];
					LocalBuilder localBuilder = ObjectSettingEmitter.Emit( ilGenerator, tableMap, classMapResult.Mappings, index );
					source.Add( new KeyValuePair<Type, int>( tableMap, index ), localBuilder );
					ilGenerator.Emit( OpCodes.Ldarg_0 );
					ilGenerator.Emit( OpCodes.Call, typeof( MappingGenerator ).GetMethod( "NextResult" ) );
				}
			}
			foreach ( KeyValuePair<TableJoinMap, KeyValuePair<LocalBuilder, LocalBuilder>> keyValuePair in dictionary1 )
			{
				Type parentType = keyValuePair.Key.ParentType;
				Type childType = keyValuePair.Key.ChildType;
				ilGenerator.Emit( OpCodes.Ldloc, source.First<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>>( (Func<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>, bool>)( x => x.Key.Key == parentType ) ).Value );
				ilGenerator.Emit( OpCodes.Ldloc, source.First<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>>( (Func<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>, bool>)( x => x.Key.Key == childType ) ).Value );
				ilGenerator.Emit( OpCodes.Ldloc, keyValuePair.Value.Key );
				ilGenerator.Emit( OpCodes.Ldloc, keyValuePair.Value.Value );
				ilGenerator.Emit( OpCodes.Call, typeof( MappingGenerator ).GetMethod( "JoinTables" ).MakeGenericMethod( parentType, childType ) );
			}
			foreach ( KeyValuePair<KeyValuePair<Type, Type>, LocalBuilder> keyValuePair in dictionary2 )
			{
				Type parentType = keyValuePair.Key.Key;
				Type childType = keyValuePair.Key.Value;
				ilGenerator.Emit( OpCodes.Ldloc, source.First<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>>( (Func<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>, bool>)( x => x.Key.Key == parentType ) ).Value );
				ilGenerator.Emit( OpCodes.Ldloc, source.First<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>>( (Func<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>, bool>)( x => x.Key.Key == childType ) ).Value );
				ilGenerator.Emit( OpCodes.Ldloc, keyValuePair.Value );
				ilGenerator.Emit( OpCodes.Call, typeof( MappingGenerator ).GetMethod( "Join" ).MakeGenericMethod( parentType, childType ) );
			}
			foreach ( KeyValuePair<KeyValuePair<Type, int>, KeyValuePair<LocalBuilder, LocalBuilder>> keyValuePair in dictionary3 )
			{
				Type parentType = keyValuePair.Key.Key;
				int childId = keyValuePair.Key.Value;
				ilGenerator.Emit( OpCodes.Ldloc, source.First<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>>( (Func<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>, bool>)( x => x.Key.Key == parentType ) ).Value );
				ilGenerator.Emit( OpCodes.Ldloc, source.First<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>>( (Func<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>, bool>)( x => x.Key.Value == childId ) ).Value );
				ilGenerator.Emit( OpCodes.Ldloc, keyValuePair.Value.Key );
				ilGenerator.Emit( OpCodes.Ldloc, keyValuePair.Value.Value );
				ilGenerator.Emit( OpCodes.Call, typeof( MappingGenerator ).GetMethod( "JoinTables" ).MakeGenericMethod( parentType, typeof( ExpandoObject ) ) );
			}
			ilGenerator.Emit( OpCodes.Ldloc, source.First<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>>( (Func<KeyValuePair<KeyValuePair<Type, int>, LocalBuilder>, bool>)( x => x.Key.Key == baseType ) ).Value );
			if ( !flag )
				ilGenerator.Emit( OpCodes.Call, ( (IEnumerable<MethodInfo>)typeof( Enumerable ).GetMethods() ).First<MethodInfo>( (Func<MethodInfo, bool>)( x =>
					{
						if ( x.Name == "FirstOrDefault" )
							return ( (IEnumerable<ParameterInfo>)x.GetParameters() ).Count<ParameterInfo>() == 1;
						return false;
					} ) ).MakeGenericMethod( typeof( T ) ) );
			ilGenerator.Emit( OpCodes.Ret );
			return (Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T>)dynamicMethod.CreateDelegate( typeof( Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T> ) );
		}

		public static void JoinTables<TParent, TChild>(
		  List<TParent> parents,
		  IEnumerable<TChild> children,
		  Func<TParent, TChild, bool> canJoin,
		  Action<TParent, List<TChild>> setFunc )
		{
			parents.ForEach( (Action<TParent>)( x => setFunc( x, children.Where<TChild>( (Func<TChild, bool>)( y => canJoin( x, y ) ) ).ToList<TChild>() ) ) );
		}

		public static void Join<TParent, TChild>(
		  List<TParent> parents,
		  List<TChild> children,
		  Action<TParent, List<TChild>> setFunc )
		{
			parents.ForEach( (Action<TParent>)( x => setFunc( x, children ) ) );
		}
	}
}
