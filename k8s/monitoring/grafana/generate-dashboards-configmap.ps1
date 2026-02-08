# Script para gerar ConfigMap dos Dashboards do Grafana
Write-Host "Gerando ConfigMap dos Dashboards..." -ForegroundColor Cyan

$dashboardsPath = "..\..\..\monitoring\grafana\dashboards"
$outputPath = "configmap-dashboards.yaml"

# Inicia o ConfigMap
$configMap = @"
apiVersion: v1
kind: ConfigMap
metadata:
  name: grafana-dashboards
  namespace: agrosolutions
data:
"@

# Adiciona cada dashboard
$dashboardFiles = Get-ChildItem -Path $dashboardsPath -Filter "*.json"
foreach ($file in $dashboardFiles) {
    $content = Get-Content $file.FullName -Raw
    $fileName = $file.Name
    
    # Indenta o conteúdo JSON
    $lines = $content -split "`n"
    $indentedContent = $lines | ForEach-Object { "    $_" }
    
    $configMap += @"

  $fileName`: |
$($indentedContent -join "`n")
"@
}

# Salva o ConfigMap
$configMap | Out-File -FilePath $outputPath -Encoding UTF8

Write-Host "ConfigMap gerado com sucesso em: $outputPath" -ForegroundColor Green
