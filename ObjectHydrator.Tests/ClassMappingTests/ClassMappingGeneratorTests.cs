using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using SqlObjectHydrator.ClassMapping;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.Tests.TestData;

namespace SqlObjectHydrator.Tests.ClassMappingTests
{
    [TestFixture]
    public class ClassMappingGeneratorTests
    {
        [Test]
        public void GenerateMap_WhenCalled_ReturnsAnInstance()
        {
            var dataReader = new Mock<IDataReader>();
            var configuration = new ObjectHydratorConfiguration<User>();
            var generator = new ClassMappingGenerator();

            Assert.IsInstanceOf<ClassMap>( generator.GenerateMap( dataReader.Object, configuration ) );
        }

        [Test]
        public void GenerateMap_WhenCalled_SetsCorrectClassType()
        {
            var dataReader = new Mock<IDataReader>();
            var configuration = new ObjectHydratorConfiguration<User>();
            var generator = new ClassMappingGenerator();

            var result = generator.GenerateMap( dataReader.Object, configuration );

            Assert.AreEqual( typeof ( User ), result.Type );
        }

        [Test]
        public void GenerateMap_WhenCalledWithNoCustomMappingsAndAllDataReaderFieldsMatch_ReturnsCorrectProperties()
        {
            var dataReader = new Mock<IDataReader>();
            dataReader.SetupGet( x => x.FieldCount ).Returns( 2 );
            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "Id" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( int ) );
            dataReader.Setup( x => x.GetName( 1 ) ).Returns( "FullName" );
            dataReader.Setup( x => x.GetFieldType( 1 ) ).Returns( typeof ( string ) );


            var configuration = new ObjectHydratorConfiguration<User>();
            var generator = new ClassMappingGenerator();

            var result = generator.GenerateMap( dataReader.Object, configuration );

            Assert.IsTrue( result.Propertys.Any( x => x.Name == "Id" && x.FieldId == 0 && x.Type == typeof ( int ) && !x.Nullable ) );
            Assert.IsTrue( result.Propertys.Any( x => x.Name == "FullName" && x.FieldId == 1 && x.Type == typeof ( string ) && !x.Nullable ) );
        }

        [Test]
        public void GenerateMap_WhenCalledWithNoCustomMappingsAndPropertyIsNullable_ReturnsCorrectProperty()
        {
            var dataReader = new Mock<IDataReader>();
            dataReader.SetupGet( x => x.FieldCount ).Returns( 1 );
            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "RefId" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( int ) );


            var configuration = new ObjectHydratorConfiguration<User>();
            var generator = new ClassMappingGenerator();

            var result = generator.GenerateMap( dataReader.Object, configuration );

            Assert.IsTrue( result.Propertys.Any( x => x.Name == "RefId" && x.FieldId == 0 && x.Type == typeof ( int ) && x.Nullable ) );
        }

        [Test]
        public void GenerateMap_WhenCalledWithNoCustomMappingsAndNoPropertyExistsForField_ReturnsCorrectProperty()
        {
            var dataReader = new Mock<IDataReader>();
            dataReader.SetupGet( x => x.FieldCount ).Returns( 1 );
            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "RefId2" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( int ) );


            var configuration = new ObjectHydratorConfiguration<User>();
            var generator = new ClassMappingGenerator();

            var result = generator.GenerateMap( dataReader.Object, configuration );

            Assert.IsTrue( !result.Propertys.Any( x => x.Name == "RefId2" && x.FieldId == 0 && x.Type == typeof ( int ) && x.Nullable ) );
        }

        [Test]
        public void GenerateMap_WhenCalledWithCustomMapForRootProperty_ReturnsCorrectProperty()
        {
            var dataReader = new Mock<IDataReader>();


            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.FullName, x => (string)x[ "FullName" ] );
            var generator = new ClassMappingGenerator();

            var result = generator.GenerateMap( dataReader.Object, configuration );

            Assert.IsTrue( result.Propertys.Any( x => x.Name == "FullName" && x.ConfigurationMapId == 0 && x.FieldId == null && x.Type == typeof ( string ) ) );
        }

        [Test]
        public void GenerateMap_WhenCalledWithCustomMapForRootPropertyAndAlreadyExistsFromDateReader_ReturnsCorrectProperty()
        {
            var dataReader = new Mock<IDataReader>();
            dataReader.SetupGet( x => x.FieldCount ).Returns( 1 );
            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "FullName" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( string ) );

            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.FullName, x => (string)x[ "FullName" ] );
            var generator = new ClassMappingGenerator();

            var result = generator.GenerateMap( dataReader.Object, configuration );

            Assert.IsTrue( result.Propertys.Count( x => x.Name == "FullName" && x.Type == typeof ( string ) ) == 1 );
        }

        [Test]
        public void GenerateMap_WhenCalledWithCustomMapForSubProperty_ReturnsCorrectProperty()
        {
            var dataReader = new Mock<IDataReader>();


            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.ContactInfo.PhoneNumber, x => (string)x[ "Number" ] );
            var generator = new ClassMappingGenerator();

            var result = generator.GenerateMap( dataReader.Object, configuration );

            Assert.IsTrue( ( result.Propertys.Single( x => x.Name == "ContactInfo" ) as ClassMap ).Propertys.Any( x => x.Name == "PhoneNumber" && x.ConfigurationMapId == 0 && x.FieldId == null && x.Type == typeof ( string ) ) );
        }

        [Test]
        public void GenerateMap_WhenCalledWithCustomMapForMultipleSubPropertys_ReturnsCorrectPropertys()
        {
            var dataReader = new Mock<IDataReader>();


            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.ContactInfo.PhoneNumber, x => (string)x[ "Number" ] )
                .Mapping( x => x.ContactInfo.Postcode, x => (string)x[ "Postcode" ] );
            var generator = new ClassMappingGenerator();

            var result = generator.GenerateMap( dataReader.Object, configuration );

            Assert.IsTrue( ( result.Propertys.Single( x => x.Name == "ContactInfo" ) as ClassMap ).Propertys.Any( x => x.Name == "PhoneNumber" && x.ConfigurationMapId == 0 && x.FieldId == null && x.Type == typeof ( string ) ) );
            Assert.IsTrue( ( result.Propertys.Single( x => x.Name == "ContactInfo" ) as ClassMap ).Propertys.Any( x => x.Name == "Postcode" && x.ConfigurationMapId == 1 && x.FieldId == null && x.Type == typeof ( string ) ) );
        }

        [Test]
        public void GenerateMap_WhenCalledWithCustomMapForSubPropertyAndAlreadyExistsFromDataReader_ReturnsCorrectClassAndPropertyMap()
        {
            var dataReader = new Mock<IDataReader>();
            dataReader.SetupGet( x => x.FieldCount ).Returns( 1 );
            dataReader.Setup( x => x.GetName( 0 ) ).Returns( "ContactInfo" );
            dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( string ) );

            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.ContactInfo.PhoneNumber, x => (string)x[ "FullName" ] );
            var generator = new ClassMappingGenerator();

            var result = generator.GenerateMap( dataReader.Object, configuration );

            Assert.IsTrue( result.Propertys.Count( x => x.Name == "ContactInfo" ) == 1 );
        }
    }
}