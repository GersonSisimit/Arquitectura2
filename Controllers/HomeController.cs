using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;

namespace ProyectoSemaforos.Controllers
{
    public class HomeController : Controller
    {
        public static int CapacidadMaxima = 5; // Capacidad total del estacionamiento
        private static int estacionados = 0; // Vehículos estacionados
        private static Queue<int> espera = new(); // Vehículos en espera
        private static SemaphoreSlim semaphore = new SemaphoreSlim(CapacidadMaxima); // Semáforo

       //Grupo 6 wudy

        public IActionResult Index()
        {
            ViewBag.CapacidadMaxima = CapacidadMaxima; // Pasar capacidad a la vista
            return View();
        }

        [HttpPost]
        public IActionResult IngresarVehiculos(int cantidad)
        {
            // Validación de entrada
            if (cantidad <= 0)
            {
                return Json(new { success = false, message = "La cantidad debe ser un número positivo." });
            }

            int ingresados = 0;

            for (int i = 0; i < cantidad; i++)
            {
                if (semaphore.Wait(0)) // Intenta adquirir el semáforo
                {
                    estacionados++;
                    ingresados++;
                }
                else
                {
                    espera.Enqueue(1); // Agregar a la cola de espera
                }
            }

            // Verificar si hay vehículos en espera
            while (espera.Count > 0 && estacionados < CapacidadMaxima)
            {
                espera.Dequeue(); // Retirar de la espera
                estacionados++; // Incrementar el número de estacionados
            }

            return Json(new { success = true, message = $"{ingresados} vehículos ingresados.", estacionados, enEspera = espera.Count });
        }

        [HttpPost]
        public IActionResult SacarVehiculos(int cantidad)
        {
            // Validación de entrada
            if (cantidad <= 0)
            {
                return Json(new { success = false, message = "La cantidad debe ser un número positivo." });
            }

            if (cantidad > estacionados)
            {
                return Json(new { success = false, message = $"No se pueden retirar más de {estacionados} vehículos." });
            }

            // Liberar el semáforo por cada vehículo que sale
            for (int i = 0; i < cantidad; i++)
            {
                estacionados--; // Restar vehículos estacionados
                semaphore.Release(); // Liberar un espacio en el semáforo
            }

            // Verificar si hay vehículos en espera
            for (int i = 0; i < cantidad; i++)
            {
                if (espera.Count > 0)
                {
                    espera.Dequeue(); // Retirar de la espera
                    estacionados++; // Incrementar el número de estacionados
                    semaphore.Wait(); // Esperar por un espacio
                }
            }

            return Json(new { success = true, message = $"{cantidad} vehículos retirados.", estacionados, enEspera = espera.Count });
        }
    }
}
