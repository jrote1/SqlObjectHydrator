using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ObjectHydrator.Configuration;
using ObjectHydrator.DataReaderMapping;

namespace ObjectHydrator
{
    public class ObjectHydrator : IObjectHydrator
    {
        public List<T> DataReaderToList<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration = null ) where T : new()
        {
            configuration = configuration ?? new ObjectHydratorConfiguration<T>();

            var mappingCache = new MappingCache();
            var containsMapping = mappingCache.ContainsMapping( dataReader, configuration ?? new ObjectHydratorConfiguration<T>() );
            if ( !containsMapping )
            {
                var validConfiguration = new ConfigurationValidator().ValidateConfiguration( dataReader, configuration );
                if ( !validConfiguration.ConfigurationValid )
                    throw new Exception( String.Join( "\n", validConfiguration.Errors ) );
                mappingCache.StoreMapping( dataReader, configuration, new MappingGenerator().GenerateMapping( dataReader, configuration ) );
            }
            return mappingCache.GetCachedMapping( dataReader, configuration )( dataReader,configuration.MappingsActions.Select( x=>x.Value ).ToList() );
        }
    }
}