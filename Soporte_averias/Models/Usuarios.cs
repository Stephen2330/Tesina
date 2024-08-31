using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
namespace Soporte_averias.Models
{
	public class Usuarios
	{

		public int TN_IdUsuario { get; set; }
		public Rol TN_IdRol { get; set; }

		[Required(ErrorMessage = "La cédula es obligatoria")]
		public Nullable<int> TN_Cedula { get; set; }

		[Required(ErrorMessage = "El nombre es obligatorio")]
		public string TC_Nombre { get; set; }

		[Required(ErrorMessage = "El primer apellido es obligatorio")]
		public string TC_PrimerApellido { get; set; }

		[Required(ErrorMessage = "El segundo apellido es obligatorio")]
		public string TC_SegundoApellido { get; set; }

		[Required(ErrorMessage = "El correo es obligatorio")]
		public string TC_Correo { get; set; }

		[Required(ErrorMessage = "El teléfono es obligatorio")]
		public Nullable<int> TN_Telefono { get; set; }

		[Required(ErrorMessage = "La contraseña es obligatoria")]
		public string TC_Clave { get; set; }


		[Required(ErrorMessage ="La confirmación de contraseña es obligatoria")]
		public string confirmar_clave { get; set; }
	}
}