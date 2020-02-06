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
			this.SetAction = (object)setAction;
			this.PropertyMapType = PropertyMapType.Func;
		}

		public PropertyMap( int columnId )
		{
			this.ColumnId = columnId;
			this.PropertyMapType = PropertyMapType.ColumnId;
		}

		public PropertyMap( string columnName )
		{
			this.ColumnName = columnName;
			this.PropertyMapType = PropertyMapType.ColumnName;
		}
	}
}
