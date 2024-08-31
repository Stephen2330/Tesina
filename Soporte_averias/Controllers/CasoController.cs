using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using iTextSharp.text.pdf;
using iTextSharp.text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PagedList;
using Soporte_averias.Models;
using Soporte_averias.Permissions;

namespace Soporte_averias.Controllers
{
	[ValidarSesionAttribute]
	[PermisosRol(Rol.Administrador)]
	public class CasoController : Controller
	{
		private SOPORTEEntities3 db = new SOPORTEEntities3();

		// GET: Caso/Index
		// GET: Caso/Index
		public ActionResult Index(int? page, int? selectEstado)
		{
			ViewBag.Title = "Lista de casos";
			ConfigureSelectListForEstado();

			int pageSize = 10;
			int pageNumber = (page ?? 1);

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

			// Obtener el total de casos filtrados
			var totalCasos = tBL_CasoQuery.Count();

			// Aplicar la paginación y ordenar los resultados
			var casosPaginas = tBL_CasoQuery
				.OrderBy(c => c.TBL_Cliente.TC_Nombre)
				.ToPagedList(pageNumber, pageSize);

			// Configurar el total de páginas en ViewBag
			ViewBag.TotalPages = (int)Math.Ceiling((double)totalCasos / pageSize);
			ViewBag.PageNumber = pageNumber;

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

		// GET: Caso/Create
		public ActionResult Create()
		{
			ConfigureViewBagForCreateEdit();
			return View();
		}

		// POST: Caso/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "TN_IdCaso,TN_IdMunicipalidad,TN_IdCliente,TN_IdProducto,TN_IdDescripcionCaso,TN_IdPrioridadCaso,TN_IdEstadoCaso,TN_IdUsuario")] TBL_Caso tBL_Caso)
		{
			if (ModelState.IsValid)
			{
				tBL_Caso.TD_FechaCreacion = DateTime.Now;
				db.TBL_Caso.Add(tBL_Caso);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			// Reasignar datos a ViewBag si el modelo no es válido
			ConfigureViewBagForCreateEdit(tBL_Caso);
			return View(tBL_Caso);
		}

		// GET: Caso/Edit/5
		public ActionResult Edit(int? id)
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

			// Configurar ViewBag para la vista de edición
			ConfigureViewBagForCreateEdit(tBL_Caso);

			return View(tBL_Caso);
		}

		// POST: Caso/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "TN_IdCaso,TN_IdMunicipalidad,TN_IdCliente,TN_IdProducto,TN_IdDescripcionCaso,TN_IdPrioridadCaso,TN_IdEstadoCaso,TN_IdUsuario")] TBL_Caso tBL_Caso)
		{
			if (ModelState.IsValid)
			{
				var existingCase = db.TBL_Caso.Find(tBL_Caso.TN_IdCaso);
				if (existingCase == null)
				{
					return HttpNotFound();
				}

				// Actualizar los campos necesarios
				UpdateCase(existingCase, tBL_Caso);

				db.Entry(existingCase).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			// Reasignar datos a ViewBag si el modelo no es válido
			ConfigureViewBagForCreateEdit(tBL_Caso);
			return View(tBL_Caso);
		}

		private void ConfigureSelectListForEstado()
		{
			var estados = db.TBL_EstadoCaso.OrderBy(e => e.TC_Nombre).ToList();
			ViewBag.Estados = new SelectList(estados, "TN_IdEstadoCaso", "TC_Nombre");
		}

		private void ConfigureViewBagForCreateEdit(TBL_Caso tBL_Caso = null)
		{

			var usuarios = db.TBL_Usuario.OrderBy(u => u.TC_Nombre).ToList()
				.Select(u => new SelectListItem
				{
					Value = u.TN_IdUsuario.ToString(),
					Text = $"{u.TC_Nombre} {u.TC_PrimerApellido}"
				}).ToList();
			usuarios.Insert(0, new SelectListItem { Value = "", Text = "Seleccione usuario" });
			ViewBag.TN_IdUsuario = new SelectList(usuarios, "Value", "Text", tBL_Caso?.TN_IdUsuario);

			var clientes = db.TBL_Cliente.OrderBy(u => u.TC_Nombre).ToList()
	.Select(u => new SelectListItem
	{
		Value = u.TN_IdCliente.ToString(),
		Text = $"{u.TC_Nombre}"
	}).ToList();
			clientes.Insert(0, new SelectListItem { Value = "", Text = "Seleccione cliente" });
			ViewBag.TN_IdCliente = new SelectList(clientes, "Value", "Text", tBL_Caso?.TN_IdCliente);

			var descripciones = db.TBL_DescripcionCaso.OrderBy(u => u.TC_Descripcion).ToList()
.Select(u => new SelectListItem
{
Value = u.TN_IdDescripcionCaso.ToString(),
Text = $"{u.TC_Descripcion}"
}).ToList();
			descripciones.Insert(0, new SelectListItem { Value = "", Text = "Seleccione descripción" });
			ViewBag.TN_IdDescripcionCaso = new SelectList(descripciones, "Value", "Text", tBL_Caso?.TN_IdDescripcionCaso);

			var estados = db.TBL_EstadoCaso.OrderBy(u => u.TC_Nombre).ToList()
.Select(u => new SelectListItem
{
Value = u.TN_IdEstadoCaso.ToString(),
Text = $"{u.TC_Nombre}"
}).ToList();
			estados.Insert(0, new SelectListItem { Value = "", Text = "Seleccione estado" });
			ViewBag.TN_IdEstadoCaso = new SelectList(estados, "Value", "Text", tBL_Caso?.TN_IdEstadoCaso);

			var municipalidades = db.TBL_Municipalidad.OrderBy(u => u.TC_Nombre).ToList()
.Select(u => new SelectListItem
{
	Value = u.TN_IdMunicipalidad.ToString(),
	Text = $"{u.TC_Nombre}"
}).ToList();
			municipalidades.Insert(0, new SelectListItem { Value = "", Text = "Seleccione municipalidad" });
			ViewBag.TN_IdMunicipalidad = new SelectList(municipalidades, "Value", "Text", tBL_Caso?.TN_IdMunicipalidad);


			var prioridades = db.TBL_PrioridadCaso.OrderBy(u => u.TC_Nombre).ToList()
			.Select(u => new SelectListItem
			{
				Value = u.TN_IdPrioridadCaso.ToString(),
				Text = $"{u.TC_Nombre}"
			}).ToList();
			prioridades.Insert(0, new SelectListItem { Value = "", Text = "Seleccione prioridad" });
			ViewBag.TN_IdPrioridadCaso = new SelectList(prioridades, "Value", "Text", tBL_Caso?.TN_IdPrioridadCaso);

			var productos = db.TBL_Producto.OrderBy(u => u.TC_Nombre).ToList()
			.Select(u => new SelectListItem
			{
				Value = u.TN_IdProducto.ToString(),
				Text = $"{u.TC_Nombre}"
			}).ToList();
			productos.Insert(0, new SelectListItem { Value = "", Text = "Seleccione producto" });
			ViewBag.TN_IdProducto = new SelectList(productos, "Value", "Text", tBL_Caso?.TN_IdProducto);



		}

		private void UpdateCase(TBL_Caso existingCase, TBL_Caso updatedCase)
		{
			existingCase.TN_IdMunicipalidad = updatedCase.TN_IdMunicipalidad;
			existingCase.TN_IdCliente = updatedCase.TN_IdCliente;
			existingCase.TN_IdProducto = updatedCase.TN_IdProducto;
			existingCase.TN_IdDescripcionCaso = updatedCase.TN_IdDescripcionCaso;
			existingCase.TN_IdPrioridadCaso = updatedCase.TN_IdPrioridadCaso;
			existingCase.TN_IdUsuario = updatedCase.TN_IdUsuario;

			// Obtener los ID de los estados
			var estadoCerrado = db.TBL_EstadoCaso.FirstOrDefault(e => e.TC_Nombre == "Cerrados")?.TN_IdEstadoCaso;
			var estadoResolviendose = db.TBL_EstadoCaso.FirstOrDefault(e => e.TC_Nombre == "Caso resolviéndose")?.TN_IdEstadoCaso;

			// Si el caso está en "Cerrado" y se actualiza a otro estado, limpiar la fecha de cierre
			if (existingCase.TN_IdEstadoCaso == estadoCerrado && updatedCase.TN_IdEstadoCaso != estadoCerrado)
			{
				existingCase.TD_FechaCierre = null;
			}

			// Si el caso se actualiza al estado "Cerrado", establecer la fecha de cierre
			if (updatedCase.TN_IdEstadoCaso == estadoCerrado)
			{
				existingCase.TD_FechaCierre = DateTime.Now;
			}

			// Actualizar el estado del caso a "Caso resolviéndose" si el nuevo estado no es "Cerrado"
			if (updatedCase.TN_IdEstadoCaso != estadoCerrado && estadoResolviendose != null)
			{
				existingCase.TN_IdEstadoCaso = estadoResolviendose.Value;
			}
			else
			{
				// Si el estado no es "Cerrado" pero no es "Caso resolviéndose", mantener el estado actual
				existingCase.TN_IdEstadoCaso = updatedCase.TN_IdEstadoCaso;
			}
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
			Document pdfDoc = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f);
			MemoryStream memoryStream = new MemoryStream();
			PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
			pdfDoc.Open();

			// Estampar la fecha y hora en el pie de página
			string fechaHoraDescarga = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
			pdfDoc.Add(new Paragraph($"Informe generado\n{fechaHoraDescarga}", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));

			// Crear tabla en PDF
			PdfPTable pdfTable = new PdfPTable(9);
			pdfTable.WidthPercentage = 100;

			float[] columnWidth = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
			pdfTable.SetWidths(columnWidth);

			// Crear estilo para los encabezados (negrita y fondo gris)
			Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD);
			BaseColor headerBackgroundColor = new BaseColor(192, 192, 192); // Gris claro

			// Añadir encabezados a la tabla con el estilo
			string[] headers = { "Fecha creación", "Municipalidad", "Cliente", "Producto", "Descripción", "Prioridad", "Estado", "Empleado asignado", "Fecha cierre caso" };

			foreach (var header in headers)
			{
				PdfPCell headerCell = new PdfPCell(new Phrase(header, headerFont))
				{
					BackgroundColor = headerBackgroundColor,
					HorizontalAlignment = Element.ALIGN_CENTER,
					NoWrap = false
				};
				pdfTable.AddCell(headerCell);
			}



			// Añadir filas de datos
			foreach (var item in pagedActividad)
			{
				PdfPCell cell;

				cell = new PdfPCell(new Phrase(item.TD_FechaCreacion.ToString()));
				cell.NoWrap = false;
				pdfTable.AddCell(cell);

				cell = new PdfPCell(new Phrase(item.TBL_Municipalidad.TC_Nombre.ToString()));
				cell.NoWrap = false;
				pdfTable.AddCell(cell);

				cell = new PdfPCell(new Phrase(item.TBL_Cliente.TC_Nombre.ToString()));
				cell.NoWrap = false;
				pdfTable.AddCell(cell);

				cell = new PdfPCell(new Phrase(item.TBL_Producto.TC_Nombre.ToString()));
				cell.NoWrap = false;
				pdfTable.AddCell(cell);

				cell = new PdfPCell(new Phrase(item.TBL_DescripcionCaso.TC_Descripcion.ToString()));
				cell.NoWrap = false;
				pdfTable.AddCell(cell);

				cell = new PdfPCell(new Phrase(item.TBL_PrioridadCaso.TC_Nombre.ToString()));
				cell.NoWrap = false;
				pdfTable.AddCell(cell);

				cell = new PdfPCell(new Phrase(item.TBL_EstadoCaso.TC_Nombre.ToString()));
				cell.NoWrap = false;
				pdfTable.AddCell(cell);

				cell = new PdfPCell(new Phrase(item.TBL_Usuario.TC_Nombre.ToString() + " " + item.TBL_Usuario.TC_PrimerApellido.ToString()));
				cell.NoWrap = false;
				pdfTable.AddCell(cell);

				// Verificar si la fecha de cierre es nula o vacía
				string fechaCierre = item.TD_FechaCierre.HasValue ? item.TD_FechaCierre.Value.ToString() : "Caso resolviéndose";
				cell = new PdfPCell(new Phrase(fechaCierre));
				cell.NoWrap = false;
				pdfTable.AddCell(cell);
			}

			pdfDoc.Add(pdfTable);
			pdfDoc.Close();

			byte[] bytes = memoryStream.ToArray();
			memoryStream.Close();
			return File(bytes, "application/pdf", "Datos_casos.pdf");
		}



