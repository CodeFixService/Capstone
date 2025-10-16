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

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            // ============================================================
            // 1️⃣ TARJETAS: Métricas generales
            // ============================================================
            TotalUsuarios = _context.Usuarios.Count();
            TotalReservas = _context.Reservas.Count();
            ReservasPendientes = _context.Reservas.Count(r => r.Estado == "Pendiente");
            TotalSolicitudes = _context.Solicitudes.Count();

            // ============================================================
            // 2️⃣ GRÁFICO: Reservas por estado
            // ============================================================
            ReservasPorEstado = _context.Reservas
                .GroupBy(r => r.Estado)
                .Select(g => new ReservasEstadoDTO
                {
                    Estado = string.IsNullOrEmpty(g.Key) ? "Sin estado" : g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            // ============================================================
            // 3️⃣ GRÁFICO: Usuarios por carrera
            // ============================================================
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

        public IActionResult OnGetExportarExcel()
        {
            // 🔹 Recargar datos antes de exportar (para evitar ceros)
            TotalUsuarios = _context.Usuarios.Count();
            TotalReservas = _context.Reservas.Count();
            ReservasPendientes = _context.Reservas.Count(r => r.Estado == "Pendiente");
            TotalSolicitudes = _context.Solicitudes.Count();

            ReservasPorEstado = _context.Reservas
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

            // 🔹 Crear libro de Excel
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte General");

            int fila = 1;
            ws.Cell(fila, 1).Value = "📊 Reporte SmartFlow";
            ws.Cell(fila, 1).Style.Font.Bold = true;
            ws.Cell(fila, 1).Style.Font.FontSize = 16;
            fila += 2;

            // 🔹 Totales generales
            ws.Cell(fila, 1).Value = "Total Usuarios";
            ws.Cell(fila, 2).Value = TotalUsuarios;
            fila++;
            ws.Cell(fila, 1).Value = "Total Reservas";
            ws.Cell(fila, 2).Value = TotalReservas;
            fila++;
            ws.Cell(fila, 1).Value = "Reservas Pendientes";
            ws.Cell(fila, 2).Value = ReservasPendientes;
            fila++;
            ws.Cell(fila, 1).Value = "Total Solicitudes";
            ws.Cell(fila, 2).Value = TotalSolicitudes;
            fila += 2;

            // 🔹 Tabla: Reservas por estado
            ws.Cell(fila, 1).Value = "Reservas por estado";
            ws.Cell(fila, 1).Style.Font.Bold = true;
            fila++;
            ws.Cell(fila, 1).Value = "Estado";
            ws.Cell(fila, 2).Value = "Cantidad";
            ws.Row(fila).Style.Font.Bold = true;
            fila++;

            foreach (var r in ReservasPorEstado)
            {
                ws.Cell(fila, 1).Value = r.Estado;
                ws.Cell(fila, 2).Value = r.Cantidad;
                fila++;
            }

            fila += 2;

            // 🔹 Tabla: Usuarios por carrera
            ws.Cell(fila, 1).Value = "Usuarios por carrera";
            ws.Cell(fila, 1).Style.Font.Bold = true;
            fila++;
            ws.Cell(fila, 1).Value = "Carrera";
            ws.Cell(fila, 2).Value = "Cantidad";
            ws.Row(fila).Style.Font.Bold = true;
            fila++;

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
                "Reporte_SmartFlow.xlsx");
        }
        public IActionResult OnGetExportarPdf()
        {
            // 🔹 Recargar datos antes de generar el PDF
            TotalUsuarios = _context.Usuarios.Count();
            TotalReservas = _context.Reservas.Count();
            ReservasPendientes = _context.Reservas.Count(r => r.Estado == "Pendiente");
            TotalSolicitudes = _context.Solicitudes.Count();

            ReservasPorEstado = _context.Reservas
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

            // 🔹 Crear documento PDF con QuestPDF
            var stream = new MemoryStream();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text("📊 Reporte SmartFlow").FontSize(20).Bold().AlignCenter();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).AlignCenter();
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        col.Item().PaddingVertical(10);

                        // 🔸 Totales
                        col.Item().Text("Resumen General").FontSize(14).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200);
                                columns.RelativeColumn();
                            });

                            table.Cell().Text("Total Usuarios");
                            table.Cell().Text(TotalUsuarios.ToString());

                            table.Cell().Text("Total Reservas");
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

                    page.Footer().AlignCenter().Text("SmartFlow © 2025 - Sistema de Gestión y Reservas").FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });

            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/pdf", "Reporte_SmartFlow.pdf");
        }
    }
}
