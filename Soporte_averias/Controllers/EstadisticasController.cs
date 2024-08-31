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
	public class EstadisticasController : Controller
    {
        // GET: Estadisticas
        public ActionResult Index()
        {
            return View();
        }
    }
}