namespace SqlObjectHydrator.Configuration
{
	public interface IDictionaryValueColumn<T> where T : new()
	{
		IDictionaryDestinationProperty<T> ValueColumn( string valueColumn );
	}
}
