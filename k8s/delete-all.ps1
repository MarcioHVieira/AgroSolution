# Delete all Kubernetes resources
Write-Host "Deleting all resources..." -ForegroundColor Yellow
kubectl delete namespace agrosolutions
Write-Host "Done!" -ForegroundColor Green
