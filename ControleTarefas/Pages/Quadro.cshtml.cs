using ControleTarefas.Models;
using ControleTarefas.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControleTarefas.Pages;

public class QuadroModel(ITarefaRepository tarefaRepository) : PageModel
{
    private readonly ITarefaRepository _tarefaRepository = tarefaRepository;

    public Tarefa Tarefa { get; set; } = new();

    public List<Tarefa> Tarefas { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Tarefa = new Tarefa
        {
            Data = DateTime.Today,
            Status = StatusTarefa.Novo
        };
        Tarefas = await _tarefaRepository.ListarAsync();
    }

    public IEnumerable<Tarefa> PorStatus(StatusTarefa status) =>
        Tarefas.Where(t => t.Status == status);

    public async Task<IActionResult> OnPostCriarAsync(Tarefa tarefa)
    {
        ValidarTarefa(tarefa);
        if (!ModelState.IsValid)
        {
            return BadRequest(ObterMensagemErro());
        }

        tarefa.Id = 0;
        tarefa.Titulo = tarefa.Titulo.Trim();
        tarefa.Descricao = tarefa.Descricao?.Trim();

        await _tarefaRepository.AdicionarAsync(tarefa);
        return Partial("_Cartao", tarefa);
    }

    public async Task<IActionResult> OnPostMoverAsync(int id, string? status)
    {
        if (id <= 0)
        {
            return BadRequest("Tarefa inválida.");
        }

        if (!Enum.TryParse<StatusTarefa>(status, true, out var statusDestino) ||
            !Enum.IsDefined(statusDestino))
        {
            return BadRequest("Status inválido.");
        }

        var sucesso = await _tarefaRepository.MoverAsync(id, statusDestino);
        if (!sucesso)
        {
            return NotFound("Tarefa não encontrada.");
        }

        return new OkResult();
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

    private string ObterMensagemErro()
    {
        var erro = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(mensagem => !string.IsNullOrWhiteSpace(mensagem));

        return erro ?? "Não foi possível salvar a tarefa. Verifique os dados informados.";
    }
}
