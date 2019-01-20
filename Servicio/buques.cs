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
    public class buques : IBuques, IDisposable
    {
        #region Instance variables

        private readonly List<IbuquesCallBack> _callbackList = new List<IbuquesCallBack>();
        private readonly string _connectionString;
        private readonly SqlTableDependency<Buques> _sqlTableDependency2;
        #endregion

        #region Constructors

        public buques()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            _sqlTableDependency2 = new SqlTableDependency<Buques>(_connectionString, "buques");

            _sqlTableDependency2.OnChanged += TableDependency2_Changed;
            _sqlTableDependency2.OnError += (sender, args) => Console.WriteLine($"Error: {args.Message}");
            _sqlTableDependency2.Start();

            while (!(_sqlTableDependency2.Status == TableDependency.Enums.TableDependencyStatus.WaitingForNotification)) { }

            Console.WriteLine(@"ESPERANDO NOTIFICACIONES 2");
        }

        #endregion

        #region SqlTableDependency2
        private void TableDependency2_Changed(
            object sender,RecordChangedEventArgs<Buques> e)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"DML: {e.ChangeType}");
            Console.WriteLine($"TABLA : BUQUES");
            this.cambiosBuques(e.Entity.BUQUE, e.Entity.VIAJE);
        }

        public IList<Buques> obtenerTodosBuque()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "SELECT * FROM [buques]";
                    var contenedores = new List<Buques>();
                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        if (sqlDataReader.HasRows)
                            while (sqlDataReader.Read())
                            {
                                contenedores.Add(new Buques
                                {
                                    BUQUE = sqlDataReader.SafeGetString("BUQUE"),
                                    VIAJE = sqlDataReader.SafeGetString("VIAJE")
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
            var registeredUser = OperationContext.Current.GetCallbackChannel<IbuquesCallBack>();
            if (!_callbackList.Contains(registeredUser))
            {
                _callbackList.Add(registeredUser);
            }
        }

        public void Unsubscribe()
        {
            var registeredUser = OperationContext.Current.GetCallbackChannel<IbuquesCallBack>();
            if (_callbackList.Contains(registeredUser))
            {
                _callbackList.Remove(registeredUser);
            }
        }


        public void cambiosBuques(string BUQUE, string VIAJE)
        {
            _callbackList.ForEach(delegate (IbuquesCallBack callback)
            {
                callback.cambiosBuques(BUQUE, VIAJE);
            });
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _sqlTableDependency2.Stop();
        }
        #endregion
    }
}
