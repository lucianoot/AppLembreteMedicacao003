using AppLembreteMedicacao.Models;

namespace AppLembreteMedicacao.Views;

public partial class ConfirmacaoPage : ContentPage
{
    private int _medicamentoId;
    private Medicamento _medicamento;

    // O construtor recebe o ID que veio da notificação (via App.xaml.cs)
    public ConfirmacaoPage(int idMed)
    {
        InitializeComponent();
        _medicamentoId = idMed;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Busca os dados do remédio no banco para exibir o nome na tela
        _medicamento = await App.Banco.GetMedicamentoPorId(_medicamentoId);

        if (_medicamento != null)
        {
            lblMensagem.Text = $"Hora de tomar: {_medicamento.Nome} ({_medicamento.Dosagem})";
        }
    }

    private async void OnSimClicked(object sender, EventArgs e)
    {
        await SalvarNoHistorico(true);
    }

    private async void OnNaoClicked(object sender, EventArgs e)
    {
        await SalvarNoHistorico(false);
    }

    private async Task SalvarNoHistorico(bool foiTomado)
    {
        var historico = new HistoricoUso
        {
            MedicamentoId = _medicamentoId,
            NomeMedicamento = _medicamento?.Nome ?? "Desconhecido",
            DataUso = DateTime.Now,
            Tomado = foiTomado
        };

        await App.Banco.InsertHistorico(historico);

        string msg = foiTomado ? "Dose registrada com sucesso!" : "Registro de dose perdida salvo.";
        await DisplayAlert("Pronto", msg, "OK");

        // Volta para a página inicial (MainPage)
        await Navigation.PopAsync();
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}