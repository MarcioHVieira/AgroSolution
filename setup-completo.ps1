# ========================================
# SETUP COMPLETO - AGROSOLUTIONS KUBERNETES
# ========================================
# Este script executa TODOS os passos necessários em sequência
# Use apenas para setup inicial ou reset completo
# ========================================

param(
    [switch]$SkipBuild,
    [switch]$SkipMinikubeStart,
    [switch]$ForceRecreate,
    [switch]$Help
)

if ($Help) {
    Write-Host @"
========================================
SETUP COMPLETO - AGROSOLUTIONS K8S
========================================

USO:
  .\setup-completo.ps1                     # Setup completo (recomendado)
  .\setup-completo.ps1 -SkipBuild          # Pula build das imagens
  .\setup-completo.ps1 -SkipMinikubeStart  # Assume que Minikube já está rodando
  .\setup-completo.ps1 -ForceRecreate      # Força recriação da estrutura K8s

DESCRIÇÃO:
  Este script executa automaticamente:
  1. Criação/verificação da estrutura K8s
  2. Verificação de secrets
  3. Geração do ConfigMap de dashboards
  4. Inicialização do Minikube (se necessário)
  5. Configuração do Docker
  6. Build das imagens Docker (se não pulado)
  7. Deploy completo
  8. Inicialização dos port-forwards

IMPORTANTE:
  - O script FAZ BACKUP automático do secrets.yaml antes de qualquer operação
  - Use -ForceRecreate apenas se quiser recriar estrutura do zero

PRÉ-REQUISITOS:
  - Docker Desktop instalado e rodando
  - Minikube instalado
  - kubectl instalado
  - Arquivo k8s/base/secrets.yaml configurado (ou será solicitado)

========================================
"@ -ForegroundColor Cyan
    exit 0
}

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AGROSOLUTIONS - SETUP COMPLETO K8S" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ========================================
# VALIDAÇÕES INICIAIS
# ========================================

Write-Host "[VALIDAÇÃO] Verificando pré-requisitos..." -ForegroundColor Yellow

# Verificar Docker
try {
    $null = docker --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? Docker encontrado" -ForegroundColor Green
    } else {
        throw "Docker não encontrado"
    }
} catch {
    Write-Host "  ? Docker não está instalado ou não está no PATH" -ForegroundColor Red
    exit 1
}

# Verificar Minikube
try {
    $null = minikube version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? Minikube encontrado" -ForegroundColor Green
    } else {
        throw "Minikube não encontrado"
    }
} catch {
    Write-Host "  ? Minikube não está instalado ou não está no PATH" -ForegroundColor Red
    exit 1
}

# Verificar kubectl
try {
    $null = kubectl version --client 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? kubectl encontrado" -ForegroundColor Green
    } else {
        throw "kubectl não encontrado"
    }
} catch {
    Write-Host "  ? kubectl não está instalado ou não está no PATH" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ========================================
# PASSO 1: CRIAR ESTRUTURA K8S
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PASSO 1/8: Verificando/Criando estrutura K8s" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# IMPORTANTE: Fazer backup do secrets.yaml se existir
$secretsBackup = $null
$dashboardsBackup = $null

if (Test-Path "k8s/base/secrets.yaml") {
    Write-Host "?? Fazendo backup do secrets.yaml..." -ForegroundColor Yellow
    $secretsBackup = Get-Content "k8s/base/secrets.yaml" -Raw
    Write-Host "   ? Backup do secrets.yaml criado" -ForegroundColor Green
}

if (Test-Path "k8s/monitoring/grafana/configmap-dashboards.yaml") {
    Write-Host "?? Fazendo backup do configmap-dashboards.yaml..." -ForegroundColor Yellow
    $dashboardsBackup = Get-Content "k8s/monitoring/grafana/configmap-dashboards.yaml" -Raw
    Write-Host "   ? Backup do configmap-dashboards.yaml criado" -ForegroundColor Green
}

# Verificar se precisa criar estrutura
$needsSetup = $false
if (-not (Test-Path "k8s")) {
    Write-Host "Pasta k8s/ não existe, criando estrutura..." -ForegroundColor Yellow
    $needsSetup = $true
} elseif (-not (Test-Path "k8s/base/namespace.yaml")) {
    Write-Host "Estrutura k8s/ incompleta, recriando..." -ForegroundColor Yellow
    $needsSetup = $true
} elseif ($ForceRecreate) {
    Write-Host "Opção -ForceRecreate especificada, recriando estrutura..." -ForegroundColor Yellow
    $needsSetup = $true
} else {
    Write-Host "? Estrutura k8s/ já existe e está completa" -ForegroundColor Green
}

if ($needsSetup -and (Test-Path "setup-k8s-structure.ps1")) {
    Write-Host "Executando setup-k8s-structure.ps1..." -ForegroundColor Yellow
    & .\setup-k8s-structure.ps1 -SkipConfirmation
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Erro ao criar estrutura K8s" -ForegroundColor Red
        exit 1
    }
    Write-Host "? Estrutura K8s criada com sucesso" -ForegroundColor Green
}

