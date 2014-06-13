using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
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
				Mappings = new Mapping
				{
					TableMaps = new Dictionary<int, Type>
					{
						{ 0, typeof( TestClass ) }
					}
				}
			};
			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable();
			mockDataReader.AddRow( table );

			var result = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			result( mockDataReader, new Dictionary<MappingEnum, object>(), new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>() );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithSingleTable_SetsPropertiesCorrectly()
		{
			var classMapResult = new ClassMapResult
			{
				Mappings = new Mapping
				{
					TableMaps = new Dictionary<int, Type>
					{
						{ 0, typeof( TestClass ) }
					}
				}
			};

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Name1", "Name 2", "Length1" );
			mockDataReader.AddRow( table, "Name 1", DBNull.Value, 1 );

			var func = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>(), new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>() );

			Assert.AreEqual( "Name 1", result.Name1 );
			Assert.AreEqual( null, result.Name2 );
			Assert.AreEqual( 1, result.Length1 );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithSingleTableAndPropertyIsNullableAndIsNull_SetsPropertiesCorrectly()
		{
			var classMapResult = new ClassMapResult
			{
				Mappings = new Mapping
				{
					TableMaps = new Dictionary<int, Type>
					{
						{ 0, typeof( TestClass ) }
					}
				}
			};

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2" );
			mockDataReader.AddRow( table, DBNull.Value );

			var func = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>(), new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>() );

			Assert.AreEqual( null, result.Length2 );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithSingleTableAndPropertyIsNullableAndIsNotNull_SetsPropertiesCorrectly()
		{
			var classMapResult = new ClassMapResult
			{
				Mappings = new Mapping
				{
					TableMaps = new Dictionary<int, Type>
					{
						{ 0, typeof( TestClass ) }
					}
				}
			};

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2" );
			mockDataReader.AddRow( table, 1 );

			var func = MappingGenerator.Generate<TestClass>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>(), new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>() );

			Assert.AreEqual( 1, result.Length2 );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithMultipeTablesUsingTableJoinIsJoinedCorrectly_SetsPropertiesCorrectly()
		{
			IMapping mapping = new Mapping();
			Func<TestClass, TestContent, bool> canJoin = ( x, y ) => x.TestContentId == y.Id;
			Action<TestClass, List<TestContent>> listSet = ( x, y ) => x.TestContents = y;

			mapping.Table<TestClass>( 0 );
			mapping.Table<TestContent>( 1 );

			mapping.TableJoin( canJoin, listSet );

			var classMapResult = new ClassMapResult
			{
				Mappings = (Mapping)mapping
			};

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2", "TestContentId" );
			var contentTable = mockDataReader.AddTable( "Id", "Name" );

			mockDataReader.AddRow( table, 1, 1 );

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
			}, new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>() );

			Assert.AreEqual( 1, result.TestContents.Count );
			Assert.AreEqual( "Name1", result.TestContents[ 0 ].Name );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithMultipeTablesUsingJoinIsJoinedCorrectly_SetsPropertiesCorrectly()
		{
			IMapping mapping = new Mapping();
			Action<TestClass, List<TestContent>> listSet = ( x, y ) => x.TestContents = y;
			mapping.Join( listSet );

			mapping.Table<TestClass>( 0 );
			mapping.Table<TestContent>( 1 );

			var classMapResult = new ClassMapResult
			{
				Mappings = (Mapping)mapping
			};

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2" );
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
			}, new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>() );

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

			mapping.Table<TestClass>( 0 );

			var classMapResult = new ClassMapResult
			{
				Mappings = (Mapping)mapping
			};

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Length2", "TestContentId" );
			var contentTable = mockDataReader.AddTable( "Id", "Name" );

			mockDataReader.AddRow( table, 1, 1 );

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
			}, new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>() );

			Assert.AreEqual( 2, result.TestContentDictionary.Count );
			Assert.AreEqual( "Name1", result.TestContentDictionary[ 1 ] );
			Assert.AreEqual( "Name2", result.TestContentDictionary[ 2 ] );
		}

		[Test]
		public void Generate_WhenInputTypeIsList_ReturnsListOfData()
		{
			var classMapResult = new ClassMapResult
			{
				Mappings = new Mapping
				{
					TableMaps = new Dictionary<int, Type>
					{
						{ 0, typeof( TestClass ) }
					}
				}
			};

			var mockDataReader = new MockDataReader();
			var table = mockDataReader.AddTable( "Name1", "Name 2", "Length1" );
			mockDataReader.AddRow( table, "Name 1", DBNull.Value, 1 );


			var func = MappingGenerator.Generate<List<TestClass>>( new MockDataReader(), classMapResult );

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>(), new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>() );

			Assert.AreEqual( "Name 1", result[ 0 ].Name1 );
			Assert.AreEqual( null, result[ 0 ].Name2 );
			Assert.AreEqual( 1, result[ 0 ].Length1 );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithVariableTableType_UsesCorrectType()
		{
			IMapping mapping = new Mapping();

			Func<Result, BaseScore, bool> canJoin = ( r, b ) => r.Id == b.ResultId;
			Action<Result, List<BaseScore>> listSet = ( r, b ) => r.Scores = b;
			Func<IDataRecord, Type> variableTableType = dataRecord =>
			{
				switch ( dataRecord.GetInt32( 4 ) )
				{
					case 1:
						return typeof( IntScore );
					default:
						return typeof( StringScore );
				}
			};

			mapping.TableJoin<Result, BaseScore>( canJoin, listSet );
			mapping.VariableTableType<BaseScore>( variableTableType );

			mapping.Table<Result>( 0 );
			mapping.Table<BaseScore>( 1 );

			var classMapResult = new ClassMapResult
			{
				Mappings = (Mapping)mapping
			};

			var mockDataReader = new MockDataReader();

			var results = mockDataReader.AddTable( "Id", "Name" );
			var scores = mockDataReader.AddTable( "Id", "ResultId", "Value", "ValueString", "ResultType" );

			mockDataReader.AddRow( results, 1, "Result 1" );
			mockDataReader.AddRow( results, 2, "Result 2" );

			mockDataReader.AddRow( scores, 1, 1, 10, "", 1 );
			mockDataReader.AddRow( scores, 1, 2, DBNull.Value, "10", 2 );

			var func = MappingGenerator.Generate<List<Result>>( new MockDataReader(), classMapResult );

			var dictionary = new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>();

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>
			{
				{
					MappingEnum.TableJoin, new List<KeyValuePair<object, object>>
					{
						new KeyValuePair<object, object>( canJoin, listSet )
					}
				},
				{
					MappingEnum.VariableTableType, new Dictionary<Type, object>
					{
						{ typeof( BaseScore ), variableTableType }
					}
				}
			}, dictionary );

			Assert.AreEqual( 1, result[ 0 ].Id );
			Assert.AreEqual( 2, result[ 1 ].Id );

			Assert.AreEqual( 1, result[ 0 ].Scores.Count );
			Assert.AreEqual( 1, result[ 1 ].Scores.Count );

			Assert.IsInstanceOf<IntScore>( result[ 0 ].Scores[ 0 ] );
			Assert.IsInstanceOf<StringScore>( result[ 1 ].Scores[ 0 ] );

			Assert.AreEqual( 10, ( result[ 0 ].Scores[ 0 ] as IntScore ).Value );
			Assert.AreEqual( "10", ( result[ 1 ].Scores[ 0 ] as StringScore ).ValueString );
		}

		[Test]
		public void GenerateToObject_WhenCalledWithVariableTableTypeAndPropertyMap_UsesPropertyMap()
		{
			IMapping mapping = new Mapping();

			Func<IDataRecord, Type> variableTableType = dataRecord => typeof( StringScore );

			mapping.VariableTableType<BaseScore>( variableTableType );
			Func<IDataRecord, string> action = dataRecord => "The Score Is: " + dataRecord.GetString( 2 );
			mapping.PropertyMap<StringScore, String>( x => x.ValueString, action );

			mapping.Table<BaseScore>( 0 );

			var classMapResult = new ClassMapResult
			{
				Mappings = (Mapping)mapping
			};

			var mockDataReader = new MockDataReader();

			var scores = mockDataReader.AddTable( "Id", "ResultId", "ValueString", "ResultType" );

			mockDataReader.AddRow( scores, 1, 1, "10", 2 );

			var func = MappingGenerator.Generate<List<BaseScore>>( new MockDataReader(), classMapResult );

			var dictionary = new Dictionary<Type, Func<IDataRecord, Dictionary<MappingEnum, object>, object>>();

			var result = func( mockDataReader, new Dictionary<MappingEnum, object>
			{
				{
					MappingEnum.VariableTableType, new Dictionary<Type, object>
					{
						{ typeof( BaseScore ), variableTableType }
					}
				},
				{
					MappingEnum.PropertyMap, new Dictionary<PropertyInfo, object>
					{
						{
							typeof( StringScore ).GetProperty( "ValueString" ), action
						}
					}
				}
			}, dictionary );

			Assert.AreEqual( 1, result[ 0 ].Id );

			Assert.IsInstanceOf<StringScore>( result[ 0 ] );

			Assert.AreEqual( "The Score Is: 10", ( result[ 0 ] as StringScore ).ValueString );
		}
	}

	public class Result
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<BaseScore> Scores { get; set; }
	}

	public class BaseScore
	{
		public int Id { get; set; }
		public int ResultId { get; set; }
		public int ResultType { get; set; }
	}

	public class StringScore : BaseScore
	{
		public string ValueString { get; set; }
	}

	public class IntScore : BaseScore
	{
		public int Value { get; set; }
	}
}