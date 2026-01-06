using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DocumentTools.DataBaseFactory;

namespace DocumentTools
{
    internal class DataBaseFactory
    {
        public interface IDatabaseMethodFactory
        {
            DbConnection CreateConnection(string DataBaseFile);

            DbCommand CreateCommand(string QueryString, DbConnection Connection);

            (DbDataAdapter, DbConnection) CreateDataAdapter(string DataBaseFile, string QueryString);

            (DbDataReader, DbConnection) CreateDataReader(string DataBaseFile, string QueryString);

            (int, DbConnection) ExecuteDataBaseSql(string DataBaseFile, string ExecuteSqlString);

            (object, DbConnection) GetDataBaseDataByOne(string DataBaseFile, string QuerySqlString);

            void CreateCommandParameter(DbCommand TempCommand, string ParameterName,object ParameterValue);
        }
    }

    public class AccessDatabaseFactory: IDatabaseMethodFactory
    {
        public DbConnection CreateConnection(string DatabaseFile)
        {
            string ConnectionStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DatabaseFile};Persist Security Info=False;";

            return new OleDbConnection(ConnectionStr);
        }

        public DbCommand CreateCommand(string QueryString, DbConnection Connection)
        {
            return new OleDbCommand(QueryString, (OleDbConnection)Connection);
        }

        public (DbDataAdapter, DbConnection) CreateDataAdapter(string DataBaseFile,string QueryString)
        {
          DbConnection TempConnection = CreateConnection(DataBaseFile);

          TempConnection.Open();

          DbCommand TempCommand = CreateCommand(QueryString, TempConnection);

          DbDataAdapter TempDataAdapter = new OleDbDataAdapter((OleDbCommand)TempCommand);
          
          return (TempDataAdapter, TempConnection);
        }

        public (DbDataReader, DbConnection) CreateDataReader(string DataBaseFile, string QueryString)
        {
            DbConnection TempConnection = CreateConnection(DataBaseFile);

            TempConnection.Open();

            DbCommand TempCommand = CreateCommand(QueryString, TempConnection);

            DbDataReader TempDataReader = TempCommand.ExecuteReader();

            return (TempDataReader, TempConnection);
        }

        public (int, DbConnection) ExecuteDataBaseSql(string DataBaseFile, string ExecuteSqlString)
        {
            DbConnection TempConnection = CreateConnection(DataBaseFile);

            TempConnection.Open();

            DbCommand TempCommand = CreateCommand(ExecuteSqlString, TempConnection);

            int AffRows = TempCommand.ExecuteNonQuery();

            return (AffRows, TempConnection);
        }

        public (object, DbConnection) GetDataBaseDataByOne(string DataBaseFile, string QuerySqlString)
        {
            DbConnection TempConnection = CreateConnection(DataBaseFile);

            TempConnection.Open();

            DbCommand TempCommand = CreateCommand(QuerySqlString, TempConnection);

            object TempResult = TempCommand.ExecuteScalar();

            return (TempResult, TempConnection);
        }

        public void CreateCommandParameter(DbCommand TempCommand, string ParameterName, object ParameterValue)
        {
            if(TempCommand is OleDbCommand TempOledCommand)
            {
                TempOledCommand.Parameters.AddWithValue(ParameterName, ParameterValue);
            }
        }
    }

    public class SqliteDatabaseFactory : IDatabaseMethodFactory
    {
        public DbConnection CreateConnection(string DatabaseFile)
        {
            string ConnectionStr = $"Data Source={DatabaseFile};Version=3;";

            return new SQLiteConnection(ConnectionStr);
        }

        public DbCommand CreateCommand(string QueryString, DbConnection Connection)
        {
            return new SQLiteCommand(QueryString, (SQLiteConnection)Connection);
        }

        public (DbDataAdapter, DbConnection) CreateDataAdapter(string DataBaseFile, string QueryString)
        {
            DbConnection TempConnection = CreateConnection(DataBaseFile);

            TempConnection.Open();

            DbCommand TempCommand = CreateCommand(QueryString, TempConnection);

            DbDataAdapter TempDataAdapter = new SQLiteDataAdapter((SQLiteCommand)TempCommand);

            return (TempDataAdapter, TempConnection);
        }

        public (DbDataReader, DbConnection) CreateDataReader(string DataBaseFile, string QueryString)
        {
            DbConnection TempConnection = CreateConnection(DataBaseFile);

            TempConnection.Open();

            DbCommand TempCommand = CreateCommand(QueryString, TempConnection);

            DbDataReader TempDataReader = TempCommand.ExecuteReader();

            return (TempDataReader, TempConnection);
        }

        public (int, DbConnection) ExecuteDataBaseSql(string DataBaseFile, string ExecuteSqlString)
        {
            DbConnection TempConnection = CreateConnection(DataBaseFile);

            TempConnection.Open();

            DbCommand TempCommand = CreateCommand(ExecuteSqlString, TempConnection);

            int AffRows = TempCommand.ExecuteNonQuery();

            return (AffRows, TempConnection);
        }
        public (object, DbConnection) GetDataBaseDataByOne(string DataBaseFile, string QuerySqlString)
        {
            DbConnection TempConnection = CreateConnection(DataBaseFile);

            TempConnection.Open();

            DbCommand TempCommand = CreateCommand(QuerySqlString, TempConnection);

            object TempResult = TempCommand.ExecuteScalar();

            return (TempResult, TempConnection);
        }

        public void CreateCommandParameter(DbCommand TempCommand, string ParameterName, object ParameterValue)
        {
            if (TempCommand is SQLiteCommand TempSqliteCommand)
            {
                TempSqliteCommand.Parameters.AddWithValue(ParameterName, ParameterValue);
            }
        }
    }
}
