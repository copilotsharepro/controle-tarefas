using ControleTarefas.Models;
using ControleTarefas.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControleTarefas.Pages;

public class ExcluirModel(ITarefaRepository tarefaRepository) : PageModel
{
    private readonly ITarefaRepository _tarefaRepository = tarefaRepository;

    public Tarefa Tarefa { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tarefa = await _tarefaRepository.ObterAsync(id);
        if (tarefa is null)
        {
            return NotFound();
        }

        Tarefa = tarefa;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var excluida = await _tarefaRepository.ExcluirAsync(id);
        if (!excluida)
        {
            return NotFound();
        }

        return RedirectToPage("/Quadro");
    }
}

