using System;
using System.Collections.Generic;

namespace SqlObjectHydrator.Configuration
{
	internal class DictionaryTableJoin<T> : IDictionaryJoinCondition<T>, IDictionaryKeyColumn<T>, IDictionaryValueColumn<T>, IDictionaryDestinationProperty<T>, IDictionaryChildTable
	where T : new()
	{
		private readonly DictionaryTableJoin _dictionaryTableJoin;

		public DictionaryTableJoin()
		{
			this._dictionaryTableJoin = new DictionaryTableJoin()
			{
				ParentTableType = typeof( T )
			};
		}

		public IDictionaryKeyColumn<T> Condition( Func<T, object, bool> condition )
		{
			this._dictionaryTableJoin.Condition = (object)condition;
			return (IDictionaryKeyColumn<T>)this;
		}

		public IDictionaryValueColumn<T> KeyColumn( string keyColumn )
		{
			this._dictionaryTableJoin.KeyColumn = keyColumn;
			return (IDictionaryValueColumn<T>)this;
		}

		public IDictionaryDestinationProperty<T> ValueColumn(
		  string valueColumn )
		{
			this._dictionaryTableJoin.ValueColumn = valueColumn;
			return (IDictionaryDestinationProperty<T>)this;
		}

		public IDictionaryChildTable SetDestinationProperty<TKey, TValue>(
		  Action<T, Dictionary<TKey, TValue>> destination )
		{
			this._dictionaryTableJoin.Destination = (object)destination;
			this._dictionaryTableJoin.KeyType = typeof( TKey );
			this._dictionaryTableJoin.ValueType = typeof( TValue );
			return (IDictionaryChildTable)this;
		}

		public ITableJoinMap ChildTable( int childTableId )
		{
			this._dictionaryTableJoin.ChildTable = childTableId;
			return (ITableJoinMap)this._dictionaryTableJoin;
		}
	}
}
