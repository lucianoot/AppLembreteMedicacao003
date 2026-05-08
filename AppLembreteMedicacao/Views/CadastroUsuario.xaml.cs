using AppLembreteMedicacao.Models;
using AppLembreteMedicacao.Views;
namespace AppLembreteMedicacao.Views
{
    public partial class CadastroUsuario : ContentPage
    {
        public CadastroUsuario()
        {
            InitializeComponent();
        }

        private async void OnSalvarClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                pickerPerfil.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Preencha todos os campos", "OK");
                return;
            }

            bool existe = await App.Banco.UsuarioExiste(txtEmail.Text);

            if (existe)
            {
                await DisplayAlert("Atenção", "Email já cadastrado!", "OK");
                return;
            }

            var usuario = new Usuario
            {
                Nome = txtNome.Text,
                Email = txtEmail.Text,
                SenhaHash = "123",
                TipoPerfil = pickerPerfil.SelectedItem.ToString()

            };
            try
            {
                await App.Banco.InsertUsuario(usuario);

                await DisplayAlert("Sucesso", "Usuário cadastrado!", "OK");
                txtNome.Text = "";
                txtEmail.Text = "";
                pickerPerfil.SelectedIndex = -1;
                // IR PARA TELA DE MEDICAMENTO
                await Navigation.PushAsync(new Novomedicacao());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }

        }
        // BOTÃO "JÁ SOU CADASTRADO"
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Login());
        }
    }
}