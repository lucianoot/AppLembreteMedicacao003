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

        string email = Preferences.Get("usuarioLogado", "");
        string tipoUsuario = Preferences.Get("TipoUsuario", "");

        // Se já tem usuário logado
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
                    // Se não tiver tipo definido, manda pro login
                    MainPage = new NavigationPage(new Login());
                    break;
            }
        }
        else
        {
            // Não está logado
            MainPage = new NavigationPage(new Login());
        }


        // Escuta o clique na notificação
        LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationTapped;
    }

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

                    if (acao == 100 || acao == 101)// 100 = Tomado, 101 = Pular
                    {
                        bool foiTomado = acao == 100;
                        // 👉 REGISTRO NO HISTÓRICO
                        var registro = new HistoricoUso
                        {
                            MedicamentoId = idMed,
                            DataUso = DateTime.Now,
                            Tomado = foiTomado,
                            NomeMedicamento = nomeMed
                        };

                        await App.Banco.InsertHistorico(registro);

                        // 👉 ATUALIZA A DOSE NO BANCO (Caso queira marcar a dose como concluída)
                        if (foiTomado)
                        {
                            await App.Banco.AtualizarDoseParaTomado(idMed);
                        }

                        // USAR MainPage em vez de Shell.Current se não estiver usando Shell
                        await Application.Current.MainPage.DisplayAlert(
                            "Lembrete",
                            $"{nomeMed} marcado como {(foiTomado ? "tomado ✅" : "pulado ⚠️")}.",
                            "OK");
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