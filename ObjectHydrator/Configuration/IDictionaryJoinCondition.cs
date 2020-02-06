using System;

namespace SqlObjectHydrator.Configuration
{
	public interface IDictionaryJoinCondition<T> where T : new()
	{
		IDictionaryKeyColumn<T> Condition( Func<T, object, bool> condition );
	}
}
