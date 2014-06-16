using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator.ClassMapping
{
	internal class Mapping : IMapping
	{
		public Dictionary<PropertyInfo, PropertyMap> PropertyMaps { get; set; }
		public Dictionary<int, Type> TableMaps { get; set; }
		public List<DictionaryTableJoin> DictionaryTableJoins { get; set; }
		public Dictionary<KeyValuePair<Type, Type>, KeyValuePair<object, object>> TableJoins { get; set; }
		public Dictionary<KeyValuePair<Type, Type>, object> Joins { get; set; }
		public Dictionary<Type, object> VariableTableTypes { get; set; }

		public Mapping()
		{
			PropertyMaps = new Dictionary<PropertyInfo, PropertyMap>();
			TableMaps = new Dictionary<int, Type>();
			DictionaryTableJoins = new List<DictionaryTableJoin>();
			TableJoins = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<object, object>>();
			Joins = new Dictionary<KeyValuePair<Type, Type>, object>();
			VariableTableTypes = new Dictionary<Type, object>();
		}

		void IMapping.Table<T>( int id )
		{
			TableMaps.Add( id, typeof( T ) );
		}

		void IMapping.PropertyMap<T, TResult>( Expression<Func<T, TResult>> property, Func<IDataRecord, TResult> setAction )
		{
			PropertyMaps.Add( GetPropertyInfo<T>( property.ToString().Split( '.' ).Last() ), new PropertyMap( setAction ) );
		}

		public void PropertyMap<T>( Expression<Func<T, object>> property, string columnName )
		{
			PropertyMaps.Add( GetPropertyInfo<T>( property.ToString().Split( '.' ).Last() ), new PropertyMap( columnName ) );
		}

		public void PropertyMap<T>( Expression<Func<T, object>> property, int columnId )
		{
			PropertyMaps.Add( GetPropertyInfo<T>( property.ToString().Split( '.' ).Last() ), new PropertyMap( columnId ) );
		}

		private static PropertyInfo GetPropertyInfo<T>( string name )
		{
			var propertyInfo = typeof( T ).GetProperty( name ) ?? typeof( T ).GetProperty( name, BindingFlags.Instance | BindingFlags.NonPublic );
			return propertyInfo;
		}

		void IMapping.TableJoin<TParent, TChild>( Func<TParent, TChild, bool> canJoin, Action<TParent, List<TChild>> listSet )
		{
			TableJoins.Add( new KeyValuePair<Type, Type>( typeof( TParent ), typeof( TChild ) ), new KeyValuePair<object, object>( canJoin, listSet ) );
		}

		void IMapping.Join<TParent, TChild>( Action<TParent, List<TChild>> listSet )
		{
			Joins.Add( new KeyValuePair<Type, Type>( typeof( TParent ), typeof( TChild ) ), listSet );
		}

		void IMapping.AddJoin( Func<ITableJoin, ITableJoinMap> func )
		{
			var tableJoinMap = func( new TableJoin() );
			if ( tableJoinMap is DictionaryTableJoin )
			{
				var dictionaryTableJoin = tableJoinMap as DictionaryTableJoin;
				DictionaryTableJoins.Add( dictionaryTableJoin );
				TableMaps.Add( dictionaryTableJoin.ChildTable, typeof( ExpandoObject ) );
			}
		}

		void IMapping.VariableTableType<T>( Func<IDataRecord, Type> action )
		{
			VariableTableTypes.Add( typeof( T ), action );
		}
	}

	internal class TableJoin : ITableJoin
	{
		public IDictionaryJoinCondition<T> DictionaryTableJoin<T>() where T : new()
		{
			return new DictionaryTableJoin<T>();
		}
	}
}