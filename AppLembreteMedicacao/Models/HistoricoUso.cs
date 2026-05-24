
using SQLite;

namespace AppLembreteMedicacao.Models
{
    public class HistoricoUso
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int MedicamentoId { get; set; }
        public DateTime DataUso { get; set; }
        public bool Tomado { get; set; }
        public string NomeMedicamento { get; set; } // Auxiliar para relatórios

        [Ignore]
        public Color StatusColor => Tomado ? Colors.Green : Colors.Red;

        [Ignore]
        public string StatusIcone => Tomado ? "✅" : "❌";

        [Ignore]
        public string StatusTexto => Tomado ? "Tomou ✔" : "Não tomou ❌";

        [Ignore]
        public string DataFormatada => DataUso.ToString("dd/MM/yyyy HH:mm");
    }
}