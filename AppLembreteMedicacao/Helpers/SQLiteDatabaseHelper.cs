using AppLembreteMedicacao.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace AppLembreteMedicacao.Helpers
{
    public class SQLiteDatabaseHelper
    {
        readonly SQLiteAsyncConnection _conn;

        public SQLiteDatabaseHelper(string path)
        {
            _conn = new SQLiteAsyncConnection(path);
            _conn.CreateTableAsync<Usuario>().Wait();
            _conn.CreateTableAsync<Medicamento>().Wait();
            _conn.CreateTableAsync<Cronograma>().Wait();
            _conn.CreateTableAsync<HistoricoUso>().Wait();
        }

        // --- MEDICAMENTO ---
        public Task<int> InsertMedicamento(Medicamento m) => _conn.InsertAsync(m);
        public Task<List<Medicamento>> GetMedicamentos() => _conn.Table<Medicamento>().ToListAsync();
        public Task<Medicamento> GetMedicamentoPorId(int id) => _conn.Table<Medicamento>().Where(m => m.Id == id).FirstOrDefaultAsync();
        public Task<int> UpdateMedicamento(Medicamento m) => _conn.UpdateAsync(m);
        public Task<int> DeleteMedicamento(int id) => _conn.DeleteAsync<Medicamento>(id);

        public async Task<Medicamento> GetUltimoMedicamento()
        {
            var lista = await _conn.Table<Medicamento>().OrderByDescending(m => m.Id).ToListAsync();
            return lista.FirstOrDefault();
        }

        // --- CRONOGRAMA (OS MÉTODOS QUE ESTAVAM FALTANDO) ---
        public Task<int> InsertCronograma(Cronograma c) => _conn.InsertAsync(c);

        public Task<List<Cronograma>> GetCronogramaPorMedicamento(int medicamentoId) =>
            _conn.Table<Cronograma>()
                 .Where(c => c.MedicamentoId == medicamentoId)
                 .ToListAsync();

        public Task<int> UpdateCronograma(Cronograma c) => _conn.UpdateAsync(c);

        public Task<int> DeleteCronograma(int id) => _conn.DeleteAsync<Cronograma>(id);

        // --- HISTÓRICO ---
        public Task<int> InsertHistorico(HistoricoUso h) => _conn.InsertAsync(h);

        public Task<List<HistoricoUso>> GetHistorico(int medicamentoId) =>
            _conn.Table<HistoricoUso>()
                 .Where(h => h.MedicamentoId == medicamentoId)
                 .OrderByDescending(h => h.DataUso)
                 .ToListAsync();

        // --- USUÁRIO ---
        public Task<int> InsertUsuario(Usuario u) => _conn.InsertAsync(u);
        public Task<Usuario> GetUsuarioEmail(string email) =>
            _conn.Table<Usuario>().Where(u => u.Email == email).FirstOrDefaultAsync();
    }
}