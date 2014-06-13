using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SqlObjectHydrator.ClassMapping;
using SqlObjectHydrator.Configuration.ExpressionGenerators;

namespace SqlObjectHydrator.ILEmitting
{
	internal class MappingGenerator
	{
		public static void NextResult( IDataReader dataReader )
		{
			dataReader.NextResult();
		}

		public static Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T> Generate<T>( IDataReader dataReader, ClassMapResult classMapResult ) where T : new()
		{
			var baseType = ( typeof( T ).IsGenericType && typeof( T ).GetGenericTypeDefinition() == typeof( List<> ) ) ? typeof( T ).GetGenericArguments()[ 0 ] : typeof( T );
			var isList = ( typeof( T ).IsGenericType && typeof( T ).GetGenericTypeDefinition() == typeof( List<> ) );

			#region working dynamicmethod

			var method = new DynamicMethod( "", typeof( T ), new[]
			{
				typeof( IDataReader ),
				typeof( Dictionary<MappingEnum, object> ),
				typeof( Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>> )
			}, true );
			var emitter = method.GetILGenerator();

			#endregion

			#region dynamicmethod that outputs dll

			//var assemblyName = new AssemblyName("SomeName");
			//var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, @"d:\");
			//var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name +  ".dll");

			//TypeBuilder builder = moduleBuilder.DefineType("Test", TypeAttributes.Public);
			//var methodBuilder = builder.DefineMethod("DynamicCreate", MethodAttributes.Public, typeof(T), new[] { typeof( IDataReader ),typeof( Dictionary<MappingEnum, object> ) });
			//var emitter = methodBuilder.GetILGenerator();

			#endregion

			//Create Func Variables
			var tableJoinsLocalBuilders = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<LocalBuilder, LocalBuilder>>();
			var joinLocalBuilders = new Dictionary<KeyValuePair<Type, Type>, LocalBuilder>();
			var dictionaryTableJoinsLocalBuilders = new Dictionary<KeyValuePair<Type, int>, KeyValuePair<LocalBuilder, LocalBuilder>>();

			#region Populate Mapping Variables

			if ( classMapResult.Mappings.TableJoins.Count > 0 )
			{
				var tableJoins = classMapResult.Mappings.TableJoins.ToList();
				var tableJoinsLocal = emitter.DeclareLocal( typeof( List<KeyValuePair<object, object>> ) );
				emitter.Emit( OpCodes.Ldarg_1 );
				emitter.Emit( OpCodes.Ldc_I4, (int)MappingEnum.TableJoin );
				emitter.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				emitter.Emit( OpCodes.Castclass, typeof( List<KeyValuePair<object, object>> ) );
				emitter.Emit( OpCodes.Stloc, tableJoinsLocal );

				foreach ( var tableJoin in classMapResult.Mappings.TableJoins )
				{
					var keyLocal = emitter.DeclareLocal( tableJoin.Value.Key.GetType() );
					var valueLocal = emitter.DeclareLocal( tableJoin.Value.Value.GetType() );

					var tempLocal = emitter.DeclareLocal( typeof( KeyValuePair<object, object> ) );

					var index = tableJoins.IndexOf( tableJoin );
					emitter.Emit( OpCodes.Ldloc, tableJoinsLocal );
					emitter.Emit( OpCodes.Ldc_I4, index );
					emitter.Emit( OpCodes.Call, typeof( List<KeyValuePair<object, object>> ).GetProperty( "Item" ).GetGetMethod() );
					emitter.Emit( OpCodes.Stloc, tempLocal );
					emitter.Emit( OpCodes.Ldloc, tempLocal );
					emitter.Emit( OpCodes.Ldfld, typeof( KeyValuePair<object, object> ).GetField( "key", BindingFlags.Instance | BindingFlags.NonPublic ) );
					emitter.Emit( OpCodes.Castclass, tableJoin.Value.Key.GetType() );
					emitter.Emit( OpCodes.Stloc, keyLocal );
					emitter.Emit( OpCodes.Ldloc, tempLocal );
					emitter.Emit( OpCodes.Ldfld, typeof( KeyValuePair<object, object> ).GetField( "value", BindingFlags.Instance | BindingFlags.NonPublic ) );
					emitter.Emit( OpCodes.Castclass, tableJoin.Value.Value.GetType() );
					emitter.Emit( OpCodes.Stloc, valueLocal );

					tableJoinsLocalBuilders.Add( tableJoin.Key, new KeyValuePair<LocalBuilder, LocalBuilder>( keyLocal, valueLocal ) );
				}
			}


			if ( classMapResult.Mappings.Joins.Count > 0 )
			{
				var joins = classMapResult.Mappings.Joins.ToList();
				var joinsLocal = emitter.DeclareLocal( typeof( List<object> ) );
				emitter.Emit( OpCodes.Ldarg_1 );
				emitter.Emit( OpCodes.Ldc_I4, (int)MappingEnum.Join );
				emitter.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				emitter.Emit( OpCodes.Castclass, typeof( List<object> ) );
				emitter.Emit( OpCodes.Stloc, joinsLocal );
				foreach ( var @join in joins )
				{
					var local = emitter.DeclareLocal( @join.Value.GetType() );

					var index = joins.IndexOf( @join );

					emitter.Emit( OpCodes.Ldloc, joinsLocal );
					emitter.Emit( OpCodes.Ldc_I4, index );
					emitter.Emit( OpCodes.Call, typeof( List<object> ).GetProperty( "Item" ).GetGetMethod() );
					emitter.Emit( OpCodes.Castclass, @join.Value.GetType() );
					emitter.Emit( OpCodes.Stloc, local );

					joinLocalBuilders.Add( @join.Key, local );
				}
			}

			if ( classMapResult.Mappings.DictionaryTableJoins.Count > 0 )
			{
				var tableJoinsLocal = emitter.DeclareLocal( typeof( List<KeyValuePair<object, object>> ) );
				emitter.Emit( OpCodes.Ldarg_1 );
				emitter.Emit( OpCodes.Ldc_I4, (int)MappingEnum.DictionaryJoin );
				emitter.Emit( OpCodes.Callvirt, typeof( Dictionary<MappingEnum, object> ).GetMethod( "GetValueOrDefault", BindingFlags.Instance | BindingFlags.NonPublic ) );
				emitter.Emit( OpCodes.Castclass, typeof( List<KeyValuePair<object, object>> ) );
				emitter.Emit( OpCodes.Stloc, tableJoinsLocal );
				foreach ( var dictionaryTableJoin in classMapResult.Mappings.DictionaryTableJoins )
				{
					var conditionLocal = emitter.DeclareLocal( dictionaryTableJoin.Condition.GetType() );
					var destinationLocal = emitter.DeclareLocal( typeof( Action<,> ).MakeGenericType( dictionaryTableJoin.ParentTableType, typeof( List<ExpandoObject> ) ) );

					var tempLocal = emitter.DeclareLocal( typeof( KeyValuePair<object, object> ) );

					var index = classMapResult.Mappings.DictionaryTableJoins.IndexOf( dictionaryTableJoin );
					emitter.Emit( OpCodes.Ldloc, tableJoinsLocal );
					emitter.Emit( OpCodes.Ldc_I4, index );
					emitter.Emit( OpCodes.Call, typeof( List<KeyValuePair<object, object>> ).GetProperty( "Item" ).GetGetMethod() );
					emitter.Emit( OpCodes.Stloc, tempLocal );
					emitter.Emit( OpCodes.Ldloc, tempLocal );
					emitter.Emit( OpCodes.Ldfld, typeof( KeyValuePair<object, object> ).GetField( "key", BindingFlags.Instance | BindingFlags.NonPublic ) );
					emitter.Emit( OpCodes.Castclass, dictionaryTableJoin.Condition.GetType() );
					emitter.Emit( OpCodes.Stloc, conditionLocal );
					emitter.Emit( OpCodes.Ldloc, tempLocal );
					emitter.Emit( OpCodes.Ldfld, typeof( KeyValuePair<object, object> ).GetField( "value", BindingFlags.Instance | BindingFlags.NonPublic ) );
					emitter.Emit( OpCodes.Castclass, dictionaryTableJoin.Destination.GetType() );
					emitter.Emit( OpCodes.Ldstr, dictionaryTableJoin.KeyColumn );
					emitter.Emit( OpCodes.Ldstr, dictionaryTableJoin.ValueColumn );
					emitter.Emit( OpCodes.Call, typeof( DynamicToDictionaryGenerator ).GetMethod( "Generate" ).MakeGenericMethod( dictionaryTableJoin.ParentTableType, dictionaryTableJoin.KeyType, dictionaryTableJoin.ValueType ) );
					emitter.Emit( OpCodes.Stloc, destinationLocal );

					dictionaryTableJoinsLocalBuilders.Add( new KeyValuePair<Type, int>( dictionaryTableJoin.ParentTableType, dictionaryTableJoin.ChildTable ), new KeyValuePair<LocalBuilder, LocalBuilder>( conditionLocal, destinationLocal ) );
				}
			}

			#endregion

			//Load Tables
			var localBuilders = new Dictionary<KeyValuePair<Type, int>, LocalBuilder>();

			var maxTableId = classMapResult.Mappings.TableMaps.Select( x=>x.Key ).OrderBy( x=>x).Last();
			for ( var i = 0; i <= maxTableId; i++ )
			{
				if ( ! classMapResult.Mappings.TableMaps.ContainsKey( i ) )
					continue;
				
				var type = classMapResult.Mappings.TableMaps[ i ];

				var localBuilder = ObjectSettingEmitter.Emit( emitter, type, classMapResult.Mappings );
				localBuilders.Add( new KeyValuePair<Type, int>( type, i ), localBuilder );
				emitter.Emit( OpCodes.Ldarg_0 );
				emitter.Emit( OpCodes.Call, typeof( MappingGenerator ).GetMethod( "NextResult" ) );
			}

			#region Run Mappings

			foreach ( var tableJoinsLocalBuilder in tableJoinsLocalBuilders )
			{
				var parentType = tableJoinsLocalBuilder.Key.Key;
				var childType = tableJoinsLocalBuilder.Key.Value;

				emitter.Emit( OpCodes.Ldloc, localBuilders.First( x => x.Key.Key == parentType ).Value );
				emitter.Emit( OpCodes.Ldloc, localBuilders.First( x => x.Key.Key == childType ).Value );
				emitter.Emit( OpCodes.Ldloc, tableJoinsLocalBuilder.Value.Key );
				emitter.Emit( OpCodes.Ldloc, tableJoinsLocalBuilder.Value.Value );
				emitter.Emit( OpCodes.Call, typeof( MappingGenerator ).GetMethod( "JoinTables" ).MakeGenericMethod( parentType, childType ) );
			}

			foreach ( var joinLocalBuilder in joinLocalBuilders )
			{
				var parentType = joinLocalBuilder.Key.Key;
				var childType = joinLocalBuilder.Key.Value;

				emitter.Emit( OpCodes.Ldloc, localBuilders.First( x => x.Key.Key == parentType ).Value );
				emitter.Emit( OpCodes.Ldloc, localBuilders.First( x => x.Key.Key == childType ).Value );
				emitter.Emit( OpCodes.Ldloc, joinLocalBuilder.Value );
				emitter.Emit( OpCodes.Call, typeof( MappingGenerator ).GetMethod( "Join" ).MakeGenericMethod( parentType, childType ) );
			}

			foreach ( var dictionaryTableJoinsLocalBuilder in dictionaryTableJoinsLocalBuilders )
			{
				var parentType = dictionaryTableJoinsLocalBuilder.Key.Key;
				var childId = dictionaryTableJoinsLocalBuilder.Key.Value;

				emitter.Emit( OpCodes.Ldloc, localBuilders.First( x => x.Key.Key == parentType ).Value );
				emitter.Emit( OpCodes.Ldloc, localBuilders.First( x => x.Key.Value == childId ).Value );
				emitter.Emit( OpCodes.Ldloc, dictionaryTableJoinsLocalBuilder.Value.Key );
				emitter.Emit( OpCodes.Ldloc, dictionaryTableJoinsLocalBuilder.Value.Value );
				emitter.Emit( OpCodes.Call, typeof( MappingGenerator ).GetMethod( "JoinTables" ).MakeGenericMethod( parentType, typeof( ExpandoObject ) ) );
			}

			#endregion

			emitter.Emit( OpCodes.Ldloc, localBuilders.First( x => x.Key.Key == baseType ).Value );
			if ( !isList )
				emitter.Emit( OpCodes.Call, typeof( Enumerable ).GetMethods().First( x => x.Name == "FirstOrDefault" && x.GetParameters().Count() == 1 ).MakeGenericMethod( typeof( T ) ) );
			emitter.Emit( OpCodes.Ret );

			#region dynamicmethod that outputs dll

			//var t = builder.CreateType();
			//assemblyBuilder.Save(assemblyName.Name + ".dll");
			//return null;

			#endregion

			#region working dynamicmethod

			return (Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T>)method.CreateDelegate( typeof( Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T> ) );

			#endregion
		}

		public static void JoinTables<TParent, TChild>( List<TParent> parents, IEnumerable<TChild> children, Func<TParent, TChild, bool> canJoin, Action<TParent, List<TChild>> setFunc )
		{
			parents.ForEach( x => setFunc( x, children.Where( y => canJoin( x, y ) ).ToList() ) );
		}

		public static void Join<TParent, TChild>( List<TParent> parents, List<TChild> children, Action<TParent, List<TChild>> setFunc )
		{
			parents.ForEach( x => setFunc( x, children ) );
		}
	}
}