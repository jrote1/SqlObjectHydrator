using System;
using System.Collections.Generic;

namespace SqlObjectHydrator.ClassMapping
{
	internal class ClassMap : BaseMap
	{
		public ClassMap( Type type, int tableId )
		{
			Type = type;
			Properties = new List<BaseMap>();
			TableId = tableId;
		}

		public int TableId { get; set; }
		public Type Type { get; private set; }
		public List<BaseMap> Properties { get; private set; }
	}
}