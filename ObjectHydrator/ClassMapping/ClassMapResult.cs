using System.Collections.Generic;

namespace SqlObjectHydrator.ClassMapping
{
	internal class ClassMapResult
	{
		public ClassMapResult()
		{
			this.TempDataStorage = new List<List<Dictionary<int, object>>>();
		}

		public List<List<Dictionary<int, object>>> TempDataStorage { get; private set; }

		public Mapping Mappings { get; set; }
	}
}
