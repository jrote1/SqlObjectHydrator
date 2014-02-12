using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.Tests.TestData;

namespace SqlObjectHydrator.Tests
{
    [TestFixture]
    public class MappingGeneratorTests
    {
        [Test]
        public void GenerateMapping_WhenCalled_FuncReturnsInstance()
        {
            var dataReader = new Mock<IDataReader>();
            var configuration = new ObjectHydratorConfiguration<User>();
            var mappingGenerator = new MappingGenerator();

            var list = mappingGenerator.GenerateMapping( dataReader.Object, configuration )( dataReader.Object, configuration.MappingsActions.Select( x => x.Value ).ToList() );
            Assert.IsInstanceOf<List<User>>( list );
        }

        [Test]
        public void GenerateMapping_WhenCalled_FuncReturnsCorrectListCount()
        {
            var dataReader = new Mock<IDataReader>();
            var calls = 0;
            dataReader.Setup( x => x.Read() ).Callback( () => calls++ ).Returns( () => calls <= 2 );
             

            var configuration = new ObjectHydratorConfiguration<User>();
            var mappingGenerator = new MappingGenerator();

            var list = mappingGenerator.GenerateMapping( dataReader.Object, configuration )( dataReader.Object, configuration.MappingsActions.Select( x => x.Value ).ToList() );
            Assert.AreEqual( 2, list.Count );
        }

        [Test]
        public void GenerateMapping_WhenAllValuesFromDataReader_FuncReturnsListWithRootValuesSet()
        {
            var dataReader = new Mock<IDataReader>();
            var calls = 0;
            dataReader.Setup( x => x.Read() ).Callback( () => calls++ ).Returns( () => calls <= 2 );
            dataReader.SetupGet( x => x.FieldCount ).Returns( 2 );

            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "Id" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( int ) );
            dataReader.Setup( x => x.GetInt32( 0 ) ).Returns( () => calls );

            dataReader.Setup( x => x.GetName( 1 ) ).Returns( "FullName" );
            dataReader.Setup( x => x.GetFieldType( 1 ) ).Returns( typeof ( string ) );
            dataReader.Setup( x => x.GetValue( 1 ) ).Returns( () => "FullName" + calls );

            var configuration = new ObjectHydratorConfiguration<User>();
            var mappingGenerator = new MappingGenerator();

            var list = mappingGenerator.GenerateMapping( dataReader.Object, configuration )( dataReader.Object, configuration.MappingsActions.Select( x => x.Value ).ToList() );

            Assert.AreEqual( 1, list[ 0 ].Id );
            Assert.AreEqual( 2, list[ 1 ].Id );

            Assert.AreEqual( "FullName" + 1, list[ 0 ].FullName );
            Assert.AreEqual( "FullName" + 2, list[ 1 ].FullName );
        }

        [Test]
        public void GenerateMapping_WhenClassContainsAllTypesAndAllReturnedFromDataReader_FuncReturnsListWithAllValuesSet()
        {
            var dataReader = new Mock<IDataReader>();
            var calls = 0;
            dataReader.Setup( x => x.Read() ).Callback( () => calls++ ).Returns( () => calls <= 1 );
            dataReader.SetupGet( x => x.FieldCount ).Returns( 12 );

            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "ABoolean" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( Boolean ) );
            dataReader.Setup( x => x.GetBoolean( 0 ) ).Returns( true );

            dataReader.Setup( x => x.GetName( 1 ) ).Returns( "AByte" );
            dataReader.Setup( x => x.GetFieldType( 1 ) ).Returns( typeof ( Byte ) );
            dataReader.Setup( x => x.GetByte( 1 ) ).Returns( 2 );

            dataReader.Setup( x => x.GetName( 2 ) ).Returns( "AChar" );
            dataReader.Setup( x => x.GetFieldType( 2 ) ).Returns( typeof ( Char ) );
            dataReader.Setup( x => x.GetChar( 2 ) ).Returns( ' ' );

            dataReader.Setup( x => x.GetName( 3 ) ).Returns( "ADateTime" );
            dataReader.Setup( x => x.GetFieldType( 3 ) ).Returns( typeof ( DateTime ) );
            dataReader.Setup( x => x.GetDateTime( 3 ) ).Returns( new DateTime( 2012, 12, 12 ) );

            dataReader.Setup( x => x.GetName( 4 ) ).Returns( "ADecimal" );
            dataReader.Setup( x => x.GetFieldType( 4 ) ).Returns( typeof ( Decimal ) );
            dataReader.Setup( x => x.GetDecimal( 4 ) ).Returns( 3 );

