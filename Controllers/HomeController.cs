using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;

namespace ProyectoSemaforos.Controllers
{
    public class HomeController : Controller
    {
        private static int _capacidadMaxima = 5; // Espacios máximos disponibles en el estacionamiento
        private static List<string> _vehiculosEstacionados = new(); // Vehículos dentro del estacionamiento
        private static Queue<string> _vehiculosEnEspera = new(); // Vehículos en espera
        private static Semaphore _semaforo = new(_capacidadMaxima, _capacidadMaxima); // Controla el acceso con semáforos
        private static object _lock = new(); // Asegura la concurrencia

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
                        _vehiculosEstacionados.Add($"Vehículo-{_vehiculosEstacionados.Count + 1}");
                        ingresados++;
                    }
                    else
                    {
                        _vehiculosEnEspera.Enqueue($"Vehículo-{_vehiculosEstacionados.Count + i + 1}");
                    }
                }

                string mensaje = ingresados > 0
                    ? $"{ingresados} vehículos ingresaron. {cantidad - ingresados} en espera."
                    : "Todos los vehículos están en espera.";

                return Json(new
                {
                    success = true,
                    estacionados = _vehiculosEstacionados.Count,
                    enEspera = _vehiculosEnEspera.Count,
                    message = mensaje
                });
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
                        _vehiculosEstacionados.RemoveAt(0);
                        _semaforo.Release(); // Libera un espacio
                        eliminados++;

                        // Si hay vehículos en espera, ingrésalos
                        if (_vehiculosEnEspera.Count > 0 && _semaforo.WaitOne(0))
                        {
                            string vehiculoEnEspera = _vehiculosEnEspera.Dequeue();
                            _vehiculosEstacionados.Add(vehiculoEnEspera);
                        }
                    }
                }

                string mensaje = eliminados > 0
                    ? $"{eliminados} vehículos salieron. {cantidad - eliminados} no se encontraron."
                    : "No hay vehículos disponibles para sacar.";

                return Json(new
                {
                    success = true,
                    estacionados = _vehiculosEstacionados.Count,
                    enEspera = _vehiculosEnEspera.Count,
                    message = mensaje
                });
            }
        }
    }
}
