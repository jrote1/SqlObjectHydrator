using System.Collections.Generic;
using System.Data;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator
{
    public interface IObjectHydrator
    {
        List<T> DataReaderToList<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration = null ) where T : new();
    }
}