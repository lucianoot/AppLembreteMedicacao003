using AppLembreteMedicacao.Models;
using System.Linq;


namespace AppLembreteMedicacao.Views;
public partial class Monitoramento : ContentPage
{

private int _medicamentoId = 0;
    
    // CONSTRUTOR VAZIO (Para o Médico/Responsável conseguir abrir a tela)
    public Monitoramento()
{
    InitializeComponent();
    // Construtor para quando você clica na lista
}
public Monitoramento(int medicamentoId)
{
    InitializeComponent();
    _medicamentoId = medicamentoId;

}
protected override async void OnAppearing()
{
    base.OnAppearing();

        // 1. Lógica de Saudação para o Usuário Logado (Médico ou Responsável)
        ConfigurarSaudacaoUsuario();

        // 2. Carrega os dados do Paciente e Histórico
        await CarregarHistorico();
    }
private void ConfigurarSaudacaoUsuario()
    {
        // Recupera os dados salvos no login
        string nomeLogado = Preferences.Get("NomeUsuario", "Usuário");
        string perfil = Preferences.Get("TipoPerfil", "Usuário");

        if (perfil == "Médico")
        {
            lblSaudacaoLogado.Text = $"Bem-vindo(a) {nomeLogado}!";
            lblFraseApoio.Text = "Acompanhe aqui a evolução do tratamento do seu paciente.";
        }
        else if (perfil == "Responsável")
        {
            lblSaudacaoLogado.Text = $"Bem-vindo(a) {nomeLogado}!";
            lblFraseApoio.Text = "Confira se os medicamentos foram administrados corretamente.";
        }
        else
        {
            lblSaudacaoLogado.Text = $"Bem-vindo(a) {nomeLogado}!";

        }
        List<HistoricoUso> listaBruta;
    }

    private async Task CarregarHistorico()
    {
        try
        {
// Busca o paciente diretamente no banco SQLite
            var paciente = await App.Banco.GetPaciente();

            if (paciente != null)
            {
                //Concatena o Nome e o Sobrenome e transforma tudo em CAIXA ALTA
                    string nomeCompleto = $"{paciente.Nome} {paciente.Sobrenome}".Trim();
                lblNomePaciente.Text = nomeCompleto.ToUpper();
            }
            else
            {
                lblNomePaciente.Text = "PACIENTE NÃO ENCONTRADO";
            }

           List<HistoricoUso> listaBruta;

            if (_medicamentoId == 0)
                listaBruta = await App.Banco.GetTodosHistorico();
            else
                listaBruta = await App.Banco.GetHistorico(_medicamentoId);

            // BUSCA A LISTA DE MEDICAMENTOS PARA SABER QUEM ESTÁ ATIVO
            var listaMedicamentos = await App.Banco.GetMedicamentos();

            if (listaBruta != null && listaBruta.Any())
            {
          
            // 1. Lógica para a Lista Detalhada
            var listaFormatada = listaBruta.Select(h => new
            {
                NomeMedicamento = h.NomeMedicamento,
                DataUso = h.DataUso.ToString("dd/MM/yyyy HH:mm"),
                TomadoTexto = h.Tomado ? "Tomou ✔" : "Não tomou ❌",
                CorStatus = h.Tomado ? Colors.Green : Colors.Red
            }).ToList();

            listaHistorico.ItemsSource = listaFormatada;
                var medicamentosAtuais = await App.Banco.GetMedicamentos();
                // 2.Cálculo de Adesão por Medicamento
                var resumoAdesao = listaBruta
                .GroupBy(h => h.NomeMedicamento)
                .Select(g =>
                {
                    // Se o nome do histórico não estiver na lista de ativos, foi EXCLUÍDO
                    var medNoBanco = medicamentosAtuais.FirstOrDefault(m => m.Nome == g.Key);
                    bool excluido = medNoBanco == null || medNoBanco.Ativo == 0;
                    bool continuo = medNoBanco != null && medNoBanco.IsContinuo;

                    int total = g.Count();
                    int tomadas = g.Count(x => x.Tomado);
                    double perc = total > 0 ? (double)tomadas / total : 0;

                    // Lógica de texto de status dinâmica
                    string textoStatus;
                    if (excluido)
                        textoStatus = "EXCLUÍDO";
                    else if (continuo)
                        textoStatus = "USO CONTÍNUO";
                    else
                        textoStatus = "EM TRATAMENTO";

                    return new
                    {
                        Nome = g.Key,
                        Total = total,
                        Tomadas = tomadas,
                        Percentual = perc,
                        CorAdesao = perc >= 0.8 ? Colors.Green : Colors.Orange,

                        StatusTexto = textoStatus,
                        CorStatusTexto = excluido ? Colors.Red : (continuo ? Colors.DarkBlue : Color.FromArgb("#2563EB")),
                        Opacidade = excluido ? 0.5 : 1.0
                    };
                })
    .ToList();

                listaEstatisticas.ItemsSource = resumoAdesao;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao processar monitoramento: " + ex.Message, "OK");
        }
    }
    private async void OnSairClicked(object sender, EventArgs e)
{
    bool confirm = await DisplayAlert("Sair", "Deseja realmente deslogar?", "Sim", "Não");
    if (confirm)
    {
        // Volta para a tela de login para permitir trocar de usuário
        Application.Current.MainPage = new NavigationPage(new Login());
    }
}
}