# 📧 AgroSolutions.Notificações

## 🎯 Visão Geral

O microserviço de **Notificações** é responsável por enviar alertas aos produtores rurais através de múltiplos canais de comunicação (email, SMS, push notifications).

## 📋 Responsabilidades

- **Envio de Notificações**: Email, SMS (futuro), Push (futuro)
- **Gestão de Preferências**: Canais e horários preferidos
- **Processamento em Fila**: Background service para envio assíncrono
- **Retry Logic**: Tentativas automáticas em falhas
- **Rastreabilidade**: Histórico completo de notificações
- **Sincronização de Dados**: Cache de propriedades e alertas

## 💻 Tecnologias

- **.NET 10** / **C# 14** / **EF Core 10**
- **RabbitMQ** - Consumer de alertas
- **SMTP** - Envio de emails
- **Background Service** - Processamento assíncrono
- **SQL Server** - Banco de dados padrão (alternativa: PostgreSQL)

## 🏗️ Estrutura

```
Notificacoes/
├── API/Controllers/
│   └── NotificacoesController.cs         # Consulta de notificações
├── Application/
│   ├── Events/
│   │   ├── AlertaGeradoEvent.cs          # Evento consumido
│   │   ├── AlertaSensorEvent.cs          # Evento de alerta técnico
│   │   └── NotificacaoEnviadaEvent.cs    # Evento publicado
│   └── Services/
│       ├── EmailService.cs               # Serviço de email
│       ├── NotificacaoService.cs         # Serviço principal
│       └── ProcessadorNotificacoesService.cs  # Processador de fila
├── Domain/
│   ├── Entities/
│   │   ├── Notificacao.cs                # Entidade de notificação
│   │   └── PropriedadeInfo.cs            # Cache de propriedade
│   └── Enums/
│       └── Enums.cs                      # TipoNotificacao, CanalNotificacao, StatusNotificacao
└── Infrastructure/
    ├── Services/
    │   ├── AlertaSensorConsumerService.cs          # Consumer alertas técnicos
    │   ├── RabbitMQNotificacoesConsumerService.cs  # Consumer alertas análise
    │   ├── PropriedadeSyncConsumerService.cs       # Sync propriedades
    │   └── ProcessadorNotificacoesBackgroundService.cs  # Background worker
    └── Metrics/
        └── NotificacoesMetrics.cs        # Métricas Prometheus
```

## 📊 Modelo de Dados

### Notificacao
```csharp
public class Notificacao
{
    public Guid Id { get; set; }
    public Guid AlertaId { get; set; }
    public Guid TalhaoId { get; set; }
    public Guid DestinatarioId { get; set; }
    public string EmailDestinatario { get; set; }
    public string NomeDestinatario { get; set; }
    
    public TipoNotificacao Tipo { get; set; }              // Email, SMS, Push, InApp
    public StatusNotificacao Status { get; set; }          // Pendente, Enviada, Falha, Reenviando
    public PrioridadeNotificacao Prioridade { get; set; }  // Baixa, Normal, Alta, Urgente
    
    public string Assunto { get; set; }
    public string Mensagem { get; set; }                   // Corpo da mensagem
    
    public DateTime DataCriacao { get; set; }
    public DateTime? DataEnvio { get; set; }
    
    public int TentativasEnvio { get; set; }
    public string? MensagemErro { get; set; }
    public string? DadosAdicionais { get; set; }           // JSON com dados extras
}
```

