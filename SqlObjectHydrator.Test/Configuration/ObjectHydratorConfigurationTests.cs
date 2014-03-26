using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.Test.ClassMapping;

namespace SqlObjectHydrator.Test.Configuration
{
	public class TestObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class TestObjectConfiguration : IObjectHydratorConfiguration
	{
		public void Mapping( IMapping mapping )
		{
			mapping.PropertyMap<TestObject, string>( x => x.Name, x => "Hello World" );
		}
	}


	[TestFixture]
	public class ObjectHydratorConfigurationTests
	{
		[Test]
		public void DataReaderToList_WithMappingThatSetsVirtualStringProperty_ReturnsList()
		{
			var dataReader = new MockDataReader();
			var table = dataReader.AddTable( "Id" );
			dataReader.AddRow( table, 1 );

			var result = dataReader.DataReaderToList<TestObject>( typeof ( TestObjectConfiguration ) );

			Assert.AreEqual( "Hello World", result[ 0 ].Name );
		}
	}
}