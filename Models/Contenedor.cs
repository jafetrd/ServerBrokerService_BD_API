using System;

namespace ConsoleApplicationServer.Models
{
    public class Contenedor
    {
        public int ID { get; set; }
        public string BUQUE { get; set; }
        public int VIAJE { get; set; }
        public string REGIMEN { get; set; }
        public DateTime FECHA_ENTRADA { get; set; }
        public string PRESENTACION { get; set; }
        public string INICIALES { get; set; }
        public int NUMERO { get; set; }
        public Decimal PESO { get; set; }
        public string UNIDADES { get; set; }
        public string PRODUCTO { get; set; }
        public string CLIENTE { get; set; }
        public Decimal PEDIMENTO { get; set; }
        public string VALOR_COMERICAL { get; set; }
        public DateTime FECHA_SALIDA { get; set; }
        public string SESION_ENTRADA { get; set; }
        public string SESION_SALIDA { get; set; }
        public short ESTADO { get; set; }
        public string AGENTE { get; set; }
    }
}