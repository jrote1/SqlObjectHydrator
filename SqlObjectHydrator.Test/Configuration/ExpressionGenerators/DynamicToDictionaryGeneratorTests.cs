using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NUnit.Framework;
using SqlObjectHydrator.Configuration.ExpressionGenerators;

namespace SqlObjectHydrator.Test.Configuration.ExpressionGenerators
{
	public class User
	{
		public Dictionary<int,string> Teams { get; set; }
	}

	[TestFixture]
	public class DynamicToDictionaryGeneratorTests
	{
		[Test]
		public void Generate_WhenListOfDynamics_ReturnsCorrectExpression()
		{
			dynamic team1 = new ExpandoObject();
			team1.key = 1;
			team1.value = "Team1";

			dynamic team2 = new ExpandoObject();
			team2.key = 2;
			team2.value = "Team2";

			Action<User, Dictionary<int, string>> setAction = ( x, dictionary ) => x.Teams = dictionary;

			var action = DynamicToDictionaryGenerator.Generate( setAction, "key", "value" );

			var dynamics = new List<ExpandoObject>
			{
				team1,
				team2
			};

			var user = new User();
			action( user, dynamics );

			Assert.AreEqual( 1, user.Teams.First().Key );
			Assert.AreEqual( "Team1", user.Teams.First().Value );
			Assert.AreEqual( 2, user.Teams.Last().Key );
			Assert.AreEqual( "Team2", user.Teams.Last().Value );

		}
	}
}
