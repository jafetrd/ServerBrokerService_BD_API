using ConsoleApplicationServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplicationServer.Cambios
{

    [ServiceContract(CallbackContract = typeof(IclienteCallback))]
    interface IClientes
    {
        [OperationContract]
        void Subscribe();

        [OperationContract]
        void Unsubscribe();

        [OperationContract]
        IList<Clientes> obtenerTodosClientes();

        [OperationContract]
        void cambiosCliente(int ID, string CLIENTE);
    }
}
