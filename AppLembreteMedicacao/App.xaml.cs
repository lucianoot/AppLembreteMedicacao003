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

        if (Banco == null)
        {
            string caminho = Path.Combine(FileSystem.AppDataDirectory, "remedios.db3");
            Banco = new SQLiteDatabaseHelper(caminho);
        }

        // VERIFICA SE JÁ TEM USUÁRIO LOGADO
        string email = Preferences.Get("usuarioLogado", "");

        if (!string.IsNullOrEmpty(email))
        {
            // Já está logado → vai direto pro app
            MainPage = new NavigationPage(new MainPage());
        }
        else
        {
            // Não está logado → vai pro login
            MainPage = new NavigationPage(new Login());
        }

        // Escuta o clique na notificação
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationTapped;
    }

    private void OnNotificationTapped(NotificationActionEventArgs e)
    {
        // Verificamos se há dados retornando (o ID que passamos no passo 1)
        if (!string.IsNullOrWhiteSpace(e.Request.ReturningData)) return;
        {
            if (int.TryParse(e.Request.ReturningData, out int idMed))
            {
                // Forçamos a abertura da tela de Confirmação na Thread principal
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Se o app estiver usando NavigationPage
                    try
                    {
                        // Se o app já estiver usando NavigationPage, apenas damos o Push
                        if (MainPage is NavigationPage navPage)
                        {
                            await navPage.PushAsync(new ConfirmacaoPage(idMed));
                        }
                        else
                        {
                            // Caso a estrutura de navegação tenha se perdido, reiniciamos com a página de confirmação
                            MainPage = new NavigationPage(new ConfirmacaoPage(idMed));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao navegar: {ex.Message}");
                    }
                });
            }
        }
    }
}