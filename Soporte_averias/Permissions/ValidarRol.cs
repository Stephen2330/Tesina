using DocumentFormat.OpenXml.Spreadsheet;
using Soporte_averias.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Soporte_averias.Permissions
{
	public class PermisosRolAttribute : ActionFilterAttribute
	{
		private Rol idrol;

		public PermisosRolAttribute(Rol _idrol)
		{
			idrol = _idrol;
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var session = filterContext.HttpContext.Session;
			var tempData = filterContext.Controller.TempData;

			if (session["usuario"] != null)
			{
				Usuarios objUsuarios = session["usuario"] as Usuarios;

				if (objUsuarios != null)
				{
					tempData["Perfil"] = true;

					if (objUsuarios.TN_IdRol != this.idrol)
					{
						filterContext.Result = new RedirectResult("~/AccesoDenegado/NoAutorizado");
					}
				}
				else
				{
					filterContext.Result = new RedirectResult("~/Acceso/Inicio_Sesion");
				}
			}
			else
			{
				filterContext.Result = new RedirectResult("~/Acceso/Inicio_Sesion");
			}
		}
	}
}//namespace