# Exemplos de Uso - ESG Sustainability API

## Índice
1. [Autenticação](#autenticação)
2. [Gestão de Emissões de Carbono](#gestão-de-emissões-de-carbono)
3. [Controle de Consumo de Energia](#controle-de-consumo-de-energia)
4. [Relatórios de Sustentabilidade](#relatórios-de-sustentabilidade)
5. [Dashboard e Estatísticas](#dashboard-e-estatísticas)
6. [Cenários Avançados](#cenários-avançados)

## Autenticação

### 1. Login de Usuário
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Resposta:**
```json
{
  "success": true,
  "message": "Login realizado com sucesso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-12-06T15:30:00Z",
    "user": {
      "id": 1,
      "username": "admin",
      "email": "admin@ecotech.com.br",
      "firstName": "Administrador",
      "lastName": "Sistema",
      "role": "Admin",
      "companyId": 1,
      "companyName": "EcoTech Solutions"
    }
  }
}
```

### 2. Registro de Novo Usuário
```bash
curl -X POST "https://localhost:7000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "joao.silva",
    "email": "joao.silva@empresa.com",
    "password": "senha123",
    "firstName": "João",
    "lastName": "Silva",
    "companyId": 1
  }'
```

### 3. Obter Perfil do Usuário
```bash
curl -X GET "https://localhost:7000/api/auth/profile" \
  -H "Authorization: Bearer {seu-token}"
```

### 4. Alterar Senha
```bash
curl -X POST "https://localhost:7000/api/auth/change-password" \
  -H "Authorization: Bearer {seu-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "senha123",
    "newPassword": "novaSenha456",
    "confirmPassword": "novaSenha456"
  }'
```

## Gestão de Emissões de Carbono

### 1. Listar Emissões com Paginação
```bash
curl -X GET "https://localhost:7000/api/emissions?page=1&pageSize=10&searchTerm=energia" \
  -H "Authorization: Bearer {seu-token}"
```

**Resposta:**
```json
{
  "success": true,
  "message": "Emissões de carbono recuperadas com sucesso",
  "data": {
    "data": [
      {
        "id": 1,
        "source": "Energia Elétrica",
        "emissionAmount": 20.5,
        "unit": "tCO2e",
        "category": "Escopo 2",
        "location": "São Paulo - SP",
        "description": "Emissões do consumo de energia elétrica",
        "recordDate": "2024-05-01T00:00:00Z",
        "companyId": 1,
        "companyName": "EcoTech Solutions"
      }
    ],
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 1,
    "totalRecords": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

### 2. Registrar Nova Emissão
```bash
curl -X POST "https://localhost:7000/api/emissions" \
  -H "Authorization: Bearer {seu-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "source": "Transporte",
    "emissionAmount": 15.5,
    "unit": "tCO2e",
    "category": "Escopo 1",
    "location": "Rio de Janeiro - RJ",
    "description": "Emissões da frota de veículos corporativos",
    "recordDate": "2024-06-01T00:00:00Z",
    "companyId": 1
  }'
```

### 3. Obter Emissão Específica
```bash
curl -X GET "https://localhost:7000/api/emissions/1" \
  -H "Authorization: Bearer {seu-token}"
```

### 4. Atualizar Emissão
```bash
curl -X PUT "https://localhost:7000/api/emissions/1" \
  -H "Authorization: Bearer {seu-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "source": "Energia Elétrica Atualizada",
    "emissionAmount": 22.0,
    "unit": "tCO2e",
    "category": "Escopo 2",
    "location": "São Paulo - SP",
    "description": "Emissões atualizadas do consumo de energia elétrica",
    "recordDate": "2024-05-01T00:00:00Z"
  }'
```

### 5. Obter Estatísticas de Emissões
```bash
curl -X GET "https://localhost:7000/api/emissions/statistics?companyId=1" \
  -H "Authorization: Bearer {seu-token}"
```

## Controle de Consumo de Energia

### 1. Registrar Consumo de Energia
```bash
curl -X POST "https://localhost:7000/api/energy-consumption" \
  -H "Authorization: Bearer {seu-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "energyType": "Solar",
    "consumptionAmount": 1500.0,
    "unit": "kWh",
    "recordDate": "2024-06-01T00:00:00Z",
    "source": "Painéis Solares",
    "cost": 750.0,
    "costCurrency": "BRL",
    "renewablePercentage": 100.0,
    "description": "Energia gerada pelos painéis solares do escritório",
    "companyId": 1
  }'
```

### 2. Listar Consumos com Filtros
```bash
curl -X GET "https://localhost:7000/api/energy-consumption?energyType=Solar&startDate=2024-01-01&endDate=2024-12-31" \
  -H "Authorization: Bearer {seu-token}"
```

### 3. Obter Estatísticas de Energia
```bash
curl -X GET "https://localhost:7000/api/energy-consumption/statistics" \
  -H "Authorization: Bearer {seu-token}"
```

**Resposta:**
```json
{
  "success": true,
  "message": "Estatísticas de energia calculadas com sucesso",
  "data": {
    "totalConsumption": 5000.0,
    "averageRenewablePercentage": 65.5,
    "totalCost": 2500.0,
    "consumptionByType": {
      "Elétrica": 2500.0,
      "Solar": 1500.0,
      "Eólica": 1000.0
    },
    "monthlyTrend": [
      {
        "month": "2024-01",
        "consumption": 800.0,
        "renewablePercentage": 60.0
      }
    ]
  }
}
```

## Relatórios de Sustentabilidade

### 1. Criar Relatório Manual
```bash
curl -X POST "https://localhost:7000/api/sustainability-reports" \
  -H "Authorization: Bearer {seu-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Relatório ESG Q2 2024",
    "year": 2024,
    "quarter": 2,
    "totalCarbonEmissions": 45.5,
    "totalEnergyConsumption": 8500.0,
    "renewableEnergyPercentage": 60.0,
    "waterConsumption": 1200.0,
    "wasteGenerated": 500.0,
    "wasteRecycled": 350.0,
    "esgScore": "B",
    "environmentalInitiatives": "Implementação de painéis solares e programa de reciclagem",
    "socialInitiatives": "Programa de diversidade e inclusão, treinamentos em sustentabilidade",
    "governanceInitiatives": "Criação do comitê de ESG e políticas de compliance",
    "challenges": "Redução das emissões de carbono da frota de veículos",
    "futureGoals": "Alcançar neutralidade de carbono até 2030",
    "companyId": 1
  }'
```

### 2. Gerar Relatório Automaticamente
```bash
curl -X POST "https://localhost:7000/api/sustainability-reports/generate?companyId=1&year=2024&quarter=3" \
  -H "Authorization: Bearer {seu-token}"
```

### 3. Listar Relatórios com Filtros
```bash
curl -X GET "https://localhost:7000/api/sustainability-reports?year=2024&quarter=2&companyId=1" \
  -H "Authorization: Bearer {seu-token}"
```

### 4. Obter Relatório Específico
```bash
curl -X GET "https://localhost:7000/api/sustainability-reports/1" \
  -H "Authorization: Bearer {seu-token}"
```

## Dashboard e Estatísticas

### 1. Estatísticas Consolidadas ESG
```bash
curl -X GET "https://localhost:7000/api/esg-dashboard/statistics" \
  -H "Authorization: Bearer {seu-token}"
```

**Resposta:**
```json
{
  "success": true,
  "message": "Estatísticas ESG calculadas com sucesso",
  "data": {
    "totalCarbonEmissions": 125.5,
    "totalEnergyConsumption": 15000.0,
    "averageRenewablePercentage": 58.3,
    "totalCompanies": 3,
    "totalReports": 8,
    "lastUpdated": "2024-06-12T10:30:00Z",
    "emissionsByCategory": {
      "Escopo 1": 45.2,
      "Escopo 2": 65.8,
      "Escopo 3": 14.5
    },
    "energyByType": {
      "Elétrica": 8500.0,
      "Solar": 4000.0,
      "Eólica": 2500.0
    }
  }
}
```

### 2. Tendências de Emissões
```bash
curl -X GET "https://localhost:7000/api/esg-dashboard/emissions-trends?period=month&companyId=1" \
  -H "Authorization: Bearer {seu-token}"
```

### 3. Ranking de Empresas
```bash
curl -X GET "https://localhost:7000/api/esg-dashboard/company-ranking?metric=emissions&limit=10" \
  -H "Authorization: Bearer {seu-token}"
```

**Resposta:**
```json
{
  "success": true,
  "message": "Ranking de empresas calculado com sucesso",
  "data": [
    {
      "companyId": 2,
      "companyName": "GreenCorp",
      "totalEmissions": 25.3,
      "rank": 1,
      "esgScore": "A"
    },
    {
      "companyId": 1,
      "companyName": "EcoTech Solutions",
      "totalEmissions": 45.5,
      "rank": 2,
      "esgScore": "B"
    }
  ]
}
```

### 4. Comparação entre Empresas
```bash
curl -X GET "https://localhost:7000/api/esg-dashboard/comparison?companyIds=1&companyIds=2" \
  -H "Authorization: Bearer {seu-token}"
```

## Cenários Avançados

### 1. Workflow Completo: Novo Usuário e Primeira Emissão

#### Passo 1: Registrar usuário
```bash
curl -X POST "https://localhost:7000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "maria.santos",
    "email": "maria.santos@greencorp.com",
    "password": "senha123",
    "firstName": "Maria",
    "lastName": "Santos",
    "companyId": 1
  }'
```

#### Passo 2: Fazer login
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "maria.santos",
    "password": "senha123"
  }'
```

#### Passo 3: Registrar primeira emissão
```bash
curl -X POST "https://localhost:7000/api/emissions" \
  -H "Authorization: Bearer {token-da-maria}" \
  -H "Content-Type: application/json" \
  -d '{
    "source": "Escritório Principal",
    "emissionAmount": 12.3,
    "unit": "tCO2e",
    "category": "Escopo 2",
    "location": "Brasília - DF",
    "description": "Primeira emissão registrada pela Maria",
    "recordDate": "2024-06-12T00:00:00Z",
    "companyId": 1
  }'
```

### 2. Busca Avançada com Múltiplos Filtros
```bash
curl -X GET "https://localhost:7000/api/emissions?page=1&pageSize=20&searchTerm=energia&category=Escopo%202&startDate=2024-01-01&endDate=2024-06-30&sortBy=emissionAmount&sortDirection=desc" \
  -H "Authorization: Bearer {seu-token}"
```

### 3. Geração de Relatório Trimestral Automatizado
```bash
# Script para gerar relatórios para todas as empresas
for company_id in 1 2 3; do
  curl -X POST "https://localhost:7000/api/sustainability-reports/generate?companyId=$company_id&year=2024&quarter=2" \
    -H "Authorization: Bearer {seu-token}"
done
```

### 4. Monitoramento de Performance
```bash
# Health check
curl -X GET "https://localhost:7000/health"

# Estatísticas em tempo real
curl -X GET "https://localhost:7000/api/esg-dashboard/statistics" \
  -H "Authorization: Bearer {seu-token}"
```

### 5. Exportação de Dados (via Query Parameters)
```bash
# Exportar emissões do último trimestre
curl -X GET "https://localhost:7000/api/emissions?startDate=2024-04-01&endDate=2024-06-30&pageSize=1000" \
  -H "Authorization: Bearer {seu-token}" \
  -H "Accept: application/json" > emissions_q2_2024.json
```

## Códigos de Status HTTP

| Código | Significado | Quando Ocorre |
|--------|-------------|---------------|
| 200 | OK | Operação bem-sucedida |
| 201 | Created | Recurso criado com sucesso |
| 400 | Bad Request | Dados inválidos ou faltando |
| 401 | Unauthorized | Token inválido ou ausente |
| 403 | Forbidden | Usuário sem permissão |
| 404 | Not Found | Recurso não encontrado |
| 409 | Conflict | Conflito (ex: email já existe) |
| 500 | Internal Server Error | Erro interno do servidor |

## Tratamento de Erros

### Exemplo de Resposta de Erro
```json
{
  "success": false,
  "message": "Dados inválidos",
  "errors": [
    "O campo EmissionAmount é obrigatório",
    "O campo Category deve ser um dos valores: Escopo 1, Escopo 2, Escopo 3"
  ],
  "timestamp": "2024-06-12T10:30:00Z"
}
```

### Exemplo de Erro de Autenticação
```json
{
  "message": "Token inválido",
  "statusCode": 401,
  "timestamp": "2024-06-12T10:30:00Z",
  "traceId": "0HN7GKQJKQK7K:00000001"
}
```

## Dicas de Performance

### 1. Use Paginação
```bash
# ✅ Bom - com paginação
curl -X GET "https://localhost:7000/api/emissions?page=1&pageSize=50"

# ❌ Evite - sem paginação (pode retornar muitos dados)
curl -X GET "https://localhost:7000/api/emissions"
```

### 2. Use Filtros Específicos
```bash
# ✅ Bom - filtro específico
curl -X GET "https://localhost:7000/api/emissions?companyId=1&category=Escopo%202"

# ❌ Evite - busca muito ampla
curl -X GET "https://localhost:7000/api/emissions?searchTerm=a"
```

### 3. Cache de Tokens
Reutilize tokens JWT até expirarem (padrão: 60 minutos) em vez de fazer login a cada requisição.

---

**Para mais exemplos e documentação interativa, acesse `/swagger` quando a aplicação estiver rodando.**