# Restaurar arquivos do backup
if ($null -ne $secretsBackup) {
    Write-Host "?? Restaurando secrets.yaml do backup..." -ForegroundColor Yellow
    $secretsBackup | Out-File -FilePath "k8s/base/secrets.yaml" -Encoding UTF8 -NoNewline
    Write-Host "   ? secrets.yaml restaurado!" -ForegroundColor Green
}

if ($null -ne $dashboardsBackup) {
    Write-Host "?? Restaurando configmap-dashboards.yaml do backup..." -ForegroundColor Yellow
    $dashboardsBackup | Out-File -FilePath "k8s/monitoring/grafana/configmap-dashboards.yaml" -Encoding UTF8 -NoNewline
    Write-Host "   ? configmap-dashboards.yaml restaurado!" -ForegroundColor Green
}

Write-Host ""

# ========================================
# PASSO 2: VERIFICAR SECRETS
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PASSO 2/8: Verificando secrets" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path "k8s/base/secrets.yaml")) {
    Write-Host "? Arquivo k8s/base/secrets.yaml não encontrado!" -ForegroundColor Red
    Write-Host ""
    Write-Host "AÇÃO NECESSÁRIA:" -ForegroundColor Yellow
    Write-Host "1. Copie o template:" -ForegroundColor Yellow
    Write-Host "   cp k8s/base/secrets.yaml.template k8s/base/secrets.yaml" -ForegroundColor Gray
    Write-Host "2. Edite k8s/base/secrets.yaml com seus valores em Base64" -ForegroundColor Yellow
    Write-Host "3. Execute este script novamente" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "? Arquivo secrets.yaml encontrado" -ForegroundColor Green
Write-Host ""

# ========================================
# PASSO 3: GERAR CONFIGMAP DOS DASHBOARDS
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PASSO 3/8: Gerando ConfigMap dos dashboards" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (Test-Path "k8s/monitoring/grafana/generate-dashboards-configmap.ps1") {
    $currentLocation = Get-Location
    try {
        Set-Location "k8s/monitoring/grafana"
        Write-Host "Executando generate-dashboards-configmap.ps1..." -ForegroundColor Yellow
        & .\generate-dashboards-configmap.ps1
        if ($LASTEXITCODE -ne 0) {
            throw "Erro ao gerar ConfigMap"
        }
        
        if (Test-Path "configmap-dashboards.yaml") {
            Write-Host "? ConfigMap dos dashboards gerado com sucesso" -ForegroundColor Green
        } else {
            throw "ConfigMap não foi gerado"
        }
    } catch {
        Write-Host "? Erro ao gerar ConfigMap dos dashboards: $_" -ForegroundColor Red
        Write-Host "? Continuando mesmo assim..." -ForegroundColor Yellow
    } finally {
        Set-Location $currentLocation
    }
} else {
    Write-Host "? Script de geração de dashboards não encontrado" -ForegroundColor Yellow
}

