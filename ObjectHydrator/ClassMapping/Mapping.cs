using SqlObjectHydrator.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SqlObjectHydrator.ClassMapping
{
	internal class Mapping : IMapping
	{
		public Dictionary<PropertyInfo, PropertyMap> PropertyMaps { get; set; }

		public Dictionary<int, Type> TableMaps { get; set; }

		public List<DictionaryTableJoin> DictionaryTableJoins { get; set; }

		public List<TableJoinMap> TableJoins { get; set; }

		public Dictionary<KeyValuePair<Type, Type>, object> Joins { get; set; }

		public Dictionary<Type, object> VariableTableTypes { get; set; }

		public Mapping()
		{
			this.PropertyMaps = new Dictionary<PropertyInfo, PropertyMap>();
			this.TableMaps = new Dictionary<int, Type>();
			this.DictionaryTableJoins = new List<DictionaryTableJoin>();
			this.TableJoins = new List<TableJoinMap>();
			this.Joins = new Dictionary<KeyValuePair<Type, Type>, object>();
			this.VariableTableTypes = new Dictionary<Type, object>();
		}

		void IMapping.Table<T>( int id )
		{
			this.TableMaps.Add( id, typeof( T ) );
		}

		void IMapping.PropertyMap<T, TResult>(
		  Expression<Func<T, TResult>> property,
		  Func<IDataRecord, TResult> setAction )
		{
			this.PropertyMaps.Add( Mapping.GetPropertyInfo<T>( ( (IEnumerable<string>)property.ToString().Split( '.' ) ).Last<string>() ), new PropertyMap( (Delegate)setAction ) );
		}

		public void PropertyMap<T, TResult>( Expression<Func<T, TResult>> property, string columnName )
		{
			this.PropertyMaps.Add( Mapping.GetPropertyInfo<T>( ( (IEnumerable<string>)property.ToString().Split( '.' ) ).Last<string>() ), new PropertyMap( columnName ) );
		}

		public void PropertyMap<T, TResult>( Expression<Func<T, TResult>> property, int columnId )
		{
			this.PropertyMaps.Add( Mapping.GetPropertyInfo<T>( ( (IEnumerable<string>)property.ToString().Split( '.' ) ).Last<string>() ), new PropertyMap( columnId ) );
		}

		private static PropertyInfo GetPropertyInfo<T>( string name )
		{
			PropertyInfo property = typeof( T ).GetProperty( name );
			if ( (object)property == null )
				property = typeof( T ).GetProperty( name, BindingFlags.Instance | BindingFlags.NonPublic );
			return property;
		}

		void IMapping.TableJoin<TParent, TChild>(
		  Func<TParent, TChild, bool> canJoin,
		  Action<TParent, List<TChild>> listSet )
		{
			this.TableJoins.Add( new TableJoinMap()
			{
				ChildType = typeof( TChild ),
				ParentType = typeof( TParent ),
				CanJoin = (object)canJoin,
				ListSet = (object)listSet
			} );
		}

		void IMapping.Join<TParent, TChild>( Action<TParent, List<TChild>> listSet )
		{
			this.Joins.Add( new KeyValuePair<Type, Type>( typeof( TParent ), typeof( TChild ) ), (object)listSet );
		}

		void IMapping.AddJoin( Func<ITableJoin, ITableJoinMap> func )
		{
			ITableJoinMap tableJoinMap = func( (ITableJoin)new TableJoin() );
			if ( !( tableJoinMap is DictionaryTableJoin ) )
				return;
			DictionaryTableJoin dictionaryTableJoin = tableJoinMap as DictionaryTableJoin;
			this.DictionaryTableJoins.Add( dictionaryTableJoin );
			this.TableMaps.Add( dictionaryTableJoin.ChildTable, typeof( ExpandoObject ) );
		}

		void IMapping.VariableTableType<T>( Func<IDataRecord, Type> action )
		{
			this.VariableTableTypes.Add( typeof( T ), (object)action );
		}
	}
}
