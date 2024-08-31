using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Soporte_averias.Controllers.Acceso
{
    public class AccesoDenegadoController : Controller
    {
		// GET: AccesoDenegadoController
		public ActionResult NoAutorizado()
		{
			return View();
		}
	}
}