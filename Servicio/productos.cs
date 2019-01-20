using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.ServiceModel;
using ConsoleApplicationServer.Models;
using ConsoleApplicationServer.Cambios;
using TableDependency.EventArgs;
using TableDependency.SqlClient;

namespace ConsoleApplicationServer.Servicio
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    class productos : IProductos, IDisposable
    {
        #region Instance variables

        private readonly List<IproductosCallBack> _callbackList = new List<IproductosCallBack>();
        private readonly string _connectionString;
        private readonly SqlTableDependency<Productos> _sqlTableDependency4;
        #endregion

        #region Constructors

        public productos()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

            _sqlTableDependency4 = new SqlTableDependency<Productos>(_connectionString, "productos");

            _sqlTableDependency4.OnChanged += TableDependency4_Changed;
            _sqlTableDependency4.OnError += (sender, args) => Console.WriteLine($"error: {args.Message}");
            _sqlTableDependency4.Start();

            while (!(_sqlTableDependency4.Status == TableDependency.Enums.TableDependencyStatus.WaitingForNotification)) { }

            Console.WriteLine(@"ESPERANDO NOTIFICACIONES 4");
        }

        #endregion

        #region SqlTableDependency4
        private void TableDependency4_Changed(
            object sender,
            RecordChangedEventArgs<Productos> e)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"DML: {e.ChangeType}");
            Console.WriteLine($"TABLA : PRODUCTOS");
            this.cambiosProductos(e.Entity.ID, e.Entity.PRODUCTO);
        }

        public IList<Productos> obtenerTodosProductos()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "SELECT * FROM [productos]";
                    var contenedores = new List<Productos>();
                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        if (sqlDataReader.HasRows)
                            while (sqlDataReader.Read())
                            {
                                contenedores.Add(new Productos
                                {
                                    PRODUCTO = sqlDataReader.SafeGetString("PRODUCTO")
                                });
                            }
                    }
                    return contenedores;
                }
            }
        }
        #endregion

        #region Publish-Subscribe design pattern


        public void Subscribe()
        {
            var registeredUser = OperationContext.Current.GetCallbackChannel<IproductosCallBack>();
            if (!_callbackList.Contains(registeredUser))
            {
                _callbackList.Add(registeredUser);
            }
        }

        public void Unsubscribe()
        {
            var registeredUser = OperationContext.Current.GetCallbackChannel<IproductosCallBack>();
            if (_callbackList.Contains(registeredUser))
            {
                _callbackList.Remove(registeredUser);
            }
        }

        public void cambiosProductos(int ID, string PRODUCTO)
        {
            _callbackList.ForEach(delegate (IproductosCallBack callback)
            {
                callback.cambiosProductos(ID, PRODUCTO);
            });
        }
        #endregion

        #region IDisposable

        public void Dispose()
        {
            _sqlTableDependency4.Stop();
        }
        #endregion
    }
}
