using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SQLite;

namespace DocumentTools
{
    internal class DataBaseHelper
    {
        //根据后缀名生成对应的数据库connection
        public static IDbConnection GetDataBaseConnection(string DatbaseFile)
        {
            string TempExtension = Path.GetExtension(DatbaseFile);
            string TempConnectStr = string.Empty;
            IDbConnection TempConnection = null;

            string[] AccessExtensions = new string[] { ".mdb",".exam"};
            string[] SqliteExtensions = new string[] { ".db",".sqlite",".sqlite3"};

            if(AccessExtensions.Contains(TempExtension))
            {
                TempConnectStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DatbaseFile};Persist Security Info=False;";
                
                TempConnection = new OleDbConnection(TempConnectStr);
            }
            else if(SqliteExtensions.Contains(TempExtension))
            {
                TempConnectStr = $"Data Source={DatbaseFile};Version=3";
                
                TempConnection = new SQLiteConnection(TempConnectStr);
            }

            return TempConnection;
        }

        public static void GetDataBaseDataAdapter(IDbConnection TempConnection,string QueryString)
        {
            if(TempConnection is OleDbConnection TempAccessConnection)
            {
                OleDbCommand TempAccessCommand = new OleDbCommand(QueryString, TempAccessConnection);
            }
        }
    }
}
