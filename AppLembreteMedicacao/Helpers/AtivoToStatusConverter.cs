using System.Globalization;

namespace AppLembreteMedicacao.Helpers
{
    // Esta classe transforma o 0 ou 1 do banco em texto para a tela
    public class AtivoToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // O 'value' é o que vem do Binding {Binding Ativo}
            if (value is int ativo)
            {
                return ativo == 1 ? "EM TRATAMENTO" : "ENCERRADO";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
