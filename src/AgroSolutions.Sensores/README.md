# 📡 AgroSolutions.Sensores

## 🎯 Visão Geral

O microserviço **Sensores** é um **simulador de cenários de alerta** projetado para gerar leituras históricas que acionam alertas específicos na plataforma AgroSolutions. Ideal para testes, desenvolvimento e demonstrações.

## 📋 Responsabilidades

- **Simulação de Cenários de Alerta**: Geração de leituras que acionam alertas específicos (seca, geada, calor excessivo, etc.)
- **Dados Históricos**: Cria leituras das últimas X horas baseadas em configurações de threshold
- **Múltiplos Níveis de Severidade**: Suporta Normal, Média, Alta, Crítica
- **Integração com IngestaoDados**: Envia leituras automaticamente para o pipeline de processamento
- **Testes de Alertas**: Facilita validação do motor de regras e sistema de notificações

## 💻 Tecnologias

- **.NET 10** / **C# 14** / **ASP.NET Core**
- **HttpClient** - Comunicação com IngestaoDados
- **API RESTful** - Endpoints sob demanda (não background service)

## 🏗️ Estrutura

```
Sensores/
├── Controllers/
│   └── SimuladorController.cs            # Endpoints de simulação
├── Services/
│   ├── IIngestaoApiClient.cs             # Interface HTTP client
│   ├── IngestaoApiClient.cs              # Cliente HTTP IngestaoDados
│   ├── ISimuladorService.cs              # Interface do simulador
│   └── SimuladorService.cs               # Implementação dos cenários
├── Models/
│   └── SimuladorModels.cs                # DTOs (SimulacaoRequestDto, ResultadoSimulacaoDto)
├── Configuration/
│   ├── SimuladorSettings.cs              # Configurações de thresholds
│   ├── ApiConfiguration.cs               # Configuração de API
│   ├── CorsConfiguration.cs              # CORS para front-end
│   └── DependencyInjectionConfiguration.cs
└── Program.cs
```

## 🎨 Design Patterns Aplicados

### Arquiteturais
- **Scenario Simulator Pattern**: Simula cenários específicos sob demanda
- **Strategy Pattern**: Cada cenário tem estratégia própria de geração

**Nota**: O simulador utiliza algoritmos diretos baseados em thresholds configuráveis, sem uso de patterns complexos de variação temporal.

## 🚨 Cenários de Alerta Suportados

### 1. Seca (Umidade do Solo Baixa)
- **Endpoint**: `POST /api/simulador/seca`
- **Sensor**: Umidade do Solo
- **Thresholds**: 
  - Normal: 30% (±5%)
  - Média: 22-30%
  - Alta: 17-22%
  - Crítica: <15%
- **Duração**: 24 horas (96 leituras a cada 15 min)

### 2. Geada (Temperatura Muito Baixa)
- **Endpoint**: `POST /api/simulador/geada`
- **Sensor**: Temperatura
- **Thresholds**:
  - Normal: 2°C (±3°C)
  - Média: 0-2°C
  - Alta: -2°C a 0°C
  - Crítica: <-2°C
- **Duração**: 6 horas (24 leituras a cada 15 min)

### 3. Calor Excessivo
- **Endpoint**: `POST /api/simulador/calor-excessivo`
- **Sensor**: Temperatura
- **Thresholds**:
  - Normal: 33-35°C
  - Média: 35-38°C
  - Alta: 38-42°C
  - Crítica: >42°C
- **Duração**: 6 horas

### 4. Excesso de Umidade
- **Endpoint**: `POST /api/simulador/excesso-umidade`
- **Sensor**: Umidade do Solo
- **Thresholds**:
  - Normal: 77-80%
  - Média: 80-85%
  - Alta: 85-90%
  - Crítica: >90%
- **Duração**: 48 horas

### 5. Risco de Praga
- **Endpoint**: `POST /api/simulador/risco-praga`
- **Sensores**: Temperatura + Umidade do Ar
- **Condições**: Temperatura 25-30°C + Umidade >70%
- **Duração**: 12 horas

## 🧮 Algoritmo de Geração

