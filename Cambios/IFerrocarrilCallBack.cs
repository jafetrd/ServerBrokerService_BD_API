using System.ServiceModel;

namespace ConsoleApplicationServer.Cambios
{
    interface IContenedorCallback
    {
        [OperationContract]
        void cambioContenedor(string BUQUE, int VIAJE, string REGIMEN);
    }
}