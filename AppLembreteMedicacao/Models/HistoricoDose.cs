using SQLite;

namespace AppLembreteMedicacao.Models
{
    public class HistoricoDose
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string NomeMedicamento { get; set; }
        public DateTime DataHora { get; set; }
        public string Status { get; set; } // "Tomado" ou "Não Tomado"

        // AJUSTE: Renomeado para StatusColor para bater com seu XAML {Binding StatusColor}
        [Ignore]
        public Color StatusColor => Status == "Tomado" ? Colors.Green : Colors.Red;

        [Ignore]
        public string StatusIcone => Status == "Tomado" ? "✅" : "❌";

        [Ignore]
        public string StatusTexto => Status == "Tomado" ? "Concluído" : "Perdido";
    }
}