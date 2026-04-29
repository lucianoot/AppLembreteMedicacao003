using AppLembreteMedicacao.Models;
using AppLembreteMedicacao.Views;
namespace AppLembreteMedicacao.Views;

public partial class Login : ContentPage
{
    public Login()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
            string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            await DisplayAlert("Erro", "Preencha todos os campos", "OK");
            return;
        }

        var usuario = await App.Banco.GetUsuarioEmail(txtEmail.Text);

        if (usuario == null || usuario.SenhaHash != txtSenha.Text)
        {
            await DisplayAlert("Erro", "Email ou senha inválidos", "OK");
            return;
        }
        // SALVA USUÁRIO LOGADO
        Preferences.Set("usuarioLogado", usuario.Email);

        await DisplayAlert("Sucesso", "Login realizado!", "OK");

        // Redirecionamento por perfil
        if (usuario.TipoPerfil == "Paciente")
        {
            await Navigation.PushAsync(new Novomedicacao());
        }
        else
        {
            await Navigation.PushAsync(new Monitoramento());
        }
        
    }
}
