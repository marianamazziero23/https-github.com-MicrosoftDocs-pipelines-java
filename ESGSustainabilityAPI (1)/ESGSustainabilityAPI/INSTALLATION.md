# Guia de Instalação e Configuração - ESG Sustainability API

## Índice
1. [Pré-requisitos](#pré-requisitos)
2. [Instalação](#instalação)
3. [Configuração](#configuração)
4. [Execução](#execução)
5. [Testes](#testes)
6. [Deployment](#deployment)
7. [Troubleshooting](#troubleshooting)

## Pré-requisitos

### Software Necessário
- **.NET Core 8.0 SDK** ou superior
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
- **Git** para controle de versão
- **Editor de código** (Visual Studio 2022, VS Code, ou Rider)

### Banco de Dados
- **SQLite** (padrão para desenvolvimento)
- **SQL Server** (recomendado para produção)
- **PostgreSQL** (suporte opcional)

### Ferramentas Opcionais
- **Postman** ou **Insomnia** para testes de API
- **Docker** para containerização
- **Azure CLI** para deploy na nuvem

## Instalação

### 1. Clone o Repositório
```bash
git clone https://github.com/seu-usuario/ESGSustainabilityAPI.git
cd ESGSustainabilityAPI
```

### 2. Verifique a Instalação do .NET
```bash
dotnet --version
# Deve retornar 8.0.x ou superior
```

### 3. Restaure as Dependências
```bash
# No diretório raiz do projeto
dotnet restore

# Para o projeto de testes
cd ESGSustainabilityAPI.Tests
dotnet restore
cd ..
```

### 4. Instale as Ferramentas do Entity Framework
```bash
dotnet tool install --global dotnet-ef
# ou para atualizar
dotnet tool update --global dotnet-ef
```

## Configuração

### 1. Configuração do Banco de Dados

#### SQLite (Desenvolvimento)
O projeto vem configurado por padrão para usar SQLite. Nenhuma configuração adicional é necessária.

#### SQL Server (Produção)
Edite o arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ESGSustainabilityDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

Para SQL Server remoto:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=seu-servidor;Database=ESGSustainabilityDB;User Id=seu-usuario;Password=sua-senha;TrustServerCertificate=true"
  }
}
```

### 2. Configuração JWT

Edite o arquivo `appsettings.json` para configurar a autenticação:
```json
{
  "Jwt": {
    "SecretKey": "SuaChaveSecretaSuperSegura_MinimoDe32Caracteres!",
    "Issuer": "ESGSustainabilityAPI",
    "Audience": "ESGSustainabilityAPI_Users",
    "ExpirationMinutes": 60
  }
}
```

**⚠️ IMPORTANTE**: Em produção, use uma chave secreta forte e armazene-a de forma segura (Azure Key Vault, AWS Secrets Manager, etc.).

### 3. Configuração de CORS

Para permitir acesso de diferentes origens, configure no `appsettings.json`:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://seu-frontend.com"
    ]
  }
}
```

### 4. Configuração de Logging

Configure os níveis de log no `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

## Execução

### 1. Aplicar Migrações do Banco de Dados
```bash
# No diretório ESGSustainabilityAPI
dotnet ef database update
```

### 2. Executar a Aplicação
```bash
# Modo desenvolvimento
dotnet run

# Ou especificando o projeto
dotnet run --project ESGSustainabilityAPI
```

### 3. Verificar se a Aplicação Está Funcionando
- **API**: https://localhost:7000
- **Swagger**: https://localhost:7000/swagger
- **Health Check**: https://localhost:7000/health

### 4. Executar em Modo Watch (Desenvolvimento)
```bash
dotnet watch run
```
Isso reiniciará automaticamente a aplicação quando houver mudanças no código.

## Testes

### 1. Executar Todos os Testes
```bash
cd ESGSustainabilityAPI.Tests
dotnet test
```

### 2. Executar Testes com Relatório Detalhado
```bash
dotnet test --verbosity normal
```

### 3. Executar Testes com Cobertura
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### 4. Executar Testes Específicos
```bash
# Testes de um controller específico
dotnet test --filter "ClassName=EmissionsControllerTests"

# Testes por categoria
dotnet test --filter "Category=Integration"
```

## Deployment

### 1. Build para Produção
```bash
dotnet publish -c Release -o ./publish
```

### 2. Docker (Opcional)

Crie um `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ESGSustainabilityAPI/ESGSustainabilityAPI.csproj", "ESGSustainabilityAPI/"]
RUN dotnet restore "ESGSustainabilityAPI/ESGSustainabilityAPI.csproj"
COPY . .
WORKDIR "/src/ESGSustainabilityAPI"
RUN dotnet build "ESGSustainabilityAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ESGSustainabilityAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ESGSustainabilityAPI.dll"]
```

Build e execução:
```bash
docker build -t esg-sustainability-api .
docker run -p 8080:80 esg-sustainability-api
```

### 3. Azure App Service

```bash
# Login no Azure
az login

# Criar grupo de recursos
az group create --name ESGSustainabilityAPI-rg --location "East US"

# Criar App Service Plan
az appservice plan create --name ESGSustainabilityAPI-plan --resource-group ESGSustainabilityAPI-rg --sku B1 --is-linux

# Criar Web App
az webapp create --resource-group ESGSustainabilityAPI-rg --plan ESGSustainabilityAPI-plan --name ESGSustainabilityAPI --runtime "DOTNETCORE|8.0"

# Deploy
dotnet publish -c Release
cd bin/Release/net8.0/publish
zip -r ../deploy.zip .
az webapp deployment source config-zip --resource-group ESGSustainabilityAPI-rg --name ESGSustainabilityAPI --src ../deploy.zip
```

## Troubleshooting

### Problemas Comuns

#### 1. Erro de Migração
```
Unable to create an object of type 'ESGDbContext'
```
**Solução**: Verifique se a string de conexão está correta no `appsettings.json`.

#### 2. Erro de Autenticação JWT
```
401 Unauthorized
```
**Solução**: 
- Verifique se o token está sendo enviado no header `Authorization: Bearer {token}`
- Confirme se a chave secreta JWT está configurada corretamente

#### 3. Erro de CORS
```
Access to fetch at 'https://localhost:7000/api/...' from origin 'http://localhost:3000' has been blocked by CORS policy
```
**Solução**: Configure as origens permitidas no `appsettings.json` ou no `Program.cs`.

#### 4. Erro de Banco de Dados
```
A network-related or instance-specific error occurred
```
**Solução**: 
- Verifique se o SQL Server está rodando
- Confirme a string de conexão
- Para SQLite, verifique as permissões de escrita no diretório

### Logs e Debugging

#### 1. Habilitar Logs Detalhados
No `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

#### 2. Debugging no Visual Studio
- Defina breakpoints no código
- Execute com F5 (Debug mode)
- Use a janela "Output" para ver logs

#### 3. Verificar Health Checks
```bash
curl https://localhost:7000/health
```

### Performance

#### 1. Otimização de Consultas
- Use `Include()` para eager loading quando necessário
- Implemente paginação em todas as listagens
- Use índices apropriados no banco de dados

#### 2. Monitoramento
- Configure Application Insights (Azure)
- Use ferramentas como MiniProfiler para análise de performance
- Monitore métricas de CPU e memória

### Segurança

#### 1. Checklist de Segurança
- [ ] Chaves JWT seguras em produção
- [ ] HTTPS habilitado
- [ ] Validação de entrada implementada
- [ ] Rate limiting configurado
- [ ] Logs de auditoria implementados

#### 2. Variáveis de Ambiente
Para produção, use variáveis de ambiente:
```bash
export ConnectionStrings__DefaultConnection="sua-string-de-conexao"
export Jwt__SecretKey="sua-chave-secreta"
```

## Suporte Adicional

### Documentação Oficial
- [.NET Core Documentation](https://docs.microsoft.com/en-us/dotnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)

### Comunidade
- [Stack Overflow](https://stackoverflow.com/questions/tagged/.net-core)
- [.NET Community](https://dotnet.microsoft.com/platform/community)
- [GitHub Discussions](https://github.com/dotnet/core/discussions)

---

**Para mais informações, consulte a documentação da API em `/swagger` quando a aplicação estiver rodando.**

