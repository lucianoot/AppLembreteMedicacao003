using AppLembreteMedicacao.Models;
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
                TipoPerfil = pickerPerfil.SelectedItem.ToString()
            };

            await App.Banco.InsertUsuario(usuario);

            await DisplayAlert("Sucesso", "Usuário cadastrado!", "OK");

            txtNome.Text = "";
            txtEmail.Text = "";
            pickerPerfil.SelectedIndex = -1;
        }
    }
}