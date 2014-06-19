using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace SqlObjectHydrator.Configuration
{
	public interface IMapping
	{
		void Table<T>( int id );
		void PropertyMap<T, TResult>( Expression<Func<T, TResult>> property, Func<IDataRecord, TResult> setAction );
		void PropertyMap<T, TResult>( Expression<Func<T, TResult>> property, string columnName );
		void PropertyMap<T, TResult>( Expression<Func<T, TResult>> property, int columnId );
		void TableJoin<TParent, TChild>( Func<TParent, TChild, bool> canJoin, Action<TParent, List<TChild>> listSet );
		void Join<TParent, TChild>( Action<TParent, List<TChild>> listSet );
		void AddJoin( Func<ITableJoin, ITableJoinMap> func );
		void VariableTableType<T>( Func<IDataRecord, Type> action );
	}

	public interface ITableJoin
	{
		IDictionaryJoinCondition<T> DictionaryTableJoin<T>() where T : new();
	}

	public interface IDictionaryJoinCondition<T> where T : new()
	{
		IDictionaryKeyColumn<T> Condition( Func<T, dynamic, bool> condition );
	}

	public interface IDictionaryKeyColumn<T> where T : new()
	{
		IDictionaryValueColumn<T> KeyColumn( string keyColumn );
	}

	public interface IDictionaryValueColumn<T> where T : new()
	{
		IDictionaryDestinationProperty<T> ValueColumn( string valueColumn );
	}

	public interface IDictionaryDestinationProperty<T> where T : new()
	{
		IDictionaryChildTable SetDestinationProperty<TKey, TValue>( Action<T, Dictionary<TKey, TValue>> destination );
	}

	public interface IDictionaryChildTable
	{
		ITableJoinMap ChildTable( int childTableId );
	}
}