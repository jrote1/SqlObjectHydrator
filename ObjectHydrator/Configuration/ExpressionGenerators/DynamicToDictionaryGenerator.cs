using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SqlObjectHydrator.Configuration.ExpressionGenerators
{
	internal static class DynamicToDictionaryGenerator
	{
		public static Action<TParent, List<ExpandoObject>> Generate<TParent, TKey, TValue>( Action<TParent, Dictionary<TKey, TValue>> setAction, string keyColumn, string valueColumn )
		{
			return ( parent, list ) => setAction( parent, list.ToDictionary( x => (TKey)x.First( y => y.Key == keyColumn ).Value, x => (TValue)x.First( y => y.Key == valueColumn ).Value ) );
		} 
	}
}
