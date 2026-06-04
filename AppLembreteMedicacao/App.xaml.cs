using AppLembreteMedicacao.Helpers;
using AppLembreteMedicacao.Views;
using AppLembreteMedicacao.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
using Microsoft.Maui.Storage;

namespace AppLembreteMedicacao;

public partial class App : Application
{
    public static SQLiteDatabaseHelper Banco { get; private set; }

    public App()
    {
        InitializeComponent();

        // 1. Inicializa o Banco de Dados
        if (Banco == null)
        {
            string caminho = Path.Combine(FileSystem.AppDataDirectory, "remedios.db3");
            Banco = new SQLiteDatabaseHelper(caminho);
        }

        // 2. CORREÇÃO DO ERRO DE LÓGICA: Varre o banco e limpa alarmes de remédios que já venceram
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await LimparNotificacoesVencidas();
        });

        // 3. Verificação de Sessão do Usuário (Navegação Inicial)
        string email = Preferences.Get("usuarioLogado", "");
        string tipoUsuario = Preferences.Get("TipoUsuario", "");

        if (!string.IsNullOrEmpty(email))
        {
            switch (tipoUsuario)
            {
                case "Paciente":
                    MainPage = new NavigationPage(new MainPage());
                    break;

                case "Médico":
                case "Responsável":
                    MainPage = new NavigationPage(new Monitoramento());
                    break;

                default:
                    MainPage = new NavigationPage(new Login());
                    break;
            }
        }
        else
        {
            MainPage = new NavigationPage(new Login());
        }

        // 4. PROTEÇÃO PARA WINDOWS MACHINE: Escuta o clique na notificação apenas no Android
        #if ANDROID
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationTapped;
        #endif
    }

    // FUNÇÃO QUE RESOLVE O BUG DAS NOTIFICAÇÕES 
    private async Task LimparNotificacoesVencidas()
    {
        try
        {
            // 1. Busca os remédios cadastrados que ainda estão marcados como ativos (Ativo == 1)
            var todosRemedios = await App.Banco.GetMedicamentosAtivos();
            DateTime hoje = DateTime.Today;

            foreach (var remedio in todosRemedios)
            {
                // 2. Verifica se o remédio NÃO é de uso contínuo (tem DataFim) 
                // e se a DataFim já passou do dia de hoje
                if (remedio.DataFim.HasValue && remedio.DataFim.Value.Date < hoje)
                {
                    // 3. Cancela o agendamento no sistema operacional pelo ID do remédio
                    LocalNotificationCenter.Current.Cancel(remedio.Id);

                    // 4. CORREÇÃO: Altera o status para 0 (int) para desativar no banco
                    remedio.Ativo = 0;

                    // Grava a alteração no banco de dados
                    await App.Banco.UpdateMedicamento(remedio);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao limpar notificações antigas: {ex.Message}");
        }
    }

    // PROCESSAMENTO DO CLIQUE DA NOTIFICAÇÃO (RODA APENAS NO MOBILE)
    private void OnNotificationTapped(NotificationActionEventArgs e)
    {
        int acao = e.ActionId;
        string data = e.Request.ReturningData;

        if (int.TryParse(data, out int idMed))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var lista = await App.Banco.GetMedicamentosAtivos();
                    var med = lista.FirstOrDefault(m => m.Id == idMed);

                    string nomeMed = med?.Nome ?? "Medicamento";

                    if (acao == 100 || acao == 101) // 100 = Tomado, 101 = Pular
                    {
                        bool foiTomado = acao == 100;
                        
                        // REGISTRO NO HISTÓRICO
                        var registro = new HistoricoUso
                        {
                            MedicamentoId = idMed,
                            DataUso = DateTime.Now,
                            Tomado = foiTomado,
                            NomeMedicamento = nomeMed
                        };

                        await App.Banco.InsertHistorico(registro);

                        // ATUALIZA A DOSE NO BANCO
                        if (foiTomado)
                        {
                            await App.Banco.AtualizarDoseParaTomado(idMed);
                        }

                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Lembrete",
                                $"{nomeMed} marcado como {(foiTomado ? "tomado ✅" : "pulado ⚠️")}.",
                                "OK");
                        }
                    }
                    else
                    {
                        // Clique normal na notificação (abre a página de detalhes)
                        if (MainPage is NavigationPage navPage)
                        {
                            await navPage.PushAsync(new ConfirmacaoPage(idMed));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao processar notificação: {ex.Message}");
                }
            });
        }
    }
}