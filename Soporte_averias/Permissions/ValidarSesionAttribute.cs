using DocumentFormat.OpenXml.Spreadsheet;
using Soporte_averias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Soporte_averias.Permissions
{

	public class ValidarSesionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var session = filterContext.HttpContext.Session;
			Usuarios objusuarios = session["usuario"] as Usuarios;

			if (objusuarios != null)
			{
				filterContext.Controller.TempData["CorreoUsuario"] = objusuarios.TC_Correo;
			}
			else
			{
				filterContext.Controller.TempData["CorreoUsuario"] = null;
				filterContext.Result = new RedirectResult("~/Acceso/Inicio_Sesion");
			}

			base.OnActionExecuted(filterContext);
		}
	}


}//namespace