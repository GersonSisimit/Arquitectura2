using System.Collections.Generic;

namespace ProyectoSemaforos.Models
{
    public class EstacionamientoViewModel
    {
        public List<string> Ocupados { get; set; }
        public List<string> EnEspera { get; set; }
        public int LugaresDisponibles { get; set; }
    }
}
