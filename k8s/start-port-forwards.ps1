# ========================================
# PORT-FORWARDS - AGROSOLUTIONS
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "INICIANDO PORT-FORWARDS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Aguardando pods ficarem prontos..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# Verificar se pods estão rodando
$podsReady = $true
$pods = kubectl get pods -n agrosolutions -o json | ConvertFrom-Json

foreach ($pod in $pods.items) {
    if ($pod.status.phase -ne "Running") {
        Write-Host "Pod $($pod.metadata.name) não está pronto ainda" -ForegroundColor Yellow
        $podsReady = $false
    }
}

if (-not $podsReady) {
    Write-Host ""
    Write-Host "Aguardando mais 10 segundos..." -ForegroundColor Gray
    Start-Sleep -Seconds 10
}

Write-Host ""
Write-Host "Iniciando port-forwards..." -ForegroundColor Yellow
Write-Host "Pressione Ctrl+C para parar todos" -ForegroundColor Gray
Write-Host ""

# Array para armazenar processos
$jobs = @()

# Iniciar port-forwards em background
$services = @(
    @{Name="identidade"; Port=5001; TargetPort=8080},
    @{Name="propriedades"; Port=5002; TargetPort=8080},
    @{Name="sensores"; Port=5003; TargetPort=8080},
    @{Name="ingestaodados"; Port=5004; TargetPort=8080},
    @{Name="processamentodados"; Port=5005; TargetPort=8080},
    @{Name="analise"; Port=5006; TargetPort=8080},
    @{Name="notificacoes"; Port=5007; TargetPort=8080},
    @{Name="prometheus"; Port=9090; TargetPort=9090},
    @{Name="grafana"; Port=3000; TargetPort=3000}
)

foreach ($svc in $services) {
    $job = Start-Job -ScriptBlock {
        param($serviceName, $localPort, $targetPort)
        kubectl port-forward -n agrosolutions service/$serviceName-service ${localPort}:${targetPort}
    } -ArgumentList $svc.Name, $svc.Port, $svc.TargetPort
    
    $jobs += $job
    Write-Host "Port-forward iniciado: $($svc.Name) -> http://localhost:$($svc.Port)" -ForegroundColor Green
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PORT-FORWARDS ATIVOS" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Microserviços:" -ForegroundColor Yellow
Write-Host "  Identidade:         http://localhost:5001/scalar/v1" -ForegroundColor Cyan
Write-Host "  Propriedades:       http://localhost:5002/scalar/v1" -ForegroundColor Cyan
Write-Host "  Sensores:           http://localhost:5003/scalar/v1" -ForegroundColor Cyan
Write-Host "  IngestaoDados:      http://localhost:5004/scalar/v1" -ForegroundColor Cyan
Write-Host "  ProcessamentoDados: http://localhost:5005/scalar/v1" -ForegroundColor Cyan
Write-Host "  Analise:            http://localhost:5006/scalar/v1" -ForegroundColor Cyan
Write-Host "  Notificacoes:       http://localhost:5007/scalar/v1" -ForegroundColor Cyan
Write-Host ""
Write-Host "Monitoring:" -ForegroundColor Yellow
Write-Host "  Prometheus:         http://localhost:9090" -ForegroundColor Cyan
Write-Host "  Grafana:            http://localhost:3000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pressione Ctrl+C para parar todos os port-forwards" -ForegroundColor Gray
Write-Host ""

# Aguardar Ctrl+C
try {
    while ($true) {
        Start-Sleep -Seconds 1
        
        # Verificar se algum job falhou
        foreach ($job in $jobs) {
            if ($job.State -eq "Failed") {
                Write-Host "Um port-forward falhou. Reiniciando..." -ForegroundColor Yellow
                $job | Remove-Job -Force
            }
        }
    }
}
finally {
    Write-Host ""
    Write-Host "Parando port-forwards..." -ForegroundColor Yellow
    $jobs | Stop-Job
    $jobs | Remove-Job -Force
    Write-Host "Port-forwards parados" -ForegroundColor Green
}
