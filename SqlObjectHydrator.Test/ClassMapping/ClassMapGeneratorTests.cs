using System;
using System.Data;
using System.Dynamic;
using System.Linq;
using NUnit.Framework;
using SqlObjectHydrator.ClassMapping;
using SqlObjectHydrator.Configuration;

namespace SqlObjectHydrator.Test.ClassMapping
{
	[TestFixture]
	public class ClassMapGeneratorTests
	{
		[Test]
		public void Generate_WhenCalled_ReturnsMethodWithCorrectType()
		{
			var dataReader = new MockDataReader();
			dataReader.AddTable();

			var classMap = ClassMapGenerator.Generate<User>( dataReader );

			Assert.AreEqual( typeof( User ), classMap.ClassMaps[ 0 ].Type );
		}

		[Test]
		public void Generate_WhenNoPropertyMapsAndPropertiesOnDataReader_ReturnsMethodWithCorrectProperties()
		{
			var dataReader = new MockDataReader();
			var userTable = dataReader.AddTable( "FirstName", "LastName" );
			dataReader.AddRow( userTable, "First Name", "Last Name" );

			var classMap = ClassMapGenerator.Generate<User>( dataReader );

			Assert.IsTrue( classMap.ClassMaps[ 0 ].Properties.Select( x => ( (PropertyMap)x ).PropertyInfo ).Contains( typeof( User ).GetProperty( "FirstName" ) ) );
			Assert.IsTrue( classMap.ClassMaps[ 0 ].Properties.Select( x => ( (PropertyMap)x ).PropertyInfo ).Contains( typeof( User ).GetProperty( "LastName" ) ) );
		}

		[Test]
		public void Generate_WhenPropertyTypesDontMatch_ReturnsMethodWithCorrectProperties()
		{
			var dataReader = new MockDataReader();
			var userTable = dataReader.AddTable( "FirstName", "LastName" );
			dataReader.AddRow( userTable, "First Name", 2 );

			var classMap = ClassMapGenerator.Generate<User>( dataReader );

			Assert.AreEqual( typeof( User ).GetProperty( "FirstName" ), ( (PropertyMap)classMap.ClassMaps[ 0 ].Properties[ 0 ] ).PropertyInfo );
			Assert.AreEqual( 1, classMap.ClassMaps[ 0 ].Properties.Count );
		}

		[Test]
		public void Generate_WhenPropertyIsNullableandTypesMatch_ReturnsMethodWithCorrectProperties()
		{
			var dataReader = new MockDataReader();
			var userTable = dataReader.AddTable( "Age" );
			dataReader.AddRow( userTable, 2 );

			var classMap = ClassMapGenerator.Generate<User>( dataReader );

			Assert.AreEqual( typeof( User ).GetProperty( "Age" ), ( (PropertyMap)classMap.ClassMaps[ 0 ].Properties[ 0 ] ).PropertyInfo );
			Assert.AreEqual( 1, classMap.ClassMaps[ 0 ].Properties.Count );
		}

		[Test]
		public void Generate_WhenPropertyDoesNotExist_ReturnsMethodWithCorrectProperties()
		{
			var dataReader = new MockDataReader();
			var userTable = dataReader.AddTable( "Ages" );
			dataReader.AddRow( userTable, 2 );

			var classMap = ClassMapGenerator.Generate<User>( dataReader );

			Assert.AreEqual( 0, classMap.ClassMaps[ 0 ].Properties.Count );
		}

		[Test]
		public void Generate_WhenExpandoObject_ReturnsMethodWithCorrectProperties()
		{
			var dataReader = new MockDataReader();
			var userTable = dataReader.AddTable( "First Name", "Age" );
			dataReader.AddRow( userTable, "", 1 );

			var classMap = ClassMapGenerator.Generate<ExpandoObject>( dataReader, typeof( Configuration ) );

			Assert.AreEqual( "FirstName", ( (ExpandoPropertyMap)classMap.ClassMaps[ 0 ].Properties[ 0 ] ).Name );
			Assert.AreEqual( typeof( string ), ( (ExpandoPropertyMap)classMap.ClassMaps[ 0 ].Properties[ 0 ] ).Type );
			Assert.AreEqual( "Age", ( (ExpandoPropertyMap)classMap.ClassMaps[ 0 ].Properties[ 1 ] ).Name );
			Assert.AreEqual( typeof( int ), ( (ExpandoPropertyMap)classMap.ClassMaps[ 0 ].Properties[ 1 ] ).Type );
		}
	}

	public class Configuration : IObjectHydratorConfiguration
	{
		public static readonly Func<IDataRecord,string> FirstNameMap = record => record.GetString( 0 );

		public void Mapping( IMapping mapping )
		{
			mapping.PropertyMap<User,string>( x=>x.FirstName,FirstNameMap );
		}
	}
}