using System;

namespace SqlObjectHydrator.ClassMapping
{
	internal class PropertyMap
	{
		public PropertyMapType PropertyMapType { get; set; }

		public object SetAction { get; set; }
		public int ColumnId { get; set; }
		public string ColumnName { get; set; }

		public PropertyMap( Delegate setAction )
		{
			SetAction = setAction;
			PropertyMapType = PropertyMapType.Func;
		}

		public PropertyMap( int columnId )
		{
			ColumnId = columnId;
			PropertyMapType = PropertyMapType.ColumnId;
		}

		public PropertyMap( string columnName )
		{
			ColumnName = columnName;
			PropertyMapType = PropertyMapType.ColumnName;
		}
	}
}