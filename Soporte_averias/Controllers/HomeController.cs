using Soporte_averias.Models;
using Soporte_averias.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Soporte_averias.Controllers
{
	[ValidarSesionAttribute]
	[PermisosRol(Rol.Administrador)]
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult About()
		{
			ViewBag.Title = "Sobre nosotros";
			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Estamos esperando su contacto.";
			ViewBag.Title = "Contacto";

			return View();
		}

		public ActionResult Cerrar_Sesion()
		{
			return RedirectToAction("Inicio_Sesion", "Acceso");
		}
	}
}