# 🌾 AgroSolutions - Plataforma de Gestão Agrícola Inteligente

## 📋 Sumário

- [Visão Geral](#visão-geral)
- [Desenho da Solução](#desenho-da-solução)
- [Arquitetura](#arquitetura)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Design Patterns Aplicados](#design-patterns-aplicados)
- [Eventos e Mensageria](#eventos-e-mensageria)
- [Pontos Fortes](#pontos-fortes)
- [Microserviços](#microserviços)
- [Requisitos](#requisitos)
- [Como Executar](#como-executar)

## 🎯 Visão Geral

O **AgroSolutions** é uma plataforma completa de gestão agrícola baseada em microserviços, projetada para monitoramento em tempo real de propriedades rurais através de sensores IoT. A solução oferece análise inteligente de dados, geração de alertas preditivos e notificações automáticas para auxiliar na tomada de decisões agrícolas.

### Principais Funcionalidades

- 📊 **Monitoramento em Tempo Real** - Coleta e processamento contínuo de dados de sensores
- 🤖 **Motor de Regras Inteligente** - Análise automatizada com geração de alertas contextuais
- 📧 **Sistema de Notificações** - Canal de comunicação (email)
- 🏡 **Gestão de Propriedades** - Controle completo de propriedades, talhões e culturas
- 🔐 **Autenticação Segura** - Sistema robusto com JWT, RSA e conformidade LGPD
- 📈 **Observabilidade** - Métricas com Prometheus e health checks

## 🏗️ Desenho da Solução

### Diagrama da Arquitetura

[![Agro-Solutions.png](https://i.postimg.cc/T1nwmj2g/Agro-Solutions.png)](https://postimg.cc/wRTgYmmj)

```
┌───────────────────────────────────────────────────────────────────────────────────────┐
│                          CAMADA DE MICROSERVIÇOS (APIs REST)                          │
│                                                                                       │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                            IDENTIDADE (5001)                                    │  │
│  │  • Autenticação JWT (RSA 2048)    • Gestão de Usuários                          │  │
│  │  • JWKS Endpoint                  • Conformidade LGPD                           │  │
│  └──────────────────┬──────────────────────────────────────────────────────────────┘  │
│                     │ Publica: UsuarioCriadoEvent, UsuarioAtualizadoEvent             │
│                     ▼                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                            PROPRIEDADES (5002)                                  │  │
│  │  • Cadastro de Propriedades        • Gestão de Talhões                          │  │
│  │  • Cadastro de Culturas            • Sincronização de Usuários                  │  │
│  └──────────────────┬──────────────────────────────────────────────────────────────┘  │
│                     │ Consome: UsuarioCriadoEvent                                     │
│                     │ Publica: PropriedadeCriadaEvent, TalhaoCriadoEvent,             │
│                     │          TalhaoAtualizadoEvent                                  │
│                     ▼                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                           SENSORES (5003)                                       │  │
│  │  • Simulador de Cenários           • Geração de Leituras                        │  │
│  │  • Testes de Alertas               • Integração c/ IngestaoDados                │  │
│  └──────────────────┬──────────────────────────────────────────────────────────────┘  │
│                     │ Envia POST para: IngestaoDados/api/leituras                     │
│                     ▼                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                           INGESTÃO DE DADOS (5004)                              │  │
│  │  • Recebe Leituras de Sensores     • Validação de Dados                         │  │
│  │  • Detecção Bateria/Sinal          • Persistência de Leituras Brutas            │  │
│  └──────────────────┬──────────────────────────────────────────────────────────────┘  │
│                     │ Publica: LeituraRecebidaEvent, AlertaSensorEvent                │
│                     ▼                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                           PROCESSAMENTO DE DADOS (5005)                         │  │
│  │  • Agregação de Leituras           • Cálculos Estatísticos                      │  │
│  │  • Média/Min/Max/DesvioPadrão      • Janelas Temporais (hora/dia/semana)        │  │
│  └──────────────────┬──────────────────────────────────────────────────────────────┘  │
│                     │ Consome: LeituraRecebidaEvent                                   │
│                     │ Publica: DadosProcessadosEvent                                  │
│                     ▼                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                           ANÁLISE (5006)                                        │  │
│  │  • Motor de Regras                 • Geração de Alertas                         │  │
│  │  • Regras: Seca, Geada, Calor      • Cache de Talhões                           │  │
│  │  • Severidade Dinâmica             • Recomendações Contextualizadas             │  │
│  └──────────────────┬──────────────────────────────────────────────────────────────┘  │
│                     │ Consome: DadosProcessadosEvent, TalhaoCriadoEvent               │
│                     │ Publica: AlertaGeradoEvent                                      │
│                     ▼                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                           NOTIFICAÇÕES (5007)                                   │  │
│  │  • Envio de Emails                 • Múltiplos Canais (SMS futuro)              │  │
│  │  • Retry com Backoff               • Cache de Propriedades                      │  │
│  │  • Templates HTML                  • Histórico de Notificações                  │  │
│  └──────────────────┬──────────────────────────────────────────────────────────────┘  │
│                     │ Consome: AlertaGeradoEvent, AlertaSensorEvent,                  │
│                     │          PropriedadeCriadaEvent                                 │
│                     │ Publica: NotificacaoEnviadaEvent                                │
│                     │ Integra: SMTP (Gmail, Outlook, SendGrid)                        │
└─────────────────────┼─────────────────────────────────────────────────────────────────┘
                      │
                      ▼
┌───────────────────────────────────────────────────────────────────────────────────────┐
│                          CAMADA DE MENSAGERIA (Event Bus)                             │
│                                                                                       │
│                             ┌───────────────────────────┐                             │
│                             │   RabbitMQ (5672/15672)   │                             │
│                             │                           │                             │
│                             │  Exchanges (Topic):       │                             │
│                             │  • agrosolutions.*        │                             │
│                             │                           │                             │
│                             │  Principais Eventos:      │                             │
│                             │  1 UsuarioCriadoEvent     │                             │
│                             │  2 PropriedadeCriadaEvent │                             │
│                             │  3 TalhaoCriadoEvent      │                             │
│                             │  4 LeituraRecebidaEvent   │                             │
│                             │  5 DadosProcessadosEvent  │                             │
│                             │  6 AlertaGeradoEvent      │                             │
│                             │  7 AlertaSensorEvent      │                             │
│                             │  8 NotificacaoEnviadaEvent│                             │
│                             └───────────────────────────┘                             │
└─────────────────────┬─────────────────────────────────────────────────────────────────┘
                      │
                      ▼
┌───────────────────────────────────────────────────────────────────────────────────────┐
│                        CAMADA DE PERSISTÊNCIA & OBSERVABILIDADE                       │
│                                                                                       │
│  ┌────────────────────────────────┐            ┌───────────────────────────────────┐  │
│  │   SQL Server / PostgreSQL      │            │    OBSERVABILIDADE                │  │
│  │                                │            │                                   │  │
│  │  Databases:                    │            │  ┌─────────────────────────────┐  │  │
│  │  • agrosolutions_identidade    │            │  │  Prometheus (9090)          │  │  │
│  │  • agrosolutions_propriedades  │            │  │  • Métricas customizadas    │  │  │
│  │  • agrosolutions_ingestao      │            │  │  • Endpoint /metrics        │  │  │
│  │  • agrosolutions_processamento │            │  │  • Business KPIs            │  │  │
│  │  • agrosolutions_analise       │            │  └────────────┬────────────────┘  │  │
│  │  • agrosolutions_notificacoes  │            │               │                   │  │
│  │                                │            │               ▼                   │  │
│  │  Pattern:                      │            │  ┌─────────────────────────────┐  │  │
│  │  Database per Service          │            │  │  Grafana (3000)             │  │  │
│  └────────────────────────────────┘            │  │  • Dashboards               │  │  │
│                                                │  │  • Alertas visuais          │  │  │
│                                                │  │  • Análise de tendências    │  │  │
│                                                │  └─────────────────────────────┘  │  │
│                                                └───────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────────────────────────────┘
```

### Fluxo Completo de Dados

#### 📝 **Fase 1: Setup Inicial**

**1.1 Cadastro de Usuário**
```
1. POST /api/auth/registrar (Identidade)
2. Identidade → RabbitMQ: UsuarioCriadoEvent
3. Propriedades ← RabbitMQ: Sincroniza usuário localmente
```

**1.2 Cadastro de Propriedade e Talhão**
```
1. POST /api/propriedades (Propriedades)
2. POST /api/propriedades/{id}/talhoes (Propriedades)
3. Propriedades → RabbitMQ: PropriedadeCriadaEvent, TalhaoCriadoEvent
4. Análise ← RabbitMQ: Cria cache de talhão para alertas
5. Notificações ← RabbitMQ: Registra destinatário de notificações
```

#### 📡 **Fase 2: Pipeline de Dados IoT (Fluxo Contínuo)**

**2.1 Simulação/Captura de Leituras**
```
1. Sensores → POST /api/leituras (IngestaoDados)
   Payload: { deviceId, tipoSensor, valor, timestamp, talhaoId }
2. IngestaoDados valida e persiste leitura bruta
3. IngestaoDados → RabbitMQ: LeituraRecebidaEvent
   {
     id, sensorId, deviceId, propriedadeId, talhaoId,
     tipoSensor, valor, unidade, timestampLeitura, qualidade
   }
```

**2.2 Verificação de Alertas Técnicos**
```
4. SE (bateriaBaixa OU sinalFraco)
   IngestaoDados → RabbitMQ: AlertaSensorEvent
   Notificações ← RabbitMQ: Envia email técnico ao administrador
```

**2.3 Processamento e Agregação**
```
5. ProcessamentoDados ← RabbitMQ: Consome LeituraRecebidaEvent
6. ProcessamentoDados processa:
   • Armazena leitura processada
   • Calcula agregações (médias horárias, diárias, semanais)
   • Computa: média, mínimo, máximo, desvio padrão
7. ProcessamentoDados → RabbitMQ: DadosProcessadosEvent
   {
     id, talhaoId, tipoSensor, dataInicio, dataFim,
     valorMedio, valorMinimo, valorMaximo, desvioPadrao, numeroLeituras
   }
```

**2.4 Análise de Regras e Geração de Alertas**
```
8. Análise ← RabbitMQ: Consome DadosProcessadosEvent
9. Análise avalia regras (exemplos):
   
   Regra de Seca:
   SE (umidadeSolo < 30% POR > 24h)
   ENTÃO: Gerar AlertaSeca (Severidade: Alta)
   
   Regra de Geada:
   SE (temperatura < 2°C POR > 3h)
   ENTÃO: Gerar AlertaGeada (Severidade: Crítica)
   
   Regra de Calor Excessivo:
   SE (temperatura > 35°C POR > 6h)
   ENTÃO: Gerar AlertaCalorExcessivo (Severidade: Média)

10. SE (regra acionada):
    Análise → RabbitMQ: AlertaGeradoEvent
    {
      alertaId, talhaoId, tipo, severidade,
      titulo, mensagem, recomendacao, valorReferencia,
      destinatarioId, emailDestinatario, nomeDestinatario
    }
```

**2.5 Notificação ao Produtor**
```
11. Notificações ← RabbitMQ: Consome AlertaGeradoEvent
12. Notificações:
    • Cria registro de notificação (status: Pendente)
    • Gera email com template HTML
    • Envia email via SMTP
    • Atualiza status (Enviada ou Falha)
    • Retry automático (até 3 tentativas se falhar)
13. Notificações → RabbitMQ: NotificacaoEnviadaEvent
    {
      notificacaoId, destinatarioId, tipo, canal,
      status, dataEnvio, sucesso
    }
```

#### 📊 **Fase 3: Observabilidade (Contínua)**

**3.1 Coleta de Métricas**
```
1. Todos microserviços → Prometheus: Endpoint /metrics
   • Métricas técnicas: CPU, memória, requests/s
   • Métricas de negócio: leituras/s, alertas gerados, emails enviados
   • Histogramas: tempos de processamento, latências
```

**3.2 Visualização**
```
2. Grafana ← Prometheus: Query de métricas
3. Dashboards exibem:
   • Status de cada talhão (Normal, Alerta, Crítico)
   • Gráficos de leituras ao longo do tempo
   • Taxa de alertas gerados por tipo
   • Performance dos microserviços
   • Taxa de sucesso de notificações
```

### Justificativa Técnica das Decisões Arquiteturais

#### 1. **Arquitetura de Microserviços**

**Decisão**: Adoção de microserviços independentes com comunicação assíncrona.

**Justificativa**:
- **Escalabilidade Independente**: Cada serviço pode escalar conforme demanda (ex: Ingestão precisa escalar mais que Identidade)
- **Deploy Independente**: Permite atualizações sem downtime total da plataforma
- **Isolamento de Falhas**: Falha em um serviço não compromete toda a aplicação
- **Flexibilidade Tecnológica**: Possibilidade de usar diferentes bancos de dados por contexto
- **Time Autonomia**: Equipes podem trabalhar independentemente em cada domínio

#### 2. **Event-Driven Architecture (EDA)**

**Decisão**: Comunicação entre microserviços através de eventos assíncronos via RabbitMQ.

**Justificativa**:
- **Desacoplamento**: Serviços não precisam conhecer implementações uns dos outros
- **Resiliência**: Mensagens persistidas garantem entrega mesmo com serviços offline
- **Auditoria**: Todo evento fica registrado para compliance e troubleshooting
- **Extensibilidade**: Novos consumidores podem ser adicionados sem modificar produtores
- **Performance**: Operações não bloqueantes melhoram throughput geral

#### 3. **Database per Service**

**Decisão**: Cada microserviço possui seu próprio banco de dados.

**Justificativa**:
- **Autonomia**: Mudanças no schema não afetam outros serviços
- **Otimização**: Escolha do banco mais adequado para cada contexto
- **Isolamento**: Falhas de banco de dados ficam contidas
- **Escalabilidade**: Bancos podem escalar independentemente

#### 4. **Shared Kernel**

**Decisão**: Biblioteca compartilhada com componentes transversais.

**Justificativa**:
- **DRY Principle**: Evita duplicação de código comum
- **Padronização**: Garante comportamento consistente entre serviços
- **Manutenibilidade**: Correções propagam para todos os serviços
- **Produtividade**: Acelera desenvolvimento de novos microserviços

#### 5. **Observabilidade com Métricas**

**Decisão**: Prometheus para coleta de métricas e health checks.

**Justificativa**:
- **Detecção Proativa**: Métricas permitem identificar problemas antes do usuário
- **Métricas de Negócio**: Monitoramento de KPIs específicos do domínio
- **Health Checks**: Verificação contínua da saúde dos serviços
- **Extensível**: Preparado para adicionar distributed tracing no futuro

### Descrição de Como os Requisitos Não Funcionais Serão Atendidos

#### 🚀 Performance

- **Async Processing**: Operações I/O assíncronas com async/await em todos os serviços
- **Database Optimization**: Índices apropriados, queries otimizadas, connection pooling do EF Core
- **Background Processing**: Processamento em background com BackgroundService do .NET
- **Parallel Consumers**: Múltiplos consumidores RabbitMQ processando mensagens em paralelo

#### 🔒 Segurança

- **Autenticação JWT**: Tokens assinados com RSA 2048 bits
- **JWKS**: Validação distribuída de tokens usando JSON Web Key Set
- **Criptografia AES-256**: Dados sensíveis criptografados em repouso
- **Argon2id**: Hash de senhas com memória 64MB, 4 iterações e paralelismo 2
- **CORS**: Configuração de origens permitidas
- **HTTPS**: Comunicação criptografada em trânsito

#### 📊 Escalabilidade

- **Stateless Services**: Serviços sem estado preparados para escalonamento horizontal
- **Message Queue**: RabbitMQ com múltiplos consumidores paralelos
- **Database per Service**: Isolamento de dados por microserviço
- **Docker Ready**: Dockerfile e docker-compose.yml funcionais

#### 🛡️ Confiabilidade

- **Retry Policies**: EF Core com retry automático em falhas transientes de banco de dados
- **Health Checks**: Endpoint /health em todos os microserviços
- **Saga Pattern**: SagaOrchestrator com compensação automática de transações distribuídas
- **Message Durability**: RabbitMQ com mensagens persistidas e acknowledgment manual
- **Isolation**: Falha em um microserviço não compromete os demais

#### 🔍 Observabilidade

- **Prometheus**: Endpoint /metrics em todos os serviços com métricas customizadas
- **Grafana**: Dashboards para visualização de métricas em tempo real
- **Health Checks**: Endpoint /health com verificação de banco e RabbitMQ
- **Business Metrics**: KPIs específicos por domínio (leituras/s, alertas gerados, etc.)

#### ⚖️ Conformidade LGPD

- **Anonimização**: Processo automatizado de anonimização de dados
- **Exclusão Automática**: Job scheduled para exclusão após período legal
- **Auditoria**: Registro de todos os acessos e modificações de dados pessoais
- **Consentimento**: Rastreamento de consentimentos e preferências
- **Criptografia**: Dados pessoais criptografados em repouso e trânsito

#### 🔄 Manutenibilidade

- **Clean Architecture**: Separação clara de responsabilidades
- **SOLID Principles**: Código orientado a princípios de design
- **Testes Automatizados**: Cobertura de testes unitários e arquiteturais
- **Documentation**: OpenAPI/Scalar para documentação interativa de APIs
- **Code Standards**: Convenções consistentes entre todos os serviços

## 💻 Tecnologias Utilizadas

### Core Framework
- **.NET 10** - Framework principal da aplicação
- **C# 14** - Linguagem de programação

### Banco de Dados
- **Entity Framework Core 10** - ORM para acesso a dados
- **Dapper 2.1** - Micro-ORM para consultas de alta performance
- **SQL Server** - Banco de dados principal (suporte também para PostgreSQL)
- **Database per Service Pattern** - Isolamento de dados por microserviço

### Mensageria
- **RabbitMQ** - Message broker para comunicação assíncrona
- **RabbitMQ.Client 7.x** - Cliente oficial para .NET

### Autenticação e Segurança
- **JWT (JSON Web Tokens)** - Autenticação stateless
- **RSA 2048 bits** - Criptografia assimétrica para tokens
- **JWKS (JSON Web Key Set)** - Distribuição de chaves públicas
- **Argon2id** - Hash de senhas

### Observabilidade
- **Prometheus** - Métricas e monitoramento
- **Grafana** - Dashboards e visualização
- **Health Checks** - Verificação de saúde dos serviços

### Documentação
- **OpenAPI (.NET 10)** - Especificação OpenAPI 3.0 nativa do .NET 10
- **Scalar** - UI moderna e interativa para visualização de APIs

### Testes
- **xUnit** - Framework de testes unitários e de integração
- **FluentAssertions** - Assertions fluentes e legíveis
- **Moq** - Mocking framework
- **ArchUnitNET** - Testes de arquitetura garantindo aderência aos princípios SOLID

### Ferramentas de Desenvolvimento
- **FluentValidation** - Validações fluentes e expressivas

## 🎨 Design Patterns Aplicados

### Arquiteturais

#### 1. **Microservices Architecture**
- Decomposição por domínio de negócio
- Serviços independentes e autônomos
- Comunicação via API REST e eventos

#### 2. **Event-Driven Architecture (EDA)**
- Comunicação assíncrona entre serviços
- Publicação e consumo de eventos de integração
- Desacoplamento temporal e espacial

#### 3. **Clean Architecture**
- Separação em camadas: Domain, Application, Infrastructure, API
- Dependências direcionadas para o centro (Domain)
- Testabilidade e manutenibilidade maximizadas

#### 4. **Domain-Driven Design (DDD)**
- Bounded Contexts claramente definidos
- Entities, Value Objects e Aggregates
- Linguagem ubíqua por contexto

### Criacionais

#### 1. **Factory Pattern**
- `DatabaseProviderStrategies` - Criação de strategies de banco de dados
- Encapsulamento da lógica de criação de objetos complexos

#### 2. **Builder Pattern**
- Configuração de serviços no `Program.cs`
- Construção fluente de objetos complexos

#### 3. **Dependency Injection (IoC)**
- Injeção de dependências nativa do .NET
- Inversão de controle para desacoplamento

### Estruturais

#### 1. **Repository Pattern**
- Abstração do acesso a dados
- Interfaces por agregado (`IUsuarioRepository`, `IPropriedadeRepository`)
- Testabilidade com mocks

#### 2. **Strategy Pattern**
- `IDatabaseProviderStrategy` - Suporte a múltiplos bancos
- Algoritmos intercambiáveis (SQL Server, PostgreSQL)

#### 3. **Adapter Pattern**
- Adaptação de bibliotecas externas (RabbitMQ, Email)
- Isolamento de dependências externas

#### 4. **Facade Pattern**
- `MicroserviceExtensions` - Simplificação de configurações complexas
- Interface unificada para subsistemas

### Comportamentais

#### 1. **Saga Pattern (Orchestration)**
- `SagaOrchestrator` - Coordenação de transações distribuídas
- Compensação automática em caso de falha
- Exemplo: Criação completa de propriedade com talhões

#### 2. **Observer Pattern**
- Event-driven communication via RabbitMQ
- Múltiplos consumidores observando eventos

#### 3. **Template Method**
- `BaseEntity` - Comportamentos comuns para entidades
- `StandardMicroserviceServices` - Template de configuração

#### 4. **Chain of Responsibility**
- Middlewares do ASP.NET Core (Exception handling, encoding, CORS)
- Pipeline de processamento de requisições

### Outros Patterns

#### 1. **Unit of Work**
- DbContext do EF Core gerencia transações
- Commit/Rollback automático

## 📨 Eventos e Mensageria

### Arquitetura de Mensageria

A solução utiliza **RabbitMQ** como message broker com os seguintes padrões:

- **Exchange Type**: Topic Exchange (flexível para roteamento)
- **Queue Type**: Durable (persistência de mensagens)
- **Serialização**: JSON (interoperabilidade)
- **Retry Policy**: Exponential backoff
- **Dead Letter Queue**: Para mensagens com falha permanente

### Eventos de Integração

#### 🔐 Identidade

**UsuarioCriadoEvent**
```csharp
{
  "Id": "guid",
  "Email": "string",
  "NomeCompleto": "string",
  "DataCriacao": "datetime"
}
```
- **Produzido por**: Identidade
- **Consumido por**: Propriedades
- **Propósito**: Sincronizar dados de usuário para outros contextos

**UsuarioAtualizadoEvent**
```csharp
{
  "Id": "guid",
  "Email": "string",
  "NomeCompleto": "string",
  "DataAtualizacao": "datetime"
}
```
- **Produzido por**: Identidade
- **Consumido por**: Propriedades, Notificações
- **Propósito**: Manter dados de usuário sincronizados

#### 🏡 Propriedades

**PropriedadeCriadaEvent**
```csharp
{
  "PropriedadeId": "guid",
  "Nome": "string",
  "Endereco": "string",
  "AreaTotal": "decimal",
  "ProprietarioId": "guid",
  "EmailProprietario": "string",
  "NomeProprietario": "string",
  "DataCriacao": "datetime"
}
```
- **Produzido por**: Propriedades
- **Consumido por**: Notificações
- **Propósito**: Configurar destinatários de alertas

**TalhaoCriadoEvent**
```csharp
{
  "TalhaoId": "guid",
  "PropriedadeId": "guid",
  "Nome": "string",
  "Area": "decimal",
  "CulturaId": "guid",
  "NomeCultura": "string"
}
```
- **Produzido por**: Propriedades
- **Consumido por**: Análise
- **Propósito**: Habilitar monitoramento e regras de alertas

#### 📡 Ingestão de Dados

**LeituraRecebidaEvent**
```csharp
{
  "Id": "guid",
  "SensorId": "guid",
  "DeviceId": "string",
  "PropriedadeId": "guid",
  "TalhaoId": "guid",
  "TipoSensor": "enum",
  "Valor": "decimal",
  "Unidade": "string",
  "TimestampLeitura": "datetime",
  "Qualidade": "enum",
  "BateriaBaixa": "bool",
  "SinalFraco": "bool"
}
```
- **Produzido por**: IngestaoDados
- **Consumido por**: ProcessamentoDados
- **Propósito**: Iniciar pipeline de processamento

**AlertaSensorEvent**
```csharp
{
  "SensorId": "guid",
  "TipoAlerta": "enum",
  "Severidade": "enum",
  "Mensagem": "string"
}
```
- **Produzido por**: IngestaoDados
- **Consumido por**: Notificações
- **Propósito**: Alertas técnicos de sensores (bateria, sinal)

#### 📊 Processamento de Dados

**DadosProcessadosEvent**
```csharp
{
  "Id": "guid",
  "TalhaoId": "guid",
  "TipoSensor": "enum",
  "DataInicio": "datetime",
  "DataFim": "datetime",
  "ValorMedio": "decimal",
  "ValorMinimo": "decimal",
  "ValorMaximo": "decimal",
  "DesvioPadrao": "decimal",
  "NumeroLeituras": "int"
}
```
- **Produzido por**: ProcessamentoDados
- **Consumido por**: Análise
- **Propósito**: Fornecer dados agregados para análise de regras

#### 🤖 Análise

**AlertaGeradoEvent**
```csharp
{
  "AlertaId": "guid",
  "TalhaoId": "guid",
  "Tipo": "enum",
  "Severidade": "enum",
  "Titulo": "string",
  "Mensagem": "string",
  "Recomendacao": "string",
  "ValorReferencia": "decimal",
  "DestinatarioId": "guid",
  "EmailDestinatario": "string"
}
```
- **Produzido por**: Análise
- **Consumido por**: Notificações
- **Propósito**: Enviar alertas aos produtores rurais

#### 📧 Notificações

**NotificacaoEnviadaEvent**
```csharp
{
  "NotificacaoId": "guid",
  "DestinatarioId": "guid",
  "Tipo": "enum",
  "Canal": "enum",
  "Status": "enum",
  "DataEnvio": "datetime"
}
```
- **Produzido por**: Notificações
- **Consumido por**: (Futuro) Analytics, Dashboard
- **Propósito**: Auditoria e métricas de notificações

### Fluxo de Eventos Principal

```
1. Sensor → IngestaoDados: POST /api/leituras
2. IngestaoDados → RabbitMQ: LeituraRecebidaEvent
3. ProcessamentoDados ← RabbitMQ: Consome LeituraRecebidaEvent
4. ProcessamentoDados → RabbitMQ: DadosProcessadosEvent
5. Análise ← RabbitMQ: Consome DadosProcessadosEvent
6. Análise → RabbitMQ: AlertaGeradoEvent
7. Notificações ← RabbitMQ: Consome AlertaGeradoEvent
8. Notificações → Email/SMS: Envia notificação ao produtor
9. Notificações → RabbitMQ: NotificacaoEnviadaEvent
```

### Configurações de Mensageria

- **Prefetch Count**: 10 mensagens por consumer
- **Acknowledgment**: Manual (garantia de processamento)
- **TTL (Time to Live)**: 5 minutos (configurável)
- **Max Retry**: 3 tentativas com exponential backoff
- **Dead Letter Exchange**: Para mensagens com falha permanente

## 🌟 Pontos Fortes

### 🔒 Segurança Robusta

#### Autenticação JWT
- **JWT com RSA 2048 bits**: Tokens assinados com criptografia assimétrica forte
- **JWKS Endpoint**: Validação distribuída sem compartilhar chave privada (`/.well-known/jwks.json`)
- **Múltiplos Perfis**: Administrador, Produtor, Técnico, Visualizador
- **Token Expiration**: Tokens com expiração configurável (padrão: 60 minutos)

#### Proteção de Dados
- **Criptografia AES-256**: Dados sensíveis criptografados em repouso no banco
- **Hash Argon2id**: Senhas com memória 64MB, 4 iterações e paralelismo 2 (resistente a ataques GPU/ASIC)
- **Connection String Masking**: Senhas mascaradas em logs para segurança
- **HTTPS**: Comunicação criptografada em trânsito

### ⚖️ Conformidade Total com LGPD

#### Direitos do Titular
- **Anonimização Automática**: Processo completo de anonimização de dados
- **Exclusão em 30 dias**: Job automatizado conforme Art. 16 LGPD
- **Exportação de Dados**: Endpoint para download de todos dados pessoais
- **Correção de Dados**: API para atualização de informações

#### Rastreabilidade
- **Auditoria Completa**: Registro de todos acessos e modificações
- **Retenção Configurável**: Políticas de retenção por tipo de dado
- **Consentimentos**: Tracking de consentimentos e preferências
- **DPO Ready**: Estrutura preparada para Data Protection Officer

#### Segurança por Design
- **Privacy by Default**: Configurações mais restritivas por padrão
- **Data Minimization**: Coleta apenas do necessário
- **Purpose Limitation**: Uso restrito ao propósito informado
- **Encryption Everywhere**: Criptografia em múltiplas camadas

### 🏗️ Arquitetura Moderna e Escalável

#### Microserviços Puros
- **Single Responsibility**: Cada serviço com propósito único e bem definido
- **Autonomous**: Deploy, scaling e desenvolvimento independentes
- **Resilient**: Isolamento de falhas e degradação graceful

#### Event-Driven
- **Async First**: Comunicação não-bloqueante entre serviços
- **Saga Pattern**: Transações distribuídas com compensação

#### Cloud Native
- **Stateless Services**: Facilita escalonamento horizontal
- **Configuration External**: 12-Factor App compliance
- **Container Ready**: Preparado para Docker e Kubernetes

### 📊 Observabilidade de Ponta a Ponta

#### Distributed Tracing
- **OpenTelemetry**: Padrão open-source de observabilidade
- **Correlation IDs**: Rastreamento de requisições através de serviços
- **Jaeger Integration**: Visualização de traces distribuídos

#### Métricas de Negócio
- **Prometheus**: Métricas customizadas por domínio
- **Business KPIs**: Leituras/segundo, alertas gerados, taxa de processamento
- **Custom Metrics**: Histogramas, counters e gauges específicos por contexto

#### Logging Estruturado
- **ILogger (.NET)**: Sistema de logging nativo estruturado
- **Contextual Logging**: Enriquecimento com correlation IDs via OpenTelemetry
- **Centralized Ready**: Arquitetura preparada para agregadores externos

### 🧪 Qualidade e Testabilidade

#### Testes Automatizados
- **Unit Tests**: Cobertura de lógica de negócio com xUnit
- **Architecture Tests**: Garantia de padrões arquiteturais com ArchUnitNET
- **Saga Integration Tests**: Validação de transações distribuídas

#### Clean Code
- **SOLID Principles**: Código orientado a princípios
- **DRY**: Reutilização através do SharedKernel
- **Readable**: Nomes expressivos e código auto-documentado

### 🚀 Performance e Eficiência

#### Otimizações de Banco
- **Connection Pooling**: Reutilização de conexões
- **Async All The Way**: Operações I/O não-bloqueantes
- **Indexed Queries**: Índices estratégicos para queries frequentes
- **Retry Policies**: Resiliência contra falhas transientes
- **Hybrid ORM**: EF Core para operações CRUD + Dapper para consultas complexas de alta performance

#### Processamento Eficiente
- **Parallel Processing**: Múltiplos consumers RabbitMQ
- **Batch Operations**: Agregação de leituras em lotes
- **Background Jobs**: Processamento assíncrono de tarefas pesadas

### 📚 Documentação Abrangente

#### APIs Documentadas
- **OpenAPI 3.0**: Especificação OpenAPI nativa do .NET 10
- **Scalar UI**: Interface moderna e interativa para testes de API
- **Auto-documentação**: Endpoints documentados automaticamente
- **Exemplos**: Requests e responses de exemplo

#### Código Documentado
- **XML Comments**: Documentação inline de classes e métodos
- **README por projeto**: Contexto e instruções específicas
- **Architecture Decision Records**: Decisões arquiteturais documentadas

### 🔄 Extensibilidade

#### Plugin Architecture
- **Strategy Pattern**: Fácil adição de novos providers (bancos, clouds)
- **Event-Driven**: Novos consumers sem modificar producers
- **Interface Segregation**: Contratos pequenos e focados

#### Multi-Database
- **Database Agnostic**: Suporte PostgreSQL e SQL Server
- **Easy Migration**: Migrations automáticas via EF Core

### 🛡️ Resiliência

#### Fault Tolerance
- **Retry Policies**: EF Core com retry automático em falhas transientes de banco (max 3 tentativas)
- **Health Checks**: Monitoramento contínuo de todas as dependências
- **Isolation**: Falha em um microserviço não afeta os outros (fault isolation)

#### Message Reliability
- **Durable Queues**: Mensagens persistidas em disco pelo RabbitMQ
- **Manual Acknowledgment**: Garantia de processamento de cada mensagem
- **Dead Letter Exchange**: Tratamento de mensagens com falha permanente
- **Retry com Exponential Backoff**: Até 3 tentativas com atraso crescente

## 🏛️ Microserviços

A solução é composta pelos seguintes microserviços (organizados por fluxo do processo):

| Ordem | Microserviço | Responsabilidade | Porta |
|-------|--------------|------------------|-------|
| 0️⃣ | **SharedKernel** | Biblioteca compartilhada com componentes transversais | N/A |
| 1️⃣ | **Identidade** | Autenticação, autorização e gestão de usuários | 5001 |
| 2️⃣ | **Propriedades** | Gestão de propriedades, talhões e culturas | 5002 |
| 3️⃣ | **Sensores** | Simulador de sensores IoT (gera leituras) | 5003 |
| 4️⃣ | **IngestaoDados** | Recebimento e validação de leituras de sensores | 5004 |
| 5️⃣ | **ProcessamentoDados** | Agregação e processamento de dados | 5005 |
| 6️⃣ | **Análise** | Motor de regras e geração de alertas | 5006 |
| 7️⃣ | **Notificações** | Envio de notificações multi-canal (email) | 5007 |

### 📁 Estrutura de Cada Microserviço

```
Microservico/
├── API/
│   ├── Controllers/                         # Endpoints REST
│   └── Extensions/                          # Extensões de API
├── Application/
│   ├── DTOs/                                # Data Transfer Objects
│   ├── Interfaces/                          # Contratos de serviços
│   ├── Services/                            # Lógica de aplicação
│   ├── Validators/                          # Validações com FluentValidation
│   ├── Events/                              # Eventos de integração
│   └── Sagas/                               # Orquestradores de saga (quando aplicável)
├── Domain/
│   ├── Entities/                            # Entidades de domínio
│   ├── Enums/                               # Enumerações
│   ├── Interfaces/                          # Contratos de repositórios
│   └── ValueObjects/                        # Objetos de valor
├── Infrastructure/
│   ├── Data/                                # DbContext e migrations
│   ├── Repositories/                        # Implementações de repositórios
│   ├── Services/                            # Serviços de infraestrutura (RabbitMQ, Email)
│   └── Metrics/                             # Métricas Prometheus
├── Configuration/
│   ├── Settings/                            # Classes de configuração
│   ├── ApiConfiguration.cs
│   ├── DependencyInjectionConfiguration.cs
│   └── MonitoringConfiguration.cs
└── Program.cs                               # Entry point
```

## 📋 Requisitos

### Software Necessário

- **.NET 10 SDK** (ou superior)
- **SQL Server 2019+** ou **PostgreSQL 14+**
- **RabbitMQ 3.13+**
- **Docker** (opcional, para RabbitMQ e bancos)
- **Visual Studio 2022** ou **VS Code** com extensões C#

### Serviços de Infraestrutura

- **RabbitMQ**: `localhost:5672` (Management UI: `localhost:15672`)
- **SQL Server**: `localhost:1433` (ou PostgreSQL: `localhost:5432`)
- **Prometheus** (opcional): `localhost:9090`
- **Grafana** (opcional): `localhost:3000`

### Microserviços (Portas por Fluxo)

| Ordem | Serviço | Porta | Acesso |
|-------|---------|-------|--------|
| 1️⃣ | Identidade | 5001 | https://localhost:5001 |
| 2️⃣ | Propriedades | 5002 | https://localhost:5002 |
| 3️⃣ | Sensores | 5003 | https://localhost:5003 |
| 4️⃣ | IngestaoDados | 5004 | https://localhost:5004 |
| 5️⃣ | ProcessamentoDados | 5005 | https://localhost:5005 |
| 6️⃣ | Análise | 5006 | https://localhost:5006 |
| 7️⃣ | Notificações | 5007 | https://localhost:5007 |

## 🚀 Como Executar

### 1. Clonar o Repositório

```bash
git clone https://github.com/your-org/agrosolutions.git
cd agrosolutions
```

### 2. Configurar Infraestrutura

#### Opção A: Docker Compose (Recomendado)

```bash
docker-compose up -d
```

#### Opção B: Instalação Manual

Instale e configure:
- RabbitMQ
- SQL Server ou PostgreSQL
- Prometheus (opcional)
- Grafana (opcional)

### 3. Configurar Strings de Conexão

Cada microserviço possui um `appsettings.json` que deve ser configurado:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=agrosolutions_identidade;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

### 4. Executar Migrations

```bash
cd src/AgroSolutions.Identidade
dotnet ef database update

cd ../AgroSolutions.Propriedades
dotnet ef database update

cd ../AgroSolutions.IngestaoDados
dotnet ef database update

cd ../AgroSolutions.ProcessamentoDados
dotnet ef database update

cd ../AgroSolutions.Analise
dotnet ef database update

cd ../AgroSolutions.Notificacoes
dotnet ef database update
```

### 5. Executar os Microserviços

**Ordem recomendada (seguindo fluxo do processo):**

```bash
# Terminal 1 - 🔐 Identidade (Base: Auth)
cd src/AgroSolutions.Identidade
dotnet run  # Porta 5001

# Terminal 2 - 🏡 Propriedades (Setup: Cadastros)
cd src/AgroSolutions.Propriedades
dotnet run  # Porta 5002

# Terminal 3 - 📡 Sensores (IoT: Simulador)
cd src/AgroSolutions.Sensores
dotnet run  # Porta 5003

# Terminal 4 - 📥 IngestaoDados (IoT: Coleta)
cd src/AgroSolutions.IngestaoDados
dotnet run  # Porta 5004

# Terminal 5 - 📊 ProcessamentoDados (IoT: Agregação)
cd src/AgroSolutions.ProcessamentoDados
dotnet run  # Porta 5005

# Terminal 6 - 🤖 Análise (IoT: Motor de Regras)
cd src/AgroSolutions.Analise
dotnet run  # Porta 5006

# Terminal 7 - 📧 Notificações (IoT: Alertas)
cd src/AgroSolutions.Notificacoes
dotnet run  # Porta 5007
```

### 6. Acessar as APIs

Todas as APIs possuem documentação interativa via Scalar (organizadas por fluxo do processo):

**🔐 Fundação**
- **Identidade** (Auth): https://localhost:5001/scalar/v1

**🏡 Configuração**
- **Propriedades** (Cadastros): https://localhost:5002/scalar/v1

**📡 Pipeline de Dados IoT**
- **Sensores** (Simulador): https://localhost:5003/scalar/v1
- **IngestaoDados** (Coleta): https://localhost:5004/scalar/v1
- **ProcessamentoDados** (Agregação): https://localhost:5005/scalar/v1
- **Análise** (Motor de Regras): https://localhost:5006/scalar/v1
- **Notificações** (Alertas): https://localhost:5007/scalar/v1

**Nota**: Acesso direto à raiz (ex: https://localhost:5001/) redireciona automaticamente para o Scalar.

## 📖 Documentação Adicional

Para documentação detalhada de cada microserviço, consulte os READMEs específicos:

- [SharedKernel](src/AgroSolutions.SharedKernel/README.md)
- [Identidade](src/AgroSolutions.Identidade/README.md)
- [Propriedades](src/AgroSolutions.Propriedades/README.md)
- [Sensores](src/AgroSolutions.Sensores/README.md)
- [IngestaoDados](src/AgroSolutions.IngestaoDados/README.md)
- [ProcessamentoDados](src/AgroSolutions.ProcessamentoDados/README.md)
- [Análise](src/AgroSolutions.Analise/README.md)
- [Notificações](src/AgroSolutions.Notificacoes/README.md)

## 📄 Licença

Este projeto está sob a licença [MIT](LICENSE).

---

**AgroSolutions** - Transformando o agronegócio através da tecnologia 🌾🚜💻
