using System;
using System.Collections.Generic;
using System.Dynamic;
using NUnit.Framework;
using SqlObjectHydrator.ClassMapping;
using SqlObjectHydrator.Configuration;
using SqlObjectHydrator.ILEmitting;
using SqlObjectHydrator.Test.ClassMapping;

namespace SqlObjectHydrator.Test.ILEmitting
{
	public class TestClass
	{
		public string Name1 { get; set; }
		public string Name2 { get; set; }
		public int Length1 { get; set; }
		public int? Length2 { get; set; }

		public int TestContentId { get; set; }
		public List<TestContent> TestContents { get; set; }
		public Dictionary<int, string> TestContentDictionary { get; set; }
	}

	public class TestContent
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	[TestFixture]
	public class MappingGeneratorTests
	{
		[Test]
		public void GenerateToObject_WhenCalled_DoesNotThrowExeception()
		{
			var classMapResult = new ClassMapResult
			{
				ClassMaps = new List<ClassMap>
				{
					new ClassMap( typeof( TestClass ), 0 )
				},
				Mappings = new Mapping()
			};
			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable();
			mockDataReader.AddRow( table );

			var result = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			result( mockDataReader, new Dictionary<MappingEnum, object>() );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithSingleTable_SetsPropertiesCorrectly()
		{
			var classMapResult = new ClassMapResult
			{
				ClassMaps = new List<ClassMap>
				{
					new ClassMap( typeof( TestClass ), 0 )
				},
				Mappings = new Mapping()
			};

			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "Name1" ), 0 ) );
			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "Name2" ), 1 ) );
			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "Length1" ), 2 ) );

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Name1", "Name 2", "Length1" );
			mockDataReader.AddRow( table, "Name 1", DBNull.Value, 1 );


			var func = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>() );

			Assert.AreEqual( "Name 1", result.Name1 );
			Assert.AreEqual( null, result.Name2 );
			Assert.AreEqual( 1, result.Length1 );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithSingleTableAndPropertyIsNullableAndIsNull_SetsPropertiesCorrectly()
		{
			var classMapResult = new ClassMapResult
			{
				ClassMaps = new List<ClassMap>
				{
					new ClassMap( typeof( TestClass ), 0 )
				},
				Mappings = new Mapping()
			};

			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "Length2" ), 0 )
			{
				Nullable = true
			} );

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2" );
			mockDataReader.AddRow( table, DBNull.Value );

			var func = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>() );

			Assert.AreEqual( null, result.Length2 );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithSingleTableAndPropertyIsNullableAndIsNotNull_SetsPropertiesCorrectly()
		{
			var classMapResult = new ClassMapResult
			{
				ClassMaps = new List<ClassMap>
				{
					new ClassMap( typeof( TestClass ), 0 )
				},
				Mappings = new Mapping()
			};

			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "Length2" ), 0 )
			{
				Nullable = true
			} );

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2" );
			mockDataReader.AddRow( table, 1 );

			var func = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>() );

			Assert.AreEqual( 1, result.Length2 );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithMultipeTablesUsingTableJoinIsJoinedCorrectly_SetsPropertiesCorrectly()
		{
			IMapping mapping = new Mapping();
			Func<TestClass, TestContent, bool> canJoin = ( x, y ) => x.TestContentId == y.Id;
			Action<TestClass, List<TestContent>> listSet = ( x, y ) => x.TestContents = y;
			mapping.TableJoin( canJoin, listSet );

			var classMapResult = new ClassMapResult
			{
				ClassMaps = new List<ClassMap>
				{
					new ClassMap( typeof( TestClass ), 0 ),
					new ClassMap( typeof( TestContent ), 1 )
				},
				Mappings = (Mapping)mapping
			};

			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "TestContentId" ), 0 )
			{
				Nullable = false
			} );
			classMapResult.ClassMaps[ 1 ].Properties.Add( new PropertyMap( typeof( TestContent ).GetProperty( "Id" ), 0 )
			{
				Nullable = false
			} );
			classMapResult.ClassMaps[ 1 ].Properties.Add( new PropertyMap( typeof( TestContent ).GetProperty( "Name" ), 1 )
			{
				Nullable = false
			} );

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2", "TestContentId" );
			var contentTable = mockDataReader.AddTable( "Id", "Name" );

			mockDataReader.AddRow( table, 1 );

			mockDataReader.AddRow( contentTable, 1, "Name1" );
			mockDataReader.AddRow( contentTable, 2, "Name2" );


			var func = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>
			{
				{
					MappingEnum.TableJoin, new List<KeyValuePair<object, object>>
					{
						new KeyValuePair<object, object>( canJoin, listSet )
					}
				}
			} );

			Assert.AreEqual( 1, result.TestContents.Count );
			Assert.AreEqual( "Name1", result.TestContents[ 0 ].Name );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithMultipeTablesUsingJoinIsJoinedCorrectly_SetsPropertiesCorrectly()
		{
			IMapping mapping = new Mapping();
			Action<TestClass, List<TestContent>> listSet = ( x, y ) => x.TestContents = y;
			mapping.Join( listSet );

			var classMapResult = new ClassMapResult
			{
				ClassMaps = new List<ClassMap>
				{
					new ClassMap( typeof( TestClass ), 0 ),
					new ClassMap( typeof( TestContent ), 1 )
				},
				Mappings = (Mapping)mapping
			};

			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "TestContentId" ), 0 )
			{
				Nullable = false
			} );
			classMapResult.ClassMaps[ 1 ].Properties.Add( new PropertyMap( typeof( TestContent ).GetProperty( "Id" ), 0 )
			{
				Nullable = false
			} );
			classMapResult.ClassMaps[ 1 ].Properties.Add( new PropertyMap( typeof( TestContent ).GetProperty( "Name" ), 1 )
			{
				Nullable = false
			} );

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2", "TestContentId" );
			var contentTable = mockDataReader.AddTable( "Id", "Name" );
			mockDataReader.AddRow( table, 1 );

			mockDataReader.AddRow( contentTable, 1, "Name1" );
			mockDataReader.AddRow( contentTable, 2, "Name2" );

			var func = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>
			{
				{
					MappingEnum.Join, new List<object>
					{
						listSet
					}
				}
			} );

			Assert.AreEqual( 2, result.TestContents.Count );
			Assert.AreEqual( "Name1", result.TestContents[ 0 ].Name );
			Assert.AreEqual( "Name2", result.TestContents[ 1 ].Name );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithMultipeTablesWithResultingDictionary_FillsDictionaryCorrectly()
		{
			IMapping mapping = new Mapping();
			Func<TestClass, dynamic, bool> condition = ( @class, o ) => true;
			Action<TestClass, Dictionary<int, string>> destination = ( @class, values ) => @class.TestContentDictionary = values;
			mapping.AddJoin( x =>
				                 x.DictionaryTableJoin<TestClass>()
				                  .Condition( condition )
				                  .KeyColumn( "Id" )
				                  .ValueColumn( "Name" )
				                  .SetDestinationProperty( destination )
				                  .ChildTable( 1 ) );

			var classMapResult = new ClassMapResult
			{
				ClassMaps = new List<ClassMap>
				{
					new ClassMap( typeof( TestClass ), 0 ),
					new ClassMap( typeof( ExpandoObject ), 1 )
				},
				Mappings = (Mapping)mapping
			};

			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "TestContentId" ), 0 )
			{
				Nullable = false
			} );
			classMapResult.ClassMaps[ 1 ].Properties.Add( new ExpandoPropertyMap( "Id", typeof( int ), 0 ) );
			classMapResult.ClassMaps[ 1 ].Properties.Add( new ExpandoPropertyMap( "Name", typeof( string ), 1 ) );

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2", "TestContentId" );
			var contentTable = mockDataReader.AddTable( "Id", "Name" );
			mockDataReader.AddRow( table, 1 );

			mockDataReader.AddRow( contentTable, 1, "Name1" );
			mockDataReader.AddRow( contentTable, 2, "Name2" );

			var func = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>
			{
				{
					MappingEnum.DictionaryJoin, new List<KeyValuePair<object, object>>
					{
						new KeyValuePair<object, object>( condition, destination )
					}
				}
			} );

			Assert.AreEqual( 2, result.TestContentDictionary.Count );
			Assert.AreEqual( "Name1", result.TestContentDictionary[ 1 ] );
			Assert.AreEqual( "Name2", result.TestContentDictionary[ 2 ] );
		}

		[Test]
		public void Generate_WhenInputTypeIsList_ReturnsListOfData()
		{
			var classMapResult = new ClassMapResult
			{
				ClassMaps = new List<ClassMap>
				{
					new ClassMap( typeof( TestClass ), 0 )
				},
				Mappings = new Mapping()
			};

			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "Name1" ), 0 ) );
			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "Name2" ), 1 ) );
			classMapResult.ClassMaps[ 0 ].Properties.Add( new PropertyMap( typeof( TestClass ).GetProperty( "Length1" ), 2 ) );

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Name1", "Name 2", "Length1" );
			mockDataReader.AddRow( table, "Name 1", DBNull.Value, 1 );


			var func = MappingGenerator.Generate<List<TestClass>>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>() );

			Assert.AreEqual( "Name 1", result[0].Name1 );
			Assert.AreEqual( null, result[0].Name2 );
			Assert.AreEqual( 1, result[0].Length1 );
		}
	}
}