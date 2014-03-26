using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationBlocks.Data;
using NUnit.Framework;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator.Database.Test.Configuration
{
	[TestFixture]
	public class ConfigurationTests
	{
		private const string Sql = @"
select 
	*
from 
	TestObject";

		[Test]
		public void DataReaderToList_WithConfigurationThatSetsPropertyToStringEmpty_ReturnsListWithPropertiesCorrectlySet()
		{
			using ( var reader = SqlHelper.ExecuteReader( GetConnectionString(), CommandType.Text, Sql ) )
			{
				var result = reader.DataReaderToList<TestObject>( typeof( TestObjectConfiguration ) );
				Assert.AreEqual( 3, result.Count );
				Assert.AreEqual( "Hello World", result[ 0 ].TeamName );
			}
		}

		private static string GetConnectionString()
		{
			return String.Format( DatabaseCreator.ConnectionString, ConfigurationManager.AppSettings[ "DatabaseServerName" ], NamespaceWideSetup.DatabaseName );
		}
	}

	public class TestObjectConfiguration : IObjectHydratorConfiguration
	{
		public void Mapping( IMapping mapping )
		{
			mapping.PropertyMap<TestObject, string>( x => x.TeamName, x => "Hello World" );
		}
	}

	public class TestObject
	{
		public int UserId { get; set; }
		public string UserFirstName { get; set; }
		public string UserLastName { get; set; }
		public string UserFullName { get { return String.Format( "{0} {1}", this.UserFirstName, this.UserLastName ); } }
		public int FixtureId { get; set; }
		public int TeamId { get; set; }
		public virtual string TeamName { get; set; }
	}
}