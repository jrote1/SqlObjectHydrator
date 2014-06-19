using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SqlObjectHydrator.Caching;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.Test.ClassMapping;

namespace SqlObjectHydrator.Test
{
	public class TransactionManager {}

	public class RootTable
	{
		public string Name { get; set; }
		internal int Size { get; set; }
		public List<Product> Products { get; set; }
		public TransactionManager TransactionManager { get; set; }
		public DateTime DueDate { get; set; }
	}

	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public bool Available { get; set; }
		public Dictionary<int, int> Stock { get; set; }
		public List<ProductRating> ProductRatings { get; set; }
	}

	public class ProductRating
	{
		public int ProductId { get; set; }
		public double Rating { get; set; }
	}

	public class ShopConfiguration : IObjectHydratorConfiguration
	{
		public void Mapping( IMapping mapping )
		{
			mapping.Table<Product>( 1 );
			mapping.Table<ProductRating>( 3 );

			mapping.PropertyMap<RootTable, int>( x => x.Size, record => record.GetInt32( 1 ) * 2 );
			mapping.PropertyMap<RootTable, TransactionManager>( x => x.TransactionManager, reader => null );
			mapping.PropertyMap<RootTable, DateTime>( x => x.DueDate, ( record ) => record.GetValue( 2 ) == DBNull.Value ? default( DateTime ) : record.GetDateTime( 2 ) );
			mapping.TableJoin<Product, ProductRating>( ( product, rating ) => product.Id == rating.ProductId, ( product, list ) => product.ProductRatings = list );
			mapping.Join<RootTable, Product>( ( x, y ) => x.Products = y );
			mapping.AddJoin( x => x.DictionaryTableJoin<Product>()
				.Condition( ( product, o ) => product.Id == o.ProductId )
				.KeyColumn( "Id" )
				.ValueColumn( "Quantity" )
				.SetDestinationProperty<int, int>( ( product, values ) => product.Stock = values )
				.ChildTable( 2 ) );
		}
	}

	[TestFixture]
	public class ObjectHydratorTests
	{
		[SetUp]
		public void SetUp()
		{
			CacheManager.MappingCaches.Clear();
		}

		[Test]
		public void DataReaderToList_WhenUsingConfiguration_ReturnsCorrectResult()
		{
			var dataReader = new MockDataReader();
			var rootTable = dataReader.AddTable( "Name", "Size", "DueDate" );
			var products = dataReader.AddTable( "Id", "Name", "Price", "Available" );
			var stock = dataReader.AddTable( "ProductId", "Id", "Quantity" );
			var ratings = dataReader.AddTable( "ProductId", "Rating" );

			dataReader.AddRow( rootTable, "The Shop", 12, DateTime.Now );
			dataReader.AddRow( products, 1, "Drink", 1.99m, true );
			dataReader.AddRow( products, 2, "Snack", 0.78m, false );
			dataReader.AddRow( stock, 1, 1, 20 );
			dataReader.AddRow( stock, 1, 2, 5 );
			dataReader.AddRow( stock, 2, 3, 0 );
			dataReader.AddRow( ratings, 2, (double)2 );

			var result = dataReader.DataReaderToList<RootTable>( typeof( ShopConfiguration ) );

			Assert.AreEqual( "The Shop", result[ 0 ].Name );
			Assert.AreEqual( 24, result[ 0 ].Size );
			Assert.AreEqual( 2, result[ 0 ].Products.Count );
			Assert.AreEqual( 1, result[ 0 ].Products[ 0 ].Id );
			Assert.AreEqual( "Drink", result[ 0 ].Products[ 0 ].Name );
			Assert.AreEqual( 1.99M, result[ 0 ].Products[ 0 ].Price );
			Assert.AreEqual( true, result[ 0 ].Products[ 0 ].Available );
			Assert.AreEqual( 2, result[ 0 ].Products[ 1 ].Id );
			Assert.AreEqual( "Snack", result[ 0 ].Products[ 1 ].Name );
			Assert.AreEqual( 0.78M, result[ 0 ].Products[ 1 ].Price );
			Assert.AreEqual( false, result[ 0 ].Products[ 1 ].Available );
			Assert.AreEqual( 2, result[ 0 ].Products[ 0 ].Stock.Count );
			Assert.AreEqual( 20, result[ 0 ].Products[ 0 ].Stock[ 1 ] );
			Assert.AreEqual( 5, result[ 0 ].Products[ 0 ].Stock[ 2 ] );
			Assert.AreEqual( 1, result[ 0 ].Products[ 1 ].Stock.Count );
			Assert.AreEqual( 0, result[ 0 ].Products[ 1 ].Stock[ 3 ] );
			Assert.AreEqual( 0, result[ 0 ].Products[ 0 ].ProductRatings.Count );
			Assert.AreEqual( 1, result[ 0 ].Products[ 1 ].ProductRatings.Count );
			Assert.AreEqual( 2, result[ 0 ].Products[ 1 ].ProductRatings[ 0 ].Rating );
		}


		[Test]
		public void DataReaderToList_WithNullablePropertyThatHasAValueOnTheFirstRowAndNullOnTheSecondRow_SetsTheSecondObjectsPropertyToNull()
		{
			var dataReader = new MockDataReader();
			var rootTable = dataReader.AddTable( "Name", "Score" );

			dataReader.AddRow( rootTable, "Team A", (int?)12 );
			dataReader.AddRow( rootTable, "Team B", DBNull.Value );

			var result = dataReader.DataReaderToList<NullableTest>();

			Assert.AreEqual( 12, result[ 0 ].Score );
			Assert.IsNull( result[ 1 ].Score );
		}

		[Test]
		public void DataReaderToList_WithTwoTableJoinsForSameTable_JoinsTables()
		{
			var dataReader = new MockDataReader();
			var rootTable = dataReader.AddTable( "HomeTeamId", "HomeTeamId1", "AwayTeamId" );
			dataReader.AddRow( rootTable, DBNull.Value, 2, 1 );

			var teamTable = dataReader.AddTable( "Id", "Name" );
			dataReader.AddRow( teamTable, 1, "Team A" );
			dataReader.AddRow( teamTable, 2, "Team B" );

			var result = dataReader.DataReaderToList<Score>( typeof( ScoreConfiguration ) );

			Assert.AreEqual( "Team A", result[ 0 ].AwayTeam.Name );
			Assert.AreEqual( "Team B", result[ 0 ].HomeTeam.Name );
		}

		[Test]
		public void DataReaderToList_WhenPropertyHasCorrosponingColmunButHasPropertyMap_OnlyUsesPropertyMap()
		{
			var dataReader = new MockDataReader();
			var rootTable = dataReader.AddTable( "IdVal", "Id" );
			dataReader.AddRow( rootTable, 1, 2 );
			dataReader.AddRow( rootTable, 1, DBNull.Value );

			var result = dataReader.DataReaderToList<PropertyMapTest>( typeof( PropertyMapTestConfiguration ) );
		}

		[Test]
		public void DataReaderToList_WhenMultipleDictionaryJoins_DoesNotUseSameExpandoObjectMap()
		{
			var dataReader = new MockDataReader();
			var rootTable = dataReader.AddTable( "Id" );
			var users = dataReader.AddTable( "Key", "Value" );
			var places = dataReader.AddTable( "Key", "Value" );

			dataReader.AddRow( rootTable, 1 );

			dataReader.AddRow( users, 1, "User 1" );

			dataReader.AddRow( places, "Place 1", "Location" );

			var result = dataReader.DataReaderToList<WhenMultipleDictionaryJoins>( typeof( WhenMultipleDictionaryJoinsConfiguration ) );
		}
	}


	public class WhenMultipleDictionaryJoins
	{
		public int Id { get; set; }
		public Dictionary<int, string> Users { get; set; }
		public Dictionary<string, string> Places { get; set; }
	}

	public class WhenMultipleDictionaryJoinsConfiguration : IObjectHydratorConfiguration
	{
		public void Mapping( IMapping mapping )
		{
			mapping.Table<WhenMultipleDictionaryJoins>( 0 );

			mapping.AddJoin( join =>
				join.DictionaryTableJoin<WhenMultipleDictionaryJoins>()
					.Condition( ( o, d ) => true )
					.KeyColumn( "Key" )
					.ValueColumn( "Value" )
					.SetDestinationProperty<int, string>( ( o, d ) => o.Users = d )
					.ChildTable( 1 ) );

			mapping.AddJoin( join =>
				join.DictionaryTableJoin<WhenMultipleDictionaryJoins>()
					.Condition( ( o, d ) => true )
					.KeyColumn( "Key" )
					.ValueColumn( "Value" )
					.SetDestinationProperty<string, string>( ( o, d ) => o.Places = d )
					.ChildTable( 2 ) );
		}
	}

	public class ScoreConfiguration : IObjectHydratorConfiguration
	{
		public void Mapping( IMapping mapping )
		{
			mapping.Table<Score>( 0 );
			mapping.Table<Team>( 1 );

			mapping.PropertyMap<Score, int>( x => x.HomeTeamId, "HomeTeamId1" );

			mapping.TableJoin<Score, Team>( ( s, t ) => s.AwayTeamId == t.Id, ( s, t ) => s.AwayTeam = t.FirstOrDefault() );
			mapping.TableJoin<Score, Team>( ( s, t ) => s.HomeTeamId == t.Id, ( s, t ) => s.HomeTeam = t.FirstOrDefault() );
		}
	}

	public class PropertyMapTest
	{
		public int Id { get; set; }
	}

	public class PropertyMapTestConfiguration : IObjectHydratorConfiguration
	{
		public void Mapping( IMapping mapping )
		{
			mapping.Table<PropertyMapTest>( 0 );

			mapping.PropertyMap<PropertyMapTest, int>( x => x.Id, "IdVal" );
		}
	}

	public class Score
	{
		public int HomeTeamId { get; set; }
		public int AwayTeamId { get; set; }

		public Team HomeTeam { get; set; }
		public Team AwayTeam { get; set; }
	}

	public class Team
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public class NullableTest
	{
		public string Name { get; set; }
		public int? Score { get; set; }
	}
}