**Nota**: O campo `Tipo` representa o canal de notificação (Email, SMS, Push). Não existe enum separado `CanalNotificacao`.
```

### PropriedadeInfo
```csharp
public class PropriedadeInfo : BaseEntity
{
    public string Nome { get; set; }
    public Guid ProprietarioId { get; set; }
    public string EmailProprietario { get; set; }
    public string NomeProprietario { get; set; }
}
```

## 🎨 Design Patterns

### Arquiteturais
- **Event-Driven Architecture**: Consumo assíncrono de alertas
- **Queue-Based Load Leveling**: Processamento em fila
- **Retry Pattern**: Tentativas automáticas com backoff exponencial

### Estruturais
- **Repository Pattern**: Abstração de persistência

### Comportamentais
- **Observer Pattern**: Consumo de eventos

## 📨 Eventos

### Consumido: AlertaGeradoEvent
```csharp
// Recebe de: Análise
// Ação: Cria e envia notificação ao produtor
```

### Consumido: AlertaSensorEvent
```csharp
// Recebe de: IngestaoDados
// Ação: Notifica problemas técnicos de sensores
```

### Consumido: PropriedadeCriadaEvent
```csharp
// Recebe de: Propriedades
// Ação: Sincroniza cache de propriedades
```

### Publicado: NotificacaoEnviadaEvent
```csharp
public record NotificacaoEnviadaEvent(
    Guid NotificacaoId,
    Guid DestinatarioId,
    TipoNotificacao Tipo,
    CanalNotificacao Canal,
    StatusNotificacao Status,
    DateTime DataEnvio,
    bool Sucesso
) : IntegrationEvent;
```

**Exchange**: `agrosolutions.notificacoes`  
**Routing Key**: `notificacao.enviada`  
**Consumidores**: (Futuro) Analytics, Dashboard

## 🎯 Fluxo de Processamento

```
1. AlertaGeradoEvent recebido do RabbitMQ
2. Cria Notificacao (Status: Pendente)
3. Adiciona à fila de processamento
4. Background Service processa fila
5. Tenta enviar email via SMTP
   ✅ Sucesso → Status: Enviada, publica NotificacaoEnviadaEvent
   ❌ Falha → Incrementa tentativas, agenda retry (backoff exponencial)
6. Após 3 tentativas falhadas → Status: Falhou
```

## 📧 Envio de Email

### EmailService

O serviço de email utiliza SMTP para envio de notificações:

```csharp
public interface IEmailService
{
    Task<bool> EnviarEmailAsync(string destinatario, string assunto, string mensagem);
}
```

**Características:**
- ✅ **SMTP**: Suporte a Gmail, Outlook, SendGrid
- ✅ **Texto Simples**: Mensagens em texto puro
- ⚠️ **HTML**: Não implementado no momento
- ✅ **Configurável**: Host, porta, credenciais via appsettings.json

## 🔁 Retry Logic

### Estratégia de Retry Simples

```csharp
public class ProcessadorNotificacoesService
{
    public async Task ProcessarNotificacoesPendentesAsync()
    {
        var pendentes = await _repository.ObterPendentesAsync();

        foreach (var notificacao in pendentes)
        {
            try
            {
                bool enviada = false;
                string? mensagemErro = null;

                if (notificacao.Tipo == TipoNotificacao.Email)
                {
                    enviada = await _emailService.EnviarEmailAsync(
                        notificacao.EmailDestinatario,
                        notificacao.Assunto,
                        notificacao.Mensagem
                    );

                    if (!enviada)
                        mensagemErro = "Falha no envio de email";
                }

                // Marca como enviada e publica evento
                await _notificacaoService.MarcarComoEnviadaAsync(notificacao.Id, enviada, mensagemErro);

                if (!enviada)
                {
                    notificacao.TentativasEnvio++;
                    notificacao.Status = notificacao.TentativasEnvio >= 3 
                        ? StatusNotificacao.Falha 
                        : StatusNotificacao.Reenviando;
                    await _repository.AtualizarAsync(notificacao);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar notificação {NotificacaoId}", notificacao.Id);
                await _notificacaoService.MarcarComoEnviadaAsync(notificacao.Id, false, ex.Message);
                
                notificacao.TentativasEnvio++;
                notificacao.Status = StatusNotificacao.Falha;
                notificacao.MensagemErro = ex.Message;
                await _repository.AtualizarAsync(notificacao);
            }
        }
    }
}
```

**Características:**
- ✅ **Retry Simples**: Até 3 tentativas
- ✅ **Status Reenviando**: Indica que tentará novamente
- ✅ **Após 3 Tentativas**: Status muda para Falha
- ✅ **Publicação de Evento**: NotificacaoEnviadaEvent
- ⚠️ **Sem Backoff Exponencial**: Implementação simplificada

## 🌐 Endpoints

### GET /api/notificacoes
Lista todas as notificações (requer role Admin ou Técnico).

### GET /api/notificacoes/{id}
Obtém notificação por ID.

### GET /api/notificacoes/destinatario/{destinatarioId}
Obtém notificações de um destinatário específico.

### GET /api/notificacoes/estatisticas
Obtém estatísticas de notificações (requer role Admin).

### POST /api/notificacoes
Cria notificação manual (requer role Admin ou Técnico).

**Response:**
```json
{
  "totalNotificacoes": 500,
  "notificacoesEnviadas": 475,
  "notificacoesFalhadas": 25,
  "taxaSucesso": 95.0,
  "notificacoesPorCanal": {
    "Email": 500,
    "SMS": 0,
    "Push": 0
  },
  "tempoMedioEnvio": "00:00:02.5"
}
```

## 🎯 Métricas

```csharp
public static class NotificacoesMetrics
{
    public static readonly Counter NotificacoesCriadas = 
        Metrics.CreateCounter(
            "agrosolutions_notificacoes_criadas_total",
            "Total de notificações criadas",
            new CounterConfiguration { LabelNames = new[] { "tipo", "prioridade" } });

