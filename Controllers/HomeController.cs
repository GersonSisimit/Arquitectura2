using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ProyectoSemaforos.Controllers
{
    public class HomeController : Controller
    {
        public static int CapacidadMaxima = 5; // Capacidad total del estacionamiento
        private static int estacionados = 0; // Veh�culos estacionados
        private static Queue<int> espera = new(); // Veh�culos en espera

        public IActionResult Index()
        {
            ViewBag.CapacidadMaxima = CapacidadMaxima; // Pasar capacidad a la vista
            return View();
        }

        [HttpPost]
        public IActionResult IngresarVehiculos(int cantidad)
        {
            // Validaci�n de entrada
            if (cantidad <= 0)
            {
                return Json(new { success = false, message = "La cantidad debe ser un n�mero positivo." });
            }

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

            // Verificar si hay veh�culos en espera
            if (espera.Count > 0 && estacionados < CapacidadMaxima)
            {
                espera.Dequeue(); // Retirar de la espera
                estacionados++; // Incrementar el n�mero de estacionados
            }

            return Json(new { success = true, message = $"{ingresados} veh�culos ingresados.", estacionados, enEspera = espera.Count });
        }

        [HttpPost]
        public IActionResult SacarVehiculos(int cantidad)
        {
            // Validaci�n de entrada
            if (cantidad <= 0)
            {
                return Json(new { success = false, message = "La cantidad debe ser un n�mero positivo." });
            }

            if (cantidad > estacionados)
            {
                return Json(new { success = false, message = $"No se pueden retirar m�s de {estacionados} veh�culos." });
            }

            estacionados -= cantidad; // Restar veh�culos estacionados

            // Verificar si hay veh�culos en espera
            for (int i = 0; i < cantidad; i++)
            {
                if (espera.Count > 0)
                {
                    espera.Dequeue(); // Retirar de la espera
                    estacionados++; // Incrementar el n�mero de estacionados
                }
            }

            return Json(new { success = true, message = $"{cantidad} veh�culos retirados.", estacionados, enEspera = espera.Count });
        }
    }
}
