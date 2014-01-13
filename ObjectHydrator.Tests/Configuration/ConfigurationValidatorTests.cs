using System;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using ObjectHydrator.Configuration;
using ObjectHydrator.Tests.TestData;

namespace ObjectHydrator.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationValidatorTests
    {
        private static Mock<IDataReader> _dataReader;
        private static ConfigurationValidator _configurationValidator;

        [SetUp]
        public void SetUp()
        {
            _configurationValidator = new ConfigurationValidator();
            _dataReader = new Mock<IDataReader>();
        }

        [Test]
        public void ValidateConfiguration_WhenCalled_ReturnsInstance()
        {
            var result = ArrangeAct();

            Assert.IsInstanceOf<ConfigurationValidatorResult>( result );
        }

        [Test]
        public void ValidateConfiguration_WhenCalledWithNoConfigurationMappings_ReturnValidConfigurationResult()
        {
            var result = ArrangeAct();

            Assert.IsTrue( result.ConfigurationValid );
        }

        [Test]
        public void ValidateConfiguration_WhenDoesNotContainDuplicatePropertyMappings_ReturnsValidConfigurationResult()
        {
            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.Id, x => 1 )
                .Mapping( x => x.FullName, x => "" );
            var result = ArrangeAct( configuration );

            Assert.IsTrue( result.ConfigurationValid );
        }

        [Test]
        public void ValidateConfiguration_WhenContainsDuplicatePropertyMappings_ReturnsInvalidConfigurationResult()
        {
            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.Id, x => 1 )
                .Mapping( x => x.Id, x => 1 )
                .Mapping( x => x.FullName, x => "" );
            var result = ArrangeAct( configuration );

            Assert.IsFalse( result.ConfigurationValid );
        }

        [Test]
        public void ValidateConfiguration_WhenContainsDuplicatePropertyMappingsButDifferentLinqVariableNamesUsed_ReturnsValidConfigurationResult()
        {
            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.Id, x => 1 )
                .Mapping( y => y.Id, x => 1 )
                .Mapping( x => x.FullName, x => "" );
            var result = ArrangeAct( configuration );

            Assert.IsFalse( result.ConfigurationValid );
        }

        [Test]
        public void ValidateConfiguration_WhenContainsDuplicatePropertyMappings_ReturnsCorrectErrorMessages()
        {
            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.Id, x => 1 )
                .Mapping( y => y.Id, x => 1 )
                .Mapping( x => x.FullName, x => "" )
                .Mapping( x => x.FullName, x => "" );
            var result = ArrangeAct( configuration );

            Assert.IsTrue( result.Errors.Any( x => x == String.Format( ConfigurationValidator.DuplicatePropertyStringFormat, typeof ( User ).FullName, "Id" ) ) );
            Assert.IsTrue( result.Errors.Any( x => x == String.Format( ConfigurationValidator.DuplicatePropertyStringFormat, typeof ( User ).FullName, "FullName" ) ) );
        }

        [Test]
        public void ValidateConfiguration_WhenInvalidValueGetter_ReturnsInvalidConfiguration()
        {
            var configuration = new ObjectHydratorConfiguration<User>()
                .Mapping( x => x.Id, x => (int) x[ "FirstName" ] );


            _dataReader.SetupGet( x => x.FieldCount ).Returns( 2 );
            _dataReader.Setup( x => x.GetName( 0 ) ).Returns( "Id" );
            _dataReader.Setup( x => x.GetFieldType( 0 ) ).Returns( typeof ( int ) );

            _dataReader.Setup( x => x.GetName( 1 ) ).Returns( "FirstName" );
            _dataReader.Setup( x => x.GetFieldType( 1 ) ).Returns( typeof ( string ) );

            var result = ArrangeAct( configuration );

            Assert.IsFalse( result.ConfigurationValid );
        }

        private static ConfigurationValidatorResult ArrangeAct( ObjectHydratorConfiguration<User> configuration = null )
        {
            var result = _configurationValidator.ValidateConfiguration( _dataReader.Object, configuration ?? new ObjectHydratorConfiguration<User>() );
            return result;
        }
    }
}