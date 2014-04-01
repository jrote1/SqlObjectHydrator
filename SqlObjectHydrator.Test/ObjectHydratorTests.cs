using System;
using System.Collections.Generic;
using NUnit.Framework;
using SqlObjectHydrator.Caching;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.Test.ClassMapping;

namespace SqlObjectHydrator.Test
{
	public class TransactionManager
	{
		
	}

	public class RootTable
	{
		public string Name { get; set; }
		internal int Size { get; set; }
		public List<Product> Products { get; set; }
		public TransactionManager TransactionManager { get; set; }
		public DateTime DueDate { get; set;  }
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
			mapping.PropertyMap<RootTable,TransactionManager>( x=>x.TransactionManager,reader=>null );
			mapping.PropertyMap<RootTable,DateTime>( x => x.DueDate, ( record ) => record.GetValue( 2) == DBNull.Value ? default( DateTime ) : record.GetDateTime( 2 ) );
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
			var rootTable = dataReader.AddTable( "Name", "Size","DueDate" );
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
	}
}