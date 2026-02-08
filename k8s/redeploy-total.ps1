# ========================================
# REDEPLOY TOTAL - AGROSOLUTIONS
# ========================================
# Script para desenvolvimento: limpa tudo e sobe ambiente novo
# ========================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  REDEPLOY TOTAL - AMBIENTE LIMPO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# ========================================
# PASSO 1: LIMPAR RECURSOS E MÉTRICAS
# ========================================

Write-Host "[1/6] Limpando recursos existentes..." -ForegroundColor Yellow

# Deletar namespace inteiro (mais rápido e limpo)
try {
    $namespace = kubectl get namespace agrosolutions 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Deletando namespace agrosolutions..." -ForegroundColor Cyan
        kubectl delete namespace agrosolutions --timeout=60s 2>&1 | Out-Null
        
        # Aguardar namespace ser deletado
        $maxWait = 30
        $waited = 0
        while ($waited -lt $maxWait) {
            $ns = kubectl get namespace agrosolutions 2>&1
            if ($LASTEXITCODE -ne 0) {
                break
            }
            Start-Sleep -Seconds 2
            $waited += 2
            Write-Host "  Aguardando namespace ser deletado... ($waited/$maxWait s)" -ForegroundColor Gray
        }
        
        Write-Host "  ? Namespace deletado (recursos e métricas limpos)" -ForegroundColor Green
    } else {
        Write-Host "  ? Namespace não existe (normal na primeira execução)" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ? Erro ao deletar namespace: $_" -ForegroundColor Yellow
    Write-Host "  ? Continuando mesmo assim..." -ForegroundColor Gray
}
Write-Host ""

# ========================================
# PASSO 2: CONFIGURAR DOCKER PARA MINIKUBE
# ========================================

Write-Host "[2/6] Configurando Docker para Minikube..." -ForegroundColor Yellow
& minikube docker-env | Invoke-Expression
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ? Erro ao configurar Docker" -ForegroundColor Red
    Write-Host "  ? Certifique-se que Minikube está rodando: minikube start" -ForegroundColor Yellow
    exit 1
}
Write-Host "  ? Docker configurado" -ForegroundColor Green
Write-Host ""

# ========================================
# PASSO 3: REMOVER IMAGENS ANTIGAS
# ========================================

Write-Host "[3/6] Removendo imagens antigas..." -ForegroundColor Yellow
$images = docker images -q agrosolutions-* 2>&1
if ($images) {
    docker rmi $(docker images -q agrosolutions-*) -f 2>&1 | Out-Null
    Write-Host "  ? Imagens antigas removidas" -ForegroundColor Green
} else {
    Write-Host "  ? Nenhuma imagem antiga encontrada" -ForegroundColor Gray
}
Write-Host ""

# ========================================
# PASSO 4: REBUILD TODAS AS IMAGENS
# ========================================

Write-Host "[4/6] Buildando todas as imagens..." -ForegroundColor Yellow
Write-Host "  (isso pode levar 10-15 minutos)" -ForegroundColor Gray
Write-Host ""

$startTime = Get-Date

# Lista de microserviços
$services = @(
    @{Name="identidade"; Path="src/AgroSolutions.Identidade"},
    @{Name="propriedades"; Path="src/AgroSolutions.Propriedades"},
    @{Name="ingestaodados"; Path="src/AgroSolutions.IngestaoDados"},
    @{Name="processamentodados"; Path="src/AgroSolutions.ProcessamentoDados"},
    @{Name="analise"; Path="src/AgroSolutions.Analise"},
    @{Name="notificacoes"; Path="src/AgroSolutions.Notificacoes"},
    @{Name="sensores"; Path="src/AgroSolutions.Sensores"}
)

$currentLocation = Get-Location
Set-Location ..

foreach ($svc in $services) {
    Write-Host "  Building $($svc.Name)..." -ForegroundColor Cyan
    
    # Executar docker build (stderr é normal no BuildKit)
    docker build -t "agrosolutions-$($svc.Name):latest" -f "$($svc.Path)/Dockerfile" .
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ? Erro ao buildar $($svc.Name)" -ForegroundColor Red
        Set-Location $currentLocation
        exit 1
    }
    Write-Host "  ? $($svc.Name) buildado com sucesso" -ForegroundColor Green
    Write-Host ""
}

