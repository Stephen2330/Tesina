using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using Soporte_averias.Models;
using System.Runtime.Remoting.Messaging;
using Microsoft.Ajax.Utilities;
using System.Web.Security;
using System.Net;
using Soporte_averias;
using System.Data.Entity.Infrastructure;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;

namespace Soporte_averias.Controllers.Acceso
{
	public class AccesoController : Controller
	{

		static string conn = "data source=STEPHEN\\SQLEXPRESS;initial catalog=SOPORTE; integrated security=True";

		// GET: Acceso
		public ActionResult Inicio_Sesion()
		{

			return View();
		}

		public ActionResult Registro()
		{

			return View();
		}

		[HttpPost]
		public ActionResult Registro(Usuarios objUsuario)
		{
			bool registrado;
			string mensaje;



			if (objUsuario.TC_Clave != objUsuario.confirmar_clave)
			{
				ViewData["mensaje"] = "Las contraseñas no coinciden.";
				return View();
			}

			if (objUsuario.TC_Clave == objUsuario.confirmar_clave)
			{
				objUsuario.TC_Clave = ConvertirSha256(objUsuario.TC_Clave);
			}

			using (SqlConnection cons = new SqlConnection(conn))
			{
				SqlCommand cmd = new SqlCommand("sp_RegistrarUsuario", cons);
				cmd.Parameters.AddWithValue("cedula", objUsuario.TN_Cedula);
				cmd.Parameters.AddWithValue("nombre", objUsuario.TC_Nombre);
				cmd.Parameters.AddWithValue("apellido1", objUsuario.TC_PrimerApellido);
				cmd.Parameters.AddWithValue("apellido2", objUsuario.TC_SegundoApellido);
				cmd.Parameters.AddWithValue("correo", objUsuario.TC_Correo);
				cmd.Parameters.AddWithValue("telefono", objUsuario.TN_Telefono);
				cmd.Parameters.AddWithValue("clave", objUsuario.TC_Clave);
				cmd.Parameters.Add("Registrado", SqlDbType.Bit).Direction = ParameterDirection.Output;
				cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;
				cmd.CommandType = CommandType.StoredProcedure;

				cons.Open();
				cmd.ExecuteNonQuery();

				registrado = Convert.ToBoolean(cmd.Parameters["Registrado"].Value);
				mensaje = cmd.Parameters["Mensaje"].Value?.ToString() ?? "Error al registrar usuario";
			}

			ViewData["mensaje"] = mensaje;

			if (registrado)
			{
				return RedirectToAction("Inicio_Sesion", "Acceso");
			}
			else
			{
				return View();
			}
		}


		[HttpPost]
		public ActionResult Inicio_Sesion(Usuarios objUsuario)
		{
			if (objUsuario.TC_Correo == null || objUsuario.TC_Clave == null)
			{
				ViewData["mensaje"] = "Los campos son obligatorios";
				return View();
			}
			else
			{
				objUsuario.TC_Clave = ConvertirSha256(objUsuario.TC_Clave);

				using (SqlConnection cons = new SqlConnection(conn))
				{
					SqlCommand cmd = new SqlCommand("sp_ValidarUsuario", cons);
					cmd.Parameters.AddWithValue("correo", objUsuario.TC_Correo);
					cmd.Parameters.AddWithValue("clave", objUsuario.TC_Clave);
					cmd.CommandType = CommandType.StoredProcedure;
					cons.Open();
					objUsuario.TN_IdUsuario = Convert.ToInt32(cmd.ExecuteScalar().ToString());
					objUsuario.TN_IdRol = (Rol)ObtenerFKIDRol(objUsuario.TN_IdUsuario, cons);
				}
			}

			if (objUsuario.TN_IdUsuario != 0)
			{
				using (var context = new SOPORTEEntities3())
				{


					//validacion de rol administrador
					bool isValid = context.TBL_Usuario.Any(x => x.TC_Correo == objUsuario.TC_Correo &&
					x.TC_Clave == objUsuario.TC_Clave && x.TN_IdRol == 1);

					if (isValid)
					{
						FormsAuthentication.SetAuthCookie(objUsuario.TC_Correo, false);

						Session["usuario"] = objUsuario;
						return RedirectToAction("Index", "Home");
					}


					bool isValidEmpleado = context.TBL_Usuario.Any(x => x.TC_Correo == objUsuario.TC_Correo &&
					x.TC_Clave == objUsuario.TC_Clave && x.TN_IdRol == 2);


					if (isValidEmpleado)
					{

						FormsAuthentication.SetAuthCookie(objUsuario.TC_Correo, false);
						Session["usuario"] = objUsuario;

						return RedirectToAction("Index", "Home_Empleado");
					}
					ViewData["mensaje"] = "Usuario o contraseña incorrectos";

					return View();
				}
			}
			else
			{
				ViewData["mensaje"] = "Usuario o contraseña incorrectos";
				return View();
			}



		}//Inicio de sesion





		public static string ConvertirSha256(string texto)
		{
			StringBuilder sb = new StringBuilder();
			using (SHA256 hash = SHA256.Create())
			{
				Encoding enc = Encoding.UTF8;
				byte[] result = hash.ComputeHash(enc.GetBytes(texto));

				foreach (byte b in result)
					sb.Append(b.ToString("x2"));

			}
			return sb.ToString();
		}

		private int ObtenerFKIDRol(int idUsuario, SqlConnection connection)
		{
			string query = "SELECT TN_IdRol FROM TBL_Usuario WHERE TN_IdUsuario = @ID";
			using (SqlCommand cmd = new SqlCommand(query, connection))
			{
				cmd.Parameters.AddWithValue("@ID", idUsuario);
				object fkResult = cmd.ExecuteScalar();

				if (fkResult != null && fkResult != DBNull.Value)
				{
					return Convert.ToInt32(fkResult);
				}
				return 0; // O cualquier valor predeterminado si no se encuentra
			}
		}

	}//class
}//namespace

