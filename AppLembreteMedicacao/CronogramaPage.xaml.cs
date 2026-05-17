using AppLembreteMedicacao.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System.Threading.Tasks;

namespace AppLembreteMedicacao.Views
{
    public partial class CronogramaPage : ContentPage
    {
        public int MedicamentoId { get; set; }

        // Adicionei ", string nome = """ aqui para aceitar o nome do remédio
        public CronogramaPage(int medicamentoId, string nome = "")
        {
            InitializeComponent();
            MedicamentoId = medicamentoId;

            // Se o nome foi enviado, ele vira o título da página lá no topo
            if (!string.IsNullOrEmpty(nome))
            {
                Title = $"Horários: {nome}";
            }

            // Feedback visual no botão (seu código original)
            btnAdicionarHorario.Pressed += async (s, e) => await btnAdicionarHorario.ScaleTo(1.1, 100);
            btnAdicionarHorario.Released += async (s, e) => await btnAdicionarHorario.ScaleTo(1, 100);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarCronogramas();
        }

        private async Task CarregarCronogramas()
        {
            try
            {
                var cronogramas = await App.Banco.GetCronogramaPorMedicamento(MedicamentoId);
                listaCronogramas.ItemsSource = cronogramas;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Falha ao carregar horários: " + ex.Message, "OK");
            }
        }

        private async void AoClicarAdicionarHorario(object sender, EventArgs e)
        {
            string hora = await DisplayPromptAsync(
                "Adicionar Horário", "Digite a hora (Ex: 08:00):",
                placeholder: "08:00", maxLength: 5, keyboard: Keyboard.Text);

            if (string.IsNullOrWhiteSpace(hora)) return;

            string frequencia = await DisplayPromptAsync(
                "Frequência", "Ex: Diária, 8 em 8 horas, etc:",
                initialValue: "diária", keyboard: Keyboard.Text);

            if (string.IsNullOrWhiteSpace(frequencia)) return;

            var novo = new Cronograma
            {
                MedicamentoId = MedicamentoId,
                Hora = hora,
                Frequencia = frequencia,
                Ativo = 1
            };

            // Insere no banco e recupera o ID
            int idGerado = await App.Banco.InsertCronograma(novo);
            novo.Id = idGerado;

            // Agendamento
            await AgendarNotificacao(hora, novo.Id);

            await CarregarCronogramas();
            await DisplayAlert("Sucesso", "Horário e Lembrete configurados!", "OK");
        }

        private async Task AgendarNotificacao(string horaStr, int cronogramaId)
        {
            try
            {
                // AJUSTE : Cancela qualquer notificação com este ID antes de criar uma nova.
                // Isso impede que o sistema operacional mantenha dois agendamentos para o mesmo horário.
                LocalNotificationCenter.Current.Cancel(cronogramaId);
                var med = await App.Banco.GetMedicamentoPorId(MedicamentoId);

                if (TimeSpan.TryParse(horaStr, out TimeSpan horarioSelecionado))
                {
                    DateTime dataNotificacao = DateTime.Today.Add(horarioSelecionado);
                    // Se o horário já passou hoje, agenda para amanhã
                    if (dataNotificacao < DateTime.Now)
                        dataNotificacao = dataNotificacao.AddDays(1);

                    var request = new NotificationRequest
                    {
                        NotificationId = cronogramaId,
                        Title = "💊 Hora do Remédio!",
                        Description = $"Tomar {med.Nome} agora.",
                        ReturningData = $"id={MedicamentoId}",
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = dataNotificacao,
                            RepeatType = NotificationRepeat.Daily // Repete diariamente
                        },
                        Android = new AndroidOptions
                        {
                            // Garante que a notificação apareça mesmo com o app fechado
                            Priority = AndroidPriority.High
                        }
                    };

                    await LocalNotificationCenter.Current.Show(request);
                    System.Diagnostics.Debug.WriteLine($"[TCC] Notificação agendada com sucesso para: {dataNotificacao} (ID: {cronogramaId})");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[TCC] Erro crítico ao agendar notificação: " + ex.Message);
            }
        }


        private async void AoClicarRemoverHorario(object sender, EventArgs e)
        {
            //  Validação do sender e do ID
            if (sender is not Button button || button.CommandParameter is not int id)
                return;

            try
            {
                // Confirmação com o usuário (Evita exclusões acidentais)
                bool confirmar = await DisplayAlert("Confirmação", "Deseja excluir este horário de lembrete?", "Sim", "Não");
                if (!confirmar) return;

                // 3. esabilita o botão para evitar cliques duplos durante o processamento
                button.IsEnabled = false;
                // Remoção no Banco de Dados SQLite
                await App.Banco.DeleteCronograma(id);

                // Remoção da Notificação Agendada no Sistema
                // Isso garante que o celular pare de tocar para este horário específico
                LocalNotificationCenter.Current.Cancel(id);

                // 6. Atualiza a lista na tela
                await CarregarCronogramas();

                // Feedback de sucesso (Opcional, mas melhora a UX)
                // await DisplayAlert("Sucesso", "Horário removido com sucesso.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Não foi possível remover o horário: " + ex.Message, "OK");
            }
            finally
            {
                // Garante que o botão volte a ficar ativo se necessário
                button.IsEnabled = true;
            }
        }
    }
}
