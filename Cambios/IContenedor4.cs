using ConsoleApplicationServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplicationServer.Cambios
{
    [ServiceContract(CallbackContract = typeof(IproductosCallBack))]
    interface IProductos
    {
        [OperationContract]
        void Subscribe();

        [OperationContract]
        void Unsubscribe();

        [OperationContract]
        IList<Productos> obtenerTodosProductos();

        [OperationContract]
        void cambiosProductos(int ID, string PRODUCTO);
    }
}
