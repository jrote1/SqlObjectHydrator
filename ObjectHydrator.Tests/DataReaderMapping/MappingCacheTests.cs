using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.DataReaderMapping;
using SqlObjectHydrator.Tests.TestData;

namespace SqlObjectHydrator.Tests.DataReaderMapping
{
	[TestFixture]
	public class MappingCacheTests
	{
		[SetUp]
		public void SetUp()
		{
			MappingCache.InternalMappingCache.Clear();
		}

		[Test]
		public void ContainsMapping_WhenNoMappingsHaveBeenCached_ReturnsFalse()
		{
			var dataReader = GetMockDataReader( 0 );
			var configuration = new ObjectHydratorConfiguration<User>();

			var mappingCache = new MappingCache();
			var result = mappingCache.ContainsMapping<User,List<User>>( dataReader.Object, configuration );

			Assert.IsFalse( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAndMatchesCurrentMapping_ReturnsTrue()
		{
			var dataReader = GetMockDataReader( 1 );
			var configuration = new ObjectHydratorConfiguration<User>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configuration, ( x, y ) => new List<User>() );

			var result = mappingCache.ContainsMapping<User,List<User>>( dataReader.Object, configuration );


			Assert.IsTrue( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAndDataReaderMatchesButNotConfiguration_ReturnsTrue()
		{
			var dataReader = GetMockDataReader( 2 );
			var configuration1 = new ObjectHydratorConfiguration<User>();
			var configuration2 = new ObjectHydratorConfiguration<User>()
				.Mapping( x => x.FullName, x => x[ "FirstName" ].ToString() );
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configuration1, ( x, y ) => new List<User>() );

			var result = mappingCache.ContainsMapping<User,List<User>>( dataReader.Object, configuration2 );


			Assert.IsFalse( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAndDataReaderNotSameButConfigurationIs_ReturnsTrue()
		{
			var dataReader1 = GetMockDataReader( 3 );
			var dataReader2 = GetMockDataReader( 4 );
			var configuration = new ObjectHydratorConfiguration<User>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader1.Object, configuration, ( x, y ) => new List<User>() );

			var result = mappingCache.ContainsMapping<User,List<User>>( dataReader2.Object, configuration );


			Assert.IsFalse( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAreSameButDateReaderAndConfigurationDifferentInstance_ReturnsTrue()
		{
			var dataReader1 = GetMockDataReader( 5 );
			var dataReader2 = GetMockDataReader( 5 );
			var configuration1 = new ObjectHydratorConfiguration<User>();
			var configuration2 = new ObjectHydratorConfiguration<User>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader1.Object, configuration1, ( x, y ) => new List<User>() );

			var result = mappingCache.ContainsMapping<User,List<User>>( dataReader2.Object, configuration2 );


			Assert.IsTrue( result );
		}

		[Test]
		public void GetCachedMapping_WhenMappingDosentExist_ReturnsNull()
		{
			var dataReader = GetMockDataReader( 6 );
			var configuration = new ObjectHydratorConfiguration<User>();
			var mappingCache = new MappingCache();

			var result = mappingCache.GetCachedMapping<User,List<User>>( dataReader.Object, configuration );

			Assert.IsNull( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAnd_ReturnsFunc()
		{
			var dataReader = GetMockDataReader( 7 );
			var configuration = new ObjectHydratorConfiguration<User>();
			Func<IDataReader, List<LambdaExpression>, List<User>> mapping = ( x, y ) => new List<User>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configuration, mapping );

			var result = mappingCache.GetCachedMapping<User,List<User>>( dataReader.Object, configuration );


			Assert.AreEqual( mapping, result );
		}

		[Test]
		public void GetCachedMapping_WhenMappingHasBeenSavedWithSameDataReaderAndConfiguration_DoesNotThrowException()
		{
			var dataReader = new Mock<IDataReader>();
			var configurationUser = new ObjectHydratorConfiguration<User>();
			var configurationContact = new ObjectHydratorConfiguration<Contact>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configurationUser, ( reader, list ) => new List<User>() );

			mappingCache.GetCachedMapping<Contact,List<Contact>>( dataReader.Object, configurationContact );
		}

		[Test]
		public void ContainsMapping_WhenNoMappingsHaveBeenCachedForObject_ReturnsFalse()
		{
			var dataReader = GetMockDataReader( 0 );
			var configuration = new ObjectHydratorConfiguration<User>();

			var mappingCache = new MappingCache();
			var result = mappingCache.ContainsMapping<User,User>( dataReader.Object, configuration );

			Assert.IsFalse( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedForObjectAndMatchesCurrentMapping_ReturnsTrue()
		{
			var dataReader = GetMockDataReader( 1 );
			var configuration = new ObjectHydratorConfiguration<User>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configuration, ( x, y ) => new User() );

			var result = mappingCache.ContainsMapping<User,User>( dataReader.Object, configuration );


			Assert.IsTrue( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedForObject_ReturnsFunc()
		{
			var dataReader = GetMockDataReader( 7 );
			var configuration = new ObjectHydratorConfiguration<User>();
			Func<IDataReader, List<LambdaExpression>, User> mapping = ( x, y ) => new User();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configuration, mapping );

			var result = mappingCache.GetCachedMapping<User,User>( dataReader.Object, configuration );


			Assert.AreEqual( mapping, result );
		}

		private static Mock<IDataReader> GetMockDataReader( int fieldCount )
		{
			var result = new Mock<IDataReader>();
			result.SetupGet( x => x.FieldCount ).Returns( fieldCount );

			for ( int i = 0; i < fieldCount; i++ )
			{
				result.Setup( x => x.GetName( i ) ).Returns( "Field" + 1 );
				result.Setup( x => x.GetFieldType( i ) ).Returns( typeof ( string ) );
			}

			return result;
		}
	}
}