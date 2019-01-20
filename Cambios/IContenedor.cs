using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using ConsoleApplicationServer.Models;

namespace ConsoleApplicationServer.Cambios
{
    [ServiceContract(CallbackContract = typeof(IContenedorCallback))]
    public interface IContenedor
    {
        [OperationContract]
        void Subscribe();

        [OperationContract]
        void Unsubscribe();

        [OperationContract]
        IList<Contenedor> obtenerTodasImportaciones();

        [OperationContract]
        IList<Contenedor> obtenerTodasExportaciones();

        [OperationContract]
        void cambioImportaciones(string ID, string BUQUE, string CONTENEDOR, string VIAJE, string FECHA_ENTRADA, string ESTADO, string ALMACEN);

        [OperationContract]
        void cambioExportaciones(string ID, string BUQUE, string CONTENEDOR, string VIAJE, string FECHA_ENTRADA, string ESTADO, string ALMACEN);
    }
}