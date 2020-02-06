namespace SqlObjectHydrator.Configuration
{
	public interface IDictionaryKeyColumn<T> where T : new()
	{
		IDictionaryValueColumn<T> KeyColumn( string keyColumn );
	}
}
