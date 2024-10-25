using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ProyectoParqueo.Controllers
{
    public class HomeController : Controller
    {
        // Par�metros del estacionamiento
        private static int _parkingSpaces = 10; // N�mero total de espacios de estacionamiento
        private static int _occupiedSpaces = 0; // Espacios actualmente ocupados
        private static ConcurrentQueue<int> _waitingVehicles = new ConcurrentQueue<int>(); // Cola de veh�culos en espera
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(_parkingSpaces, _parkingSpaces); // Sem�foro para controlar el acceso
        private static int _vehicleIdCounter = 0; // Contador para identificar veh�culos �nicos

        // Lista de hilos activos
        private static List<Thread> _vehicleThreads = new List<Thread>();

        // Objeto para sincronizaci�n
        private static object _lock = new object();

        // P�gina principal
        public IActionResult Index()
        {
            ViewBag.TotalSpaces = _parkingSpaces;
            ViewBag.OccupiedSpaces = _occupiedSpaces;
            return View();
        }

        // Acci�n para agregar veh�culos
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

            return Json(new { success = true, message = $"{vehicleCount} veh�culos han intentado ingresar." });
        }

        // M�todo para gestionar el ingreso de veh�culos al estacionamiento
        private void ManageParking(int vehicleId)
        {
            // Intentar entrar al estacionamiento
            _semaphore.Wait();

            // Remover de la cola de espera
            int dequeuedVehicleId;
            _waitingVehicles.TryDequeue(out dequeuedVehicleId);

            Interlocked.Increment(ref _occupiedSpaces);

            // Simular el tiempo que el veh�culo est� estacionado
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

        // Acci�n para obtener el estado del estacionamiento
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
