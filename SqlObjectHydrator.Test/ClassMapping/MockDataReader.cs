using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlObjectHydrator.Test.ClassMapping
{
	public class MockDataReader : IDataReader
	{
		private readonly List<List<List<object>>> _tablesValues;
		private readonly List<List<string>> _tableColumnNames;

		public MockDataReader()
		{
			_tablesValues = new List<List<List<object>>>();
			_tableColumnNames = new List<List<string>>();
		}

		public int AddTable( params string[] columnNames )
		{
			_tableColumnNames.Add( columnNames.ToList() );
			_tablesValues.Add( new List<List<object>>() );
			if ( _currentTable == -1 )
				_currentTable = 0;
			return _tableColumnNames.Count - 1;
		}

		public void AddRow( int tableId, params object[] values )
		{
			_tablesValues[ tableId ].Add( values.ToList() );
		}

		#region DataReaderMethods

		private int _currentTable = -1;
		private int _currentRow = -1;

		public void Dispose() {}

		public string GetName( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			return _tableColumnNames[ _currentTable ][ i ];
		}

		public string GetDataTypeName( int i )
		{
			if ( _currentTable == -1 )
				if ( _tablesValues[ _currentTable ].Count == 0 )
					return typeof( object ).FullName;
			return _tablesValues[ _currentTable ][ 0 ].GetType().FullName;
		}

		public Type GetFieldType( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _tablesValues[ _currentTable ].Count == 0 )
				return typeof( object );
			return _tablesValues[ _currentTable ][ 0 ][ i ].GetType();
		}

		public object GetValue( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return _tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public int GetValues( object[] values )
		{
			throw new NotImplementedException();
		}

		public int GetOrdinal( string name )
		{
			throw new NotImplementedException();
		}

		public bool GetBoolean( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (bool)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public byte GetByte( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (byte)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length )
		{
			throw new NotImplementedException();
		}

		public char GetChar( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (char)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length )
		{
			throw new NotImplementedException();
		}

		public Guid GetGuid( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (Guid)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public short GetInt16( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (Int16)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public int GetInt32( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (Int32)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public long GetInt64( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (Int64)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public float GetFloat( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (float)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public double GetDouble( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (Double)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public string GetString( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (String)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public decimal GetDecimal( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (Decimal)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public DateTime GetDateTime( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return (DateTime)_tablesValues[ _currentTable ][ _currentRow ][ i ];
		}

		public IDataReader GetData( int i )
		{
			throw new NotImplementedException();
		}

		public bool IsDBNull( int i )
		{
			if ( _currentTable == -1 )
				throw new ArgumentOutOfRangeException( "i", "No Tables" );
			if ( _currentRow == -1 )
				throw new ArgumentOutOfRangeException( "i", "Call Read First" );
			return _tablesValues[ _currentTable ][ _currentRow ][ i ] == null;
		}

		public int FieldCount
		{
			get
			{
				if ( _currentTable == -1 )
					throw new ArgumentOutOfRangeException( "", "No Tables" );
				return _tableColumnNames[ _currentTable ].Count;
			}
		}

		object IDataRecord.this[ int i ]
		{
			get { throw new NotImplementedException(); }
		}

		object IDataRecord.this[ string name ]
		{
			get { throw new NotImplementedException(); }
		}

		public void Close() {}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public bool NextResult()
		{
			if ( ( _currentTable + 1 ) < _tableColumnNames.Count )
			{
				_currentTable++;
				_currentRow = -1;
				return true;
			}
			return false;
		}

		public bool Read()
		{
			if ( ( _currentRow + 1 ) < _tablesValues[ _currentTable ].Count )
			{
				_currentRow++;
				return true;
			}
			return false;
		}

		public int Depth { get; private set; }
		public bool IsClosed { get; private set; }
		public int RecordsAffected { get; private set; }

		#endregion
	}
}