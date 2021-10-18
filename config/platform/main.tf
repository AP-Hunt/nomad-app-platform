module "ingress" {
    source = "./modules/ingress"

    nomad_addr = var.nomad_addr
    consul_address = "127.0.0.1:8500"
}