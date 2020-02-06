using System;

namespace SqlObjectHydrator.Configuration
{
	internal class DictionaryTableJoin : ITableJoinMap
	{
		public Type ParentTableType { get; set; }

		public object Condition { get; set; }

		public object Destination { get; set; }

		public int ChildTable { get; set; }

		public string KeyColumn { get; set; }

		public Type KeyType { get; set; }

		public string ValueColumn { get; set; }

		public Type ValueType { get; set; }
	}
}
