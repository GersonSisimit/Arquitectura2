using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ProyectoSemaforos.Controllers
{
    public class HomeController : Controller
    {
        public static int CapacidadMaxima = 5; // Capacidad total del estacionamiento
        private static int estacionados = 0; // Vehículos estacionados
        private static Queue<int> espera = new(); // Vehículos en espera

        public IActionResult Index()
        {
            ViewBag.CapacidadMaxima = CapacidadMaxima; // Pasar capacidad a la vista
            return View();
        }

        [HttpPost]
        public IActionResult IngresarVehiculos(int cantidad)
        {
            int ingresados = 0;

            for (int i = 0; i < cantidad; i++)
            {
                if (estacionados < CapacidadMaxima)
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
            if (espera.Count > 0 && estacionados < CapacidadMaxima)
            {
                espera.Dequeue(); // Retirar de la espera
                estacionados++; // Incrementar el número de estacionados
            }

            return Json(new { success = true, message = $"{ingresados} vehículos ingresados.", estacionados, enEspera = espera.Count });
        }

        [HttpPost]
        public IActionResult SacarVehiculos(int cantidad)
        {
            if (cantidad > estacionados)
            {
                return Json(new { success = false, message = "No hay suficientes vehículos estacionados." });
            }

            estacionados -= cantidad; // Restar vehículos estacionados

            // Verificar si hay vehículos en espera
            if (espera.Count > 0)
            {
                for (int i = 0; i < cantidad; i++)
                {
                    if (espera.Count > 0)
                    {
                        espera.Dequeue(); // Retirar de la espera
                        estacionados++; // Incrementar el número de estacionados
                    }
                }
            }

            return Json(new { success = true, message = $"{cantidad} vehículos retirados.", estacionados, enEspera = espera.Count });
        }
    }
}
