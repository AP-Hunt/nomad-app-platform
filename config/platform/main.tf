module "ingress" {
    source = "./modules/ingress"

    nomad_addr = var.nomad_addr
    consul_address = "127.0.0.1:8500"
}

module "kratos" {
    source = "./modules/kratos"

    nomad_addr = var.nomad_addr
}

module "api" {
    source = "./modules/api"

    nomad_addr = var.nomad_addr
}