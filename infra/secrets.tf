resource "aws_secretsmanager_secret" "estoque_service" {
  name                    = "${var.project_name}/estoque-service"
  recovery_window_in_days = 0

  tags = { Name = "estoque-service-secrets" }
}

resource "aws_secretsmanager_secret_version" "estoque_service" {
  secret_id = aws_secretsmanager_secret.estoque_service.id

  secret_string = jsonencode({
    "MongoDB__ConnectionString" = "mongodb://admin:${var.mongodb_root_password}@${data.aws_instance.services.private_ip}:27017/?authSource=admin"
    "RabbitMQ__Host"            = data.aws_instance.services.private_ip
    "RabbitMQ__UserName"        = var.rabbitmq_username
    "RabbitMQ__Password"        = var.rabbitmq_password
    "Jwt__Key"                  = var.jwt_key
  })
}
