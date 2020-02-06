using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SqlObjectHydrator.Configuration.ExpressionGenerators
{
	internal static class DynamicToDictionaryGenerator
	{
		public static Action<TParent, List<ExpandoObject>> Generate<TParent, TKey, TValue>(
		  Action<TParent, Dictionary<TKey, TValue>> setAction,
		  string keyColumn,
		  string valueColumn )
		{
			return (Action<TParent, List<ExpandoObject>>)( ( parent, list ) => setAction( parent, list.ToDictionary<ExpandoObject, TKey, TValue>( (Func<ExpandoObject, TKey>)( x => (TKey)x.First<KeyValuePair<string, object>>( (Func<KeyValuePair<string, object>, bool>)( y => y.Key == keyColumn ) ).Value ), (Func<ExpandoObject, TValue>)( x => (TValue)x.First<KeyValuePair<string, object>>( (Func<KeyValuePair<string, object>, bool>)( y => y.Key == valueColumn ) ).Value ) ) ) );
		}
	}
}
