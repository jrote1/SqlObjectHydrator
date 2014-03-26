using System;
using System.Collections.Generic;
using System.Data;
using SqlObjectHydrator.ILEmitting;

namespace SqlObjectHydrator.Caching
{
	internal class MappingCache<T>
	{
		public MappingCache( Func<IDataReader, Dictionary<MappingEnum, object>, T> func, Dictionary<MappingEnum, object> mappings )
		{
			Func = func;
			Mappings = mappings;
		}

		private Func<IDataReader, Dictionary<MappingEnum, object>, T> Func { get; set; }
		private Dictionary<MappingEnum, object> Mappings { get; set; }

		public T Run( IDataReader dataReader )
		{
			return Func( dataReader, Mappings );
		}
	}
}