using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using PagedList;
using Soporte_averias.Models;
using OfficeOpenXml;
using Soporte_averias.Permissions;

namespace Soporte_averias.Controllers.Empleado
{
	[ValidarSesionAttribute]
	[PermisosRol(Rol.Empleado)]
	public class Caso_EmpleadoController : Controller
    {
		private SOPORTEEntities3 db = new SOPORTEEntities3();

		public ActionResult Index(int? page, int? selectEstado)
		{
			ViewBag.Title = "Lista de casos";

			// Obtener la lista de estados de caso
			var estados = db.TBL_EstadoCaso.OrderBy(e => e.TC_Nombre).ToList();
			var estadosSelectList = new SelectList(estados, "TN_IdEstadoCaso", "TC_Nombre");

			ViewBag.Estados = estadosSelectList;

			int pageSize = 10;
			int pageNumber = (page ?? 1);
			ViewBag.PageNumber = pageNumber;

			IQueryable<TBL_Caso> tBL_CasoQuery = db.TBL_Caso
				.Include(t => t.TBL_Cliente)
				.Include(t => t.TBL_DescripcionCaso)
				.Include(t => t.TBL_EstadoCaso)

				.Include(t => t.TBL_Municipalidad)

				.Include(t => t.TBL_PrioridadCaso)
				.Include(t => t.TBL_Producto)
				.Include(t => t.TBL_Usuario);

			if (selectEstado.HasValue)
			{
				tBL_CasoQuery = tBL_CasoQuery.Where(c => c.TBL_EstadoCaso.TN_IdEstadoCaso == selectEstado);
			}

			int totalItems = tBL_CasoQuery.Count(); // Cant. elementos totales
			int totalPages = (int)Math.Ceiling((double)totalItems / pageSize); // Cant. total de páginas
			ViewBag.TotalPages = totalPages;

			// Obtener datos de los empleados con nombre y primer apellido


			var casosPaginas = tBL_CasoQuery.OrderBy(c => c.TBL_Cliente.TC_Nombre)
											.ToPagedList(pageNumber, pageSize);

			return View(casosPaginas);
		}

		// GET: Caso/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			TBL_Caso tBL_Caso = db.TBL_Caso.Find(id);
			if (tBL_Caso == null)
			{
				return HttpNotFound();
			}
			return View(tBL_Caso);
		}

		public ActionResult ExportToPdf(int? selectEstado, int? page)
		{
			int pageNumber = page ?? 1;

			var actividad = db.TBL_Caso.AsQueryable();

			if (selectEstado.HasValue)
			{
				actividad = actividad.Where(m => m.TBL_EstadoCaso.TN_IdEstadoCaso.Equals(selectEstado));
			}
			actividad = actividad.OrderBy(m => m.TN_IdEstadoCaso);
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
			PdfPTable pdfTable = new PdfPTable(11);
			pdfTable.WidthPercentage = 100;


			// Crear estilo para los encabezados (negrita y fondo gris)
			Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD);
			BaseColor headerBackgroundColor = new BaseColor(192, 192, 192); // Gris claro

			// Añadir encabezados a la tabla con el estilo
			PdfPCell headerCell;

			headerCell = new PdfPCell(new Phrase("Período de garantía", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Fecha creación caso", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Empleado asignado", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Municipalidad", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Cliente", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Producto", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Descripción caso", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Prioridad caso", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Estado caso", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Quien reporta caso", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			headerCell = new PdfPCell(new Phrase("Fecha cierre caso", headerFont));
			headerCell.BackgroundColor = headerBackgroundColor;
			headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
			pdfTable.AddCell(headerCell);

			foreach (var item in pagedActividad)
			{
				pdfTable.AddCell(item.TBL_Municipalidad.TC_Nombre.ToString());
				pdfTable.AddCell(item.TBL_Cliente.TC_Nombre.ToString());
				pdfTable.AddCell(item.TBL_Producto.TC_Nombre.ToString());
				pdfTable.AddCell(item.TBL_DescripcionCaso.TC_Descripcion.ToString());
				pdfTable.AddCell(item.TBL_PrioridadCaso.TC_Nombre.ToString());
				pdfTable.AddCell(item.TBL_EstadoCaso.TC_Nombre.ToString());
				pdfTable.AddCell(item.TBL_Usuario.TC_Nombre.ToString() + " " + item.TBL_Usuario.TC_PrimerApellido.ToString());


			}



			pdfDoc.Add(pdfTable);
			pdfDoc.Close();

			byte[] bytes = memoryStream.ToArray();
			memoryStream.Close();
			return File(bytes, "application/pdf", "Datos_casos.pdf");
		}

		//Crear ExportToExcel
		public ActionResult ExportToExcel(int? selectEstado)
		{
			ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

			var actividad = db.TBL_Caso.AsQueryable();

			if (selectEstado.HasValue)
			{
				actividad = actividad.Where(m => m.TBL_EstadoCaso.TN_IdEstadoCaso.Equals(selectEstado));
			}

			else
			{
				ViewData["Mensaje"] = "*No se encontraron datos*";
			}

			// Ordenar los datos por FECHA
			actividad = actividad.OrderBy(m => m.TN_IdEstadoCaso);
			var data = actividad.ToList();

			// Crear el archivo Excel utilizando EPPlus
			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Casos");

				// Encabezados
				worksheet.Cells[1, 1].Value = "Período de garantía";
				worksheet.Cells[1, 2].Value = "Fecha creación caso";
				worksheet.Cells[1, 3].Value = "Empleado asignado";
				worksheet.Cells[1, 4].Value = "Municipalidad";
				worksheet.Cells[1, 5].Value = "Cliente";
				worksheet.Cells[1, 6].Value = "Producto";
				worksheet.Cells[1, 7].Value = "Descripción caso";
				worksheet.Cells[1, 8].Value = "Prioridad caso";
				worksheet.Cells[1, 9].Value = "Estado caso";
				worksheet.Cells[1, 10].Value = "Quien reporta caso";
				worksheet.Cells[1, 11].Value = "Fecha cierre caso";


				// Aplicar formato a los encabezados
				using (var range = worksheet.Cells[1, 1, 1, 11])
				{
					range.Style.Font.Bold = true;
					range.Style.Fill.PatternType = ExcelFillStyle.Solid;
					range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
				}

				// Llenar el contenido de la tabla
				for (int i = 0; i < data.Count; i++)
				{
					worksheet.Cells[i + 2, 4].Value = data[i].TBL_Municipalidad.TC_Nombre.ToString();
					worksheet.Cells[i + 2, 5].Value = data[i].TBL_Cliente.TC_Nombre.ToString();
					worksheet.Cells[i + 2, 6].Value = data[i].TBL_Producto.TC_Nombre.ToString();
					worksheet.Cells[i + 2, 7].Value = data[i].TBL_DescripcionCaso.TC_Descripcion.ToString();
					worksheet.Cells[i + 2, 8].Value = data[i].TBL_PrioridadCaso.TC_Nombre.ToString();
					worksheet.Cells[i + 2, 9].Value = data[i].TBL_EstadoCaso.TC_Nombre.ToString();
					worksheet.Cells[i + 2, 10].Value = data[i].TBL_Usuario.TC_Nombre.ToString() + " " + data[i].TBL_Usuario.TC_PrimerApellido.ToString();

				}

				// Configurar el ancho de las columnas automáticamente
				worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

				// Convertir el archivo Excel en un arreglo de bytes
				byte[] excelBytes = package.GetAsByteArray();

				// Descargar el archivo
				return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Datos_casos.xlsx");
			}
		}

	}
}