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
    }
}
