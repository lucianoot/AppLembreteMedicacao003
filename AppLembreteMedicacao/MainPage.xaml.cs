using AppLembreteMedicacao.Helpers;
using AppLembreteMedicacao.Models;
using AppLembreteMedicacao.Views;
using Plugin.LocalNotification;
using Microsoft.Maui.Storage;

namespace AppLembreteMedicacao;

public partial class MainPage : ContentPage
{
    private Medicamento _medicamentoParaEdicao;

    public MainPage()
    {
        InitializeComponent();

        // Zoom ao tocar no botão de cronograma
        btnCronograma.Pressed += async (s, e) => await btnCronograma.ScaleTo(1.2, 100);
        btnCronograma.Released += async (s, e) => await btnCronograma.ScaleTo(1, 100);

    }


    private async void AoClicarSair(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Sair", "Deseja realmente sair?", "Sim", "Não");

        if (confirmar)
        {
            // 1. Limpa o email salvo para não logar sozinho na próxima vez
            Preferences.Remove("UserEmail");

            // 2. Volta para a tela de Login 
            App.Current.MainPage = new NavigationPage(new AppLembreteMedicacao.Views.Login());
        }
    }


    //  NOVO MÉTODO DE CLIQUE (Substitui o OnMedicamentoSelecionado)
    private async void OnMedicamentoTapped(object sender, TappedEventArgs e)
    {
        // Pega o remédio que foi passado pelo CommandParameter no XAML
        var medicamento = e.Parameter as Medicamento;

        if (medicamento == null) return;

        // Abre o menu de opções nativo
        string acao = await DisplayActionSheet($"Opções para: {medicamento.Nome}",
            "Cancelar", "Remover", "Ver Horários", "Gerar Ciclo 6h/6h", "Gerar Ciclo 8h/8h", "Gerar Ciclo 12h/12h", "Gerar Ciclo 24h/24h");

        switch (acao)
        {
            case "Ver Horários":
                // Agora passando o ID e o Nome
                await Navigation.PushAsync(new CronogramaPage(medicamento.Id, medicamento.Nome));
                break;

            case "Gerar Ciclo 6h/6h":
                await GerarCicloAutomatico(medicamento.Id, 6);
                break;

            case "Gerar Ciclo 8h/8h":
                await GerarCicloAutomatico(medicamento.Id, 8);
                break;

            case "Gerar Ciclo 12h/12h":
                await GerarCicloAutomatico(medicamento.Id, 12);
                break;

            case "Gerar Ciclo 24h/24h":
                await GerarCicloAutomatico(medicamento.Id, 24);
                break;

            case "Remover":
                bool confirmar = await DisplayAlert("Atenção", $"Excluir {medicamento.Nome}?", "Sim", "Não");
                if (confirmar)
                {
                    await App.Banco.DeleteMedicamento(medicamento.Id);
                    CarregarMedicamentos(); // Atualiza a lista na tela
                }
                break;
        }
    }

    // Método para salvar o remédio (campos da tela)
    private async void AoClicarSalvar(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(entNome.Text))
        {
            await DisplayAlert("Erro", "Por favor, preencha o nome do remédio.", "OK");
            return;
        }

        try
        {
            var novo = new Medicamento
            {
                Nome = entNome.Text,
                Dosagem = entDose.Text,
                DataInicio = dtInicio.Date,
                DataFim = dtFim.Date,
                Ativo = 1,
                // Inicialmente zero, será atualizado quando você gerar o ciclo
                IntervaloHoras = 0
            };

            await App.Banco.InsertMedicamento(novo);

            entNome.Text = string.Empty;
            entDose.Text = string.Empty;

            await DisplayAlert("Sucesso", "Remédio cadastrado! Agora toque nele para definir o ciclo.", "OK");
            CarregarMedicamentos();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível salvar: " + ex.Message, "OK");
        }
    }

    private async Task GerarCicloAutomatico(int medId, int intervalo)
    {
        // Atualiza o medicamento com o intervalo escolhido 
        var listaMed = await App.Banco.GetMedicamentos();
        var medicamento = listaMed.FirstOrDefault(m => m.Id == medId);

        if (medicamento != null)
        {
            medicamento.IntervaloHoras = intervalo;
            await App.Banco.UpdateMedicamento(medicamento);
        }
        // ------------------------------------------------------------------

        DateTime horaAtual = DateTime.Now;
        int repeticoes = 24 / intervalo;

        for (int i = 0; i < repeticoes; i++)
        {
            var novoHorario = new Cronograma
            {
                MedicamentoId = medId,
                Hora = horaAtual.AddHours(i * intervalo).ToString(@"HH\:mm"),
                Frequencia = "Automático",
                Ativo = 1
            };
            await App.Banco.InsertCronograma(novoHorario);
        }

        await DisplayAlert("Sucesso", $"Ciclo de {intervalo}h criado!", "OK");
        CarregarMedicamentos(); // Recarrega a lista para mostrar o "6h/6h" na tela
    }

    private async void AoClicarCronograma(object sender, EventArgs e)
    {
        var ultimoRemedio = await App.Banco.GetUltimoMedicamento();
        if (ultimoRemedio != null)
        {
            await Navigation.PushAsync(new CronogramaPage(ultimoRemedio.Id, ultimoRemedio.Nome));
        }
        else
        {
            await DisplayAlert("Atenção", "Cadastre um remédio primeiro.", "OK");
        }
    }

    // Botão flutuante (+)
    private async void AoClicarNovoRemedio(object sender, EventArgs e)
    {
        try
        {


            // Foca o cursor automaticamente no campo Nome do Remédio
            entNome.Focus();

            // Feedback visual simples no botão que foi clicado
            if (sender is VisualElement botao)
            {
                await botao.ScaleTo(1.2, 100);
                await botao.ScaleTo(1.0, 100);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao focar: {ex.Message}");
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        CarregarMedicamentos();
    }

    private async void CarregarMedicamentos()
    {
        try
        {
            var lista = await App.Banco.GetMedicamentos();
            listaMedicamentos.ItemsSource = lista;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro: {ex.Message}");
        }
    }

    private async void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        var lista = await App.Banco.GetMedicamentos();
        if (lista == null || lista.Count == 0) return;

        string prontuario = $"📋 PRONTUÁRIO - {DateTime.Now:dd/MM/yyyy}\n\n";
        foreach (var m in lista) prontuario += $"💊 {m.Nome} ({m.Dosagem})\n";

        string hashSeguro = SecurityHelper.GerarHash(prontuario);

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Compartilhar Prontuário",
            Text = $"Hash de Segurança:\n{hashSeguro}",
            Uri = "App Meu Remédio"
        });
    }
}
