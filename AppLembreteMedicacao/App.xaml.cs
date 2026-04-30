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
            MainPage = new NavigationPage(new Novomedicacao());
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
                    if (MainPage is NavigationPage navPage)
                    {
                        await navPage.PushAsync(new ConfirmacaoPage(idMed));
                    }
                    else
                    {
                        // Caso não esteja em uma NavigationPage, criamos uma
                        MainPage = new NavigationPage(new ConfirmacaoPage(idMed));
                    }
                });
            }
        }
    }
}   