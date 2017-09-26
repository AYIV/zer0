using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using zer0.core.Contracts;

namespace zer0.storage.sqlite
{
	public sealed class SQLiteStorage : ConfigProviderBase, IStorage
	{
		public override string Provider => "SQLite";

		private const string DbFilePath = @"./zer0.sqlite";

		private readonly string _connectionString;
		
		public SQLiteStorage(IModule module) : base(module)
		{
			_connectionString = $"Data Source={DbFilePath};Version=3;";
		}

		protected override void SafeInit()
		{
			if (!File.Exists(DbFilePath))
			{
				SQLiteConnection.CreateFile(DbFilePath);
				SeedConfig();
			}
			
			TestConnection();
		}

		private void SeedConfig() => Run(c => c.Execute(@"
			CREATE TABLE IF NOT EXISTS [Config] (
				[Provider]	NVARCHAR(256)	NOT NULL,
				[Key]		TEXT			NOT NULL,
				[Value]		TEXT			NOT NULL,
				[Created]	TIMESTAMP					DEFAULT CURRENT_TIMESTAMP
			)"));

		public T One<T>(string query) => Run(c => c.QueryFirstOrDefault<T>(query));

		public IEnumerable<T> Many<T>(string query) => Run(c => c.Query<T>(query));

		public override T Key<T>(string key) => One<T>($"select [Value] from [Config] where [Key] = '{key}' and [Provider] = '{ModuleName}'");

		public override IEnumerable<T> Keys<T>() => Many<T>($"select [Value] from [Config] where [Provider] = '{ModuleName}'");

		private bool TestConnection() => Run(c => true);

		private T Run<T>(Func<IDbConnection, T> action = null)
		{
			if (action == null) return default(T);

			using (var connection = new SQLiteConnection(_connectionString))
			{
				connection.Open();

				return action.Invoke(connection);
			}
		}
    }
}
