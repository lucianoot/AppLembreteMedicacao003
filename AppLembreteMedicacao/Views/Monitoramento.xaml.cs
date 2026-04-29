using AppLembreteMedicacao.Models;
using System.Linq;

namespace AppLembreteMedicacao.Views;

public partial class Monitoramento : ContentPage
{
    public Monitoramento()
    {
        InitializeComponent();
    }
 protected override async void OnAppearing()
    {
        base.OnAppearing();

        var historico = await App.Banco.GetTodosHistorico();

        var listaFormatada = historico.Select(h => new
        {
            NomeMedicamento = h.NomeMedicamento,
            DataUso = h.DataUso.ToString("dd/MM/yyyy HH:mm"),
            TomadoTexto = h.Tomado ? "Tomou ✔" : "Não tomou ❌"
        }).ToList();

        listaHistorico.ItemsSource = listaFormatada;
    }
}
