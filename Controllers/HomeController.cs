using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ProyectoParqueo.Controllers
{
    public class HomeController : Controller
    {
        // Parámetros del estacionamiento
        private static int _parkingSpaces = 10; // Número total de espacios de estacionamiento
        private static int _occupiedSpaces = 0; // Espacios actualmente ocupados
        private static ConcurrentQueue<int> _waitingVehicles = new ConcurrentQueue<int>(); // Cola de vehículos en espera
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(_parkingSpaces, _parkingSpaces); // Semáforo para controlar el acceso
        private static int _vehicleIdCounter = 0; // Contador para identificar vehículos únicos

        // Lista de hilos activos
        private static List<Thread> _vehicleThreads = new List<Thread>();

        // Objeto para sincronización
        private static object _lock = new object();

        // Página principal
        public IActionResult Index()
        {
            ViewBag.TotalSpaces = _parkingSpaces;
            ViewBag.OccupiedSpaces = _occupiedSpaces;
            return View();
        }

        // Acción para agregar vehículos
        [HttpPost]
        public JsonResult AddVehicles(int vehicleCount)
        {
            for (int i = 0; i < vehicleCount; i++)
            {
                int vehicleId = Interlocked.Increment(ref _vehicleIdCounter);
                _waitingVehicles.Enqueue(vehicleId);

                Thread vehicleThread = new Thread(() => ManageParking(vehicleId));
                vehicleThread.Start();

                lock (_lock)
                {
                    _vehicleThreads.Add(vehicleThread);
                }
            }

            return Json(new { success = true, message = $"{vehicleCount} vehículos han intentado ingresar." });
        }

        // Método para gestionar el ingreso de vehículos al estacionamiento
        private void ManageParking(int vehicleId)
        {
            // Intentar entrar al estacionamiento
            _semaphore.Wait();

            // Remover de la cola de espera
            int dequeuedVehicleId;
            _waitingVehicles.TryDequeue(out dequeuedVehicleId);

            Interlocked.Increment(ref _occupiedSpaces);

            // Simular el tiempo que el vehículo está estacionado
            Thread.Sleep(3000); // Esperar 3 segundos

            // Salir del estacionamiento
            Interlocked.Decrement(ref _occupiedSpaces);
            _semaphore.Release();

            // Remover el hilo de la lista de hilos activos
            lock (_lock)
            {
                _vehicleThreads.Remove(Thread.CurrentThread);
            }
        }

        // Acción para obtener el estado del estacionamiento
        [HttpGet]
        public JsonResult GetParkingStatus()
        {
            return Json(new
            {
                totalSpaces = _parkingSpaces,
                occupiedSpaces = _occupiedSpaces,
                waitingVehicles = _waitingVehicles.Count,
                threadCount = _vehicleThreads.Count
            });
        }
    }
}
