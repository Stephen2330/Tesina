using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using iTextSharp.text;
using OfficeOpenXml.Style;
using Soporte_averias.Models;
using OfficeOpenXml;
using Soporte_averias.Permissions;
using System.Security.Cryptography;
using System.Text;

namespace Soporte_averias.Controllers
{

	[ValidarSesionAttribute]
	[PermisosRol(Rol.Administrador)]
	public class UsuarioController : Controller
    {
        private SOPORTEEntities3 db = new SOPORTEEntities3();

        // GET: Usuario
        public ActionResult Index(string searchText, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.PageNumber = pageNumber;
			IQueryable<TBL_Usuario> usuarios = db.TBL_Usuario.Include(u => u.TBL_Rol);

			if (!string.IsNullOrEmpty(searchText))
			{
				usuarios = usuarios.Where(u => u.TC_Nombre.Contains(searchText));
			}

			int totalItems = usuarios.Count();
			int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
			ViewBag.totalPages = totalPages;
			ViewBag.CurrentFilter = searchText;

			var usuariosOrdenados = usuarios.OrderBy(u => u.TC_Nombre);
			var usuariosPagina = usuariosOrdenados.Skip((pageNumber - 1) * pageSize).Take(pageSize);

			return View(usuariosPagina.ToList());
		}

        // GET: Usuario/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TBL_Usuario tBL_Usuario = db.TBL_Usuario.Find(id);
            if (tBL_Usuario == null)
            {
                return HttpNotFound();
            }
            return View(tBL_Usuario);
        }

		// GET: Usuario/Create
		public ActionResult Create()
		{
			var roles = db.TBL_Rol.OrderBy(r => r.TC_Nombre).ToList();
			roles.Insert(0, new TBL_Rol { TN_IdRol = 0, TC_Nombre = "Seleccione rol" });

			ViewBag.TN_IdRol = new SelectList(roles, "TN_IdRol", "TC_Nombre");
			return View();
		}

		// POST: Usuario/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "TN_IdUsuario,TN_IdRol,TN_Cedula,TC_Nombre,TC_PrimerApellido,TC_SegundoApellido,TC_Correo,TN_Telefono,TC_Clave")] TBL_Usuario tBL_Usuario)
		{
			if (ModelState.IsValid)
			{
				tBL_Usuario.TC_Clave = ConvertirSha256(tBL_Usuario.TC_Clave);



				db.TBL_Usuario.Add(tBL_Usuario);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			var roles = db.TBL_Rol.OrderBy(r => r.TC_Nombre).ToList();
			return View(tBL_Usuario);
		}
		// GET: Usuario/Edit/5
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			TBL_Usuario tBL_Usuario = db.TBL_Usuario.Find(id);
			if (tBL_Usuario == null)
			{
				return HttpNotFound();
			}

			// Obtener roles y establecer ViewBag.Roles
			var roles = db.TBL_Rol.OrderBy(r => r.TC_Nombre).ToList();
			roles.Insert(0, new TBL_Rol { TN_IdRol = 0, TC_Nombre = "Seleccione rol" });
			ViewBag.Roles = new SelectList(roles, "TN_IdRol", "TC_Nombre", tBL_Usuario.TN_IdRol);

			return View(tBL_Usuario);
		}

		// POST: Usuario/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "TN_IdUsuario,TN_IdRol,TN_Cedula,TC_Nombre,TC_PrimerApellido,TC_SegundoApellido,TC_Correo,TN_Telefono, TC_Clave")] TBL_Usuario tBL_Usuario)
		{
			if (ModelState.IsValid)
			{
				// Encontrar el usuario en la base de datos
				var usuarioExistente = db.TBL_Usuario.Find(tBL_Usuario.TN_IdUsuario);

				if (usuarioExistente != null)
				{
					// Si la contraseña ha cambiado, encriptarla
					if (!string.IsNullOrEmpty(tBL_Usuario.TC_Clave) && tBL_Usuario.TC_Clave != usuarioExistente.TC_Clave)
					{
						tBL_Usuario.TC_Clave = ConvertirSha256(tBL_Usuario.TC_Clave);
					}
					else
					{
						// Mantener la contraseña actual si no se ha modificado
						tBL_Usuario.TC_Clave = usuarioExistente.TC_Clave;
					}

					// Actualizar los campos que no son la contraseña
					db.Entry(usuarioExistente).CurrentValues.SetValues(tBL_Usuario);
					db.SaveChanges();
				}

				return RedirectToAction("Index");
			}
			ViewBag.TN_IdRol = new SelectList(db.TBL_Rol, "TN_IdRol", "TC_Nombre", tBL_Usuario.TN_IdRol);

			return View(tBL_Usuario);
		}

