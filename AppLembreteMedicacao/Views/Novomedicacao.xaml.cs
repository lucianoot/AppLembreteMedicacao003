namespace AppLembreteMedicacao.Views;
using Plugin.LocalNotification; //07/04
using AppLembreteMedicacao.Models; // 07/04
using System.Collections.Generic;

public partial class Novomedicacao : ContentPage
{
    public Novomedicacao()
    {
        InitializeComponent();
    }
    private async void OnMonitoramentoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Monitoramento());
    }
    async void OnSalvarClicked(object sender, EventArgs e)
    {
        // objeto com os dados da tela
        var novoMed = new Medicamento
        {
            Nome = txtNome.Text, 
            Dosagem = txtDosagem.Text,
            IntervaloHoras = int.Parse(txtIntervalo.Text),

            // TIMEZONE: Pegamos a data/hora local e convertemos para UTC para o banco
            DataInicio = DateTime.Now.ToUniversalTime(),
            Ativo = 1 // Garante que ele comece ativo
    };

        // 2. Salvamos no Banco de Dados 
        // await App.Database.SaveMedicamentoAsync(novoMed);
        await App.Banco.InsertMedicamento(novoMed);

        // --- INÍCIO DA ALTERAÇÃO CLASSE DOSE ---
        // Gerar as primeiras 3 doses previstas para este medicamento
        var listaDoses = new List<Dose>();

        for (int i = 1; i <= 3; i++)
        {
            var novaDose = new Dose
            {
                MedicamentoId = novoMed.Id, // Vincula à chave estrangeira do remédio
                NomeMedicamento = novoMed.Nome,
                HorarioPrevisto = DateTime.Now.AddHours(novoMed.IntervaloHoras * i),
                Status = "Pendente" // Status inicial
            };
            listaDoses.Add(novaDose);
        }

        // Salva a lista de doses no SQLite
        await App.Banco.InsertDoses(listaDoses);

        // 3. AGENDAMENTO DA NOTIFICAÇÃO
        var request = new NotificationRequest
        {
            NotificationId = novoMed.Id,
            Title = "Hora do seu Remédio!",
            Description = $"Tomar: {novoMed.Nome} - {novoMed.Dosagem}",
            // O segredo está aqui: enviamos o ID nos dados de retorno
            ReturningData = novoMed.Id.ToString(),
            Schedule = new NotificationRequestSchedule
            {
                // O Plugin cuida da conversão de volta para o horário do celular
                NotifyTime = novoMed.DataInicio.ToLocalTime(),
                RepeatType = NotificationRepeat.TimeInterval,
                NotifyRepeatInterval = TimeSpan.FromHours(novoMed.IntervaloHoras)
            }
        };

        await LocalNotificationCenter.Current.Show(request);

        await DisplayAlert("Sucesso", "Lembrete configurado!", "OK");
        await Navigation.PopAsync();

    }

    private Medicamento _medicamentoParaEdicao;

    // Construtor para EDITAR (preenche os campos com os dados do banco)
    public Novomedicacao(Medicamento med)
    {
        InitializeComponent();
        _medicamentoParaEdicao = med;

        // Preenche a tela com os valores atuais para você poder mudar
        txtNome.Text = med.Nome;
        txtDosagem.Text = med.Dosagem;
        // Ajuste para carregar o intervalo na edição
        if (txtIntervalo != null)
            txtIntervalo.Text = med.IntervaloHoras.ToString();
    }
}