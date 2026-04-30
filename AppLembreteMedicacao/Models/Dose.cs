using SQLite;

namespace AppLembreteMedicacao.Models
{
    public class Dose
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int MedicamentoId { get; set; }
        public DateTime HorarioPrevisto { get; set; }
        public string Status { get; set; } = "Pendente";
        public string NomeMedicamento { get; set; }
    }
}
