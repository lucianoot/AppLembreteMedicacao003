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
        // Validação de campos vazios
        if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
            string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            await DisplayAlert("Erro", "Preencha todos os campos", "OK");
            return;
        }

        // Busca o usuário no banco
        var usuario = await App.Banco.GetUsuarioEmail(txtEmail.Text);

        // Valida se o usuário existe e se a senha está correta
        if (usuario == null || usuario.SenhaHash != txtSenha.Text)
        {
            await DisplayAlert("Erro", "Email ou senha inválidos", "OK");
            return;
        }

        // Salva os dados da sessão
        Preferences.Set("usuarioLogado", usuario.Email);
        Preferences.Set("perfilUsuario", usuario.TipoPerfil);

        await DisplayAlert("Sucesso", "Login realizado!", "OK");

        // Redirecionamento por perfil 
        if (usuario.TipoPerfil == "Paciente")
        {
            // Paciente vai gerenciar seus remédios
            await Navigation.PushAsync(new ListaMedicacao());
        }
        else if (usuario.TipoPerfil == "Médico" || usuario.TipoPerfil == "Responsável")
        {
            // Médico e Responsável vão para o monitoramento geral
            await Navigation.PushAsync(new Monitoramento());
        }
        else
        {
            // Caso o perfil seja diferente, abre a lista por padrão
            await Navigation.PushAsync(new ListaMedicacao());
        }
    } // Aqui faltava essa chave para fechar o OnLoginClicked!

    private async void OnCadastroClicked(object sender, EventArgs e)
    {
        // Se tiver tela de cadastro, descomente a linha abaixo e ajuste o nome da View
        // await Navigation.PushAsync(new CadastroUsuario());
    }
}