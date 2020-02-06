namespace SqlObjectHydrator.Configuration
{
	public interface IDictionaryChildTable
	{
		ITableJoinMap ChildTable( int childTableId );
	}
}
