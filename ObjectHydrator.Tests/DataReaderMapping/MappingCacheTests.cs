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
		[Test]
		public void ContainsMapping_WhenNoMappingsHaveBeenCached_ReturnsFalse()
		{
			var dataReader = GetMockDataReader( 0 );
			var configuraton = new ObjectHydratorConfiguration<User>();

			var mappingCache = new MappingCache();
			var result = mappingCache.ContainsMapping( dataReader.Object, configuraton );

			Assert.IsFalse( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAndMatchesCurrentMapping_ReturnsTrue()
		{
			var dataReader = GetMockDataReader( 1 );
			var configuraton = new ObjectHydratorConfiguration<User>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configuraton, ( x, y ) => new List<User>() );

			var result = mappingCache.ContainsMapping( dataReader.Object, configuraton );


			Assert.IsTrue( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAndDataReaderMatchesButNotConfiguration_ReturnsTrue()
		{
			var dataReader = GetMockDataReader( 2 );
			var configuraton1 = new ObjectHydratorConfiguration<User>();
			var configuraton2 = new ObjectHydratorConfiguration<User>()
				.Mapping( x => x.FullName, x => x[ "FirstName" ].ToString() );
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configuraton1, ( x, y ) => new List<User>() );

			var result = mappingCache.ContainsMapping( dataReader.Object, configuraton2 );


			Assert.IsFalse( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAndDataReaderNotSameButConfigurationIs_ReturnsTrue()
		{
			var dataReader1 = GetMockDataReader( 3 );
			var dataReader2 = GetMockDataReader( 4 );
			var configuraton = new ObjectHydratorConfiguration<User>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader1.Object, configuraton, ( x, y ) => new List<User>() );

			var result = mappingCache.ContainsMapping( dataReader2.Object, configuraton );


			Assert.IsFalse( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAreSameButDateReaderAndConfigurationDifferentInstance_ReturnsTrue()
		{
			var dataReader1 = GetMockDataReader( 5 );
			var dataReader2 = GetMockDataReader( 5 );
			var configuraton1 = new ObjectHydratorConfiguration<User>();
			var configuraton2 = new ObjectHydratorConfiguration<User>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader1.Object, configuraton1, ( x, y ) => new List<User>() );

			var result = mappingCache.ContainsMapping( dataReader2.Object, configuraton2 );


			Assert.IsTrue( result );
		}

		[Test]
		public void GetCachedMapping_WhenMappingDosentExist_ReturnsNull()
		{
			var dataReader = GetMockDataReader( 6 );
			var configuraton = new ObjectHydratorConfiguration<User>();
			var mappingCache = new MappingCache();

			var result = mappingCache.GetCachedMapping( dataReader.Object, configuraton );

			Assert.IsNull( result );
		}

		[Test]
		public void ContainsMapping_WhenMappingsHaveBeenCachedAnd_ReturnsFunc()
		{
			var dataReader = GetMockDataReader( 7 );
			var configuraton = new ObjectHydratorConfiguration<User>();
			Func<IDataReader, List<LambdaExpression>, List<User>> mapping = ( x, y ) => new List<User>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configuraton, mapping );

			var result = mappingCache.GetCachedMapping( dataReader.Object, configuraton );


			Assert.AreEqual( mapping, result );
		}

		[Test]
		public void GetCachedMapping_WhenMappingHasBeenSavedWithSameDataReaderAndConfiguration_DoesNotThrowExeception()
		{
			MappingCache.InternalMappingCache = new Dictionary<int, object>();

			var dataReader = new Mock<IDataReader>();
			var configurationUser = new ObjectHydratorConfiguration<User>();
			var configurationContact = new ObjectHydratorConfiguration<Contact>();
			var mappingCache = new MappingCache();

			mappingCache.StoreMapping( dataReader.Object, configurationUser, ( reader, list ) => new List<User>() );

			mappingCache.GetCachedMapping( dataReader.Object, configurationContact );
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