using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartFlow.Web.Pages.Coordinador.Reportes
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public IndexModel(SmartFlowContext context) => _context = context;

        // 🔹 Propiedades principales
        public int TotalUsuarios { get; set; }
        public int TotalReservas { get; set; }
        public int ReservasPendientes { get; set; }
        public int TotalSolicitudes { get; set; }

        // 🔹 Datos para gráficos
        public List<ReservasEstadoDTO> ReservasPorEstado { get; set; } = new();
        public List<UsuariosCarreraDTO> UsuariosPorCarrera { get; set; } = new();
        public List<ReservasMensualesDTO> ReservasMensuales { get; set; } = new();

        // 🔹 Filtros
        [BindProperty(SupportsGet = true)] public DateTime? FechaInicio { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? FechaFin { get; set; }
        [BindProperty(SupportsGet = true)] public string? Estado { get; set; }

        // DTOs internos
        public class ReservasEstadoDTO { public string Estado { get; set; } = ""; public int Cantidad { get; set; } }
        public class UsuariosCarreraDTO { public string Carrera { get; set; } = ""; public int Cantidad { get; set; } }
        public class ReservasMensualesDTO
        {
            public string Mes { get; set; } = "";
            public int Aprobadas { get; set; }
            public int Rechazadas { get; set; }
            public int Pendientes { get; set; }
        }

        // ===============================================================
        // 🔹 Cargar Reporte principal
        // ===============================================================
        public void OnGet()
        {
            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            if (coordId == null) { Response.Redirect("/Login/Login"); return; }

            var carreraCoord = _context.Usuarios
                .Where(u => u.Id == coordId)
                .Select(u => u.CarreraId)
                .FirstOrDefault();

            if (carreraCoord == null)
            {
                TotalUsuarios = TotalReservas = ReservasPendientes = TotalSolicitudes = 0;
                return;
            }

            // Filtro base: sólo su carrera
            var reservasQuery = _context.Reservas
                .Include(r => r.Usuario)
                .Where(r => r.Usuario.CarreraId == carreraCoord)
                .AsQueryable();

            if (FechaInicio.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaInicio >= FechaInicio.Value);
            if (FechaFin.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaFin <= FechaFin.Value);
            if (!string.IsNullOrEmpty(Estado))
                reservasQuery = reservasQuery.Where(r => r.Estado == Estado);

            // Tarjetas
            TotalUsuarios = _context.Usuarios.Count(u => u.CarreraId == carreraCoord);
            TotalReservas = reservasQuery.Count();
            ReservasPendientes = reservasQuery.Count(r => r.Estado == "Pendiente");
            TotalSolicitudes = _context.Solicitudes.Count(s => s.Usuario.CarreraId == carreraCoord);

            // Gráficos
            ReservasPorEstado = reservasQuery
                .GroupBy(r => r.Estado)
                .Select(g => new ReservasEstadoDTO { Estado = g.Key, Cantidad = g.Count() })
                .ToList();

            UsuariosPorCarrera = new List<UsuariosCarreraDTO>
            {
                new UsuariosCarreraDTO
                {
                    Carrera = _context.Carreras.FirstOrDefault(c => c.Id == carreraCoord)?.Nombre ?? "Sin carrera",
                    Cantidad = TotalUsuarios
                }
            };

            ReservasMensuales = reservasQuery
                .AsEnumerable()
                .GroupBy(r => new { r.FechaInicio.Year, r.FechaInicio.Month })
                .Select(g => new ReservasMensualesDTO
                {
                    Mes = $"{g.Key.Month:D2}/{g.Key.Year}",
                    Aprobadas = g.Count(r => r.Estado == "Aprobada"),
                    Rechazadas = g.Count(r => r.Estado == "Rechazada"),
                    Pendientes = g.Count(r => r.Estado == "Pendiente")
                })
                .OrderBy(g => g.Mes)
                .ToList();
        }

        public double PorcentajeAprobadas =>
            TotalReservas == 0 ? 0 :
            Math.Round((double)_context.Reservas.Count(r => r.Estado == "Aprobada") / TotalReservas * 100, 1);

        public double PorcentajeRechazadas =>
            TotalReservas == 0 ? 0 :
            Math.Round((double)_context.Reservas.Count(r => r.Estado == "Rechazada") / TotalReservas * 100, 1);

        // ===============================================================
        // 📊 EXPORTAR A EXCEL
        // ===============================================================
        public IActionResult OnGetExportarExcel()
        {
            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            var carreraCoord = _context.Usuarios.Where(u => u.Id == coordId).Select(u => u.CarreraId).FirstOrDefault();
            if (carreraCoord == null) return RedirectToPage();

            var reservasQuery = _context.Reservas
                .Include(r => r.Usuario)
                .Where(r => r.Usuario.CarreraId == carreraCoord)
                .AsQueryable();

            if (FechaInicio.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaInicio >= FechaInicio.Value);
            if (FechaFin.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaFin <= FechaFin.Value);
            if (!string.IsNullOrEmpty(Estado))
                reservasQuery = reservasQuery.Where(r => r.Estado == Estado);

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte Coordinador");
            int fila = 1;
            ws.Cell(fila, 1).Value = "📊 Reporte Coordinador - SmartFlow";
            ws.Cell(fila, 1).Style.Font.Bold = true;
            ws.Cell(fila, 1).Style.Font.FontSize = 16;
            fila += 2;

            ws.Cell(fila, 1).Value = "Total Usuarios";
            ws.Cell(fila, 2).Value = _context.Usuarios.Count(u => u.CarreraId == carreraCoord); fila++;
            ws.Cell(fila, 1).Value = "Total Reservas (filtradas)";
            ws.Cell(fila, 2).Value = reservasQuery.Count(); fila++;
            ws.Cell(fila, 1).Value = "Reservas Pendientes";
            ws.Cell(fila, 2).Value = reservasQuery.Count(r => r.Estado == "Pendiente"); fila++;
            ws.Cell(fila, 1).Value = "Total Solicitudes";
            ws.Cell(fila, 2).Value = _context.Solicitudes.Count(s => s.Usuario.CarreraId == carreraCoord); fila += 2;

            ws.Cell(fila, 1).Value = "Reservas por Estado";
            ws.Cell(fila, 1).Style.Font.Bold = true; fila++;
            ws.Cell(fila, 1).Value = "Estado"; ws.Cell(fila, 2).Value = "Cantidad"; ws.Row(fila).Style.Font.Bold = true; fila++;

            var estados = reservasQuery.GroupBy(r => r.Estado).Select(g => new { g.Key, Cantidad = g.Count() }).ToList();
            foreach (var e in estados)
            {
                ws.Cell(fila, 1).Value = e.Key;
                ws.Cell(fila, 2).Value = e.Cantidad;
                fila++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Reporte_Coordinador.xlsx");
        }

        // ===============================================================
        // 📄 EXPORTAR A PDF
        // ===============================================================
        public IActionResult OnGetExportarPdf()
        {
            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            var carreraCoord = _context.Usuarios.Where(u => u.Id == coordId).Select(u => u.CarreraId).FirstOrDefault();
            if (carreraCoord == null) return RedirectToPage();

            var reservasQuery = _context.Reservas
                .Include(r => r.Usuario)
                .Where(r => r.Usuario.CarreraId == carreraCoord)
                .AsQueryable();

            if (FechaInicio.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaInicio >= FechaInicio.Value);
            if (FechaFin.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaFin <= FechaFin.Value);
            if (!string.IsNullOrEmpty(Estado))
                reservasQuery = reservasQuery.Where(r => r.Estado == Estado);

            var stream = new MemoryStream();
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Text("📊 Reporte de Coordinador - SmartFlow").Bold().FontSize(18).AlignCenter();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        col.Item().PaddingVertical(10);
                        col.Item().Text("Resumen General").FontSize(14).Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            t.Cell().Text("Total Usuarios"); t.Cell().Text(_context.Usuarios.Count(u => u.CarreraId == carreraCoord).ToString());
                            t.Cell().Text("Total Reservas"); t.Cell().Text(reservasQuery.Count().ToString());
                            t.Cell().Text("Pendientes"); t.Cell().Text(reservasQuery.Count(r => r.Estado == "Pendiente").ToString());
                            t.Cell().Text("Total Solicitudes"); t.Cell().Text(_context.Solicitudes.Count(s => s.Usuario.CarreraId == carreraCoord).ToString());
                        });

                        col.Item().PaddingVertical(15);
                        col.Item().Text("Reservas por Estado").FontSize(14).Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            t.Header(h => { h.Cell().Text("Estado").Bold(); h.Cell().Text("Cantidad").Bold(); });
                            var estados = reservasQuery.GroupBy(r => r.Estado).Select(g => new { g.Key, Cantidad = g.Count() }).ToList();
                            foreach (var e in estados)
                            {
                                t.Cell().Text(e.Key);
                                t.Cell().Text(e.Cantidad.ToString());
                            }
                        });
                    });
                    page.Footer().AlignCenter().Text("SmartFlow © 2025 - Coordinador").FontSize(9);
                });
            });

            document.GeneratePdf(stream);
            return File(stream.ToArray(), "application/pdf", "Reporte_Coordinador.pdf");
        }
    }
}
