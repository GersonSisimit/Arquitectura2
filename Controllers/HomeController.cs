using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;

namespace ProyectoSemaforos.Controllers
{
    public class HomeController : Controller
    {
        private static int _capacidadMaxima = 10; // Capacidad fija del estacionamiento
        private static List<string> _vehiculosEstacionados = new(); // Lista de veh�culos dentro
        private static Semaphore _semaforo = new(_capacidadMaxima, _capacidadMaxima); // Controla el acceso al estacionamiento
        private static object _lock = new(); // Para asegurar la concurrencia

        public IActionResult Index() => View();

        [HttpPost]
        public JsonResult IngresarVehiculos(int cantidad)
        {
            lock (_lock)
            {
                int ingresados = 0;

                for (int i = 0; i < cantidad; i++)
                {
                    if (_semaforo.WaitOne(0)) // Si hay espacio disponible
                    {
                        _vehiculosEstacionados.Add($"Veh�culo-{_vehiculosEstacionados.Count + 1}");
                        ingresados++;
                    }
                    else
                    {
                        break; // No hay m�s espacio disponible
                    }
                }

                string mensaje = ingresados > 0
                    ? $"{ingresados} veh�culos ingresados correctamente."
                    : "No hay espacio disponible.";

                return Json(new { success = ingresados > 0, message = mensaje });
            }
        }

        [HttpPost]
        public JsonResult SacarVehiculos(int cantidad)
        {
            lock (_lock)
            {
                int eliminados = 0;

                for (int i = 0; i < cantidad; i++)
                {
                    if (_vehiculosEstacionados.Count > 0)
                    {
                        _vehiculosEstacionados.RemoveAt(0); // Remueve el primer veh�culo
                        _semaforo.Release(); // Libera un espacio
                        eliminados++;
                    }
                    else
                    {
                        break; // No hay m�s veh�culos para sacar
                    }
                }

                string mensaje = eliminados > 0
                    ? $"{eliminados} veh�culos salieron del estacionamiento."
                    : "No hay veh�culos para sacar.";

                return Json(new { success = eliminados > 0, message = mensaje });
            }
        }
    }
}
