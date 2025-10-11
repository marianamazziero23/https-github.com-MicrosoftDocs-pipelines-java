ESG Sustainability API

# Visão Geral

A ESG Sustainability API é uma aplicação robusta desenvolvida em .NET Core 8 que oferece uma solução completa para gerenciamento e monitoramento de dados ESG (Environmental, Social, and Governance) de empresas. A API permite o registro, acompanhamento e análise de emissões de carbono, consumo de energia, relatórios de sustentabilidade e estatísticas consolidadas.

## Características Principais

- Funcionalidades ESG:

- Gestão de Emissões de Carbono: Registro e monitoramento detalhado de emissões por categoria (Escopo 1, 2 e 3)
- Controle de Consumo de Energia: Acompanhamento de diferentes tipos de energia e percentual renovável
- Relatórios de Sustentabilidade: Geração automática e manual de relatórios trimestrais e anuais
- Dashboard Analítico: Estatísticas consolidadas, tendências e rankings de performance ESG

- Arquitetura e Tecnologia:

- Framework: .NET Core 8.0
- Padrão Arquitetural: MVVM (Model-View-ViewModel)
- Banco de Dados: SQLite (desenvolvimento) / SQL Server (produção)
- ORM: Entity Framework Core 9.0
- Autenticação: JWT (JSON Web Tokens)
- Documentação: Swagger/OpenAPI 3.0
- Testes: xUnit com cobertura de integração

- Segurança e Qualidade:

- Autenticação JWT com roles (Admin, Manager, User)
- Autorização baseada em políticas para endpoints críticos
- Hash de senhas com BCrypt
- Validação robusta de dados de entrada
- Tratamento centralizado de exceções
- Paginação otimizada para grandes volumes de dados
- Health Checks para monitoramento

## Estrutura do Projeto

```
ESGSustainabilityAPI/
├── Controllers/           # Controllers da API
│   ├── AuthController.cs
│   ├── EmissionsController.cs
│   ├── EnergyConsumptionController.cs
│   ├── ESGDashboardController.cs
│   └── SustainabilityReportsController.cs
├── Models/               # Modelos de dados
│   ├── CarbonEmission.cs
│   ├── Company.cs
│   ├── EnergyConsumption.cs
│   ├── SustainabilityReport.cs
│   └── User.cs
├── ViewModels/           # ViewModels (MVVM)
│   ├── AuthViewModels.cs
│   ├── CarbonEmissionViewModels.cs
│   ├── CommonViewModels.cs
│   ├── EnergyConsumptionViewModels.cs
│   ├── ESGStatisticsViewModel.cs
│   └── SustainabilityReportViewModels.cs
├── Data/                 # Contexto do banco de dados
│   └── ESGDbContext.cs
├── Services/             # Serviços da aplicação
│   └── JwtService.cs
├── Migrations/           # Migrações do Entity Framework
└── Tests/               # Testes unitários e de integração
```

### Endpoints da API

Autenticação (`/api/auth`)
- `POST /login` - Autenticação de usuário
- `POST /register` - Registro de novo usuário
- `GET /profile` - Perfil do usuário autenticado
- `POST /change-password` - Alteração de senha

Emissões de Carbono (`/api/emissions`)
- `GET /` - Listar emissões (com paginação e filtros)
- `GET /{id}` - Obter emissão específica
- `POST /` - Registrar nova emissão
- `PUT /{id}` - Atualizar emissão
- `DELETE /{id}` - Remover emissão
- `GET /statistics` - Estatísticas de emissões

Consumo de Energia (`/api/energy-consumption`)
- `GET /` - Listar consumos (com paginação e filtros)
- `GET /{id}` - Obter consumo específico
- `POST /` - Registrar novo consumo
- `PUT /{id}` - Atualizar consumo
- `DELETE /{id}` - Remover consumo
- `GET /statistics` - Estatísticas de energia

Relatórios de Sustentabilidade (`/api/sustainability-reports`)
- `GET /` - Listar relatórios (com filtros)
- `GET /{id}` - Obter relatório específico
- `POST /` - Criar novo relatório
- `PUT /{id}` - Atualizar relatório
- `DELETE /{id}` - Remover relatório
- `POST /generate` - Gerar relatório automaticamente

Dashboard ESG (`/api/esg-dashboard`)
- `GET /statistics` - Estatísticas consolidadas
- `GET /emissions-trends` - Tendências de emissões
- `GET /company-ranking` - Ranking de empresas
- `GET /comparison` - Comparação entre empresas

