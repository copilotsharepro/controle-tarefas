namespace ControleTarefas.Models;

public class Tarefa
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime Data { get; set; } = DateTime.Today;
    public StatusTarefa Status { get; set; } = StatusTarefa.Novo;
}

