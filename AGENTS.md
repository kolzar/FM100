# AGENTS.md

## Project stack
- Language: C#
- Runtime: .NET 10
- IDE: Visual Studio
- API: ASP.NET Core Web API
- Database: SQLite
- Data access: Dapper
- Tests: xUnit
- Containers: Docker
- Future deployment: Kubernetes, Helm, Terraform, Azure

## Main rules
- Do not use Python.
- Do not use Entity Framework.
- Do not introduce unnecessary dependencies.
- Prefer simple, readable code.
- Follow Microsoft C# conventions.
- Use async/await where it makes sense.
- Keep methods small and focused.
- Avoid duplicated logic.
- Prefer dependency injection.
- Prefer explicit names over clever code.

## Architecture
- Use clean separation between API, Application, Domain, Infrastructure, and Tests.
- Controllers must be thin.
- Business logic must not live inside controllers.
- Repository classes handle database access only.
- Services orchestrate business rules.
- Domain models must not depend on infrastructure concerns.

## Database
- Use SQLite.
- Use Dapper for queries and commands.
- Keep SQL readable.
- Do not generate Entity Framework migrations.
- Store SQL scripts in `/scripts` or `/database`.

## API rules
- Use RESTful endpoints.
- Use DTOs for request and response models.
- Validate input.
- Return proper HTTP status codes.
- Add Swagger/OpenAPI support.
- Keep route names clear and consistent.

## Testing
- Use xUnit.
- Add tests for every new business feature.
- Prefer unit tests for services.
- Prefer integration tests for repositories when useful.
- Do not delete existing tests unless obsolete, and explain why.

## Docker
- Provide a working Dockerfile when needed.
- Keep images small.
- Do not hardcode secrets.
- Use environment variables for configuration.

## Kubernetes / Helm / Terraform
- Keep manifests simple.
- Prefer Helm values for environment-specific configuration.
- Do not hardcode production secrets.
- Terraform should be readable and modular.
- Explain infrastructure changes before applying them.

## Security
- Never commit secrets, passwords, tokens, or connection strings.
- Use appsettings.Development.json only for local non-secret settings.
- Use environment variables or secret managers for sensitive values.
- Validate external input.

## Codex behavior
- Before changing code, inspect the existing structure.
- Preserve the current project style.
- Make small, reviewable changes.
- After changes, build and run tests when possible.
- Explain what changed and why.
- If something is ambiguous, make the safest reasonable assumption and continue.
