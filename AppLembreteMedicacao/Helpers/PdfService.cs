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
            try
            {
                // Configura a licença apenas aqui
                QuestPDF.Settings.License = LicenseType.Community;

                string nomeArquivo = $"Monitoramento_{DateTime.Now:yyyyMMdd}.pdf";
                string caminho = Path.Combine(FileSystem.CacheDirectory, nomeArquivo);

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(1, Unit.Centimetre);

                        // USA UMA FONTE PADRÃO PARA EVITAR O ERRO DE HELPERS
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Courier"));

                        page.Header().Text("RELATÓRIO DE MONITORAMENTO").FontSize(16);

                        page.Content().Column(col =>
                        {
                            col.Item().PaddingVertical(5).LineHorizontal(1);

                            foreach (var item in historico)
                            {
                                string status = item.Tomado ? "TOMOU" : "PULOU";
                                col.Item().Text($"{item.DataUso:dd/MM HH:mm} - {item.NomeMedicamento} - {status}");
                            }

                            col.Item().PaddingTop(10).Text($"Assinatura: {hashSeguranca}").FontSize(8);
                        });
                    });
                }).GeneratePdf(caminho);

                return caminho;
            }
            catch (Exception ex)
            {
                // Se der erro de inicialização, o erro será capturado aqui
                throw new Exception("Erro técnico ao gerar PDF (SkiaSharp): " + ex.Message);
            }
        }
    }
}