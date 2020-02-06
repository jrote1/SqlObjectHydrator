using System;

namespace SqlObjectHydrator.ClassMapping
{
	internal class TableJoinMap
	{
		public Type ParentType { get; set; }

		public Type ChildType { get; set; }

		public object CanJoin { get; set; }

		public object ListSet { get; set; }
	}
}
