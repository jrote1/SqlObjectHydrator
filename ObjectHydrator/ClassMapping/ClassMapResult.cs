using System.Collections.Generic;

namespace SqlObjectHydrator.ClassMapping
{
	internal class ClassMapResult
	{
		public ClassMapResult()
		{
			ClassMaps = new List<ClassMap>();
			TempDataStorage = new List<List<Dictionary<int, object>>>();
		}

		public List<List<Dictionary<int, object>>> TempDataStorage { get; private set; }
		public List<ClassMap> ClassMaps { get; set; }
		public Mapping Mappings { get; set; }
	}
}