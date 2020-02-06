using System;
using System.Reflection;

namespace SqlObjectHydrator.ClassMapping
{
	internal static class PropertyExtensionMethods
	{
		public static bool IsNullable( this PropertyInfo propertyInfo )
		{
			if ( !propertyInfo.PropertyType.IsGenericType )
				return false;
			return propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof( Nullable<> );
		}

		public static Type GetActualPropertyType( this PropertyInfo propertyInfo )
		{
			if ( propertyInfo.IsNullable() )
				return propertyInfo.PropertyType.GetGenericArguments()[ 0 ];
			return propertyInfo.PropertyType;
		}
	}
}
