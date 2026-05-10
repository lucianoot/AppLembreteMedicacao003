using AppLembreteMedicacao.Models;
using System.Collections.ObjectModel;

namespace AppLembreteMedicacao.Views
{
    public partial class HistoricoDosesPage : ContentPage
    {
        // Criamos a coleção que será ligada à CollectionView do XAML
        public ObservableCollection<HistoricoDose> Lista { get; set; } = new ObservableCollection<HistoricoDose>();

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
                // Busca os dados do banco usando o Helper e converte para o modelo de exibição
                var dados = await App.Banco.GetHistoricoParaExibicao();

                // Limpa a lista atual e preenche com os novos dados
                Lista.Clear();
                foreach (var item in dados)
                {
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
