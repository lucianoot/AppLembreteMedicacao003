using AppLembreteMedicacao.Models;
using System.Collections.ObjectModel;

namespace AppLembreteMedicacao.Views
{
    public partial class HistoricoDosesPage : ContentPage
    {
        // CORREÇÃO: Mudamos de HistoricoDose para HistoricoUso
        public ObservableCollection<HistoricoUso> Lista { get; set; } = new ObservableCollection<HistoricoUso>();

        public HistoricoDosesPage()
        {
            InitializeComponent();

            // Vinculamos a lista do C# com o x:Name="listaHistorico" do seu XAML
            listaHistorico.ItemsSource = Lista;
        }

        // Atualiza os dados toda vez que a tela for aberta
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarHistorico();
        }

        private async Task CarregarHistorico()
        {
            try
            {
                // Busca os dados atualizados do banco usando a classe HistoricoUso
                var dados = await App.Banco.GetHistoricoParaExibicao();

                // Limpa a lista atual e preenche com os novos dados
                Lista.Clear();
                foreach (var item in dados)
                {
                    // Agora o item adicionado é do tipo HistoricoUso
                    Lista.Add(item);
                }
            }
            catch (Exception ex)
            {
                // Caso ocorra algum erro no banco de dados, exibe um alerta
                await DisplayAlert("Erro", "Não foi possível carregar o histórico: " + ex.Message, "OK");
            }
        }
    }
}
