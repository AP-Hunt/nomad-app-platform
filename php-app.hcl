job "php-app" {
  datacenters = ["dc1"]
  type        = "service"

  group "processes" {
    network {
      port "http" { to = 80 }
    }

    task "web" {
      driver = "docker"
      config {
        image = "registry.service.apps.internal:5000/containers/php-app:latest"
        ports = ["http"]

        advertise_ipv6_address = true
      }

      env {
        PORT = "[::]:${NOMAD_PORT_http} ipv6only=off"
      }

      service {
        name         = "php-app"
        port         = "http"
        address_mode = "driver"

        check {
          type     = "http"
          path     = "/"
          interval = "5s"
          timeout  = "5s"
        }
      }
    }

    scaling {
      enabled = true
      min     = 3
      max     = 5
    }
  }

  update {
    auto_promote      = true
    canary            = 1
    healthy_deadline  = "90s"
    max_parallel      = 3
    min_healthy_time  = "10s"
    progress_deadline = "2m"
  }
}
