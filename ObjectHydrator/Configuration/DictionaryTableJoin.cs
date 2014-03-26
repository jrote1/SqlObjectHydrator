using System;
using System.Collections.Generic;

namespace SqlObjectHydrator.Configuration
{
	internal class DictionaryTableJoin : ITableJoinMap
	{
		public Type ParentTableType { get; set; }
		public object Condition { get; set; }
		public object Destination { get; set; }
		public int ChildTable { get; set; }
		public string KeyColumn { get; set; }
		public Type KeyType { get; set; }
		public string ValueColumn { get; set; }
		public Type ValueType { get; set; }
	}

	internal class DictionaryTableJoin<T> : IDictionaryJoinCondition<T>,
	                                      IDictionaryKeyColumn<T>,
	                                      IDictionaryValueColumn<T>,
	                                      IDictionaryDestinationProperty<T>,
	                                      IDictionaryChildTable where T : new()
	{
		private readonly DictionaryTableJoin _dictionaryTableJoin;

		public DictionaryTableJoin()
		{
			_dictionaryTableJoin = new DictionaryTableJoin
			{
				ParentTableType = typeof(T)
			};
			
		}

		public IDictionaryKeyColumn<T> Condition( Func<T, dynamic, bool> condition )
		{
			_dictionaryTableJoin.Condition = condition;
			return this;
		}

		public IDictionaryValueColumn<T> KeyColumn( string keyColumn )
		{
			_dictionaryTableJoin.KeyColumn = keyColumn;
			return this;
		}

		public IDictionaryDestinationProperty<T> ValueColumn( string valueColumn )
		{
			_dictionaryTableJoin.ValueColumn = valueColumn;
			return this;
		}

		public IDictionaryChildTable SetDestinationProperty<TKey, TValue>( Action<T, Dictionary<TKey, TValue>> destination )
		{
			_dictionaryTableJoin.Destination = destination;
			_dictionaryTableJoin.KeyType = typeof( TKey );
			_dictionaryTableJoin.ValueType = typeof( TValue );
			return this;
		}

		public ITableJoinMap ChildTable( int childTableId )
		{
			_dictionaryTableJoin.ChildTable = childTableId;
			return _dictionaryTableJoin;
		}
	}
}