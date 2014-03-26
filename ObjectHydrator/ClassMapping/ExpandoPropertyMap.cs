using System;

namespace SqlObjectHydrator.ClassMapping
{
	internal class ExpandoPropertyMap : BaseMap
	{
		public ExpandoPropertyMap( string name, Type type, int fieldId )
		{
			Name = name;
			Type = type;
			FieldId = fieldId;
		}

		public string Name { get; set; }
		public Type Type { get; set; }
		public int FieldId { get; set; }
	}
}