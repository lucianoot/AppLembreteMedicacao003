using AppLembreteMedicacao.Helpers;
using AppLembreteMedicacao.Models;
using AppLembreteMedicacao.Views;
using Microsoft.Maui.Storage;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace AppLembreteMedicacao;

public partial class MainPage : ContentPage
{
    private Medicamento _medicamentoParaEdicao;

    public MainPage()
    {
        InitializeComponent();

        // Registro dos botões da notificação
        ConfigurarCategoriasDeNotificacao();

        // Zoom ao tocar no botão de cronograma
        btnCronograma.Pressed += async (s, e) => await btnCronograma.ScaleTo(1.2, 100);
        btnCronograma.Released += async (s, e) => await btnCronograma.ScaleTo(1, 100);

    }
    private void ConfigurarCategoriasDeNotificacao()
    {
        var acoes = new HashSet<NotificationAction>
        {
            new NotificationAction(100) { Title = "Dose Tomada" },
        new NotificationAction(101) { Title = "Pular Dose" }
    };

        // Na v10, usamos o tipo "Status" para agrupar os botões.
        var categoria = new NotificationCategory(NotificationCategoryType.Status)
        {
            ActionList = acoes
        };

        //LocalNotificationCenter.Current.RegisterCategory(categoria);
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
            "Cancelar", "Remover", "Editar", "Ver Horários", "Gerar Ciclo 6h/6h", "Gerar Ciclo 8h/8h", "Gerar Ciclo 12h/12h", "Gerar Ciclo 24h/24h");

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

            case "Editar":
                // Guarda o medicamento para edição
                _medicamentoParaEdicao = medicamento;

                // Preenche os campos da tela
                entNome.Text = medicamento.Nome;
                entDose.Text = medicamento.Dosagem;
                dtInicio.Date = medicamento.DataInicio;
                dtFim.Date = medicamento.DataFim ?? DateTime.Today;

                await DisplayAlert("Edição", "Edite os dados e clique em salvar.", "OK");
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
            if (_medicamentoParaEdicao == null)
            {
                // ✅ NOVO
                var novo = new Medicamento
                {
                    Nome = entNome.Text,
                    Dosagem = entDose.Text,
                    DataInicio = dtInicio.Date,
                    DataFim = dtFim.Date,
                    Ativo = 1,
                    IntervaloHoras = 0
                };

                await App.Banco.InsertMedicamento(novo);

                // PEGA O ID REAL DO BANCO
                var ultimo = await App.Banco.GetUltimoMedicamento();

                // NOTIFICAÇÃO COM BOTÕES
                var notifNovo = new NotificationRequest
                {
                    NotificationId = ultimo.Id,
                    Title = "Hora do Remédio 💊",
                    Description = $"Tomar {novo.Nome}",

                    ReturningData = ultimo.Id.ToString(),
                    // Vincula aos botões configurados no construtor
                    CategoryType = NotificationCategoryType.Status,

                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Now.AddSeconds(5)
                    },

                    Android = new AndroidOptions
                    {

                        LaunchAppWhenTapped = true
                    }
                };

                // 🔥 mostra a notificação
                await LocalNotificationCenter.Current.Show(notifNovo);

                // mensagem
                await DisplayAlert("Sucesso", "Medicamento cadastrado com notificação!", "OK");
            }
            else
            {
                // ✏️ EDITAR
                _medicamentoParaEdicao.Nome = entNome.Text;
                _medicamentoParaEdicao.Dosagem = entDose.Text;
                _medicamentoParaEdicao.DataInicio = dtInicio.Date;
                _medicamentoParaEdicao.DataFim = dtFim.Date;

                await App.Banco.UpdateMedicamento(_medicamentoParaEdicao);

                await DisplayAlert("Sucesso", "Medicamento atualizado!", "OK");

                // Limpa modo edição
                _medicamentoParaEdicao = null;
            }

            // Limpa campos
            entNome.Text = string.Empty;
            entDose.Text = string.Empty;

            CarregarMedicamentos();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível salvar: " + ex.Message, "OK");
        }
    }

    private async Task GerarCicloAutomatico(int medId, int intervalo)
    {
        // 1. Busca o medicamento para saber o nome
        var listaMed = await App.Banco.GetMedicamentos();
        var medicamento = listaMed.FirstOrDefault(m => m.Id == medId);
        if (medicamento == null) return;

        medicamento.IntervaloHoras = intervalo;
        await App.Banco.UpdateMedicamento(medicamento);

        DateTime horaBase = DateTime.Now;
        int repeticoes = 24 / intervalo;

        List<Dose> listaDosesParaBanco = new List<Dose>();

        for (int i = 0; i < repeticoes; i++)
        {
            DateTime dataDose = horaBase.AddHours(i * intervalo);

            // 2. Cria a dose para o banco (Status Pendente)
            var novaDose = new Dose
            {
                MedicamentoId = medId,
                NomeMedicamento = medicamento.Nome,
                Status = "Pendente",
                HorarioPrevisto = dataDose
            };
            listaDosesParaBanco.Add(novaDose);

            // 3. AGENDA A NOTIFICAÇÃO PARA CADA DOSE
            var notif = new NotificationRequest
            {
                // O ID da notificação precisa ser ÚNICO. 
                // Usamos o ID do med + o index para não sobrescrever
                NotificationId = medId * 100 + i,
                Title = "Hora do Remédio 💊",
                Description = $"Tomar {medicamento.Nome}",
                ReturningData = medId.ToString(), // Passa o ID do Medicamento para o banco atualizar
                CategoryType = NotificationCategoryType.Status,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = dataDose // Agenda para o horário futuro
                },
                Android = new AndroidOptions { LaunchAppWhenTapped = true }
            };

            await LocalNotificationCenter.Current.Show(notif);
        }

        // 4. Salva todas as doses de uma vez no banco
        await App.Banco.InsertDoses(listaDosesParaBanco);

        await DisplayAlert("Sucesso", $"Ciclo de {intervalo}h criado e notificações agendadas!", "OK");
        CarregarMedicamentos();
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
        try
        {
            // 1. Busca TODO o histórico (Monitoramento) do banco
            var historico = await App.Banco.GetTodosHistorico();

            if (historico == null || !historico.Any())
            {
                await DisplayAlert("Aviso", "Ainda não há registros de doses tomadas para exportar.", "OK");
                return;
            }

            // 2. Gera o Hash baseado no histórico para garantir que os dados não foram alterados
            string dadosParaHash = string.Join("|", historico.Select(h => $"{h.DataUso}-{h.Tomado}"));
            string hashSeguro = SecurityHelper.GerarHash(dadosParaHash);

            // 3. Gera o ARQUIVO PDF de MONITORAMENTO
            string caminhoDoPdf = PdfService.GerarPdfMonitoramento(historico, hashSeguro);

            // 4. Compartilha o arquivo
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Relatório de Monitoramento - " + DateTime.Now.ToString("dd/MM/yyyy"),
                File = new ShareFile(caminhoDoPdf)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao gerar relatório de monitoramento: " + ex.Message, "OK");
        }
    }
}