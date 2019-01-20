using System;

namespace ConsoleApplicationServer.Models
{
    public class Contenedor
    {
        public string ID { get; set; }
        public string BUQUE { get; set; }
        public string CONTENEDOR { get; set; }
        public string VIAJE { get; set; }
        public string FECHA_ENTRADA { get; set; }
        public string ESTADO { get; set; }
        public string ALMACEN { get; set; }

        public string REGIMEN { get; set; }

        public string PRESENTACION { get; set; }
        public string INICIALES { get; set; }
        public string NUMERO { get; set; }
    }

    public class Clientes
    {
        public int ID { get; set; }
        public string CLIENTE { get; set; }
    }

    public class Buques
    {
        public string ID { get; set; }
        public string BUQUE { get; set; }
        public string VIAJE { get; set; }
    }

    public class Productos
    {
        public int ID { get; set; }
        public string PRODUCTO { get; set; }
    }
}