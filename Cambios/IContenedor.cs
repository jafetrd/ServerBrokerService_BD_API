using System.Collections.Generic;
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
        IList<Contenedor> GetAllContenedores();

        [OperationContract]
        void PublishContenedorChange(string regimen, string buque, int viaje);
    }

}