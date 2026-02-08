# ========================================
# SETUP COMPLETO - ESTRUTURA KUBERNETES AGROSOLUTIONS
# ========================================
# Este script cria toda a estrutura de arquivos necessária para deploy no Kubernetes
# Pode ser executado para setup inicial ou para recriar a estrutura
# ========================================

param(
    [switch]$Force,
    [switch]$SkipConfirmation
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AGROSOLUTIONS - SETUP ESTRUTURA K8S" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se já existe estrutura k8s
if (Test-Path "k8s") {
    Write-Host "ATENÇÃO: A pasta 'k8s' já existe!" -ForegroundColor Yellow
    Write-Host ""
    
    if (-not $SkipConfirmation) {
        $response = Read-Host "Deseja remover e recriar toda a estrutura? (S/N)"
        if ($response -ne "S" -and $response -ne "s") {
            Write-Host "Operação cancelada pelo usuário." -ForegroundColor Yellow
            exit 0
        }
    }
    
    Write-Host "Removendo estrutura existente..." -ForegroundColor Yellow
    Remove-Item -Path "k8s" -Recurse -Force
    Write-Host "? Estrutura antiga removida" -ForegroundColor Green
    Write-Host ""
}

Write-Host "Criando estrutura de pastas..." -ForegroundColor Yellow

# Criar estrutura de pastas
$folders = @(
    "k8s",
    "k8s/base",
    "k8s/microservices",
    "k8s/microservices/identidade",
    "k8s/microservices/propriedades",
    "k8s/microservices/ingestaodados",
    "k8s/microservices/processamentodados",
    "k8s/microservices/analise",
    "k8s/microservices/notificacoes",
    "k8s/microservices/sensores",
    "k8s/monitoring",
    "k8s/monitoring/prometheus",
    "k8s/monitoring/grafana"
)

foreach ($folder in $folders) {
    New-Item -ItemType Directory -Path $folder -Force | Out-Null
}

Write-Host "? Estrutura de pastas criada" -ForegroundColor Green
Write-Host ""

# ========================================
# ARQUIVOS BASE
# ========================================

Write-Host "Criando arquivos base..." -ForegroundColor Yellow

# namespace.yaml
$namespaceContent = @'
apiVersion: v1
kind: Namespace
metadata:
  name: agrosolutions
  labels:
    name: agrosolutions
    environment: development
'@
$namespaceContent | Out-File -FilePath "k8s/base/namespace.yaml" -Encoding UTF8

# configmap.yaml
$configmapContent = @'
apiVersion: v1
kind: ConfigMap
metadata:
  name: agrosolutions-config
  namespace: agrosolutions
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:8080"
  Jwt__RequireHttpsMetadata: "false"
'@
$configmapContent | Out-File -FilePath "k8s/base/configmap.yaml" -Encoding UTF8

# secrets.yaml.template
$secretsTemplateContent = @'
apiVersion: v1
kind: Secret
metadata:
  name: agrosolutions-secrets
  namespace: agrosolutions
type: Opaque
data:
  # Codifique seus valores em Base64 antes de usar
  # Para codificar: [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("sua-string"))
  
  CONNECTION_STRING: ""
  RABBITMQ_HOST: ""
  RABBITMQ_PORT: ""
  RABBITMQ_USER: ""
  RABBITMQ_PASSWORD: ""
  SMTP_SERVER: ""
  SMTP_PORT: ""
  SMTP_USER: ""
  SMTP_PASSWORD: ""
  FROM_EMAIL: ""
  FROM_NAME: ""
  ENABLE_SSL: ""
  TIMEOUT_SECONDS: ""
  SIMULADOR_EMAIL: ""
  SIMULADOR_SENHA: ""
  GRAFANA_ADMIN_PASSWORD: ""
'@
$secretsTemplateContent | Out-File -FilePath "k8s/base/secrets.yaml.template" -Encoding UTF8

Write-Host "? Arquivos base criados" -ForegroundColor Green
Write-Host ""

# ========================================
# FUNÇÃO PARA CRIAR DEPLOYMENT E SERVICE
# ========================================

function New-MicroserviceFiles {
    param(
        [string]$ServiceName,
        [string]$Image,
        [int]$Port,
        [string]$EnvVars
    )
    
    $deploymentContent = @"
apiVersion: apps/v1
kind: Deployment
metadata:
  name: $ServiceName
  namespace: agrosolutions
  labels:
    app: $ServiceName
    tier: backend
spec:
  replicas: 1
  selector:
    matchLabels:
      app: $ServiceName
  template:
    metadata:
      labels:
        app: $ServiceName
        tier: backend
    spec:
      containers:
      - name: $ServiceName
        image: $Image
        imagePullPolicy: Never
        ports:
        - containerPort: 8080
          name: http
          protocol: TCP
        envFrom:
        - configMapRef:
            name: agrosolutions-config
$EnvVars
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 15
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
"@
    
    $serviceContent = @"
apiVersion: v1
kind: Service
metadata:
  name: $ServiceName-service
  namespace: agrosolutions
  labels:
    app: $ServiceName
    tier: backend
spec:
  type: ClusterIP
  selector:
    app: $ServiceName
  ports:
  - port: 8080
    targetPort: 8080
    protocol: TCP
    name: http
"@
    
    $deploymentContent | Out-File -FilePath "k8s/microservices/$ServiceName/deployment.yaml" -Encoding UTF8
    $serviceContent | Out-File -FilePath "k8s/microservices/$ServiceName/service.yaml" -Encoding UTF8
}

# ========================================
# MICROSERVIÇOS
# ========================================

Write-Host "Criando configurações dos microserviços..." -ForegroundColor Yellow

# Identidade
$identidadeEnv = @'
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: CONNECTION_STRING
        - name: EmailSettings__SmtpServer
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SMTP_SERVER
        - name: EmailSettings__SmtpPort
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SMTP_PORT
        - name: EmailSettings__SmtpUser
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SMTP_USER
        - name: EmailSettings__SmtpPassword
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SMTP_PASSWORD
        - name: EmailSettings__FromEmail
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: FROM_EMAIL
        - name: EmailSettings__FromName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: FROM_NAME
        - name: EmailSettings__EnableSsl
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: ENABLE_SSL
        - name: EmailSettings__TimeoutSeconds
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: TIMEOUT_SECONDS
        - name: EmailSettings__UseMockEmail
          value: "false"
        - name: RabbitMQ__HostName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_HOST
        - name: RabbitMQ__Port
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PORT
        - name: RabbitMQ__UserName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_USER
        - name: RabbitMQ__Password
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PASSWORD
'@
New-MicroserviceFiles -ServiceName "identidade" -Image "agrosolutions-identidade:latest" -Port 8080 -EnvVars $identidadeEnv

# Propriedades
$propriedadesEnv = @'
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: CONNECTION_STRING
        - name: Identidade__Url
          value: "http://identidade-service:8080"
        - name: RabbitMQ__HostName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_HOST
        - name: RabbitMQ__Port
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PORT
        - name: RabbitMQ__UserName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_USER
        - name: RabbitMQ__Password
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PASSWORD
'@
New-MicroserviceFiles -ServiceName "propriedades" -Image "agrosolutions-propriedades:latest" -Port 8080 -EnvVars $propriedadesEnv

# IngestaoDados
$ingestaodadosEnv = @'
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: CONNECTION_STRING
        - name: Identidade__Url
          value: "http://identidade-service:8080"
        - name: RabbitMQ__HostName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_HOST
        - name: RabbitMQ__Port
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PORT
        - name: RabbitMQ__UserName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_USER
        - name: RabbitMQ__Password
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PASSWORD
'@
New-MicroserviceFiles -ServiceName "ingestaodados" -Image "agrosolutions-ingestaodados:latest" -Port 8080 -EnvVars $ingestaodadosEnv

# ProcessamentoDados
$processamentodadosEnv = @'
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: CONNECTION_STRING
        - name: Identidade__Url
          value: "http://identidade-service:8080"
        - name: RabbitMQ__HostName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_HOST
        - name: RabbitMQ__Port
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PORT
        - name: RabbitMQ__UserName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_USER
        - name: RabbitMQ__Password
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PASSWORD
'@
New-MicroserviceFiles -ServiceName "processamentodados" -Image "agrosolutions-processamentodados:latest" -Port 8080 -EnvVars $processamentodadosEnv

# Analise
$analiseEnv = @'
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: CONNECTION_STRING
        - name: Identidade__Url
          value: "http://identidade-service:8080"
        - name: RabbitMQ__HostName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_HOST
        - name: RabbitMQ__Port
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PORT
        - name: RabbitMQ__UserName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_USER
        - name: RabbitMQ__Password
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PASSWORD
'@
New-MicroserviceFiles -ServiceName "analise" -Image "agrosolutions-analise:latest" -Port 8080 -EnvVars $analiseEnv

# Notificacoes
$notificacoesEnv = @'
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: CONNECTION_STRING
        - name: Identidade__Url
          value: "http://identidade-service:8080"
        - name: RabbitMQ__HostName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_HOST
        - name: RabbitMQ__Port
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PORT
        - name: RabbitMQ__UserName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_USER
        - name: RabbitMQ__Password
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: RABBITMQ_PASSWORD
        - name: EmailSettings__SmtpServer
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SMTP_SERVER
        - name: EmailSettings__SmtpPort
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SMTP_PORT
        - name: EmailSettings__SmtpUser
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SMTP_USER
        - name: EmailSettings__SmtpPassword
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SMTP_PASSWORD
        - name: EmailSettings__FromEmail
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: FROM_EMAIL
        - name: EmailSettings__FromName
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: FROM_NAME
        - name: EmailSettings__EnableSsl
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: ENABLE_SSL
        - name: EmailSettings__TimeoutSeconds
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: TIMEOUT_SECONDS
'@
New-MicroserviceFiles -ServiceName "notificacoes" -Image "agrosolutions-notificacoes:latest" -Port 8080 -EnvVars $notificacoesEnv

# Sensores
$sensoresEnv = @'
        env:
        - name: Simulador__IngestaoApi__BaseUrl
          value: "http://ingestaodados-service:8080"
        - name: Simulador__Autenticacao__IdentidadeUrl
          value: "http://identidade-service:8080"
        - name: Simulador__Autenticacao__Email
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SIMULADOR_EMAIL
        - name: Simulador__Autenticacao__Senha
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: SIMULADOR_SENHA
'@
New-MicroserviceFiles -ServiceName "sensores" -Image "agrosolutions-sensores:latest" -Port 8080 -EnvVars $sensoresEnv

Write-Host "? Microserviços configurados" -ForegroundColor Green
Write-Host ""

# ========================================
# MONITORING - PROMETHEUS
# ========================================

Write-Host "Criando configurações do Prometheus..." -ForegroundColor Yellow

# Prometheus PVC
$prometheusPvcContent = @'
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: prometheus-pvc
  namespace: agrosolutions
  labels:
    app: prometheus
    tier: monitoring
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 5Gi
'@
$prometheusPvcContent | Out-File -FilePath "k8s/monitoring/prometheus/pvc.yaml" -Encoding UTF8

# Prometheus ConfigMap
$prometheusConfigContent = @'
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-config
  namespace: agrosolutions
data:
  prometheus.yml: |
    global:
      scrape_interval: 15s
      evaluation_interval: 15s
      external_labels:
        cluster: agrosolutions-k8s
        environment: development
    
    scrape_configs:
      - job_name: 'prometheus'
        static_configs:
          - targets: ['localhost:9090']
      
      - job_name: 'identidade'
        metrics_path: '/metrics'
        scrape_interval: 10s
        scrape_timeout: 5s
        static_configs:
          - targets: ['identidade-service:8080']
            labels:
              service: 'identidade'
      
      - job_name: 'propriedades'
        metrics_path: '/metrics'
        scrape_interval: 10s
        scrape_timeout: 5s
        static_configs:
          - targets: ['propriedades-service:8080']
            labels:
              service: 'propriedades'
      
      - job_name: 'ingestaodados'
        metrics_path: '/metrics'
        scrape_interval: 10s
        scrape_timeout: 5s
        static_configs:
          - targets: ['ingestaodados-service:8080']
            labels:
              service: 'ingestaodados'
      
      - job_name: 'processamentodados'
        metrics_path: '/metrics'
        scrape_interval: 10s
        scrape_timeout: 5s
        static_configs:
          - targets: ['processamentodados-service:8080']
            labels:
              service: 'processamentodados'
      
      - job_name: 'analise'
        metrics_path: '/metrics'
        scrape_interval: 10s
        scrape_timeout: 5s
        static_configs:
          - targets: ['analise-service:8080']
            labels:
              service: 'analise'
      
      - job_name: 'notificacoes'
        metrics_path: '/metrics'
        scrape_interval: 10s
        scrape_timeout: 5s
        static_configs:
          - targets: ['notificacoes-service:8080']
            labels:
              service: 'notificacoes'
      
      - job_name: 'sensores'
        metrics_path: '/metrics'
        scrape_interval: 10s
        scrape_timeout: 5s
        static_configs:
          - targets: ['sensores-service:8080']
            labels:
              service: 'sensores'
'@
$prometheusConfigContent | Out-File -FilePath "k8s/monitoring/prometheus/configmap.yaml" -Encoding UTF8

# Prometheus Deployment
$prometheusDeploymentContent = @'
apiVersion: apps/v1
kind: Deployment
metadata:
  name: prometheus
  namespace: agrosolutions
  labels:
    app: prometheus
    tier: monitoring
spec:
  replicas: 1
  selector:
    matchLabels:
      app: prometheus
  template:
    metadata:
      labels:
        app: prometheus
        tier: monitoring
    spec:
      containers:
      - name: prometheus
        image: prom/prometheus:latest
        args:
          - '--config.file=/etc/prometheus/prometheus.yml'
          - '--storage.tsdb.path=/prometheus'
          - '--web.console.libraries=/usr/share/prometheus/console_libraries'
          - '--web.console.templates=/usr/share/prometheus/consoles'
        ports:
        - containerPort: 9090
          name: http
          protocol: TCP
        volumeMounts:
        - name: prometheus-config
          mountPath: /etc/prometheus
        - name: prometheus-storage
          mountPath: /prometheus
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
      volumes:
      - name: prometheus-config
        configMap:
          name: prometheus-config
      - name: prometheus-storage
        persistentVolumeClaim:
          claimName: prometheus-pvc
'@
$prometheusDeploymentContent | Out-File -FilePath "k8s/monitoring/prometheus/deployment.yaml" -Encoding UTF8

# Prometheus Service
$prometheusServiceContent = @'
apiVersion: v1
kind: Service
metadata:
  name: prometheus-service
  namespace: agrosolutions
  labels:
    app: prometheus
    tier: monitoring
spec:
  type: NodePort
  selector:
    app: prometheus
  ports:
  - port: 9090
    targetPort: 9090
    nodePort: 30090
    protocol: TCP
    name: http
'@
$prometheusServiceContent | Out-File -FilePath "k8s/monitoring/prometheus/service.yaml" -Encoding UTF8

Write-Host "? Prometheus configurado" -ForegroundColor Green
Write-Host ""

# ========================================
# MONITORING - GRAFANA
# ========================================

Write-Host "Criando configurações do Grafana..." -ForegroundColor Yellow

# Grafana PVC
$grafanaPvcContent = @'
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: grafana-pvc
  namespace: agrosolutions
  labels:
    app: grafana
    tier: monitoring
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 2Gi
'@
$grafanaPvcContent | Out-File -FilePath "k8s/monitoring/grafana/pvc.yaml" -Encoding UTF8

# Grafana Datasource ConfigMap
$grafanaDatasourceContent = @'
apiVersion: v1
kind: ConfigMap
metadata:
  name: grafana-datasource
  namespace: agrosolutions
data:
  prometheus.yml: |
    apiVersion: 1
    
    datasources:
      - name: Prometheus
        type: prometheus
        access: proxy
        url: http://prometheus-service:9090
        uid: prometheus
        isDefault: true
        editable: false
        jsonData:
          timeInterval: "5s"
          queryTimeout: "60s"
'@
$grafanaDatasourceContent | Out-File -FilePath "k8s/monitoring/grafana/configmap-datasource.yaml" -Encoding UTF8

# Grafana Dashboard Provider ConfigMap
$grafanaDashboardProviderContent = @'
apiVersion: v1
kind: ConfigMap
metadata:
  name: grafana-dashboard-provider
  namespace: agrosolutions
data:
  dashboard-provider.yml: |
    apiVersion: 1
    
    providers:
      - name: 'AgroSolutions Dashboards'
        orgId: 1
        folder: 'AgroSolutions'
        type: file
        disableDeletion: false
        updateIntervalSeconds: 10
        allowUiUpdates: true
        options:
          path: /var/lib/grafana/dashboards
'@
$grafanaDashboardProviderContent | Out-File -FilePath "k8s/monitoring/grafana/configmap-dashboard-provider.yaml" -Encoding UTF8

# Grafana Deployment
$grafanaDeploymentContent = @'
apiVersion: apps/v1
kind: Deployment
metadata:
  name: grafana
  namespace: agrosolutions
  labels:
    app: grafana
    tier: monitoring
spec:
  replicas: 1
  selector:
    matchLabels:
      app: grafana
  template:
    metadata:
      labels:
        app: grafana
        tier: monitoring
    spec:
      containers:
      - name: grafana
        image: grafana/grafana:latest
        ports:
        - containerPort: 3000
          name: http
          protocol: TCP
        env:
        - name: GF_SECURITY_ADMIN_USER
          value: "admin"
        - name: GF_SECURITY_ADMIN_PASSWORD
          valueFrom:
            secretKeyRef:
              name: agrosolutions-secrets
              key: GRAFANA_ADMIN_PASSWORD
        - name: GF_PATHS_PROVISIONING
          value: "/etc/grafana/provisioning"
        volumeMounts:
        - name: grafana-storage
          mountPath: /var/lib/grafana
        - name: grafana-datasource
          mountPath: /etc/grafana/provisioning/datasources
        - name: grafana-dashboard-provider
          mountPath: /etc/grafana/provisioning/dashboards
        - name: grafana-dashboards
          mountPath: /var/lib/grafana/dashboards
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "250m"
      volumes:
      - name: grafana-storage
        persistentVolumeClaim:
          claimName: grafana-pvc
      - name: grafana-datasource
        configMap:
          name: grafana-datasource
      - name: grafana-dashboard-provider
        configMap:
          name: grafana-dashboard-provider
      - name: grafana-dashboards
        configMap:
          name: grafana-dashboards
'@
$grafanaDeploymentContent | Out-File -FilePath "k8s/monitoring/grafana/deployment.yaml" -Encoding UTF8

# Grafana Service
$grafanaServiceContent = @'
apiVersion: v1
kind: Service
metadata:
  name: grafana-service
  namespace: agrosolutions
  labels:
    app: grafana
    tier: monitoring
spec:
  type: NodePort
  selector:
    app: grafana
  ports:
  - port: 3000
    targetPort: 3000
    nodePort: 30300
    protocol: TCP
    name: http
'@
$grafanaServiceContent | Out-File -FilePath "k8s/monitoring/grafana/service.yaml" -Encoding UTF8

# Script para gerar ConfigMap dos dashboards
$generateDashboardsScript = @'
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
'@
$generateDashboardsScript | Out-File -FilePath "k8s/monitoring/grafana/generate-dashboards-configmap.ps1" -Encoding UTF8

Write-Host "? Grafana configurado" -ForegroundColor Green
Write-Host ""

# ========================================
# SCRIPTS AUXILIARES
# ========================================

Write-Host "Criando scripts auxiliares..." -ForegroundColor Yellow

# Copiar scripts se já existirem, senão criar templates básicos
if (Test-Path "build-images.ps1") {
    Copy-Item "build-images.ps1" "k8s/" -Force
} else {
    # Criar template básico
    @'
# Build de todas as imagens Docker
Write-Host "Building Docker images..." -ForegroundColor Cyan
# Adicione seus comandos de build aqui
'@ | Out-File -FilePath "k8s/build-images.ps1" -Encoding UTF8
}

if (Test-Path "deploy-all.ps1") {
    Copy-Item "deploy-all.ps1" "k8s/" -Force
}

if (Test-Path "update-monitoring.ps1") {
    Copy-Item "update-monitoring.ps1" "k8s/" -Force
}

if (Test-Path "delete-all.ps1") {
    Copy-Item "delete-all.ps1" "k8s/" -Force
} else {
    # Criar script de delete
    @'
# Delete all Kubernetes resources
Write-Host "Deleting all resources..." -ForegroundColor Yellow
kubectl delete namespace agrosolutions
Write-Host "Done!" -ForegroundColor Green
'@ | Out-File -FilePath "k8s/delete-all.ps1" -Encoding UTF8
}

if (Test-Path "start-port-forwards.ps1") {
    Copy-Item "start-port-forwards.ps1" "k8s/" -Force
}

# .gitignore
$gitignoreContent = @'
# Secrets
base/secrets.yaml

# Dashboards ConfigMap (gerado automaticamente)
monitoring/grafana/configmap-dashboards.yaml

# Logs
*.log
'@
$gitignoreContent | Out-File -FilePath "k8s/.gitignore" -Encoding UTF8

Write-Host "? Scripts auxiliares criados" -ForegroundColor Green
Write-Host ""

# ========================================
# VALIDAÇÃO
# ========================================

Write-Host "Validando estrutura criada..." -ForegroundColor Yellow

$errors = 0

# Verificar arquivos críticos
$criticalFiles = @(
    "k8s/base/namespace.yaml",
    "k8s/base/configmap.yaml",
    "k8s/base/secrets.yaml.template",
    "k8s/monitoring/prometheus/configmap.yaml",
    "k8s/monitoring/grafana/configmap-datasource.yaml"
)

foreach ($file in $criticalFiles) {
    if (-not (Test-Path $file)) {
        Write-Host "? Arquivo não encontrado: $file" -ForegroundColor Red
        $errors++
    }
}

# Verificar microserviços
$microservices = @("identidade", "propriedades", "ingestaodados", "processamentodados", "analise", "notificacoes", "sensores")
foreach ($svc in $microservices) {
    if (-not (Test-Path "k8s/microservices/$svc/deployment.yaml")) {
        Write-Host "? Deployment não encontrado: $svc" -ForegroundColor Red
        $errors++
    }
    if (-not (Test-Path "k8s/microservices/$svc/service.yaml")) {
        Write-Host "? Service não encontrado: $svc" -ForegroundColor Red
        $errors++
    }
}

if ($errors -eq 0) {
    Write-Host "? Estrutura validada com sucesso!" -ForegroundColor Green
} else {
    Write-Host "? Foram encontrados $errors erro(s)" -ForegroundColor Red
}
Write-Host ""

# ========================================
# RESUMO E PRÓXIMOS PASSOS
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SETUP CONCLUÍDO COM SUCESSO!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Estrutura criada:" -ForegroundColor Yellow
Write-Host "  ? Base (namespace, configmap, secrets template)" -ForegroundColor Green
Write-Host "  ? 7 Microserviços (deployment + service)" -ForegroundColor Green
Write-Host "  ? Monitoring (Prometheus + Grafana)" -ForegroundColor Green
Write-Host "  ? Scripts auxiliares" -ForegroundColor Green
Write-Host ""

Write-Host "?? PRÓXIMOS PASSOS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1??  Configurar Secrets:" -ForegroundColor Cyan
Write-Host "   cd k8s/base" -ForegroundColor Gray
Write-Host "   cp secrets.yaml.template secrets.yaml" -ForegroundColor Gray
Write-Host "   # Edite secrets.yaml e adicione seus valores em Base64" -ForegroundColor Gray
Write-Host ""

Write-Host "2??  Gerar ConfigMap dos Dashboards do Grafana:" -ForegroundColor Cyan
Write-Host "   cd k8s/monitoring/grafana" -ForegroundColor Gray
Write-Host "   .\generate-dashboards-configmap.ps1" -ForegroundColor Gray
Write-Host "   cd ../../.." -ForegroundColor Gray
Write-Host ""

Write-Host "3??  Iniciar Minikube:" -ForegroundColor Cyan
Write-Host "   minikube start --driver=docker --cpus=4 --memory=8192" -ForegroundColor Gray
Write-Host ""

Write-Host "4??  Configurar Docker para usar registry do Minikube:" -ForegroundColor Cyan
Write-Host "   minikube docker-env | Invoke-Expression" -ForegroundColor Gray
Write-Host ""

Write-Host "5??  Build das imagens Docker:" -ForegroundColor Cyan
Write-Host "   cd k8s" -ForegroundColor Gray
Write-Host "   .\build-images.ps1" -ForegroundColor Gray
Write-Host ""

Write-Host "6??  Deploy completo:" -ForegroundColor Cyan
Write-Host "   .\deploy-all.ps1" -ForegroundColor Gray
Write-Host ""

Write-Host "7??  Iniciar port-forwards:" -ForegroundColor Cyan
Write-Host "   .\start-port-forwards.ps1" -ForegroundColor Gray
Write-Host ""

Write-Host "?? Documentação:" -ForegroundColor Yellow
Write-Host "   - Consulte k8s/README.md para mais informações" -ForegroundColor Gray
Write-Host "   - Consulte k8s/monitoring/README.md para detalhes do monitoring" -ForegroundColor Gray
Write-Host ""

Write-Host "??  IMPORTANTE:" -ForegroundColor Yellow
Write-Host "   - Não commite o arquivo k8s/base/secrets.yaml (já está no .gitignore)" -ForegroundColor Red
Write-Host "   - O configmap-dashboards.yaml será gerado automaticamente" -ForegroundColor Gray
Write-Host ""
