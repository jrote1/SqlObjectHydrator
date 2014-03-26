using NUnit.Framework;
using SqlObjectHydrator.Caching;
using SqlObjectHydrator.Test.ClassMapping;

namespace SqlObjectHydrator.Test.Caching
{
	public class TestClass1 {}

	public class TestClass2 {}

	public class Configuration1 {}

	public class Configuration2 {}

	public class CacheManagerTests
	{
		[SetUp]
		public void SetUp()
		{
			CacheManager.MappingCaches.Clear();
		}

		[Test]
		public void ContainsMappingCache_WhenDoesNotContainCache_ReturnsFalse()
		{
			var dataReader = new MockDataReader();
			dataReader.AddTable();

			Assert.IsFalse( CacheManager.ContainsMappingCache<TestClass1>( dataReader, null ) );
		}

		[Test]
		public void ContainsMappingCache_WhenContainsCache_ReturnsFalse()
		{
			var dataReader = new MockDataReader();
			dataReader.AddTable();

			CacheManager.StoreMappingCache( () => new MappingCache<TestClass1>( null, null ), dataReader, null );

			Assert.IsTrue( CacheManager.ContainsMappingCache<TestClass1>( dataReader, null ) );
		}

		[Test]
		public void ContainsMappingCache_WhenContainsCacheButDoesNotMatch_ReturnsFalse()
		{
			var dataReader = new MockDataReader();
			dataReader.AddTable();

			CacheManager.StoreMappingCache( () => new MappingCache<TestClass1>( null, null ), dataReader, null );

			Assert.IsFalse( CacheManager.ContainsMappingCache<TestClass2>( dataReader, null ) );
		}

		[Test]
		public void ContainsMappingCache_WhenContainsCacheButDataReaderDoesNotMatch_ReturnsFalse()
		{
			var dataReader1 = new MockDataReader();
			var table1 = dataReader1.AddTable( "Name" );
			dataReader1.AddRow( table1, "" );
			var dataReader2 = new MockDataReader();
			var table2 = dataReader2.AddTable( "Age" );
			dataReader1.AddRow( table2, 1 );

			CacheManager.StoreMappingCache( () => new MappingCache<TestClass1>( null, null ), dataReader1, null );

			Assert.IsTrue( CacheManager.ContainsMappingCache<TestClass1>( dataReader1, null ) );
			Assert.IsFalse( CacheManager.ContainsMappingCache<TestClass1>( dataReader2, null ) );
		}

		[Test]
		public void ContainsMappingCache_WhenContainsCacheSameDataReaderButDifferentConfiguration_ReturnsFalse()
		{
			var dataReader = new MockDataReader();
			var table1 = dataReader.AddTable( "Name" );
			dataReader.AddRow( table1, "" );

			CacheManager.StoreMappingCache( () => new MappingCache<TestClass1>( null, null ), dataReader, typeof( Configuration1 ) );

			Assert.IsTrue( CacheManager.ContainsMappingCache<TestClass1>( dataReader, typeof( Configuration1 ) ) );
			Assert.IsFalse( CacheManager.ContainsMappingCache<TestClass1>( dataReader, typeof( Configuration2 ) ) );
		}

		[Test]
		public void GetMappingCache_WhenContainsCacheSameDataReaderButDifferentConfiguration_ReturnsFalse()
		{
			var dataReader = new MockDataReader();
			var table1 = dataReader.AddTable( "Name" );
			dataReader.AddRow( table1, "" );

			var mappingCache = new MappingCache<TestClass1>( null, null );
			CacheManager.StoreMappingCache( () => mappingCache, dataReader, typeof( Configuration1 ) );

			Assert.AreEqual( mappingCache, CacheManager.GetMappingCache<TestClass1>( dataReader, typeof( Configuration1 ) ) );
		}
	}
}