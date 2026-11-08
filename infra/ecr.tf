resource "aws_ecr_repository" "estoque_service" {
  name                 = "${var.project_name}-estoque-service"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = { Name = "estoque-service" }
}

resource "aws_ecr_lifecycle_policy" "estoque_service" {
  repository = aws_ecr_repository.estoque_service.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Manter apenas as últimas 5 imagens"
      selection = {
        tagStatus   = "any"
        countType   = "imageCountMoreThan"
        countNumber = 5
      }
      action = { type = "expire" }
    }]
  })
}