    public static readonly Counter NotificacoesEnviadas = 
        Metrics.CreateCounter(
            "agrosolutions_notificacoes_enviadas_total",
            "Total de notificações enviadas com sucesso",
            new CounterConfiguration { LabelNames = new[] { "tipo", "prioridade" } });

    public static readonly Counter NotificacoesFalhadas = 
        Metrics.CreateCounter(
            "agrosolutions_notificacoes_falhadas_total",
            "Total de notificações que falharam",
            new CounterConfiguration { LabelNames = new[] { "tipo", "motivo" } });

    public static readonly Counter EmailsEnviados = 
        Metrics.CreateCounter(
            "agrosolutions_notificacoes_emails_enviados_total",
            "Total de e-mails enviados com sucesso");

    public static readonly Counter EmailsFalhados = 
        Metrics.CreateCounter(
            "agrosolutions_notificacoes_emails_falhados_total",
            "Total de e-mails que falharam",
            new CounterConfiguration { LabelNames = new[] { "motivo" } });

    public static readonly Gauge NotificacoesPendentes = 
        Metrics.CreateGauge(
            "agrosolutions_notificacoes_pendentes",
            "Número de notificações pendentes de envio",
            new GaugeConfiguration { LabelNames = new[] { "prioridade" } });

    public static readonly Histogram TempoEnvioEmail = 
        Metrics.CreateHistogram(
            "agrosolutions_notificacoes_envio_email_duracao_segundos",
            "Tempo de envio de um e-mail",
            new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.1, 2, 10) });

    public static readonly Histogram TempoProcessamentoNotificacao = 
        Metrics.CreateHistogram(
            "agrosolutions_notificacoes_processamento_duracao_segundos",
            "Tempo de processamento de uma notificação",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
                LabelNames = new[] { "tipo" }
            });
}
```

## 🌟 Pontos Fortes

### 1. Processamento Assíncrono
- **Background Service**: Não bloqueia API
- **Fila Persistida**: Garantia de entrega
- **Retry Automático**: Resiliência em falhas

### 2. Multi-Canal (Preparado)
- **Email**: Implementado (SMTP)
- **SMS**: Estrutura pronta (Twilio, AWS SNS)
- **Push Notifications**: Estrutura pronta (Firebase, OneSignal)

### 3. Configuração Flexível
- **SMTP Configurável**: Suporte a Gmail, Outlook, SendGrid
- **Priorização**: Baixa, Normal, Alta, Urgente
- **Múltiplos Status**: Pendente, Enviada, Falha, Reenviando
- **Dados Adicionais**: JSON flexível para metadados

### 4. Rastreabilidade Completa
- **Histórico**: Todas notificações registradas
- **Status**: Pendente, Enviada, Falha, Reenviando
- **Tentativas**: Número de tentativas e mensagens de erro
- **Métricas**: Estatísticas de envio e taxa de sucesso

## 🚀 Como Executar

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=agrosolutions_notificacoes;Username=postgres;Password=postgres"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "seu-email@gmail.com",
    "Password": "sua-senha-app",
    "FromEmail": "noreply@agrosolutions.com",
    "FromName": "AgroSolutions - Alertas"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "ExchangeName": "agrosolutions.notificacoes"
  },
  "Processamento": {
    "IntervaloProcessamentoSegundos": 30,
    "TamanhoLoteProcessamento": 50,
    "MaxTentativasEnvio": 3
  }
}
```

```bash
dotnet ef database update
dotnet run
```

**Documentação da API**: https://localhost:5007/scalar/v1

**Nota**: Acesso direto à raiz (https://localhost:5007/) redireciona automaticamente para o Scalar.

### Configurar Gmail para Envio

1. Habilitar autenticação de 2 fatores
2. Gerar senha de aplicativo
3. Usar senha de aplicativo no appsettings.json

---

**Notificações** - Mantendo produtores sempre informados 📧🔔
