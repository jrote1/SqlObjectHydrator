using System.Collections.Generic;
using System.Data;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator
{
	public class ObjectHydrator : IObjectHydrator
	{
		public List<T> DataReaderToList<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration = null ) where T : new()
		{
			return dataReader.DataReaderToList( configuration );
		}

		public T DataReaderToObject<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration = null ) where T : new()
		{
			return dataReader.DataReaderToObject( configuration );
		}
	}
}