job "test-app" {
    datacenters = ["dc1"]
    type = "service"

    group "processes" {
        network {
            port "http" { to = 8080 }
        }

        task "web" {
            driver = "docker"
            config {
                image = "registry.service.apps.internal:5000/containers/test-app:latest"
                args = []
                ports = ["http"]

                advertise_ipv6_address = true
            }

            env {
                PORT = "${NOMAD_PORT_http}"
            }

            service {
                name         = "test-app"
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
    }
}
