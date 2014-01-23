using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.DataReaderMapping;

namespace SqlObjectHydrator
{
    public class ObjectHydrator : IObjectHydrator
    {
        private static readonly Object storeMappingLock = new object();

        public List<T> DataReaderToList<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration = null ) where T : new()
        {
            configuration = configuration ?? new ObjectHydratorConfiguration<T>();

            var mappingCache = new MappingCache();
            var containsMapping = ContainsMapping( dataReader, configuration, mappingCache );
            if ( !containsMapping )
            {
                //var validConfiguration = new ConfigurationValidator().ValidateConfiguration( dataReader, configuration );
                //if ( !validConfiguration.ConfigurationValid )
                //    throw new Exception( String.Join( "\n", validConfiguration.Errors ) );

                lock ( storeMappingLock )
                {
                    if ( !ContainsMapping( dataReader, configuration, mappingCache ) )
                        mappingCache.StoreMapping( dataReader, configuration, new MappingGenerator().GenerateMapping( dataReader, configuration ) );
                }
            }
            return mappingCache.GetCachedMapping( dataReader, configuration )( dataReader, configuration.MappingsActions.Select( x => x.Value ).ToList() );
        }

        private static bool ContainsMapping<T>( IDataReader dataReader, ObjectHydratorConfiguration<T> configuration, MappingCache mappingCache ) where T : new()
        {
            return mappingCache.ContainsMapping( dataReader, configuration );
        }
    }
}