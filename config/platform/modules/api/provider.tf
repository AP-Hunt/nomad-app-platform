terraform {
  required_providers {
    nomad = {
      source = "hashicorp/nomad"
      version = "1.4.15"
    }
  }
}

provider "nomad" {
  address   = var.nomad_addr
  region    = "global"
}