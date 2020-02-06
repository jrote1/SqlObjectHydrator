using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator.ClassMapping
{
	internal class TableJoin : ITableJoin
	{
		public IDictionaryJoinCondition<T> DictionaryTableJoin<T>() where T : new()
		{
			return (IDictionaryJoinCondition<T>)new DictionaryTableJoin<T>();
		}
	}
}