Write-Host ""

# ========================================
# PASSO 4: INICIAR MINIKUBE
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PASSO 4/8: Inicializando Minikube" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not $SkipMinikubeStart) {
    # Verificar se Minikube já está rodando
    try {
        $minikubeStatus = minikube status 2>&1 | Out-String
        if ($minikubeStatus -match "Running" -or $minikubeStatus -match "host: Running") {
            Write-Host "? Minikube já está rodando" -ForegroundColor Green
        } else {
            throw "Minikube não está rodando ou não existe"
        }
    } catch {
        Write-Host "Minikube não está rodando ou não existe, iniciando..." -ForegroundColor Yellow
        Write-Host "(isso pode levar alguns minutos...)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "?? Dica: Se falhar, execute manualmente:" -ForegroundColor Gray
        Write-Host "   minikube delete --all --purge" -ForegroundColor Gray
        Write-Host "   minikube start --driver=docker --cpus=4 --memory=8192" -ForegroundColor Gray
        Write-Host ""
        
        # Tentar iniciar Minikube com configurações otimizadas
        Write-Host "Tentando iniciar Minikube com configurações otimizadas..." -ForegroundColor Yellow
        minikube start --driver=docker --cpus=4 --memory=8192 --kubernetes-version=v1.31.0 --force-systemd=false --wait=apiserver --wait-timeout=10m
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host ""
            Write-Host "? Erro ao iniciar Minikube" -ForegroundColor Red
            Write-Host ""
            Write-Host "?? SOLUÇÃO:" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "1. Delete o Minikube completamente:" -ForegroundColor Cyan
            Write-Host "   minikube delete --all --purge" -ForegroundColor Gray
            Write-Host ""
            Write-Host "2. Recrie manualmente:" -ForegroundColor Cyan
            Write-Host "   minikube start --driver=docker --cpus=4 --memory=8192 --kubernetes-version=v1.31.0" -ForegroundColor Gray
            Write-Host ""
            Write-Host "3. Verifique se funcionou:" -ForegroundColor Cyan
            Write-Host "   kubectl get nodes" -ForegroundColor Gray
            Write-Host ""
            Write-Host "4. Execute o script novamente:" -ForegroundColor Cyan
            Write-Host "   .\setup-completo.ps1 -SkipMinikubeStart" -ForegroundColor Gray
            Write-Host ""
            Write-Host "?? Mais detalhes em: SOLUCAO-MINIKUBE-API-SERVER.md" -ForegroundColor Gray
            Write-Host ""
            exit 1
        }
        
        # Verificar se cluster está funcionando
        Write-Host ""
        Write-Host "Verificando se cluster está acessível..." -ForegroundColor Yellow
        $maxAttempts = 6
        $attempt = 0
        $clusterReady = $false
        
        while ($attempt -lt $maxAttempts -and -not $clusterReady) {
            try {
                $null = kubectl get nodes 2>&1
                if ($LASTEXITCODE -eq 0) {
                    $clusterReady = $true
                    Write-Host "? Cluster está acessível!" -ForegroundColor Green
                } else {
                    throw "Cluster não respondeu"
                }
            } catch {
                $attempt++
                Write-Host "  Tentativa $attempt/$maxAttempts - Aguardando cluster..." -ForegroundColor Gray
                Start-Sleep -Seconds 10
            }
        }
        
        if (-not $clusterReady) {
            Write-Host "? Cluster não ficou pronto" -ForegroundColor Red
            Write-Host "Execute 'minikube logs' para ver detalhes" -ForegroundColor Yellow
            exit 1
        }
        
        Write-Host "? Minikube iniciado com sucesso" -ForegroundColor Green
    }
} else {
    Write-Host "? Pulando inicialização do Minikube (--SkipMinikubeStart)" -ForegroundColor Yellow
}

