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
    public class tabla_contenedor : IContenedor, IDisposable
    {
        #region Instance variables

        private readonly List<IContenedorCallback> _callbackList =
                new List<IContenedorCallback>();
        private readonly string _connectionString;
        private readonly SqlTableDependency<Contenedor> _sqlTableDependency;

        #endregion

        #region Constructors

        public tabla_contenedor()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

            _sqlTableDependency = new SqlTableDependency<Contenedor>(_connectionString, "tabla_patio_contenedor");

            _sqlTableDependency.OnChanged += TableDependency_Changed;
            _sqlTableDependency.OnError += (sender, args) => Console.WriteLine($"Error: {args.Message}");
            _sqlTableDependency.Start();

            Console.WriteLine(@"Waiting for receiving notifications...");
        }

        #endregion

        #region SqlTableDependency

        private void TableDependency_Changed(
            object sender,
            RecordChangedEventArgs<Contenedor> e)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"DML: {e.ChangeType}");
            Console.WriteLine($"regimen: {e.Entity.REGIMEN}");
            Console.WriteLine($"buque: {e.Entity.BUQUE}");
            Console.WriteLine($"viaje: {e.Entity.VIAJE}");

            this.PublishContenedorChange(e.Entity.REGIMEN, e.Entity.BUQUE, e.Entity.VIAJE);
        }

        #endregion

        #region Publish-Subscribe design pattern

        public IList<Contenedor> GetAllContenedores()
        {
            var contenedores = new List<Contenedor>();

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "SELECT * FROM [tabla_patio_contenedor]";

                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            string regimen = sqlDataReader.GetString(sqlDataReader.GetOrdinal("REGIMEN"));
                            string buque = sqlDataReader.GetString(sqlDataReader.GetOrdinal("BUQUE"));
                            int viaje = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("VIAJE"));

                            contenedores.Add(new Contenedor
                            {
                                REGIMEN = regimen,
                                BUQUE = buque,
                                VIAJE = viaje
                            });
                        }
                    }
                }
            }

            return contenedores;
        }

        public void Subscribe()
        {
            var registeredUser = OperationContext.Current.GetCallbackChannel<IContenedorCallback>();
            if (!_callbackList.Contains(registeredUser))
            {
                _callbackList.Add(registeredUser);
            }
        }

        public void Unsubscribe()
        {
            var registeredUser = OperationContext.Current.GetCallbackChannel<IContenedorCallback>();
            if (_callbackList.Contains(registeredUser))
            {
                _callbackList.Remove(registeredUser);
            }
        }

        public void PublishContenedorChange(string regimen, string buque, int viaje)
        {
            _callbackList.ForEach(delegate (IContenedorCallback callback) {
                callback.cambioContenedor(buque, viaje, regimen);
            });
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _sqlTableDependency.Stop();
        }



        #endregion
    }
}