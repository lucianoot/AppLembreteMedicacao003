using AppLembreteMedicacao.Models;
using System.Linq;

namespace AppLembreteMedicacao.Views;

public partial class Monitoramento : ContentPage
{
    private int _medicamentoId = 0;

    // CONSTRUTOR VAZIO (Para o Médico/Responsável conseguir abrir a tela)
    public Monitoramento()
    {
        InitializeComponent();
        // Construtor para quando você clica na lista
    }
    public Monitoramento(int medicamentoId)
    {
        InitializeComponent();
        _medicamentoId = medicamentoId;

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CarregarHistorico();
    }
    private async Task CarregarHistorico()
    {
        try
        {
            // 1. Busca o paciente diretamente
            var paciente = await App.Banco.GetPaciente();

            if (paciente != null)
            {
                lblNomePaciente.Text = paciente.Nome.ToUpper();
            }
            else
            {
                lblNomePaciente.Text = "PACIENTE NÃO ENCONTRADO";
            }
            List<HistoricoUso> listaBruta;

            if (_medicamentoId == 0)
                listaBruta = await App.Banco.GetTodosHistorico();
            else
                listaBruta = await App.Banco.GetHistorico(_medicamentoId);

            if (listaBruta != null && listaBruta.Any())
            {
                // 1. Lógica para a Lista Detalhada
                var listaFormatada = listaBruta.Select(h => new
                {
                    NomeMedicamento = h.NomeMedicamento,
                    DataUso = h.DataUso.ToString("dd/MM/yyyy HH:mm"),
                    TomadoTexto = h.Tomado ? "Tomou ✔" : "Não tomou ❌",
                    CorStatus = h.Tomado ? Colors.Green : Colors.Red
                }).ToList();

                listaHistorico.ItemsSource = listaFormatada;

                // 2. NOVA LÓGICA: Cálculo de Adesão por Medicamento
                var resumoAdesao = listaBruta
                    .GroupBy(h => h.NomeMedicamento)
                    .Select(g => new
                    {
                        Nome = g.Key,
                        Total = g.Count(),
                        Tomadas = g.Count(x => x.Tomado),
                        // Cálculo da porcentagem (ex: 0.75 para 75%)
                        Percentual = (double)g.Count(x => x.Tomado) / g.Count(),
                        CorAdesao = ((double)g.Count(x => x.Tomado) / g.Count()) >= 0.8 ? Colors.Green : Colors.Orange
                    })
                    .ToList();

                // Vincula ao novo componente visual que vamos adicionar no XAML
                listaEstatisticas.ItemsSource = resumoAdesao;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao processar monitoramento: " + ex.Message, "OK");
        }
    }
    private async void OnSairClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sair", "Deseja realmente deslogar?", "Sim", "Não");
        if (confirm)
        {
            // Volta para a tela de login para permitir trocar de usuário
            Application.Current.MainPage = new NavigationPage(new Login());
        }
    }
}