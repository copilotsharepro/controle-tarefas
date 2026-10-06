using ControleTarefas.Data;
using ControleTarefas.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleTarefas.Repositories;

public class TarefaRepository(AppDbContext context) : ITarefaRepository
{
    private readonly AppDbContext _context = context;

    public Task<List<Tarefa>> ListarAsync() =>
        _context.Tarefas
            .AsNoTracking()
            .OrderBy(t => t.Data)
            .ThenBy(t => t.Id)
            .ToListAsync();

    public Task<Tarefa?> ObterAsync(int id) =>
        _context.Tarefas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task AdicionarAsync(Tarefa tarefa)
    {
        _context.Tarefas.Add(tarefa);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AtualizarAsync(int id, Tarefa tarefa)
    {
        var existente = await _context.Tarefas.FirstOrDefaultAsync(t => t.Id == id);
        if (existente is null)
        {
            return false;
        }

        existente.Titulo = tarefa.Titulo.Trim();
        existente.Descricao = tarefa.Descricao?.Trim();
        existente.Data = tarefa.Data;
        existente.Status = tarefa.Status;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var existente = await _context.Tarefas.FirstOrDefaultAsync(t => t.Id == id);
        if (existente is null)
        {
            return false;
        }

        _context.Tarefas.Remove(existente);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MoverAsync(int id, StatusTarefa status)
    {
        var existente = await _context.Tarefas.FirstOrDefaultAsync(t => t.Id == id);
        if (existente is null)
        {
            return false;
        }

        existente.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }
}