### Lógica de Geração de Leituras
```csharp
public async Task<ResultadoSimulacaoDto> SimularSecaAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true)
{
    var config = _settings.RegrasSeca;
    
    // Define valor base conforme severidade
    var umidadeBase = severidade switch {
        Severidade.Normal => config.ThresholdNormal + 5.0m,
        Severidade.Media => (config.ThresholdMedia + config.ThresholdNormal) / 2,
        Severidade.Alta => (config.ThresholdAlta + config.ThresholdMedia) / 2,
        Severidade.Critica => config.ThresholdCritica - 2.0m,
        _ => config.ThresholdNormal
    };
    
    // Gera leituras históricas
    var totalLeituras = (config.DuracaoHoras * 60) / config.IntervaloMinutos;
    var dataInicio = DateTime.UtcNow.AddHours(-config.DuracaoHoras);
    
    for (int i = 0; i < totalLeituras; i++)
    {
        // Aplica variação aleatória configurável
        var valor = AplicarVariacao(umidadeBase, _settings.Simulacao.VariacaoAleatoria);
        valor = Math.Max(0, Math.Min(100, valor)); // Limita entre 0-100%
        
        var leitura = new LeituraSimuladaDto {
            TalhaoId = talhaoId,
            TipoSensor = "UmidadeSolo",
            Valor = Math.Round(valor, 2),
            DataHora = dataInicio.AddMinutes(i * config.IntervaloMinutos)
        };
        
        if (enviarParaApi)
            await _ingestaoClient.EnviarLeituraAsync(leitura);
    }
    
    return resultado;
}

private decimal AplicarVariacao(decimal valorBase, decimal percentualVariacao)
{
    var variacao = valorBase * (percentualVariacao / 100m);
    return valorBase + ((decimal)_random.NextDouble() * variacao * 2) - variacao;
}
```

## 🌐 Endpoints

### POST /api/simulador/seca
Simula cenário de seca (umidade do solo abaixo de 30%).

**Request:**
```json
{
  "talhaoId": "guid-do-talhao",
  "severidade": "Alta",
  "enviarParaApi": true
}
```

**Response:**
```json
{
  "sucesso": true,
  "cenario": "SECA",
  "severidade": "Alta",
  "leiturasGeradas": 96,
  "valorMinimo": 14.2,
  "valorMaximo": 18.5,
  "valorMedio": 16.8,
  "dataInicio": "2024-02-03T10:30:00Z",
  "dataFim": "2024-02-04T10:30:00Z",
  "mensagem": "Simulação de SECA concluída. 96 leituras geradas com umidade média de 16.80%"
}
```

### POST /api/simulador/geada
Simula cenário de geada (temperatura abaixo de 2°C).

**Request:**
```json
{
  "talhaoId": "guid-do-talhao",
  "severidade": "Critica",
  "enviarParaApi": true
}
```

### POST /api/simulador/calor-excessivo
Simula cenário de calor excessivo (temperatura alta).

### POST /api/simulador/excesso-umidade
Simula cenário de excesso de umidade (solo encharcado).

### POST /api/simulador/risco-praga
Simula condições favoráveis ao desenvolvimento de pragas.

**Severidades Disponíveis:**
- `Normal`: Próximo ao threshold, pode ou não gerar alerta
- `Media`: Aciona alerta de severidade média
- `Alta`: Aciona alerta de severidade alta
- `Critica`: Aciona alerta de severidade crítica

## ⚙️ Configuração

### appsettings.json
```json
{
  "SimuladorSettings": {
    "IngestaoApiUrl": "https://localhost:5004",
    "Simulacao": {
      "VariacaoAleatoria": 5.0,
      "DelayEntreLeiturasMs": 100
    },
    "RegrasSeca": {
      "ThresholdNormal": 30.0,
      "ThresholdMedia": 25.0,
      "ThresholdAlta": 20.0,
      "ThresholdCritica": 15.0,
      "DuracaoHoras": 24,
      "IntervaloMinutos": 15
    },
    "RegrasGeada": {
      "ThresholdNormal": 2.0,
      "ThresholdMedia": 1.0,
      "ThresholdAlta": 0.0,
      "ThresholdCritica": -2.0,
      "DuracaoHoras": 6,
      "IntervaloMinutos": 15
    },
    "RegrasCalorExcessivo": {
      "ThresholdNormal": 35.0,
      "ThresholdMedia": 38.0,
      "ThresholdAlta": 40.0,
      "ThresholdCritica": 42.0,
      "DuracaoHoras": 6,
      "IntervaloMinutos": 15
    },
    "RegrasExcessoUmidade": {
      "ThresholdNormal": 80.0,
      "ThresholdMedia": 85.0,
      "ThresholdAlta": 90.0,
      "ThresholdCritica": 95.0,
      "DuracaoHoras": 48,
      "IntervaloMinutos": 15
    },
    "RegrasRiscoPraga": {
      "TemperaturaMinima": 25.0,
      "TemperaturaMaxima": 30.0,
      "UmidadeMinima": 70.0,
      "DuracaoHoras": 12,
      "IntervaloMinutos": 15
    }
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:4200"]
  }
}
```

## 🌟 Pontos Fortes

