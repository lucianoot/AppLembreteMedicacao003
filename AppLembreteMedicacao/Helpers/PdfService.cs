using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AppLembreteMedicacao.Models;
using PdfColors = QuestPDF.Helpers.Colors;

namespace AppLembreteMedicacao.Helpers
{
    public class PdfService
    {
        public static string GerarPdfMonitoramento(List<HistoricoUso> historico, string hashSeguranca)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            string nomeArquivo = $"Monitoramento_{DateTime.Now:yyyyMMdd}.pdf";
            string caminho = Path.Combine(FileSystem.CacheDirectory, nomeArquivo);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1, Unit.Centimetre);

                    // Cabeçalho
                    page.Header().Text("RELATÓRIO DE MONITORAMENTO").FontSize(20).SemiBold().FontColor(PdfColors.Blue.Medium);

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text($"Relatório gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);

                        // Tabela de Histórico
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Data/Hora
                                columns.RelativeColumn(4); // Medicamento
                                columns.RelativeColumn(2); // Status
                            });

                            // Cabeçalho da Tabela
                            table.Header(header =>
                            {
                                header.Cell().Background(PdfColors.Grey.Lighten2).Padding(2).Text("Data/Hora").SemiBold();
                                header.Cell().Background(PdfColors.Grey.Lighten2).Padding(2).Text("Medicamento").SemiBold();
                                header.Cell().Background(PdfColors.Grey.Lighten2).Padding(2).Text("Status").SemiBold();
                            });

                            // Dados das Doses
                            foreach (var item in historico)
                            {
                                table.Cell().BorderBottom(1).BorderColor(PdfColors.Grey.Lighten3).Padding(2).Text(item.DataUso.ToString("dd/MM HH:mm"));
                                table.Cell().BorderBottom(1).BorderColor(PdfColors.Grey.Lighten3).Padding(2).Text(item.NomeMedicamento);

                                var statusText = item.Tomado ? "TOMOU" : "PULOU";
                                var statusColor = item.Tomado ? PdfColors.Green.Medium : PdfColors.Red.Medium;

                                table.Cell().BorderBottom(1).BorderColor(PdfColors.Grey.Lighten3).Padding(2).Text(statusText).FontColor(statusColor).Bold();
                            }
                        });

                        // Rodapé de Segurança
                        col.Item().PaddingTop(20).Background(PdfColors.Grey.Lighten4).Padding(5)
                           .Text($"Assinatura Digital (Hash): {hashSeguranca}").FontSize(8).Italic();
                    });

                    page.Footer().AlignCenter().Text(x => {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(caminho);

            return caminho;
        }
    }
}