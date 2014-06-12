using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SqlObjectHydrator.Database.Test
{
	public class DatabaseCreator
	{
		private readonly string _databaseServerName;
		private readonly string _databaseFileFolder;
		public const string ConnectionString = "Data Source={0};Integrated Security=true;Initial Catalog={1}";
		private const string DeletionSql = @"IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}')
begin
alter database {0} set SINGLE_USER with rollback immediate
DROP DATABASE [{0}]
end
";

		public DatabaseCreator( string databaseServerName, string databaseFileFolder )
		{
			_databaseServerName = databaseServerName;
			_databaseFileFolder = databaseFileFolder;
		}

		public void CreateDatabase( string databaseName )
		{

			var sqlChunks = new List<SqlChunk>
			{
				new SqlChunk
				{
					Sql = String.Format( DeletionSql, databaseName ),
					DatabaseToExecuteOn = "master"
				},
				new SqlChunk
				{
					Sql = GetDatabaseCreationSql( databaseName ),
					DatabaseToExecuteOn = "master"
				},
				new SqlChunk
				{
					Sql = GetSqlScript( "CreateDatabaseTables" ),
					DatabaseToExecuteOn = databaseName
				},
				new SqlChunk
				{
					Sql = GetSqlScript( "InsertData" ),
					DatabaseToExecuteOn = databaseName
				}
			};

			foreach ( var sqlChunk in sqlChunks )
				ExecuteSql( sqlChunk.Sql, sqlChunk.DatabaseToExecuteOn );

		}

		private string GetDatabaseCreationSql( string databaseName )
		{
			return String.Format(
								 (string)GetSqlScript( "CreateDatabase" ),
								 databaseName,
								 _databaseFileFolder );
		}

		private static string GetSqlScript( string sqlChunk )
		{
			using ( var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream( "SqlObjectHydrator.Database.Test.DatabaseSchema." + sqlChunk + ".sql" ) )
			{
				using ( var reader = new StreamReader( stream ) )
				{
					return reader.ReadToEnd();
				}
			}
		}

		private void ExecuteSql( string sql, string databaseToExecuteOn )
		{
			using ( var connection = new SqlConnection( GetConnectionString( databaseToExecuteOn ) ) )
			{
				connection.Open();

				var sqlParts = sql.Split( new[]
				{
					"GO"
				}, StringSplitOptions.RemoveEmptyEntries );

				sqlParts = sqlParts.Select( x => x.Trim() ).Where( x => !String.IsNullOrEmpty( x ) ).ToArray();

				foreach ( var sqlPart in sqlParts )
					ExecuteCommand( connection, sqlPart.Trim() );

				connection.Close();
			}
		}

		private static void ExecuteCommand( SqlConnection connection, string sqlPart )
		{
			var command = connection.CreateCommand();
			command.CommandText = sqlPart;
			command.ExecuteNonQuery();
		}

		public string GetConnectionString( string databaseName )
		{
			return String.Format(
				ConnectionString,
				_databaseServerName,
				databaseName );
		}
	}
}