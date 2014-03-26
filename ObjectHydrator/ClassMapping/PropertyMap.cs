using System.Reflection;

namespace SqlObjectHydrator.ClassMapping
{
	internal class PropertyMap : BaseMap
	{
		public PropertyMap( PropertyInfo propertyInfo, int fieldId )
		{
			PropertyInfo = propertyInfo;
			FieldId = fieldId;
		}

		public PropertyInfo PropertyInfo { get; set; }
		public object Map { get; set; }
		public int FieldId { get; set; }
		public bool Nullable { get; set; }
	}
}