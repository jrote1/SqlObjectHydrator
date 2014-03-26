using System;
using System.Collections.Generic;
using System.Data;

namespace SqlObjectHydrator
{
	internal class TempDataReader : IDataReader
	{
		private readonly List<List<Dictionary<int, object>>> _data;
		private readonly int _hashcode;

		private int _currentTableId;
		private int _currentRowId = -1;

		public TempDataReader( List<List<Dictionary<int, object>>> data, int hashcode )
		{
			_data = data;
			_hashcode = hashcode;
		}

		public void Dispose() {}

		public string GetName( int i )
		{
			throw new NotImplementedException();
		}

		public string GetDataTypeName( int i )
		{
			throw new NotImplementedException();
		}

		public Type GetFieldType( int i )
		{
			return _data[ 0 ][ 0 ].GetType();
		}

		public object GetValue( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return _data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
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
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (bool)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public byte GetByte( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (byte)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length )
		{
			throw new NotImplementedException();
		}

		public char GetChar( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (char)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length )
		{
			throw new NotImplementedException();
		}

		public Guid GetGuid( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (Guid)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public short GetInt16( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (Int16)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public int GetInt32( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (Int32)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public long GetInt64( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (Int64)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public float GetFloat( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (float)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public double GetDouble( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (double)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public string GetString( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (string)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public decimal GetDecimal( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (decimal)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public DateTime GetDateTime( int i )
		{
			if ( _currentRowId > -1 && _currentTableId < _data.Count )
				return (DateTime)_data[ _currentTableId ][ _currentRowId ][ i ];
			throw new IndexOutOfRangeException();
		}

		public IDataReader GetData( int i )
		{
			throw new NotImplementedException();
		}

		public bool IsDBNull( int i )
		{
			throw new NotImplementedException();
		}

		public int FieldCount
		{
			get { return _data[ _currentTableId ][ 0 ].Count; }
		}

		object IDataRecord.this[ int i ]
		{
			get { throw new NotImplementedException(); }
		}

		object IDataRecord.this[ string name ]
		{
			get { throw new NotImplementedException(); }
		}

		public void Close()
		{
			throw new NotImplementedException();
		}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public bool NextResult()
		{
			_currentTableId++;
			_currentRowId = -1;
			return ( _currentTableId < _data.Count );
		}

		public bool Read()
		{
			_currentRowId++;
			return ( _currentRowId < _data[ _currentTableId ].Count );
		}

		public int Depth { get; private set; }
		public bool IsClosed { get; private set; }
		public int RecordsAffected { get; private set; }

		public override int GetHashCode()
		{
			return _hashcode;
		}
	}
}