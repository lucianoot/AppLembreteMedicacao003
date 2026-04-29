
using AppLembreteMedicacao.Models;
namespace AppLembreteMedicacao.Views;

public partial class ListaMedicacao : ContentPage
{
    public ListaMedicacao()
	{
		InitializeComponent();
    }
    // Recarrega a lista sempre que a tela aparece
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var itens = await App.Banco.GetMedicamentos();
            listaMedicamentos.ItemsSource = itens;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Erro ao carregar banco: " + ex.Message, "OK");
        }
    }

    // Lógica do Botão TOMAR 
    private async void OnTomarClicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        var med = btn?.CommandParameter as Medicamento;

        if (med != null)
        {
            
            var historico = new HistoricoUso
            {
                MedicamentoId = med.Id, 
                NomeMedicamento = med.Nome,
                DataUso = DateTime.Now,
                Tomado = true
            };

            // Salva na tabela de histórico
            await App.Banco.SalvarHistorico(historico);

            await DisplayAlert("Sucesso", $"{med.Nome} marcado como tomado!", "OK");
        }
    }

    private async void OnVerHistoricoClicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        var med = btn?.CommandParameter as Medicamento;

        if (med != null)
        {
            await Navigation.PushAsync(new Monitoramento(med.Id));
        }
    }

    private async void OnNovaMedicacaoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Novomedicacao());
    }
    private async void OnSairClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Deseja realmente sair?", "Sim", "Não");
        if (confirm)
        {
            Application.Current.MainPage = new NavigationPage(new Login());
        }
    }
}