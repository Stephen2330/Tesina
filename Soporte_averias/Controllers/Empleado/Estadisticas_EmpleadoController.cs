using Soporte_averias.Models;
using Soporte_averias.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Soporte_averias.Controllers.Empleado
{

	[ValidarSesionAttribute]
	[PermisosRol(Rol.Empleado)]
	public class Estadisticas_EmpleadoController : Controller
    {
        // GET: Estadisticas_Empleado
        public ActionResult Index()
        {
            return View();
        }
    }
}