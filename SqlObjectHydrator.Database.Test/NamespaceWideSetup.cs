using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SqlObjectHydrator.Database.Test
{

	[SetUpFixture]
	public class NamespaceWideSetup
	{
		public const string DatabaseName = "SqlObjectHydratorDatabaseTests";

		[SetUp]
		public void Setup()
		{
			var databaseCreator = new DatabaseCreator(
				ConfigurationManager.AppSettings[ "DatabaseServerName" ],
				ConfigurationManager.AppSettings[ "DatabaseFolder" ] ?? "D:\\databases\\" );

			databaseCreator.CreateDatabase( DatabaseName );

		}
	}
}