Set-Location $currentLocation

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalMinutes
Write-Host ""
Write-Host "  ? Todas as imagens buildadas em $([math]::Round($duration, 1)) minutos" -ForegroundColor Green
Write-Host ""

# ========================================
# PASSO 5: GERAR E APLICAR CONFIGMAP DOS DASHBOARDS
# ========================================

Write-Host "[5/6] Gerando ConfigMap dos dashboards do Grafana..." -ForegroundColor Yellow

$currentLocation = Get-Location
$configMapCreated = $false

try {
    # Verificar se script existe
    if (Test-Path "monitoring/grafana/generate-dashboards-configmap.ps1") {
        Set-Location "monitoring/grafana"
        
        Write-Host "  Executando generate-dashboards-configmap.ps1..." -ForegroundColor Cyan
        & .\generate-dashboards-configmap.ps1
        
        if (Test-Path "configmap-dashboards.yaml") {
            Write-Host "  ? ConfigMap dos dashboards gerado" -ForegroundColor Green
            $configMapCreated = $true
        } else {
            Write-Host "  ? ConfigMap não foi gerado, criando vazio..." -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ? Script de geração não encontrado, criando ConfigMap vazio..." -ForegroundColor Yellow
    }
    
    # Se não foi criado, criar ConfigMap vazio
    if (-not $configMapCreated) {
        Set-Location "monitoring/grafana"
        @"
apiVersion: v1
kind: ConfigMap
metadata:
  name: grafana-dashboards
  namespace: agrosolutions
data:
  dummy.json: |
    {}
"@ | Out-File -FilePath "configmap-dashboards.yaml" -Encoding UTF8
        Write-Host "  ? ConfigMap vazio criado" -ForegroundColor Green
    }
    
} catch {
    Write-Host "  ? Erro ao gerar ConfigMap: $_" -ForegroundColor Yellow
    Write-Host "  ? Criando ConfigMap vazio..." -ForegroundColor Gray
    Set-Location "monitoring/grafana"
    @"
apiVersion: v1
kind: ConfigMap
metadata:
  name: grafana-dashboards
  namespace: agrosolutions
data:
  dummy.json: |
    {}
"@ | Out-File -FilePath "configmap-dashboards.yaml" -Encoding UTF8
    Write-Host "  ? ConfigMap vazio criado" -ForegroundColor Green
} finally {
    Set-Location $currentLocation
}

# IMPORTANTE: Aplicar o ConfigMap ANTES do deploy
Write-Host "  Aplicando ConfigMap no cluster..." -ForegroundColor Cyan
try {
    kubectl apply -f monitoring/grafana/configmap-dashboards.yaml
    Write-Host "  ? ConfigMap aplicado no cluster" -ForegroundColor Green
} catch {
    Write-Host "  ? Erro ao aplicar ConfigMap, será aplicado durante o deploy" -ForegroundColor Yellow
}

Write-Host ""

# ========================================
# PASSO 6: DEPLOY COMPLETO
# ========================================

Write-Host "[6/6] Fazendo deploy completo..." -ForegroundColor Yellow
Write-Host "  (isso pode levar 2-3 minutos)" -ForegroundColor Gray
Write-Host ""

& .\deploy-all.ps1

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "  ? Erro no deploy" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ========================================
# RESUMO FINAL
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ? REDEPLOY COMPLETO FINALIZADO!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "?? Status do Cluster:" -ForegroundColor Yellow
kubectl get pods -n agrosolutions
Write-Host ""

Write-Host "?? Próximos Passos:" -ForegroundColor Yellow
Write-Host "  1. Iniciar port-forwards:" -ForegroundColor Cyan
Write-Host "     .\start-port-forwards.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Acessar aplicações:" -ForegroundColor Cyan
Write-Host "     Grafana:    http://localhost:3000" -ForegroundColor Gray
Write-Host "     Prometheus: http://localhost:9090" -ForegroundColor Gray
Write-Host "     APIs:       http://localhost:500X/scalar/v1" -ForegroundColor Gray
Write-Host ""

Write-Host "? Ambiente pronto para testes!" -ForegroundColor Green
Write-Host ""
