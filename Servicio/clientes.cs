using ConsoleApplicationServer.Cambios;
using ConsoleApplicationServer.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TableDependency.EventArgs;
using TableDependency.SqlClient;

namespace ConsoleApplicationServer.Servicio
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    class clientes: IClientes, IDisposable
    { 
    
        #region Instance variables

        private readonly List<IclienteCallback> _callbackList = new List<IclienteCallback>();
        private readonly string _connectionString;

        private readonly SqlTableDependency<Clientes> _sqlTableDependency3;
        #endregion

        #region Constructors

        public clientes()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

            _sqlTableDependency3 = new SqlTableDependency<Clientes>(_connectionString, "clientes");

            _sqlTableDependency3.OnChanged += TableDependency3_Changed;
            _sqlTableDependency3.OnError += (sender, args) => Console.WriteLine($"error: {args.Message}");
            _sqlTableDependency3.Start();

            while (!(_sqlTableDependency3.Status == TableDependency.Enums.TableDependencyStatus.WaitingForNotification)) { }

            Console.WriteLine(@"ESPERANDO NOTIFICACIONES 3");
        }

        #endregion

        #region SqlTableDependency3
        private void TableDependency3_Changed(
            object sender,
            RecordChangedEventArgs<Clientes> e)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"DML: {e.ChangeType}");
            Console.WriteLine($"TABLA : CLIENTES");
            this.cambiosCliente(e.Entity.ID, e.Entity.CLIENTE);
        }

        public IList<Clientes> obtenerTodosClientes()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "SELECT * FROM [clientes]";
                    var contenedores = new List<Clientes>();
                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        if (sqlDataReader.HasRows)
                            while (sqlDataReader.Read())
                            {
                                contenedores.Add(new Clientes
                                {
                                    CLIENTE = sqlDataReader.SafeGetString("CLIENTE")
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
            var registeredUser = OperationContext.Current.GetCallbackChannel<IclienteCallback>();
            if (!_callbackList.Contains(registeredUser))
            {
                _callbackList.Add(registeredUser);
            }
        }

        public void Unsubscribe()
        {
            var registeredUser = OperationContext.Current.GetCallbackChannel<IclienteCallback>();
            if (_callbackList.Contains(registeredUser))
            {
                _callbackList.Remove(registeredUser);
            }
        }

        public void cambiosCliente(int ID, string CLIENTE)
        {
            _callbackList.ForEach(delegate (IclienteCallback callback)
            {
                callback.cambiosCliente(ID, CLIENTE);
            });
        }
        #endregion

        #region IDisposable

        public void Dispose()
        { 
            _sqlTableDependency3.Stop();
        }
        #endregion
    
}
}
