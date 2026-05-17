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
                string.IsNullOrWhiteSpace(txtSobrenome.Text) ||
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
                Sobrenome = txtSobrenome.Text,
                Email = txtEmail.Text,
                SenhaHash = "123",
                TipoPerfil = pickerPerfil.SelectedItem.ToString()

            };
            try
            {
                await App.Banco.InsertUsuario(usuario);

                await DisplayAlert("Sucesso", "Usuário cadastrado!", "OK");
                txtNome.Text = "";
                txtSobrenome.Text = "";
                txtEmail.Text = "";
                pickerPerfil.SelectedIndex = -1;
                // Se for Médico ou Responsável, pula a MainPage e vai para o Monitoramento
                if (usuario.TipoPerfil == "Médico" || usuario.TipoPerfil == "Responsável")
                {
                    await Navigation.PushAsync(new Monitoramento());
                }
                else
                {
                    // Se for Paciente, vai para a tela de cadastrar medicamentos
                    await Navigation.PushAsync(new MainPage());
                }
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