            dataReader.Setup( x => x.GetName( 5 ) ).Returns( "ADouble" );
            dataReader.Setup( x => x.GetFieldType( 5 ) ).Returns( typeof ( Double ) );
            dataReader.Setup( x => x.GetDouble( 5 ) ).Returns( 4 );

            dataReader.Setup( x => x.GetName( 6 ) ).Returns( "ASingle" );
            dataReader.Setup( x => x.GetFieldType( 6 ) ).Returns( typeof ( Single ) );
            dataReader.Setup( x => x.GetFloat( 6 ) ).Returns( 5 );

            dataReader.Setup( x => x.GetName( 7 ) ).Returns( "AGuid" );
            dataReader.Setup( x => x.GetFieldType( 7 ) ).Returns( typeof ( Guid ) );
            dataReader.Setup( x => x.GetGuid( 7 ) ).Returns( new Guid( "f993fbdd-0b59-46c0-9be2-49e1f52eb52c" ) );

            dataReader.Setup( x => x.GetName( 8 ) ).Returns( "AInt16" );
            dataReader.Setup( x => x.GetFieldType( 8 ) ).Returns( typeof ( Int16 ) );
            dataReader.Setup( x => x.GetInt16( 8 ) ).Returns( 6 );

            dataReader.Setup( x => x.GetName( 9 ) ).Returns( "AInt32" );
            dataReader.Setup( x => x.GetFieldType( 9 ) ).Returns( typeof ( Int32 ) );
            dataReader.Setup( x => x.GetInt32( 9 ) ).Returns( 7 );

            dataReader.Setup( x => x.GetName( 10 ) ).Returns( "AInt64" );
            dataReader.Setup( x => x.GetFieldType( 10 ) ).Returns( typeof ( Int64 ) );
            dataReader.Setup( x => x.GetInt64( 10 ) ).Returns( 8 );

            dataReader.Setup( x => x.GetName( 11 ) ).Returns( "AString" );
            dataReader.Setup( x => x.GetFieldType( 11 ) ).Returns( typeof ( String ) );
            dataReader.Setup( x => x.GetValue( 11 ) ).Returns( "The String" );

            var configuration = new ObjectHydratorConfiguration<ClassWithAllTypes>();
            var mappingGenerator = new MappingGenerator();

            var list = mappingGenerator.GenerateMapping( dataReader.Object, configuration )( dataReader.Object, configuration.MappingsActions.Select( x => x.Value ).ToList() );

