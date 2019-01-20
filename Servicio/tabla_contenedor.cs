using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.ServiceModel;
using ConsoleApplicationServer.Models;
using ConsoleApplicationServer.Cambios;
using TableDependency.EventArgs;
using TableDependency.SqlClient;
using static ConsoleApplicationServer.Servicio.extensionDataRead;
namespace ConsoleApplicationServer.Servicio
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class tabla_contenedor : IContenedor, IDisposable
    {
        #region Instance variables

        private readonly List<IContenedorCallback> _callbackList = new List<IContenedorCallback>();
        private readonly string _connectionString;
        private readonly SqlTableDependency<Contenedor> _sqlTableDependency;
        #endregion

        #region Constructors

        public tabla_contenedor()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

            _sqlTableDependency = new SqlTableDependency<Contenedor>(_connectionString, "temporal");

            _sqlTableDependency.OnChanged += TableDependency_Changed;
            _sqlTableDependency.OnError += (sender, args) => Console.WriteLine($"error: {args.Message}");
            _sqlTableDependency.Start();

            while (!(_sqlTableDependency.Status == TableDependency.Enums.TableDependencyStatus.WaitingForNotification)){ }

            Console.WriteLine(@"ESPERANDO NOTIFICACIONES 1");
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
            Console.WriteLine($"TABLA : CONTENEDOR");
            switch (e.Entity.REGIMEN)
            {
                case "IMPO":
                this.cambioImportaciones(e.Entity.ID, e.Entity.BUQUE, e.Entity.INICIALES + e.Entity.NUMERO, e.Entity.VIAJE, e.Entity.FECHA_ENTRADA, e.Entity.ESTADO, e.Entity.ALMACEN);
                    break;
                case "EXPO":
                this.cambioExportaciones(e.Entity.ID, e.Entity.BUQUE, e.Entity.INICIALES + e.Entity.NUMERO, e.Entity.VIAJE, e.Entity.FECHA_ENTRADA, e.Entity.ESTADO, e.Entity.ALMACEN);
                    break;
            }
        }

        public IList<Contenedor> obtenerTodasImportaciones()
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "SELECT * FROM [Temporal] WHERE REGIMEN = @REGIMEN";
                    sqlCommand.Parameters.AddWithValue("@REGIMEN", "IMPO");

                    return GetContenedors(sqlCommand);
                }
            }
        }

        public IList<Contenedor> obtenerTodasExportaciones()
        {

            //List<Contenedor> contenedores;
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = "SELECT * FROM [Temporal] WHERE REGIMEN = @REGIMEN";
                    sqlCommand.Parameters.AddWithValue("@REGIMEN", "EXPO");

                    return GetContenedors(sqlCommand);
                }
            }
        }

        private IList<Contenedor> GetContenedors(SqlCommand sqlCommand)
        {
            var contenedores = new List<Contenedor>();
            using (var sqlDataReader = sqlCommand.ExecuteReader())
            {
                if (sqlDataReader.HasRows)
                    while (sqlDataReader.Read())
                    {
                        contenedores.Add(new Contenedor
                        {
                            ID = sqlDataReader.SafeGetString("ID"),
                            BUQUE = sqlDataReader.SafeGetString("BUQUE"),
                            CONTENEDOR = sqlDataReader.SafeGetString("INICIALES") + sqlDataReader.SafeGetString("NUMERO"),
                            VIAJE = sqlDataReader.SafeGetString("VIAJE"),
                            FECHA_ENTRADA = sqlDataReader.SafeGetString("FECHA_ENTRADA"),
                            ESTADO = sqlDataReader.SafeGetString("ESTADO"),
                            ALMACEN = sqlDataReader.SafeGetString("ALMACEN")
                        });
                    }
            }
            return contenedores;
        }
        #endregion

        #region Publish-Subscribe design pattern


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


        public void cambioImportaciones(string ID, string BUQUE, string CONTENEDOR,string VIAJE, string FECHA_ENTRADA,string ESTADO,string ALMACEN)
        {
            _callbackList.ForEach(delegate (IContenedorCallback callback) {
                callback.cambiosImpo(ID, BUQUE, CONTENEDOR, VIAJE, FECHA_ENTRADA,ESTADO,ALMACEN);
            });
        }

        public void cambioExportaciones(string ID, string BUQUE, string CONTENEDOR, string VIAJE, string FECHA_ENTRADA,string ESTADO,string ALMACEN)
        {
            _callbackList.ForEach(delegate (IContenedorCallback callback) 
            {callback.cambiosExpo(ID, BUQUE, CONTENEDOR, VIAJE, FECHA_ENTRADA, ESTADO,ALMACEN);
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

    public static class extensionDataRead
    {
        public static string SafeGetString(this SqlDataReader reader, string Columna)
        {
            int columIndex = reader.GetOrdinal(Columna);
            if (!reader.IsDBNull(columIndex))
            {
                return reader.GetString(columIndex);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}