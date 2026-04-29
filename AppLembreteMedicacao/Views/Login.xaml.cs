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

        await DisplayAlert("Sucesso", "Login realizado!", "OK");

        //Ir para tela de medicamento
        await Navigation.PushAsync(new Novomedicacao());
    }
}
