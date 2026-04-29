using AppLembreteMedicacao.Helpers;
using AppLembreteMedicacao.Views;
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
        if (string.IsNullOrWhiteSpace(e.Request.ReturningData)) return;

        var idStr = e.Request.ReturningData.Replace("id=", "");

        if (int.TryParse(idStr, out int idMed))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var navPage = MainPage as NavigationPage;
                if (navPage != null)
                {
                    await navPage.PushAsync(new ConfirmacaoPage(idMed));
                }
            });
        }
    }
}