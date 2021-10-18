output "ingress_outputs" {
    value = jsonencode(module.ingress.*[0])
}