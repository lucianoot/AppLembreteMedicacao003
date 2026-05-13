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

    // Variável de controle para evitar processamento duplo em cliques rápidos
    private bool _estaProcessandoNotificacao = false;

    public App()
    {
        InitializeComponent();

        if (Banco == null)
        {
            string caminho = Path.Combine(FileSystem.AppDataDirectory, "remedios.db3");
            Banco = new SQLiteDatabaseHelper(caminho);
        }

        DefinirPaginaInicial();

        // Escuta o clique na notificação
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationTapped;
    }

    private void DefinirPaginaInicial()
    {
        string email = Preferences.Get("usuarioLogado", "");
        string tipoUsuario = Preferences.Get("TipoUsuario", "");

        if (!string.IsNullOrEmpty(email))
        {
            MainPage = tipoUsuario switch
            {
                "Paciente" => new NavigationPage(new MainPage()),
                "Médico" or "Responsável" => new NavigationPage(new Monitoramento()),
                _ => new NavigationPage(new Login()),
            };
        }
        else
        {
            MainPage = new NavigationPage(new Login());
        }
    }

    private void OnNotificationTapped(NotificationActionEventArgs e)
    {
        if (_estaProcessandoNotificacao) return;
        _estaProcessandoNotificacao = true;

        // Extração do ID do medicamento do ReturningData
        string rawData = e.Request.ReturningData;
        int idMed = 0;
        if (!string.IsNullOrEmpty(rawData))
        {
            // Se você usa "id=10", isso limpa o texto e pega só o número
            string apenasNumeros = rawData.Replace("id=", "");
            int.TryParse(apenasNumeros, out idMed);
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // SEMPRE cancela a notificação para limpar a barra de status
                LocalNotificationCenter.Current.Cancel(e.Request.NotificationId);

                if (idMed <= 0) return;

                // CASO 1: Usuário clicou em um dos botões (Tomar ou Pular)
                if (e.ActionId == 100 || e.ActionId == 101)
                {
                    bool foiTomado = e.ActionId == 100;
                    var med = await Banco.GetMedicamentoPorId(idMed);
                    string nomeMed = med?.Nome ?? "Medicamento";

                    var registro = new HistoricoUso
                    {
                        MedicamentoId = idMed,
                        DataUso = DateTime.Now,
                        Tomado = foiTomado,
                        NomeMedicamento = nomeMed
                    };

                    await Banco.InsertHistorico(registro);
                    if (foiTomado) await Banco.AtualizarDoseParaTomado(idMed);

                    await MainPage.DisplayAlert("Lembrete",
                        $"{nomeMed} marcado como {(foiTomado ? "tomado ✅" : "pulado ⚠️")}.", "OK");
                }
                // CASO 2: Usuário clicou no corpo da notificação (ActionId padrão é 0 ou diferente de 100/101)
                else
                {
                    if (MainPage is NavigationPage navPage)
                    {
                        // Verifica se a página atual já não é a de confirmação para não abrir duplicado
                        var paginaAtual = navPage.Navigation.NavigationStack.LastOrDefault();
                        if (paginaAtual is not ConfirmacaoPage)
                        {
                            await navPage.PushAsync(new ConfirmacaoPage(idMed));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TCC] Erro ao processar: {ex.Message}");
            }
            finally
            {
                // Libera para o próximo clique após um pequeno delay de segurança
                await Task.Delay(500);
                _estaProcessandoNotificacao = false;
            }
        });
    }
}