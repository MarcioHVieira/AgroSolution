# ========================================
# SCRIPT DE DEPLOY COMPLETO - AGROSOLUTIONS KUBERNETES
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AGROSOLUTIONS - DEPLOY KUBERNETES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se minikube está rodando
Write-Host "[1/9] Verificando Minikube..." -ForegroundColor Yellow
try {
    $null = kubectl get nodes 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Minikube está rodando" -ForegroundColor Green
    } else {
        Write-Host "?  Não foi possível verificar Minikube, mas continuando..." -ForegroundColor Yellow
    }
} catch {
    Write-Host "?  Não foi possível verificar Minikube, mas continuando..." -ForegroundColor Yellow
}
Write-Host ""

# 1. Criar Namespace
Write-Host "[2/9] Criando Namespace..." -ForegroundColor Yellow
kubectl apply -f base/namespace.yaml
if ($LASTEXITCODE -ne 0) { Write-Host "? Erro ao criar namespace" -ForegroundColor Red; exit 1 }
Write-Host "? Namespace criado" -ForegroundColor Green
Write-Host ""

# 2. Criar ConfigMap
Write-Host "[3/9] Criando ConfigMap..." -ForegroundColor Yellow
kubectl apply -f base/configmap.yaml
if ($LASTEXITCODE -ne 0) { Write-Host "? Erro ao criar configmap" -ForegroundColor Red; exit 1 }
Write-Host "? ConfigMap criado" -ForegroundColor Green
Write-Host ""

# 3. Criar Secrets
Write-Host "[4/9] Criando Secrets..." -ForegroundColor Yellow
kubectl apply -f base/secrets.yaml
if ($LASTEXITCODE -ne 0) { Write-Host "? Erro ao criar secrets" -ForegroundColor Red; exit 1 }
Write-Host "Secrets criados" -ForegroundColor Green
Write-Host ""

# 3.5. Criar ConfigMap dos Dashboards do Grafana
Write-Host "[4.5/9] Criando ConfigMap dos Dashboards do Grafana..." -ForegroundColor Yellow
if (Test-Path "monitoring/grafana/configmap-dashboards.yaml") {
    kubectl apply -f monitoring/grafana/configmap-dashboards.yaml
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "Erro ao criar ConfigMap de dashboards, criando vazio..." -ForegroundColor Yellow
        kubectl create configmap grafana-dashboards -n agrosolutions --from-literal=dummy.json="{}"
    }
    Write-Host "ConfigMap de dashboards criado" -ForegroundColor Green
} else {
    Write-Host "Arquivo configmap-dashboards.yaml não encontrado, criando vazio..." -ForegroundColor Yellow
    kubectl create configmap grafana-dashboards -n agrosolutions --from-literal=dummy.json="{}"
    Write-Host "ConfigMap vazio criado" -ForegroundColor Green
}
Write-Host ""

# 4. Deploy Microserviços
Write-Host "[5/9] Fazendo deploy dos microserviços..." -ForegroundColor Yellow

$microservices = @(
    "identidade",
    "propriedades",
    "ingestaodados",
    "processamentodados",
    "analise",
    "notificacoes",
    "sensores"
)

foreach ($service in $microservices) {
    Write-Host "  Deploying $service..." -ForegroundColor Cyan
    kubectl apply -f "microservices/$service/deployment.yaml"
    kubectl apply -f "microservices/$service/service.yaml"
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "  ? Erro ao fazer deploy de $service" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ? $service deployado" -ForegroundColor Green
}
Write-Host ""

# 5. Deploy Monitoring - Prometheus
Write-Host "[6/9] Fazendo deploy do Prometheus..." -ForegroundColor Yellow
kubectl apply -f monitoring/prometheus/pvc.yaml
kubectl apply -f monitoring/prometheus/configmap.yaml
kubectl apply -f monitoring/prometheus/deployment.yaml
kubectl apply -f monitoring/prometheus/service.yaml
if ($LASTEXITCODE -ne 0) { Write-Host "? Erro ao fazer deploy do Prometheus" -ForegroundColor Red; exit 1 }
Write-Host "? Prometheus deployado" -ForegroundColor Green
Write-Host ""

# 6. Deploy Monitoring - Grafana
Write-Host "[7/9] Fazendo deploy do Grafana..." -ForegroundColor Yellow
kubectl apply -f monitoring/grafana/pvc.yaml
kubectl apply -f monitoring/grafana/configmap-datasource.yaml
kubectl apply -f monitoring/grafana/configmap-dashboard-provider.yaml
kubectl apply -f monitoring/grafana/deployment.yaml
kubectl apply -f monitoring/grafana/service.yaml
if ($LASTEXITCODE -ne 0) { Write-Host "? Erro ao fazer deploy do Grafana" -ForegroundColor Red; exit 1 }
Write-Host "? Grafana deployado" -ForegroundColor Green
Write-Host ""

# 7. Aguardar Pods
Write-Host "[8/9] Aguardando pods ficarem prontos..." -ForegroundColor Yellow
Write-Host "Isso pode levar alguns minutos..." -ForegroundColor Gray
Start-Sleep -Seconds 5

kubectl wait --for=condition=ready pod -l app=identidade -n agrosolutions --timeout=300s
kubectl wait --for=condition=ready pod -l app=propriedades -n agrosolutions --timeout=300s
kubectl wait --for=condition=ready pod -l app=ingestaodados -n agrosolutions --timeout=300s
kubectl wait --for=condition=ready pod -l app=processamentodados -n agrosolutions --timeout=300s
kubectl wait --for=condition=ready pod -l app=analise -n agrosolutions --timeout=300s
kubectl wait --for=condition=ready pod -l app=notificacoes -n agrosolutions --timeout=300s
kubectl wait --for=condition=ready pod -l app=sensores -n agrosolutions --timeout=300s

Write-Host "? Todos os microserviços estão prontos" -ForegroundColor Green
Write-Host ""

# 8. Exibir Status
Write-Host "[9/9] Status do Cluster:" -ForegroundColor Yellow
Write-Host ""
kubectl get pods -n agrosolutions
Write-Host ""
kubectl get services -n agrosolutions
Write-Host ""

# 9. Informações Finais
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DEPLOY CONCLUÍDO COM SUCESSO!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? URLs de Acesso:" -ForegroundColor Yellow
Write-Host ""

$minikubeIP = minikube ip

Write-Host "Prometheus:  http://${minikubeIP}:30090" -ForegroundColor Cyan
Write-Host "Grafana:     http://${minikubeIP}:30300" -ForegroundColor Cyan
Write-Host "             Usuário: admin" -ForegroundColor Gray
Write-Host "             Senha: (definida no secrets)" -ForegroundColor Gray
Write-Host ""
Write-Host "Para acessar os microserviços, use port-forward:" -ForegroundColor Yellow
Write-Host ".\start-port-forwards.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para ver logs de um serviço:" -ForegroundColor Yellow
Write-Host "kubectl logs -f deployment/<nome> -n agrosolutions" -ForegroundColor Cyan
Write-Host ""