Write-Host ""

# ========================================
# PASSO 5: CONFIGURAR DOCKER
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PASSO 5/8: Configurando Docker para Minikube" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Configurando Docker para usar registry do Minikube..." -ForegroundColor Yellow
& minikube docker-env | Invoke-Expression
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Erro ao configurar Docker" -ForegroundColor Red
    exit 1
}
Write-Host "? Docker configurado" -ForegroundColor Green
Write-Host ""

# ========================================
# PASSO 6: BUILD DAS IMAGENS
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PASSO 6/8: Build das imagens Docker" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not $SkipBuild) {
    if (Test-Path "k8s/build-images.ps1") {
        $currentLocation = Get-Location
        try {
            Set-Location "k8s"
            Write-Host "Executando build-images.ps1 (isso pode levar 10-15 minutos)..." -ForegroundColor Yellow
            & .\build-images.ps1
            if ($LASTEXITCODE -ne 0) {
                throw "Erro no build das imagens"
            }
            Write-Host "? Build das imagens concluído" -ForegroundColor Green
        } catch {
            Write-Host "? Erro no build das imagens: $_" -ForegroundColor Red
            exit 1
        } finally {
            Set-Location $currentLocation
        }
    } else {
        Write-Host "? Arquivo k8s/build-images.ps1 não encontrado" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "? Pulando build das imagens (--SkipBuild)" -ForegroundColor Yellow
    Write-Host "  Certifique-se de que as imagens já foram buildadas!" -ForegroundColor Yellow
}

Write-Host ""

# ========================================
# PASSO 7: DEPLOY COMPLETO
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PASSO 7/8: Deploy no Kubernetes" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se deploy-all.ps1 existe (pode estar na raiz ou dentro de k8s/)
$deployScript = $null
if (Test-Path "k8s/deploy-all.ps1") {
    $deployScript = "k8s/deploy-all.ps1"
} elseif (Test-Path "deploy-all.ps1") {
    # Copiar para dentro de k8s/ se estiver na raiz
    Write-Host "Copiando deploy-all.ps1 para k8s/..." -ForegroundColor Yellow
    Copy-Item "deploy-all.ps1" "k8s/" -Force
    $deployScript = "k8s/deploy-all.ps1"
}

