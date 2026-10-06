using ControleTarefas.Models;
using ControleTarefas.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControleTarefas.Pages;

public class EditarModel(ITarefaRepository tarefaRepository) : PageModel
{
    private readonly ITarefaRepository _tarefaRepository = tarefaRepository;

    public Tarefa Tarefa { get; set; } = new();

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

    public async Task<IActionResult> OnPostAsync(int id, Tarefa tarefa)
    {
        Tarefa = tarefa;
        ValidarTarefa(Tarefa);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Tarefa.Titulo = Tarefa.Titulo.Trim();
        Tarefa.Descricao = Tarefa.Descricao?.Trim();

        var atualizada = await _tarefaRepository.AtualizarAsync(id, Tarefa);
        if (!atualizada)
        {
            return NotFound();
        }

        return RedirectToPage("/Quadro");
    }

    private void ValidarTarefa(Tarefa tarefa)
    {
        if (string.IsNullOrWhiteSpace(tarefa.Titulo))
        {
            ModelState.AddModelError(nameof(Tarefa.Titulo), "Informe o título da tarefa.");
        }

        if (tarefa.Data == default)
        {
            ModelState.AddModelError(nameof(Tarefa.Data), "Informe uma data válida.");
        }

        if (!Enum.IsDefined(tarefa.Status))
        {
            ModelState.AddModelError(nameof(Tarefa.Status), "Selecione um status válido.");
        }
    }
}