### 1. Testes de Alertas
- **Cenários Específicos**: Gera exatamente as condições necessárias para acionar alertas
- **Múltiplas Severidades**: Testa todos os níveis de alerta (Normal, Média, Alta, Crítica)
- **Dados Históricos**: Cria histórico completo necessário para regras de duração
- **Reproduzível**: Mesma configuração gera resultados consistentes

### 2. Flexibilidade
- **Configurável**: Thresholds, durações e intervalos via appsettings.json
- **Sob Demanda**: Não roda continuamente, apenas quando solicitado
- **Controle Fino**: Opção de enviar ou não para API (útil para testes locais)
- **Múltiplos Cenários**: Suporta 5 tipos diferentes de alertas

### 3. Desenvolvimento e Testes
- **Sem Hardware**: Não requer dispositivos físicos ou sensores reais
- **Validação de Regras**: Testa motor de regras do microserviço Análise
- **Testes de Notificações**: Valida envio de emails e notificações
- **Pipeline Completo**: Testa IngestaoDados → ProcessamentoDados → Análise → Notificações

### 4. Demonstração
- **Visual**: Mostra alertas sendo gerados em tempo real
- **Didático**: Ajuda a entender fluxo de dados e regras de negócio
- **Interativo**: Front-end pode acionar simulações sob demanda

## 🎯 Casos de Uso

### 1. Desenvolvimento
- Testar motor de regras sem dados reais
- Desenvolver front-end com alertas funcionais
- Validar pipeline completo de dados

### 2. Testes Automatizados
- Testes de integração do pipeline
- Validação de regras de alerta
- Testes de regressão

### 3. Demonstrações
- Apresentações para clientes mostrando alertas em ação
- Provas de conceito
- Treinamento de usuários

### 4. Debugging
- Reproduzir cenários específicos que geraram bugs
- Validar correções em regras de alerta
- Testar novos thresholds antes de aplicar em produção

## 🚀 Como Executar

### Standalone
```bash
dotnet run
```

**Documentação da API**: https://localhost:5003/scalar/v1

**Nota**: Acesso direto à raiz (https://localhost:5003/) redireciona automaticamente para o Scalar.

### Com Docker
```bash
docker build -t agrosolutions-sensores .
docker run -p 5003:5003 agrosolutions-sensores
```

## 📝 Exemplos de Uso

### Simular Seca Severa
```bash
curl -X POST https://localhost:5003/api/simulador/seca \
  -H "Content-Type: application/json" \
  -d '{
    "talhaoId": "seu-guid-aqui",
    "severidade": "Alta",
    "enviarParaApi": true
  }'
```

### Simular Geada Crítica (Teste de Alerta)
```bash
curl -X POST https://localhost:5003/api/simulador/geada \
  -H "Content-Type: application/json" \
  -d '{
    "talhaoId": "seu-guid-aqui",
    "severidade": "Critica",
    "enviarParaApi": true
  }'
```

### Simular Sem Enviar para API (Teste Local)
```bash
curl -X POST https://localhost:5003/api/simulador/calor-excessivo \
  -H "Content-Type: application/json" \
  -d '{
    "talhaoId": "seu-guid-aqui",
    "severidade": "Media",
    "enviarParaApi": false
  }'
```

## 🔗 Integração com IngestaoDados

O simulador envia leituras para o endpoint `/api/leituras` do microserviço IngestaoDados:

```csharp
public class IngestaoApiClient : IIngestaoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _ingestaoUrl;

    public async Task<bool> EnviarLeituraAsync(LeituraSimuladaDto leitura)
    {
        var dto = new {
            DeviceId = $"SIM-{leitura.TalhaoId}-{leitura.TipoSensor}",
            TipoSensor = leitura.TipoSensor,
            Valor = leitura.Valor,
            Unidade = ObterUnidade(leitura.TipoSensor),
            TimestampLeitura = leitura.DataHora,
            TalhaoId = leitura.TalhaoId
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_ingestaoUrl}/api/leituras",
            dto
        );
        
        return response.IsSuccessStatusCode;
    }
}
```

## 🚀 Melhorias Futuras

- **Mais Cenários**: Adicionar simulações para outros tipos de alerta (vento forte, chuva intensa, etc.)
- **Dados Mais Realistas**: Adicionar variação temporal (hora do dia afeta temperatura)
- **Múltiplos Talhões**: Simular cenários em múltiplos talhões simultaneamente
- **Configuração por API**: Endpoints para ajustar thresholds dinamicamente
- **Interface Visual**: Dashboard de controle e visualização de simulações
- **Simulação Contínua**: Opção de background service para simulação IoT realista contínua

---

**Sensores** - Simulando cenários de alerta para testes 📡🚨
