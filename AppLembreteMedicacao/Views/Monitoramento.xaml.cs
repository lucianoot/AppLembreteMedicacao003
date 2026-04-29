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
            List<HistoricoUso> listaBruta;

            // Se o ID for 0, busca tudo (visão do Médico/Responsável)
            // Se tiver ID, busca só daquele remédio específico
            if (_medicamentoId == 0)
            {
                listaBruta = await App.Banco.GetTodosHistorico();
            }
            else
            {
                listaBruta = await App.Banco.GetHistorico(_medicamentoId);
            }

            if (listaBruta != null)
            {
                // Formata a lista para exibir textos bonitos na tela
                var listaFormatada = listaBruta.Select(h => new
                {
                    NomeMedicamento = h.NomeMedicamento,
                    DataUso = h.DataUso.ToString("dd/MM/yyyy HH:mm"),
                    TomadoTexto = h.Tomado ? "Tomou ✔" : "Não tomou ❌",
                    CorStatus = h.Tomado ? Colors.Green : Colors.Red
                }).ToList();

                listaHistorico.ItemsSource = listaFormatada;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível carregar o histórico: " + ex.Message, "OK");
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