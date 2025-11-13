using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SmartFlow.Web.Pages.Admin.Reportes
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        // 🔹 Propiedades para las tarjetas
        public int TotalUsuarios { get; set; }
        public int TotalReservas { get; set; }
        public int ReservasPendientes { get; set; }
        public int TotalSolicitudes { get; set; }

        // 🔹 Datos para los gráficos
        public List<ReservasEstadoDTO> ReservasPorEstado { get; set; } = new();
        public List<UsuariosCarreraDTO> UsuariosPorCarrera { get; set; } = new();
        public List<ReservasMensualesDTO> ReservasMensuales { get; set; } = new();

        // 🔹 Filtros (vienen del formulario)
        [BindProperty(SupportsGet = true)] public DateTime? FechaInicio { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? FechaFin { get; set; }
        [BindProperty(SupportsGet = true)] public string? Estado { get; set; }
        [BindProperty(SupportsGet = true)] public int? CarreraId { get; set; }

        // 🔹 Para desplegar carreras en el filtro
        public List<Carrera> CarrerasDisponibles { get; set; } = new();
        public class ReservasMensualesDTO
        {
            public string Mes { get; set; } = "";
            public int Aprobadas { get; set; }
            public int Rechazadas { get; set; }
            public int Pendientes { get; set; }
        }

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }


        public void OnGet()
        {
            // ============================================================
            // 4️⃣ GRÁFICO: Reservas mensuales comparativas
            // ============================================================
            // ============================================================
            // 4️⃣ GRÁFICO: Reservas mensuales comparativas
            // ============================================================
            var reservasFiltradas = _context.Reservas.AsQueryable();

            if (FechaInicio.HasValue)
                reservasFiltradas = reservasFiltradas.Where(r => r.FechaInicio >= FechaInicio.Value);
            if (FechaFin.HasValue)
                reservasFiltradas = reservasFiltradas.Where(r => r.FechaFin <= FechaFin.Value);

            // 👇 Se trae a memoria después del agrupamiento
            ReservasMensuales = reservasFiltradas
                .AsEnumerable() // 💡 convierte la consulta en memoria para permitir string.Format
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


            CarrerasDisponibles = _context.Carreras.OrderBy(c => c.Nombre).ToList();

            var reservasQuery = _context.Reservas.AsQueryable();

            // 🔹 Aplicar filtros dinámicos
            if (FechaInicio.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaInicio >= FechaInicio.Value);

            if (FechaFin.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaFin <= FechaFin.Value);

            if (!string.IsNullOrWhiteSpace(Estado))
                reservasQuery = reservasQuery.Where(r => r.Estado == Estado);

            if (CarreraId.HasValue)
                reservasQuery = reservasQuery.Where(r => r.Usuario.CarreraId == CarreraId.Value);

            // 🔹 Tarjetas principales
            TotalUsuarios = _context.Usuarios.Count();
            TotalReservas = reservasQuery.Count();
            ReservasPendientes = reservasQuery.Count(r => r.Estado == "Pendiente");
            TotalSolicitudes = _context.Solicitudes.Count();

            // 🔹 Gráficos
            ReservasPorEstado = reservasQuery
                .GroupBy(r => r.Estado)
                .Select(g => new ReservasEstadoDTO
                {
                    Estado = string.IsNullOrEmpty(g.Key) ? "Sin estado" : g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            UsuariosPorCarrera = (from u in _context.Usuarios
                                  join c in _context.Carreras on u.CarreraId equals c.Id into carreraJoin
                                  from c in carreraJoin.DefaultIfEmpty()
                                  group u by (c != null ? c.Nombre : "Sin carrera") into g
                                  select new UsuariosCarreraDTO
                                  {
                                      Carrera = g.Key,
                                      Cantidad = g.Count()
                                  }).ToList();
        }

        public double PorcentajeAprobadas => TotalReservas == 0 ? 0 :
        Math.Round((double)_context.Reservas.Count(r => r.Estado == "Aprobada") / TotalReservas * 100, 1);

        public double PorcentajeRechazadas => TotalReservas == 0 ? 0 :
        Math.Round((double)_context.Reservas.Count(r => r.Estado == "Rechazada") / TotalReservas * 100, 1);

        // ============================================================
        // 📄 DTOs para los gráficos
        // ============================================================
        public class ReservasEstadoDTO
        {
            public string Estado { get; set; } = "";
            public int Cantidad { get; set; }
        }

        public class UsuariosCarreraDTO
        {
            public string Carrera { get; set; } = "";
            public int Cantidad { get; set; }
        }

        // ============================================================
        // 📤 EXPORTAR A EXCEL (con filtros activos)
        // ============================================================
        public IActionResult OnGetExportarExcel()
        {
            var reservasQuery = _context.Reservas.AsQueryable();

            // 🔹 Aplicar filtros si existen
            if (FechaInicio.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaInicio >= FechaInicio.Value);
            if (FechaFin.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaFin <= FechaFin.Value);
            if (!string.IsNullOrWhiteSpace(Estado))
                reservasQuery = reservasQuery.Where(r => r.Estado == Estado);
            if (CarreraId.HasValue)
                reservasQuery = reservasQuery.Where(r => r.Usuario.CarreraId == CarreraId.Value);

            // 🔹 Recalcular métricas filtradas
            TotalUsuarios = _context.Usuarios.Count();
            TotalReservas = reservasQuery.Count();
            ReservasPendientes = reservasQuery.Count(r => r.Estado == "Pendiente");
            TotalSolicitudes = _context.Solicitudes.Count();

            ReservasPorEstado = reservasQuery
                .GroupBy(r => r.Estado)
                .Select(g => new ReservasEstadoDTO
                {
                    Estado = string.IsNullOrEmpty(g.Key) ? "Sin estado" : g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            UsuariosPorCarrera = (from u in _context.Usuarios
                                  join c in _context.Carreras on u.CarreraId equals c.Id into carreraJoin
                                  from c in carreraJoin.DefaultIfEmpty()
                                  group u by (c != null ? c.Nombre : "Sin carrera") into g
                                  select new UsuariosCarreraDTO
                                  {
                                      Carrera = g.Key,
                                      Cantidad = g.Count()
                                  }).ToList();

            // 🔹 Crear Excel
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte Filtrado");

            int fila = 1;
            ws.Cell(fila, 1).Value = "📊 Reporte SmartFlow (con filtros activos)";
            ws.Cell(fila, 1).Style.Font.Bold = true;
            ws.Cell(fila, 1).Style.Font.FontSize = 16;
            fila += 2;

            // 🔸 Totales generales
            ws.Cell(fila, 1).Value = "Total Usuarios";
            ws.Cell(fila, 2).Value = TotalUsuarios; fila++;
            ws.Cell(fila, 1).Value = "Total Reservas (filtradas)";
            ws.Cell(fila, 2).Value = TotalReservas; fila++;
            ws.Cell(fila, 1).Value = "Reservas Pendientes";
            ws.Cell(fila, 2).Value = ReservasPendientes; fila++;
            ws.Cell(fila, 1).Value = "Total Solicitudes";
            ws.Cell(fila, 2).Value = TotalSolicitudes; fila += 2;

            // 🔸 Reservas por Estado
            ws.Cell(fila, 1).Value = "Reservas por Estado";
            ws.Cell(fila, 1).Style.Font.Bold = true; fila++;
            ws.Cell(fila, 1).Value = "Estado";
            ws.Cell(fila, 2).Value = "Cantidad";
            ws.Row(fila).Style.Font.Bold = true; fila++;

            foreach (var r in ReservasPorEstado)
            {
                ws.Cell(fila, 1).Value = r.Estado;
                ws.Cell(fila, 2).Value = r.Cantidad;
                fila++;
            }

            fila += 2;

            // 🔸 Usuarios por Carrera
            ws.Cell(fila, 1).Value = "Usuarios por Carrera";
            ws.Cell(fila, 1).Style.Font.Bold = true; fila++;
            ws.Cell(fila, 1).Value = "Carrera";
            ws.Cell(fila, 2).Value = "Cantidad";
            ws.Row(fila).Style.Font.Bold = true; fila++;

            foreach (var u in UsuariosPorCarrera)
            {
                ws.Cell(fila, 1).Value = u.Carrera;
                ws.Cell(fila, 2).Value = u.Cantidad;
                fila++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Reporte_SmartFlow_Filtrado.xlsx");
        }

        // ============================================================
        // 📄 EXPORTAR A PDF (con filtros activos)
        // ============================================================
        public IActionResult OnGetExportarPdf()
        {
            var reservasQuery = _context.Reservas.AsQueryable();

            // 🔹 Aplicar filtros
            if (FechaInicio.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaInicio >= FechaInicio.Value);
            if (FechaFin.HasValue)
                reservasQuery = reservasQuery.Where(r => r.FechaFin <= FechaFin.Value);
            if (!string.IsNullOrWhiteSpace(Estado))
                reservasQuery = reservasQuery.Where(r => r.Estado == Estado);
            if (CarreraId.HasValue)
                reservasQuery = reservasQuery.Where(r => r.Usuario.CarreraId == CarreraId.Value);

            // 🔹 Recalcular métricas
            TotalUsuarios = _context.Usuarios.Count();
            TotalReservas = reservasQuery.Count();
            ReservasPendientes = reservasQuery.Count(r => r.Estado == "Pendiente");
            TotalSolicitudes = _context.Solicitudes.Count();

            ReservasPorEstado = reservasQuery
                .GroupBy(r => r.Estado)
                .Select(g => new ReservasEstadoDTO
                {
                    Estado = string.IsNullOrEmpty(g.Key) ? "Sin estado" : g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            UsuariosPorCarrera = (from u in _context.Usuarios
                                  join c in _context.Carreras on u.CarreraId equals c.Id into carreraJoin
                                  from c in carreraJoin.DefaultIfEmpty()
                                  group u by (c != null ? c.Nombre : "Sin carrera") into g
                                  select new UsuariosCarreraDTO
                                  {
                                      Carrera = g.Key,
                                      Cantidad = g.Count()
                                  }).ToList();

            // 🔹 Generar PDF con QuestPDF
            var stream = new MemoryStream();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text("📊 Reporte SmartFlow (con filtros activos)").FontSize(20).Bold().AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).AlignCenter();
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        col.Item().PaddingVertical(10);

                        // 🔸 Resumen General
                        col.Item().Text("Resumen General").FontSize(14).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(200);
                                c.RelativeColumn();
                            });

                            table.Cell().Text("Total Usuarios");
                            table.Cell().Text(TotalUsuarios.ToString());

                            table.Cell().Text("Total Reservas (filtradas)");
                            table.Cell().Text(TotalReservas.ToString());

                            table.Cell().Text("Reservas Pendientes");
                            table.Cell().Text(ReservasPendientes.ToString());

                            table.Cell().Text("Total Solicitudes");
                            table.Cell().Text(TotalSolicitudes.ToString());
                        });

                        col.Item().PaddingVertical(15);
                        col.Item().Text("Reservas por Estado").FontSize(14).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Estado").Bold();
                                header.Cell().Text("Cantidad").Bold();
                            });
                            foreach (var r in ReservasPorEstado)
                            {
                                table.Cell().Text(r.Estado);
                                table.Cell().Text(r.Cantidad.ToString());
                            }
                        });

                        col.Item().PaddingVertical(15);
                        col.Item().Text("Usuarios por Carrera").FontSize(14).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Carrera").Bold();
                                header.Cell().Text("Cantidad").Bold();
                            });
                            foreach (var u in UsuariosPorCarrera)
                            {
                                table.Cell().Text(u.Carrera);
                                table.Cell().Text(u.Cantidad.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text("SmartFlow © 2025 - Sistema de Gestión y Reservas")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });

            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/pdf", "Reporte_SmartFlow_Filtrado.pdf");
        }

    }
}
