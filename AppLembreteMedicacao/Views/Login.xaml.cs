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
        // 1. Validação de campos vazios
        if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            await DisplayAlert("Atenção", "Por favor, preencha o e-mail e a senha.", "OK");
            return;
        }

        // 2. Busca o usuário no banco de dados SQLite
        var usuario = await App.Banco.GetUsuarioEmail(txtEmail.Text);

        // 3. Validação de existência e senha
        // Nota: Em um sistema real, aqui você usaria a comparação de Hash de senha
        if (usuario == null || usuario.SenhaHash != txtSenha.Text)
        {
            await DisplayAlert("Erro", "E-mail ou senha incorretos.", "OK");
            return;
        }

        // 4. PERSISTÊNCIA DE SESSÃO
        // Salvamos o primeiro nome separado no cadastro para a saudação dinâmica
        Preferences.Set("usuarioLogado", usuario.Email);
        Preferences.Set("perfilUsuario", usuario.TipoPerfil);
        Preferences.Set("NomeUsuario", usuario.Nome);

        // 5. REDIRECIONAMENTO POR PERFIL
        if (usuario.TipoPerfil == "Paciente")
        {
            // Define a MainPage como a página inicial (reseta a pilha de navegação)
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
        else if (usuario.TipoPerfil == "Médico" || usuario.TipoPerfil == "Responsável")
        {
            // Navega para a tela de monitoramento
            await Navigation.PushAsync(new Monitoramento());
        }
        else
        {
            // Fallback para garantir que o usuário não fique preso
            await Navigation.PushAsync(new ListaMedicacao());
        }
    }

    private async void OnCadastroClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CadastroUsuario());
    }
}