if ($null -eq $deployScript) {
    Write-Host "? Arquivo deploy-all.ps1 não encontrado" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Solução:" -ForegroundColor Yellow
    Write-Host "   Execute o deploy manualmente:" -ForegroundColor Gray
    Write-Host "   cd k8s" -ForegroundColor Gray
    Write-Host "   kubectl apply -f base/" -ForegroundColor Gray
    Write-Host "   kubectl apply -f microservices/" -ForegroundColor Gray
    Write-Host "   kubectl apply -f monitoring/" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

$currentLocation = Get-Location
try {
    Set-Location "k8s"
    Write-Host "Executando deploy-all.ps1 (isso pode levar 5-10 minutos)..." -ForegroundColor Yellow
    & .\deploy-all.ps1
    if ($LASTEXITCODE -ne 0) {
        throw "Erro no deploy"
    }
    Write-Host "? Deploy concluído com sucesso" -ForegroundColor Green
} catch {
    Write-Host "? Erro no deploy: $_" -ForegroundColor Red
    exit 1
} finally {
    Set-Location $currentLocation
}

Write-Host ""

# ========================================
# PASSO 8: PORT-FORWARDS
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PASSO 8/8: Iniciando port-forwards" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Você deseja iniciar os port-forwards agora? (S/N)" -ForegroundColor Yellow
$response = Read-Host

if ($response -eq "S" -or $response -eq "s") {
    # Verificar se start-port-forwards.ps1 existe
    $portForwardScript = $null
    if (Test-Path "k8s/start-port-forwards.ps1") {
        $portForwardScript = "k8s/start-port-forwards.ps1"
    } elseif (Test-Path "start-port-forwards.ps1") {
        Write-Host "Copiando start-port-forwards.ps1 para k8s/..." -ForegroundColor Yellow
        Copy-Item "start-port-forwards.ps1" "k8s/" -Force
        $portForwardScript = "k8s/start-port-forwards.ps1"
    }
    
    if ($null -ne $portForwardScript) {
        $currentLocation = Get-Location
        try {
            Set-Location "k8s"
            Write-Host "Iniciando port-forwards..." -ForegroundColor Yellow
            & .\start-port-forwards.ps1
        } finally {
            Set-Location $currentLocation
        }
    } else {
        Write-Host "? Arquivo start-port-forwards.ps1 não encontrado" -ForegroundColor Yellow
        Write-Host "Execute manualmente: cd k8s; .\start-port-forwards.ps1" -ForegroundColor Gray
    }
} else {
    Write-Host "? Port-forwards não iniciados" -ForegroundColor Yellow
    Write-Host "Execute manualmente quando necessário:" -ForegroundColor Gray
    Write-Host "  cd k8s" -ForegroundColor Gray
    Write-Host "  .\start-port-forwards.ps1" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# RESUMO FINAL
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? SETUP COMPLETO FINALIZADO!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "?? Status do Cluster:" -ForegroundColor Yellow
kubectl get pods -n agrosolutions
Write-Host ""

Write-Host "?? URLs de Acesso:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Grafana:            http://localhost:3000" -ForegroundColor Cyan
Write-Host "  Prometheus:         http://localhost:9090" -ForegroundColor Cyan
Write-Host "  Identidade:         http://localhost:5001/scalar/v1" -ForegroundColor Cyan
Write-Host "  Propriedades:       http://localhost:5002/scalar/v1" -ForegroundColor Cyan
Write-Host "  IngestaoDados:      http://localhost:5003/scalar/v1" -ForegroundColor Cyan
Write-Host "  ProcessamentoDados: http://localhost:5004/scalar/v1" -ForegroundColor Cyan
Write-Host "  Analise:            http://localhost:5005/scalar/v1" -ForegroundColor Cyan
Write-Host "  Notificacoes:       http://localhost:5006/scalar/v1" -ForegroundColor Cyan
Write-Host "  Sensores:           http://localhost:5008/scalar/v1" -ForegroundColor Cyan
Write-Host ""

Write-Host "?? Credenciais Grafana:" -ForegroundColor Yellow
Write-Host "  Usuário: admin" -ForegroundColor Gray
Write-Host "  Senha: (definida no secrets.yaml - GRAFANA_ADMIN_PASSWORD)" -ForegroundColor Gray
Write-Host ""

Write-Host "?? Comandos Úteis:" -ForegroundColor Yellow
Write-Host "  Ver logs:           kubectl logs -f deployment/<nome> -n agrosolutions" -ForegroundColor Gray
Write-Host "  Ver todos os pods:  kubectl get pods -n agrosolutions" -ForegroundColor Gray
Write-Host "  Reiniciar service:  kubectl rollout restart deployment/<nome> -n agrosolutions" -ForegroundColor Gray
Write-Host "  Dashboard K8s:      minikube dashboard" -ForegroundColor Gray
Write-Host ""

Write-Host "?? Documentação:" -ForegroundColor Yellow
Write-Host "  Consulte GUIA-RAPIDO-K8S.md para referência rápida" -ForegroundColor Gray
Write-Host "  Consulte INDICE-DOCUMENTACAO.md para navegação completa" -ForegroundColor Gray
Write-Host ""

Write-Host "?? Tudo pronto! Bom desenvolvimento!" -ForegroundColor Green
Write-Host ""
