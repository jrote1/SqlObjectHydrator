namespace SqlObjectHydrator.Configuration
{
	public interface ITableJoin
	{
		IDictionaryJoinCondition<T> DictionaryTableJoin<T>() where T : new();
	}
}
