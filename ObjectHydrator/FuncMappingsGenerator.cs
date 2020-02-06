using SqlObjectHydrator.ClassMapping;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.ILEmitting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlObjectHydrator
{
	internal static class FuncMappingsGenerator
	{
		public static Dictionary<MappingEnum, object> Generate( Mapping mappings )
		{
			return new Dictionary<MappingEnum, object>()
	  {
		{
		  MappingEnum.DictionaryJoin,
		  (object) mappings.DictionaryTableJoins.Select<DictionaryTableJoin, KeyValuePair<object, object>>((Func<DictionaryTableJoin, KeyValuePair<object, object>>) (x => new KeyValuePair<object, object>(x.Condition, x.Destination))).ToList<KeyValuePair<object, object>>()
		},
		{
		  MappingEnum.Join,
		  (object) mappings.Joins.Select<KeyValuePair<KeyValuePair<Type, Type>, object>, object>((Func<KeyValuePair<KeyValuePair<Type, Type>, object>, object>) (x => x.Value)).ToList<object>()
		},
		{
		  MappingEnum.TableJoin,
		  (object) mappings.TableJoins
		},
		{
		  MappingEnum.PropertyMap,
		  (object) mappings.PropertyMaps
		},
		{
		  MappingEnum.VariableTableType,
		  (object) mappings.VariableTableTypes
		}
	  };
		}
	}
}
