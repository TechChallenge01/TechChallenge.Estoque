output "estoque_service_url" {
  description = "URL pública do Estoque Service"
  value       = "http://${aws_lb.estoque_service.dns_name}"
}

output "ecr_estoque_repository_url" {
  description = "URL do repositório ECR do Estoque Service"
  value       = aws_ecr_repository.estoque_service.repository_url
}
