using AppLembreteMedicacao.Models;
using System.Linq;

namespace AppLembreteMedicacao.Views;

public partial class Monitoramento : ContentPage
{
    private int _medicamentoId = 0;

    public Monitoramento()
    {
        InitializeComponent();
    }

    public Monitoramento(int medicamentoId)
    {
        InitializeComponent();
        _medicamentoId = medicamentoId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarDadosMonitoramento();
    }

    private async Task CarregarDadosMonitoramento()
    {
        try
        {
            // Busca dados do paciente
            var paciente = await App.Banco.GetPaciente();
            lblNomePaciente.Text = paciente?.Nome?.ToUpper() ?? "PACIENTE";

            // Busca histórico bruto
            List<HistoricoUso> listaBruta;
            if (_medicamentoId == 0)
                listaBruta = await App.Banco.GetTodosHistorico();
            else
                listaBruta = await App.Banco.GetHistorico(_medicamentoId);

            if (listaBruta != null && listaBruta.Any())
            {
                // LÓGICA DO FAROL
                var resumoAdesao = listaBruta
                    .GroupBy(h => h.NomeMedicamento)
                    .Select(g => 
                    {
                        int total = g.Count();
                        int tomadas = g.Count(x => x.Tomado);
                        double perc = (double)tomadas / total;

                        // Definição das cores do Farol
                        Color corFarol;
                        if (perc >= 0.8) corFarol = Colors.Green;       // Verde: Bom
                        else if (perc >= 0.5) corFarol = Colors.Orange; // Laranja: Atenção
                        else corFarol = Colors.Red;                    // Vermelho: Crítico

                        return new
                        {
                            Nome = g.Key,
                            Total = total,
                            Tomadas = tomadas,
                            Percentual = perc,
                            CorStatus = corFarol
                        };
                    })
                    .OrderByDescending(x => x.Percentual) // Opcional: Mostra os melhores primeiro
                    .ToList();

                listaEstatisticas.ItemsSource = resumoAdesao;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Erro ao carregar relatório: " + ex.Message, "OK");
        }
    }

    private async void OnSairClicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Sair", "Deseja realmente sair?", "Sim", "Não"))
        {
            Application.Current.MainPage = new NavigationPage(new Login());
        }
    }
}