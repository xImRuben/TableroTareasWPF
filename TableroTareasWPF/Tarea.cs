using System;

namespace TableroTareasWPF
{
    public class Tarea
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string Prioridad { get; set; } = "Media";

        // NUEVO: Requisito del PDF para el informe
        public string Responsable { get; set; } = "Sin asignar";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}