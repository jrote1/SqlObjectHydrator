using System.Collections.Generic;
using System.Data;
using Moq;
using NUnit.Framework;
using SqlObjectHydrator.Tests.TestData;

namespace SqlObjectHydrator.Tests
{
    [TestFixture]
    public class ObjectHydratorTests
    {
        [Test]
        public void DataReaderToList_WhenCalled_ReturnsInstance()
        {
            var dataReader = new Mock<IDataReader>();

            var objectHydrator = new ObjectHydrator();

            var result = objectHydrator.DataReaderToList<User>( dataReader.Object );

            Assert.IsInstanceOf<List<User>>( result );
        }

        //[Test]
        //public void DataReaderToList_WhenCalledWithInvalidConfiguration_ThrowsException()
        //{
        //    var dataReader = new Mock<IDataReader>();
        //    dataReader.SetupGet( x => x[ 0 ] ).Returns( "Hello World" );

        //    var configuration = new ObjectHydratorConfiguration<User>()
        //        .Mapping( x => x.Id, x => x[ 0 ] );

        //    var objectHydrator = new ObjectHydrator();

        //    Assert.Throws<Exception>( () => objectHydrator.DataReaderToList( dataReader.Object, configuration ) );
        //}


    }
}