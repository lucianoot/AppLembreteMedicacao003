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
        await ProcessarAcao(true);
    }

    private async void OnNaoClicked(object sender, EventArgs e)
    {
        await ProcessarAcao(false);
    }

    private async Task ProcessarAcao(bool foiTomado)
    {
        // 1. REGISTRO NO HISTÓRICO (Para relatórios/CRUD)
        var historico = new HistoricoUso
        {
            MedicamentoId = _medicamentoId,
            NomeMedicamento = _medicamento?.Nome ?? "Desconhecido",
            DataUso = DateTime.Now,
            Tomado = foiTomado
        };
        await App.Banco.InsertHistorico(historico);

        // 2. ATUALIZAÇÃO DA TABELA DOSE 
        // Buscamos as doses do remédio e pegamos a primeira que ainda está "Pendente"
        var doses = await App.Banco.GetDosesPorMedicamento(_medicamentoId);
        var doseParaAtualizar = doses.FirstOrDefault(d => d.Status == "Pendente");

        if (doseParaAtualizar != null)
        {
            // Atualiza o Status
            doseParaAtualizar.Status = foiTomado ? "Tomado" : "Perdido";

            // ADICIONADO: Registra o horário da ação
            if (foiTomado)
            {
                doseParaAtualizar.dataHoraTomada = DateTime.Now;
            }

            // Chamamos o Update do banco
            await App.Banco.UpdateDose(doseParaAtualizar);
        }

        // 3. FEEDBACK E SAÍDA
        string msg = foiTomado ? "Dose registrada!" : "Registro de dose perdida salvo.";
        await DisplayAlert("Pronto", msg, "OK");

        await Navigation.PopToRootAsync();
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}