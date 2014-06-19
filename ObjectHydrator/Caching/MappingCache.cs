using System;
using System.Collections.Generic;
using System.Data;
using SqlObjectHydrator.ILEmitting;

namespace SqlObjectHydrator.Caching
{
	internal class MappingCache<T>
	{
		public MappingCache( Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Tuple<int,Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T> func, Dictionary<MappingEnum, object> mappings )
		{
			Func = func;
			Mappings = mappings;
			TypeMaps = new Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>();
		}

		private Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T> Func { get; set; }
		private Dictionary<MappingEnum, object> Mappings { get; set; }
		private Dictionary<Tuple<int, Type>, Func<IDataRecord, Dictionary<MappingEnum, object>, object>> TypeMaps { get; set; }

		public T Run( IDataReader dataReader ) 
		{
			return Func( dataReader, Mappings, TypeMaps );
		}
	}
}