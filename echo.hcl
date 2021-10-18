job "echo" {
    datacenters = ["dc1"]
    type = "service"

    group "processes" {
        network {
            mode = "cni/calico"
            port "http" { to = 80 }
        }

        task "web" {
            driver = "docker"
            config {
                image = "hashicorp/http-echo"
                args = [
                    "-listen", "0.0.0.0:${NOMAD_PORT_http}",
                    "-text", "hello world from index ${NOMAD_ALLOC_INDEX}"
                ]
                ports = ["http"]

                advertise_ipv6_address = false
            }
        }

        service {
            name         = "echo"
            port         = "http"
            address_mode = "alloc"

            check {
                type     = "http"
                path     = "/"
                interval = "5s"
                timeout  = "5s"
            }

            tags = [
                "traefik.http.routers.echo.rule=Host(`echo.paas.dev`)",
                "traefik.enable=true"
            ]
        }

        scaling {
            enabled = true
            min     = 3
            max     = 3
        }
    }
}
