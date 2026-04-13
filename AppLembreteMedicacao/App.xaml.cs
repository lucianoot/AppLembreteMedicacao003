using AppLembreteMedicacao.Helpers;
using AppLembreteMedicacao.Views;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

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

        MainPage = new NavigationPage(new MainPage());

        // Escuta o clique na notificação
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationTapped;
    }

    private void OnNotificationTapped(NotificationActionEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Request.ReturningData)) return;

        // Extrai o ID (ex: "id=5")
        var idStr = e.Request.ReturningData.Replace("id=", "");

        if (int.TryParse(idStr, out int idMed))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var navPage = MainPage as NavigationPage;
                if (navPage != null)
                {
                    // Abre a tela de confirmação passando o ID
                    await navPage.PushAsync(new ConfirmacaoPage(idMed));
                }
            });
        }
    }
}