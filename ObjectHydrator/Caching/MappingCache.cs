using SqlObjectHydrator.ILEmitting;
using System;
using System.Collections.Generic;
using System.Data;

namespace SqlObjectHydrator.Caching
{
	internal class MappingCache<T>
	{
		public MappingCache(
		  System.Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Tuple<int, Type>, System.Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T> func,
		  Dictionary<MappingEnum, object> mappings )
		{
			this.Func = func;
			this.Mappings = mappings;
			this.TypeMaps = new Dictionary<Tuple<int, Type>, System.Func<IDataRecord, Dictionary<MappingEnum, object>, object>>();
		}

		private System.Func<IDataReader, Dictionary<MappingEnum, object>, Dictionary<Tuple<int, Type>, System.Func<IDataRecord, Dictionary<MappingEnum, object>, object>>, T> Func { get; set; }

		private Dictionary<MappingEnum, object> Mappings { get; set; }

		private Dictionary<Tuple<int, Type>, System.Func<IDataRecord, Dictionary<MappingEnum, object>, object>> TypeMaps { get; set; }

		public T Run( IDataReader dataReader )
		{
			return this.Func( dataReader, this.Mappings, this.TypeMaps );
		}
	}
}