#### Instalação e Configuração

- Pré-requisitos:

- .NET Core 8.0 SDK
- SQLite (desenvolvimento) ou SQL Server (produção)
- Visual Studio 2022 ou VS Code

- Passo a Passo:

1. **Clone o repositório**
```bash
git clone <repository-url>
cd ESGSustainabilityAPI
```

2. **Restaure as dependências**
```bash
dotnet restore
```

3. **Configure a string de conexão**
Edite o arquivo `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ESGSustainabilityDB.db"
  }
}
```

4. **Execute as migrações**
```bash
dotnet ef database update
```

5. **Execute a aplicação**
```bash
dotnet run
```

6. **Acesse a documentação**
Abra o navegador em: `https://localhost:7000/swagger`

##### Configuração de Autenticação

- JWT Settings:

Configure as chaves JWT no `appsettings.json`:
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

- Usuários Padrão
A aplicação vem com usuários pré-configurados para teste:

| Username | Password | Role | Email |
|----------|----------|------|-------|
| admin | admin123 | Admin | admin@ecotech.com.br |
| manager | manager123 | Manager | manager@ecotech.com.br |
| user | user123 | User | user@ecotech.com.br |

- Exemplos de Uso

- Autenticação

# Login
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

### Registrar Emissão de Carbono
```bash
curl -X POST "https://localhost:7000/api/emissions" \
  -H "Authorization: Bearer {seu-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "source": "Transporte",
    "emissionAmount": 15.5,
    "unit": "tCO2e",
    "category": "Escopo 1",
    "location": "São Paulo - SP",
    "description": "Emissões da frota de veículos",
    "companyId": 1
  }'
```

### Obter Estatísticas ESG
```bash
curl -X GET "https://localhost:7000/api/esg-dashboard/statistics" \
  -H "Authorization: Bearer {seu-token}"
```

## Testes

### Executar Testes Unitários
```bash
cd ESGSustainabilityAPI.Tests
dotnet test
```

### Cobertura de Testes
- **41 testes** implementados
- **4 controllers** testados
- **Testes de integração** com banco em memória
- **Validação de status codes** (200, 201, 400, 404)
- **Testes de autenticação** e autorização

## Funcionalidades Avançadas

### Paginação
Todos os endpoints de listagem suportam paginação:
```
GET /api/emissions?page=1&pageSize=10&searchTerm=energia
```

### Filtros Avançados
- **Busca textual** em múltiplos campos
- **Filtros por data** (startDate, endDate)
- **Ordenação** customizável (sortBy, sortDirection)
- **Filtros específicos** por categoria, tipo, empresa

### Validações
- **Validação de entrada** com Data Annotations
- **Validação de negócio** customizada
- **Tratamento de erros** padronizado
- **Mensagens de erro** localizadas em português

### Performance
- **Índices otimizados** no banco de dados
- **Consultas eficientes** com Entity Framework
- **Paginação** para grandes volumes
- **Lazy loading** configurado adequadamente

## Monitoramento e Saúde

### Health Checks
A API inclui endpoints de monitoramento:
- `GET /health` - Status geral da aplicação
- `GET /health/ready` - Verificação de prontidão
- `GET /health/live` - Verificação de vitalidade

### Logging
- **Logs estruturados** com diferentes níveis
- **Rastreamento de operações** críticas
- **Logs de auditoria** para alterações importantes

## Contribuição

### Padrões de Código
- **Clean Code** e princípios SOLID
- **Nomenclatura** em português para domínio de negócio
- **Comentários XML** para documentação
- **Async/await** para operações assíncronas

### Estrutura de Commits
- `feat:` Nova funcionalidade
- `fix:` Correção de bug
- `docs:` Documentação
- `test:` Testes
- `refactor:` Refatoração

## Licença

Este projeto está licenciado sob a MIT License - veja o arquivo [LICENSE](LICENSE) para detalhes.

## Suporte

Para dúvidas ou suporte:
- **Email**: suporte@esgsustainabilityapi.com
- **Documentação**: [Swagger UI](https://localhost:7000/swagger)
- **Issues**: [GitHub Issues](https://github.com/seu-usuario/ESGSustainabilityAPI/issues)

---

Desenvolvido com ❤️ para um futuro mais sustentável

