using System;
using System.ServiceModel;
using ConsoleApplicationServer.Servicio;

namespace ConsoleApplicationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new ServiceHost(typeof(tabla_contenedor));
            host.Open();
            Console.WriteLine($"Servicio iniciado en {host.Description.Endpoints[0].Address}");
            Console.WriteLine("Presiona cualquier tecla para terminar el servicio");
            Console.ReadLine();
            host.Close();
        }
    }
}