            Assert.AreEqual( true, list[ 0 ].ABoolean );
            Assert.AreEqual( 2, list[ 0 ].AByte );
            Assert.AreEqual( ' ', list[ 0 ].AChar );
            Assert.AreEqual( new DateTime( 2012, 12, 12 ), list[ 0 ].ADateTime );
            Assert.AreEqual( 3, list[ 0 ].ADecimal );
            Assert.AreEqual( 4, list[ 0 ].ADouble );
            Assert.AreEqual( 5, list[ 0 ].ASingle );
            Assert.AreEqual( new Guid( "f993fbdd-0b59-46c0-9be2-49e1f52eb52c" ), list[ 0 ].AGuid );
            Assert.AreEqual( 6, list[ 0 ].AInt16 );
            Assert.AreEqual( 7, list[ 0 ].AInt32 );
            Assert.AreEqual( 8, list[ 0 ].AInt64 );
            Assert.AreEqual( "The String", list[ 0 ].AString );
        }

        [Test]
        public void GenerateMapping_WhenPropertyHasMapping_FuncReturnsLisWithCorrectRootValuesSet()
        {
            var dataReader = new Mock<IDataReader>();
            var calls = 0;
            dataReader.Setup( x => x.Read() ).Callback( () => calls++ ).Returns( () => calls <= 2 );
            dataReader.SetupGet( x => x.FieldCount ).Returns( 2 );

            dataReader.SetupGet( x => x[ 0 ] ).Returns( () => calls );
            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "Id" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( int ) );
            dataReader.Setup( x => x.GetInt32( 0 ) ).Returns( () => calls );

            dataReader.SetupGet( x => x[ 1 ] ).Returns( "FullName" );
            dataReader.Setup( x => x.GetName( 1 ) ).Returns( "FullName" );
            dataReader.Setup( x => x.GetFieldType( 1 ) ).Returns( typeof ( string ) );
            dataReader.Setup( x => x.GetString( 1 ) ).Returns( () => "FullName" );

            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.FullName, x => (string)x[ 1 ] + (int)x[ 0 ] );
            var mappingGenerator = new MappingGenerator();

            var list = mappingGenerator.GenerateMapping( dataReader.Object, configuration )( dataReader.Object, configuration.MappingsActions.Select( x => x.Value ).ToList() );

            Assert.AreEqual( 1, list[ 0 ].Id );
            Assert.AreEqual( 2, list[ 1 ].Id );

            Assert.AreEqual( "FullName" + 1, list[ 0 ].FullName );
            Assert.AreEqual( "FullName" + 2, list[ 1 ].FullName );
        }

        [Test]
        public void GenerateMapping_WhenAllValuesFromDataReaderAndFieldIsNullable_FuncReturnsListWithRootValuesSet()
        {
            var dataReader = new Mock<IDataReader>();
            var calls = 0;
            dataReader.Setup( x => x.Read() ).Callback( () => calls++ ).Returns( () => calls <= 2 );
            dataReader.SetupGet( x => x.FieldCount ).Returns( 1 );

            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "RefId" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( int ) );
            dataReader.Setup( x => x.GetValue( 0 ) ).Returns( () => ( calls == 1 ) ? (object)DBNull.Value : 2 );
            dataReader.Setup( x => x.GetInt32( 0 ) ).Returns( 2 );

            var configuration = new ObjectHydratorConfiguration<User>();
            var mappingGenerator = new MappingGenerator();

            var list = mappingGenerator.GenerateMapping( dataReader.Object, configuration )( dataReader.Object, configuration.MappingsActions.Select( x => x.Value ).ToList() );

            Assert.AreEqual( null, list[ 0 ].RefId );
            Assert.AreEqual( 2, list[ 1 ].RefId.Value );
        }

        [Test]
        public void GenerateMapping_WhenMappingExistsAndIsSubType_FuncReturnsListWithValuesSet()
        {
            var dataReader = new Mock<IDataReader>();
            var calls = 0;
            dataReader.Setup( x => x.Read() ).Callback( () => calls++ ).Returns( () => calls <= 1 );
            dataReader.SetupGet( x => x.FieldCount ).Returns( 1 );

            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "Phonenumber" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( string ) );
            dataReader.Setup( x => x.GetString( 0 ) ).Returns( "Number" );

            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.ContactInfo.PhoneNumber, x => x.GetString( 0 ) );
            var mappingGenerator = new MappingGenerator();

            var mapping = mappingGenerator.GenerateMapping( dataReader.Object, configuration );
            var list = mapping( dataReader.Object, configuration.MappingsActions.Select( x => x.Value ).ToList() );

            Assert.AreEqual( "Number", list[ 0 ].ContactInfo.PhoneNumber );
        }

        [Test]
        public void GenerateMapping_WhenMappingExistsAndIsSubTypeOfSubType_FuncReturnsListWithValuesSet()
        {
            var dataReader = new Mock<IDataReader>();
            var calls = 0;
            dataReader.Setup( x => x.Read() ).Callback( () => calls++ ).Returns( () => calls <= 1 );
            dataReader.SetupGet( x => x.FieldCount ).Returns( 1 );

            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "Id" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( int ) );
            dataReader.Setup( x => x.GetInt32( 0 ) ).Returns( 2 );
            

            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.ContactInfo.ContactId.Id, x => x.GetInt32( 0 ) );
            var mappingGenerator = new MappingGenerator();

            var mapping = mappingGenerator.GenerateMapping( dataReader.Object, configuration );
            var list = mapping( dataReader.Object, configuration.MappingsActions.Select( x => x.Value ).ToList() );

            Assert.AreEqual( 2, list[ 0 ].ContactInfo.ContactId.Id );
        }

		[Test]
        public void GenerateMapping_WhenClassContainsAllTypesAndAllReturnedFromDataReader_FuncReturnsObjectWithAllValuesSet()
        {
            var dataReader = new Mock<IDataReader>();
            var calls = 0;
            dataReader.Setup( x => x.Read() ).Callback( () => calls++ ).Returns( () => calls <= 1 );
            dataReader.SetupGet( x => x.FieldCount ).Returns( 12 );

            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "ABoolean" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( Boolean ) );
            dataReader.Setup( x => x.GetBoolean( 0 ) ).Returns( true );

            dataReader.Setup( x => x.GetName( 1 ) ).Returns( "AByte" );
            dataReader.Setup( x => x.GetFieldType( 1 ) ).Returns( typeof ( Byte ) );
            dataReader.Setup( x => x.GetByte( 1 ) ).Returns( 2 );

            dataReader.Setup( x => x.GetName( 2 ) ).Returns( "AChar" );
            dataReader.Setup( x => x.GetFieldType( 2 ) ).Returns( typeof ( Char ) );
            dataReader.Setup( x => x.GetChar( 2 ) ).Returns( ' ' );

            dataReader.Setup( x => x.GetName( 3 ) ).Returns( "ADateTime" );
            dataReader.Setup( x => x.GetFieldType( 3 ) ).Returns( typeof ( DateTime ) );
            dataReader.Setup( x => x.GetDateTime( 3 ) ).Returns( new DateTime( 2012, 12, 12 ) );

            dataReader.Setup( x => x.GetName( 4 ) ).Returns( "ADecimal" );
            dataReader.Setup( x => x.GetFieldType( 4 ) ).Returns( typeof ( Decimal ) );
            dataReader.Setup( x => x.GetDecimal( 4 ) ).Returns( 3 );

            dataReader.Setup( x => x.GetName( 5 ) ).Returns( "ADouble" );
            dataReader.Setup( x => x.GetFieldType( 5 ) ).Returns( typeof ( Double ) );
            dataReader.Setup( x => x.GetDouble( 5 ) ).Returns( 4 );

            dataReader.Setup( x => x.GetName( 6 ) ).Returns( "ASingle" );
            dataReader.Setup( x => x.GetFieldType( 6 ) ).Returns( typeof ( Single ) );
            dataReader.Setup( x => x.GetFloat( 6 ) ).Returns( 5 );

            dataReader.Setup( x => x.GetName( 7 ) ).Returns( "AGuid" );
            dataReader.Setup( x => x.GetFieldType( 7 ) ).Returns( typeof ( Guid ) );
            dataReader.Setup( x => x.GetGuid( 7 ) ).Returns( new Guid( "f993fbdd-0b59-46c0-9be2-49e1f52eb52c" ) );

            dataReader.Setup( x => x.GetName( 8 ) ).Returns( "AInt16" );
            dataReader.Setup( x => x.GetFieldType( 8 ) ).Returns( typeof ( Int16 ) );
            dataReader.Setup( x => x.GetInt16( 8 ) ).Returns( 6 );

            dataReader.Setup( x => x.GetName( 9 ) ).Returns( "AInt32" );
            dataReader.Setup( x => x.GetFieldType( 9 ) ).Returns( typeof ( Int32 ) );
            dataReader.Setup( x => x.GetInt32( 9 ) ).Returns( 7 );

            dataReader.Setup( x => x.GetName( 10 ) ).Returns( "AInt64" );
            dataReader.Setup( x => x.GetFieldType( 10 ) ).Returns( typeof ( Int64 ) );
            dataReader.Setup( x => x.GetInt64( 10 ) ).Returns( 8 );

            dataReader.Setup( x => x.GetName( 11 ) ).Returns( "AString" );
            dataReader.Setup( x => x.GetFieldType( 11 ) ).Returns( typeof ( String ) );
            dataReader.Setup( x => x.GetValue( 11 ) ).Returns( "The String" );

            var configuration = new ObjectHydratorConfiguration<ClassWithAllTypes>();
            var mappingGenerator = new MappingGenerator();

            var list = mappingGenerator.GenerateSingleObjectMapping( dataReader.Object, configuration )( dataReader.Object, configuration.MappingsActions.Select( x => x.Value ).ToList() );

            Assert.AreEqual( true, list.ABoolean );
            Assert.AreEqual( 2, list.AByte );
            Assert.AreEqual( ' ', list.AChar );
            Assert.AreEqual( new DateTime( 2012, 12, 12 ), list.ADateTime );
            Assert.AreEqual( 3, list.ADecimal );
            Assert.AreEqual( 4, list.ADouble );
            Assert.AreEqual( 5, list.ASingle );
            Assert.AreEqual( new Guid( "f993fbdd-0b59-46c0-9be2-49e1f52eb52c" ), list.AGuid );
            Assert.AreEqual( 6, list.AInt16 );
            Assert.AreEqual( 7, list.AInt32 );
            Assert.AreEqual( 8, list.AInt64 );
            Assert.AreEqual( "The String", list.AString );
        }

        [Test]
        public void GenerateMapping_WhenStringReturnsDBNull_ReturnsNull()
        {
            var dataReader = new Mock<IDataReader>();
            var calls = 0;
            dataReader.Setup(x => x.Read()).Callback(() => calls++).Returns(() => calls <= 1);
            dataReader.SetupGet(x => x.FieldCount).Returns(2);

            dataReader.Setup(x => x.GetName(0)).Returns("FullName");
            dataReader.Setup(x => x.GetFieldType(0)).Returns(typeof(string));
            dataReader.Setup(x => x.GetValue(0)).Returns(DBNull.Value);

            var configuration = new ObjectHydratorConfiguration<User>();
            var mappingGenerator = new MappingGenerator();

            var list = mappingGenerator.GenerateMapping(dataReader.Object, configuration)(dataReader.Object, configuration.MappingsActions.Select(x => x.Value).ToList());

            Assert.AreEqual(null, list[0].FullName);
        }
    }
}