		public ActionResult ExportToPdf(string searchText, int? page)
		{
			int pageNumber = page ?? 1;

			var actividad = db.TBL_Usuario.AsQueryable();

			if (!string.IsNullOrEmpty(searchText))
			{
				actividad = actividad.Where(m => m.TC_Nombre.Contains(searchText));
			}
			actividad = actividad.OrderBy(m => m.TC_Nombre);
			var pagedActividad = actividad.ToList();

			// Crear el documento PDF
			Document pdfDoc = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
			MemoryStream memoryStream = new MemoryStream();
			PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
			pdfDoc.Open();

			// Estampar la fecha y hora en el pie de página
			string fechaHoraDescarga = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
			pdfDoc.Add(new Paragraph($"Informe generado\n{fechaHoraDescarga}", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));

			//// Crear una celda divisora con fondo gris y borde inferior
			//PdfPCell dividerCell = new PdfPCell(new Phrase(" "));
			//dividerCell.Colspan = 6; // Establece el número de columnas que la celda ocupará
			//dividerCell.BackgroundColor = new BaseColor(192, 192, 192); // Gris claro
			//dividerCell.Border = PdfPCell.BOTTOM_BORDER; // Agrega un borde inferior a la celda

			// Crear tabla en PDF
			PdfPTable pdfTable = new PdfPTable(7);
			pdfTable.WidthPercentage = 100;


			// Crear estilo para los encabezados (negrita y fondo gris)
			Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD);
			BaseColor headerBackgroundColor = new BaseColor(192, 192, 192); // Gris claro

			// Añadir encabezados a la tabla con el estilo
			PdfPCell headerCell;

			headerCell = new PdfPCell(new Phrase("Rol", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Cédula", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Nombre", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Primer apellido", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Segundo apellido", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Correo", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Teléfono", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);



			foreach (var item in pagedActividad)
			{
				pdfTable.AddCell(item.TBL_Rol.TC_Nombre.ToString());
				pdfTable.AddCell(item.TN_Cedula.ToString());
				pdfTable.AddCell(item.TC_Nombre.ToString());
				pdfTable.AddCell(item.TC_PrimerApellido.ToString());
				pdfTable.AddCell(item.TC_SegundoApellido.ToString());
				pdfTable.AddCell(item.TC_Correo.ToString());
				pdfTable.AddCell(item.TN_Telefono.ToString());


			}



			pdfDoc.Add(pdfTable);
			pdfDoc.Close();

			byte[] bytes = memoryStream.ToArray();
			memoryStream.Close();
			return File(bytes, "application/pdf", "Datos_usuarios.pdf");
		}

		//Crear ExportToExcel
		public ActionResult ExportToExcel(string searchText)
		{
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

			var actividad = db.TBL_Usuario.AsQueryable();

			if (!string.IsNullOrEmpty(searchText))
			{
				actividad = actividad.Where(m => m.TC_Nombre.ToString().Contains(searchText));
			}
			else
			{
				ViewData["Mensaje"] = "*No se encontraron datos*";
			}

			// Ordenar los datos por FECHA
			actividad = actividad.OrderBy(m => m.TC_Nombre);

			var data = actividad.ToList();

			// Crear el archivo Excel utilizando EPPlus
			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Usuarios");

				// Encabezados
				worksheet.Cells[1, 1].Value = "Rol";
				worksheet.Cells[1, 2].Value = "Cédula";
				worksheet.Cells[1, 3].Value = "Nombre";
				worksheet.Cells[1, 4].Value = "Primer apellido";
				worksheet.Cells[1, 5].Value = "Segundo apellido";
				worksheet.Cells[1, 6].Value = "Correo";
				worksheet.Cells[1, 7].Value = "Teléfono";


				// Aplicar formato a los encabezados
				using (var range = worksheet.Cells[1, 1, 1, 10])
				{
					range.Style.Font.Bold = true;
					range.Style.Fill.PatternType = ExcelFillStyle.Solid;
					range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
				}

				// Llenar el contenido de la tabla
				for (int i = 0; i < data.Count; i++)
				{
					worksheet.Cells[i + 2, 1].Value = data[i].TBL_Rol.TC_Nombre.ToString();
					worksheet.Cells[i + 2, 2].Value = data[i].TN_Cedula.ToString();
					worksheet.Cells[i + 2, 3].Value = data[i].TC_Nombre.ToString();
					worksheet.Cells[i + 2, 3].Value = data[i].TC_PrimerApellido.ToString();
					worksheet.Cells[i + 2, 3].Value = data[i].TC_SegundoApellido.ToString();
					worksheet.Cells[i + 2, 3].Value = data[i].TC_Correo.ToString();
					worksheet.Cells[i + 2, 3].Value = data[i].TN_Telefono.ToString();

				}

				// Configurar el ancho de las columnas automáticamente
				worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

				// Convertir el archivo Excel en un arreglo de bytes
				byte[] excelBytes = package.GetAsByteArray();

				// Descargar el archivo
				return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Datos_usuarios.xlsx");
			}
		}
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





		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }


}


