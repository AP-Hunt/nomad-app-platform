job "ingress" {
    datacenters = ["dc1"]
    type = "system"

    group "traefik" {
        network {
            port "http" {
                static = 80
            }

            port "api" {
                static = 4000
            }
        }

        service {
            name = "traefik"
            check {
                name        = "alive"
                type        = "tcp"
                port        = "http"
                interval    = "10s"
                timeout     = "2s"
            }
        }

        task "traefik" {
            driver = "docker"

            config {
                image = "traefik:2.5"
                network_mode = "host"

                volumes = [
                    "local/traefik.toml:/etc/traefik/traefik.toml"
                ]
            }

            template {
                destination = "local/traefik.toml"
                data = <<EOF
[entryPoints]
    [entryPoints.http]
    address = ":80"
    [entryPoints.traefik]
    address = ":4000"

[api]
    dashboard   = false
    insecure    = true

[providers.consulCatalog]
    prefix              = "traefik"
    exposedByDefault    = false

    [providers.consulCatalog.endpoint]
        address = "${consul_address}"
        scheme  = "http"
EOF
            }
        }
    }
}
