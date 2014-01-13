using System.Collections.Generic;
using System.Data;
using ObjectHydrator.Configuration;

namespace ObjectHydrator
{
    public interface IObjectHydrator
    {
        List<T> DataReaderToList<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration = null ) where T : new();
    }
}