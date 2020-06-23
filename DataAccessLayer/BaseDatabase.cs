using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;
using SQLite;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using AppSettings;
using MSGraph.Response;
using MSGraph;
using UwpSqLiteDal;

namespace UwpSqliteDal
{
    public abstract class BaseDatabase
    {
        
        static readonly string _databasePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, Configuration.DatabaseName);
        //static readonly Lazy<SQLiteAsyncConnection> _databaseConnectionHolder = new Lazy<SQLiteAsyncConnection>(() => new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache));

        //static SQLite.SQLiteAsyncConnection DatabaseConnection => _databaseConnectionHolder.Value;
    }
}
