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

            DbDataAdapter CreateDataAdapter(DbCommand Command);

            DbDataReader CreateDataReader(DbCommand Command);
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

        public DbDataAdapter CreateDataAdapter(DbCommand Command)
        {
            return new OleDbDataAdapter((OleDbCommand)Command);
        }

        public DbDataReader CreateDataReader(DbCommand Command)
        {
            return ((OleDbCommand)Command).ExecuteReader();
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

        public DbDataAdapter CreateDataAdapter(DbCommand Command)
        {
            return new SQLiteDataAdapter((SQLiteCommand)Command);
        }

        public DbDataReader CreateDataReader(DbCommand Command)
        {
            return ((SQLiteCommand)Command).ExecuteReader();
        }
    }
}
