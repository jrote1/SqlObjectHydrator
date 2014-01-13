using System;
using System.Data;
using System.Linq.Expressions;
using NUnit.Framework;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.Tests.TestData;

namespace SqlObjectHydrator.Tests.Configuration
{
    [TestFixture]
    public class ObjectHydratorConfigurationTests
    {
        [Test]
        public void Mapping_WhenCalled_AddsMappingToMappingActions()
        {
            Expression<Func<User, string>> mappingColumnName = x => x.FullName;
            Expression<Func<IDataRecord, String>> mappingValue = x => String.Format( "{0} {1}", x[ "FirstName" ], x[ "LastName" ] );

            var hydratorConfiguration = new ObjectHydratorConfiguration<User>();

            hydratorConfiguration
                .Mapping( mappingColumnName, mappingValue );

            Assert.AreEqual( mappingColumnName, hydratorConfiguration.MappingsActions[ 0 ].Key );
            Assert.AreEqual( mappingValue, hydratorConfiguration.MappingsActions[ 0 ].Value );
        }

        [Test]
        public void GetHashCode_WhenContainDifferentMappings_AreNotEqual()
        {
            var configuration1 = new ObjectHydratorConfiguration<User>()
                .Mapping(x => x.Id, x => (int)x["Id"]);

            var configuration2 = new ObjectHydratorConfiguration<User>();

            Assert.AreNotEqual(configuration1, configuration2);
            Assert.IsFalse(configuration1 == configuration2);
            Assert.IsTrue(configuration1 != configuration2);
        }

        [Test]
        public void GetHashCode_WhenBothContainNoMappings_AreEqual()
        {
            var configuration1 = new ObjectHydratorConfiguration<User>();

            var configuration2 = new ObjectHydratorConfiguration<User>();

            Assert.AreEqual(configuration1, configuration2);
            Assert.IsFalse(configuration1 != configuration2);
            Assert.IsTrue(configuration1 == configuration2);
        }

        [Test]
        public void GetHashCode_WhenBothContainSameMapping_AreEqual()
        {
            var configuration1 = new ObjectHydratorConfiguration<User>()
                .Mapping(x => x.Id, x => (int)x["Id"]);

            var configuration2 = new ObjectHydratorConfiguration<User>()
                .Mapping(x => x.Id, x => (int)x["Id"]);

            Assert.AreEqual(configuration1, configuration2);
            Assert.IsFalse(configuration1 != configuration2);
            Assert.IsTrue(configuration1 == configuration2);
            ;
        }

        [Test]
        public void GetHashCode_WhenBothContainSameMappingButDifferentVariableNameUsed_AreEqual()
        {
            var configuration1 = new ObjectHydratorConfiguration<User>()
                .Mapping(x => x.Id, x => (int)x["Id"]);

            var configuration2 = new ObjectHydratorConfiguration<User>()
                .Mapping(x => x.Id, y => (int)y["Id"]);

            Assert.AreEqual(configuration1, configuration2);
            Assert.IsFalse(configuration1 != configuration2);
            Assert.IsTrue(configuration1 == configuration2);
        }
    }
}