resource "aws_cloudwatch_log_group" "estoque_service" {
  name              = "/ecs/estoque-service"
  retention_in_days = 7

  tags = { Name = "estoque-service-logs" }
}
