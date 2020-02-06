using System;
using System.Collections.Generic;

namespace SqlObjectHydrator.Configuration
{
	public interface IDictionaryDestinationProperty<T> where T : new()
	{
		IDictionaryChildTable SetDestinationProperty<TKey, TValue>(
		  Action<T, Dictionary<TKey, TValue>> destination );
	}
}
