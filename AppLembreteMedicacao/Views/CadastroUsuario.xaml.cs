using AppLembreteMedicacao.Models;
using AppLembreteMedicacao.Views;
using AppLembreteMedicacao.Helpers;

namespace AppLembreteMedicacao.Views;

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
            string.IsNullOrWhiteSpace(txtSenha.Text) ||
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
            SenhaHash = SecurityHelper.GerarHash(txtSenha.Text),
            TipoPerfil = pickerPerfil.SelectedItem.ToString()
        };

        try
        {
            // 1. Grava no Banco SQLite
            await App.Banco.InsertUsuario(usuario);

            // 2. Salva imediatamente na sessão local do aparelho
            Preferences.Set("usuarioLogado", usuario.Email);
            Preferences.Set("NomeUsuario", usuario.Nome);
            Preferences.Set("TipoPerfil", usuario.TipoPerfil);

            await DisplayAlert("Sucesso", "Usuário cadastrado com sucesso!", "OK");

            // 3. Limpa os campos da tela
            txtNome.Text = "";
            txtSobrenome.Text = "";
            txtEmail.Text = "";
            txtSenha.Text = "";
            pickerPerfil.SelectedIndex = -1;

            // 4. Redirecionamento correto resetando a MainPage
            if (usuario.TipoPerfil == "Médico" || usuario.TipoPerfil == "Responsável")
            {
                Application.Current.MainPage = new NavigationPage(new Monitoramento());
            }
            else
            {
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    // BOTÃO "JÁ SOU CADASTRADO" 
    public async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Login());
    }
}