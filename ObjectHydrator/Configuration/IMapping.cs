using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace SqlObjectHydrator.Configuration
{
	public interface IMapping
	{
		void Table<T>( int id );

		void PropertyMap<T, TResult>(
		  Expression<Func<T, TResult>> property,
		  Func<IDataRecord, TResult> setAction );

		void PropertyMap<T, TResult>( Expression<Func<T, TResult>> property, string columnName );

		void PropertyMap<T, TResult>( Expression<Func<T, TResult>> property, int columnId );

		void TableJoin<TParent, TChild>(
		  Func<TParent, TChild, bool> canJoin,
		  Action<TParent, List<TChild>> listSet );

		void Join<TParent, TChild>( Action<TParent, List<TChild>> listSet );

		void AddJoin( Func<ITableJoin, ITableJoinMap> func );

		void VariableTableType<T>( Func<IDataRecord, Type> action );
	}
}
