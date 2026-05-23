using AppLembreteMedicacao.Helpers;
using AppLembreteMedicacao.Models;
using AppLembreteMedicacao.Views;
using Microsoft.Maui.Storage;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System;


namespace AppLembreteMedicacao;

public partial class MainPage : ContentPage
{
    private Medicamento _medicamentoParaEdicao;

    public MainPage()
    {
        InitializeComponent();

        // Registro dos botões da notificação
        ConfigurarCategoriasDeNotificacao();

        //Data mínima
        dtInicio.MinimumDate = new DateTime(2000, 1, 1);
        // Impede selecionar uma data futura apenas para a Data Final (V)
        dtFim.MaximumDate = new DateTime(2100, 12, 31);
    }


    
    private async void OnVerHistoricoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HistoricoDosesPage());
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

        // coloquei como comentário o erro LocalNotificationCenter.Current.RegisterCategory(categoria);
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
            "Cancelar", "Remover", "Editar", "Gerar Ciclo 6h/6h", "Gerar Ciclo 8h/8h", "Gerar Ciclo 12h/12h", "Gerar Ciclo 24h/24h");

        switch (acao)
        {
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
                // 1. Ativa o modo de edição guardando o medicamento na variável global
                _medicamentoParaEdicao = medicamento;

                // 2. Preenche os campos da tela com os dados atuais do remédio
                entNome.Text = medicamento.Nome;
                entDose.Text = medicamento.Dosagem;
                dtInicio.Date = medicamento.DataInicio;
                dtFim.Date = medicamento.DataFim ?? DateTime.Today;
                chkIsContinuo.IsChecked = medicamento.IsContinuo;

                // 3. Modifica o botão de Salvar para indicar que é uma ATUALIZAÇÃO 13/05 (V)
                btnSalvar.Text = "Atualizar Dados";
                btnSalvar.BackgroundColor = Colors.Orange; // Muda para laranja para dar destaque

                // 4. Avisa o usuário e foca no primeiro campo
                await DisplayAlert("Modo Edição", "Altere os dados e clique em Atualizar Dados.", "OK");
                entNome.Focus();
                break;

            case "Remover":
                bool confirmar = await DisplayAlert("Atenção", $"Deseja interromper o tratamento de {medicamento.Nome}?", "Sim", "Não");
                if (confirmar)
                {
                  
                    // Desativa o medicamento no banco
                    await App.Banco.DesativarMedicamento(medicamento);

                    // Cancela as notificações de Ciclos Automáticos (Mantenha a correção anterior)
                    for (int i = 0; i < 150; i++)
                    {
                        LocalNotificationCenter.Current.Cancel((medicamento.Id * 1000) + i);
                    }

                    await DisplayAlert("Sucesso", "Tratamento encerrado e notificações removidas.", "OK");

 CarregarMedicamentos();
                }
                break;
        }
    }
    private void OnContinuoCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        // Se for contínuo (true), IsVisible da data fim será false
        dtFim.IsVisible = !e.Value;
    }
    // Método para salvar o remédio (campos da tela)
    private async void AoClicarSalvar(object sender, EventArgs e)
    {

        if (string.IsNullOrWhiteSpace(entNome.Text))
        {
            await DisplayAlert("Erro", "Por favor, preencha o nome do remédio.", "OK");
            return;
        }

        // 2. VALIDAÇÃO DE DATA (Inserida aqui para travar o salvamento se estiver errado)10/05 (V)
        if (!chkIsContinuo.IsChecked && dtFim.Date < dtInicio.Date)
        {
            await DisplayAlert("Data Inválida", "A data final não pode ser anterior à data de início.", "OK");
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
                    IsContinuo = chkIsContinuo.IsChecked,
                    // Se for contínuo, DataFim é null. Se não, pega o valor do DatePicker
                    DataFim = chkIsContinuo.IsChecked ? null : dtFim.Date,
                    Ativo = 1,
                    IntervaloHoras = 0
                };

                await App.Banco.InsertMedicamento(novo);

                // PEGA O ID REAL DO BANCO
                var ultimo = await App.Banco.GetUltimoMedicamento();

                
                // mensagem
                await DisplayAlert("Sucesso", "Medicamento cadastrado!", "OK");
            }
            else
            {
                // ✏️ EDITAR
                _medicamentoParaEdicao.Nome = entNome.Text;
                _medicamentoParaEdicao.Dosagem = entDose.Text;
                _medicamentoParaEdicao.DataInicio = dtInicio.Date;
                _medicamentoParaEdicao.IsContinuo = chkIsContinuo.IsChecked;
                _medicamentoParaEdicao.DataFim = dtFim.Date;

                await App.Banco.UpdateMedicamento(_medicamentoParaEdicao);

                await DisplayAlert("Sucesso", "Medicamento atualizado!", "OK");

                // Limpa modo edição
                _medicamentoParaEdicao = null;

            }

            // LIMPEZA E RESET 13/05 (V)

            // 1. Limpa os campos de texto
            entNome.Text = string.Empty;
            entDose.Text = string.Empty;

            // 2. Volta o texto do botão para "Salvar"
            btnSalvar.Text = "Salvar Medicamento";

            // 3. Volta a cor para o azul original do seu XAML
            btnSalvar.BackgroundColor = Color.FromArgb("#0056b3");

            // 4. Limpa a variável global para que o próximo clique seja um NOVO cadastro
            _medicamentoParaEdicao = null;

            // 5. Atualiza a lista na tela
            CarregarMedicamentos();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Não foi possível salvar: " + ex.Message, "OK");
        }
    }

    private async Task GerarCicloAutomatico(int medId, int intervalo)
    {
        try
        {
            // 1. Busca o medicamento para saber o nome e as configurações de data
            var lista = await App.Banco.GetMedicamentosAtivos();
            var medicamento = lista.FirstOrDefault(m => m.Id == medId);
            if (medicamento == null) return;

            medicamento.IntervaloHoras = intervalo;
            await App.Banco.UpdateMedicamento(medicamento);

            List<Dose> listaDosesParaBanco = new List<Dose>();

            // 2. DETERMINA ATÉ QUE DIA VAMOS GERAR AS NOTIFICAÇÕES
            // Se for contínuo, agendamos 30 dias. Se tiver DataFim, usamos a DataFim real (até o fim da noite, 23:59).
            DateTime dataLimite = medicamento.IsContinuo
                ? DateTime.Now.AddDays(30)
                : (medicamento.DataFim?.Date.AddHours(23).AddMinutes(59) ?? DateTime.Now.AddDays(30));

            DateTime horaDose = DateTime.Now;
            int contadorId = 0;

            // 3. O LAÇO VAI AVANÇANDO DE X EM X HORAS E AGENDANDO CADA NOTIFICAÇÃO INDIVIDUALMENTE
            while (horaDose <= dataLimite)
            {
                // Cria o registro da dose para o banco local (Tabela de históricos/cronograma)
                var novaDose = new Dose
                {
                    MedicamentoId = medId,
                    NomeMedicamento = medicamento.Nome,
                    Status = "Pendente",
                    HorarioPrevisto = horaDose
                };
                listaDosesParaBanco.Add(novaDose);

                // Cria a requisição de notificação para esta dose específica
                var notif = new NotificationRequest
                {
                    // Garante um ID único combinando o ID do remédio com o número da dose
                    NotificationId = (medId * 1000) + contadorId,
                    Title = "Hora do Remédio 💊",
                    Description = $"Tomar {medicamento.Nome} ({medicamento.Dosagem})",
                    ReturningData = medId.ToString(),
                    CategoryType = NotificationCategoryType.Status,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = horaDose // Toca exatamente neste dia e hora calculados
                    },
                    Android = new AndroidOptions { LaunchAppWhenTapped = true }
                };

                // Envia para o agendador do celular
                await LocalNotificationCenter.Current.Show(notif);

                // Avança o relógio para o próximo horário (ex: se era 08:00 e o intervalo é 6h, vira 14:00)
                horaDose = horaDose.AddHours(intervalo);
                contadorId++;
            }

            // 4. Salva todas as doses geradas de uma vez só no banco SQLite
            await App.Banco.InsertDoses(listaDosesParaBanco);

            // Mensagem customizada para o usuário
            string mensagemSucesso = medicamento.IsContinuo
                ? $"Ciclo de {intervalo}h criado para os próximos 30 dias!"
                : $"Ciclo de {intervalo}h criado com sucesso até {medicamento.DataFim:dd/MM/yyyy}!";

            await DisplayAlert("Sucesso", mensagemSucesso, "OK");
            CarregarMedicamentos();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro no Ciclo", "Não foi possível agendar o ciclo: " + ex.Message, "OK");
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

    private async void AoClicarVerHistorico(object sender, EventArgs e)
    {
        // Apenas navega para a página de histórico que você criou
        await Navigation.PushAsync(new HistoricoDosesPage());
    }



    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // 1. Busca o nome salvo no login/cadastro. 
        // Se não houver nome, exibe "Usuário".
        string nomeLogado = Preferences.Get("NomeUsuario", "Usuário");

        // 2. Atualiza a Label de boas-vindas
        if (lblBoasVindas != null)
        {
            lblBoasVindas.Text = $"Bem-vindo(a), {nomeLogado}!";
        }

        // 3. Carrega a lista de medicamentos do banco de dados de forma assíncrona
        await CarregarMedicamentos();
    }

    private async Task CarregarMedicamentos()
    {
        try
        {
            // 1. Busca os remédios ativos no banco de dados
            var lista = await App.Banco.GetMedicamentosAtivos();

            // 2. CORREÇÃO: Usa o nome exato que está no seu x:Name do XAML
            listaMedicamentos.ItemsSource = lista;
        }
        catch (Exception ex)
        {
            // Registra o erro caso algo dê errado na busca
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar: {ex.Message}");
        }
    }

    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
   
        try
        {
            var lista = await App.Banco.GetMedicamentos();
            if (lista == null || !lista.Any())
            {
                await DisplayAlert("Prontuário", "Não há registros para compartilhar.", "OK");
                return;
            }

            string cabecalho = $"📋 MEU PRONTUÁRIO - {DateTime.Now:dd/MM/yyyy}\n";
            cabecalho += "------------------------------------------\n\n";
            string corpo = "";

            foreach (var m in lista)
            {
                string status = m.Ativo == 1 ? "✅ Ativo" : "❌ Inativo";
                corpo += $"💊 Remédio: {m.Nome}\n   " +
                         $"Dose: {m.Dosagem}\n   " +
                         $"Início: {m.DataInicio:dd/MM/yyyy}\n   " +
                         $"Status: {status}\n";
                corpo += "------------------------------------------\n";
            }

            string hashSeguro = SecurityHelper.GerarHash(corpo);
            string textoFinal = cabecalho + corpo + $"\nHash de Segurança: {hashSeguro}\n\n" +
                                                    $"Nota: O código acima é uma assinatura digital gerada pelo aplicativo para garantir a integridade e a origem autêntica deste prontuário.\n" +
                                                    $"Gerado pelo App Meu Remédio.";

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "Compartilhar Prontuário",
                Text = textoFinal
            });
        }
        catch (Exception ex) { await DisplayAlert("Erro", ex.Message, "OK"); }
    }

}
    