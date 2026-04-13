using AppLembreteMedicacao.Models;
using Plugin.LocalNotification;
using System.Threading.Tasks;

namespace AppLembreteMedicacao.Views
{
    public partial class CronogramaPage : ContentPage
    {
        public int MedicamentoId { get; set; }

        public CronogramaPage(int medicamentoId)
        {
            InitializeComponent();
            MedicamentoId = medicamentoId;

            // Feedback visual no botão
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
                var med = await App.Banco.GetMedicamentoPorId(MedicamentoId);

                if (TimeSpan.TryParse(horaStr, out TimeSpan horarioSelecionado))
                {
                    DateTime dataNotificacao = DateTime.Today.Add(horarioSelecionado);

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
                            RepeatType = NotificationRepeat.Daily
                        }
                    };

                    await LocalNotificationCenter.Current.Show(request);
                    System.Diagnostics.Debug.WriteLine($"[TCC] Sucesso ao agendar: {dataNotificacao}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[TCC] Erro ao agendar: " + ex.Message);
            }
        }

       
        private async void AoClicarRemoverHorario(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not int id) return;

            bool confirmar = await DisplayAlert("Atenção", "Remover este horário?", "Sim", "Não");
            if (!confirmar) return;

            await App.Banco.DeleteCronograma(id);
            LocalNotificationCenter.Current.Cancel(id);

            await CarregarCronogramas();
        }
    }
}