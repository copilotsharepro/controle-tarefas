# Controle de Tarefas

Aplicação web ASP.NET Core Razor Pages para controle de tarefas em quadro Kanban.

## Requisitos

- .NET SDK 10
- Ferramenta local `dotnet-ef` (restaurada pelo manifesto local)

## Como executar

1. Restaurar ferramentas locais:

   ```bash
   dotnet tool restore
   ```

2. Restaurar dependências do projeto:

   ```bash
   dotnet restore ControleTarefas/ControleTarefas.csproj
   ```

3. Aplicar migrations no banco configurado:

   ```bash
   dotnet ef database update --project ControleTarefas/ControleTarefas.csproj
   ```

4. Executar a aplicação:

   ```bash
   dotnet run --project ControleTarefas/ControleTarefas.csproj
   ```

## Banco de dados

- Banco SQLite padrão: `ControleTarefas/tarefas.db`.
- A string de conexão fica em `ControleTarefas/appsettings.json` (`ConnectionStrings:Default`).

## Migrations

- Criar nova migration:

  ```bash
  dotnet ef migrations add NomeDaMigration --project ControleTarefas/ControleTarefas.csproj --output-dir Data/Migrations
  ```

- Aplicar migrations:

  ```bash
  dotnet ef database update --project ControleTarefas/ControleTarefas.csproj
  ```