		// Exportar a Excel
		public ActionResult ExportToExcel()
		{
			var casos = db.TBL_Caso
				.Include(c => c.TBL_Cliente)
				.Include(c => c.TBL_DescripcionCaso)
				.Include(c => c.TBL_EstadoCaso)
				.Include(c => c.TBL_Municipalidad)
				.Include(c => c.TBL_PrioridadCaso)
				.Include(c => c.TBL_Producto)
				.Include(c => c.TBL_Usuario)
				.ToList();

			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Casos");

				// Establecer encabezados
				worksheet.Cells[1, 1].Value = "Cliente";
				worksheet.Cells[1, 2].Value = "Descripción";
				worksheet.Cells[1, 3].Value = "Empleado";
				worksheet.Cells[1, 4].Value = "Estado";
				worksheet.Cells[1, 5].Value = "Municipalidad";
				worksheet.Cells[1, 6].Value = "Prioridad";
				worksheet.Cells[1, 7].Value = "Producto";
				worksheet.Cells[1, 8].Value = "Fecha creación";
				worksheet.Cells[1, 9].Value = "Fecha cierre";

				// Establecer formato
				worksheet.Cells["A1:K1"].Style.Font.Bold = true;
				worksheet.Cells["A1:K1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
				worksheet.Cells["A1:K1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

				// Agregar datos
				var row = 2;
				foreach (var caso in casos)
				{
					worksheet.Cells[row, 1].Value = caso.TBL_Cliente.TC_Nombre;
					worksheet.Cells[row, 2].Value = caso.TBL_DescripcionCaso.TC_Descripcion;
					worksheet.Cells[row, 3].Value = $"{caso.TBL_Usuario.TC_Nombre} {caso.TBL_Usuario.TC_PrimerApellido}";
					worksheet.Cells[row, 4].Value = caso.TBL_EstadoCaso.TC_Nombre;
					worksheet.Cells[row, 5].Value = caso.TBL_Municipalidad.TC_Nombre;
					worksheet.Cells[row, 6].Value = caso.TBL_PrioridadCaso.TC_Nombre;
					worksheet.Cells[row, 7].Value = caso.TBL_Producto.TC_Nombre;
					worksheet.Cells[row, 8].Value = caso.TD_FechaCreacion.HasValue ? caso.TD_FechaCreacion.Value.ToString("dd/MM/yyyy") : "";
					worksheet.Cells[row, 9].Value = caso.TD_FechaCierre.HasValue ? caso.TD_FechaCierre.Value.ToString("dd/MM/yyyy") : "Caso resolviéndose";
					row++;
				}

				// Ajustar columnas
				worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

				// Guardar en memoria
				var stream = new MemoryStream();
				package.SaveAs(stream);
				stream.Position = 0;

				return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Casos.xlsx");
			}
		}
	}
}
