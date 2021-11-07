job "kratos" {
    datacenters = ["dc1"]
    type = "system"

    group "kratos" {
        network {
            mode = "cni/calico"
            port "http" {
                to = 4433
            }

            port "admin" {
                to = "4434"
            }
        }

        service {
            name = "kratos"
            check {
                name        = "alive"
                type        = "http"
                port        = "http"
                path        = "/health/ready"
                interval    = "10s"
                timeout     = "2s"
            }

            tags = [
                "traefik.http.routers.kratos.rule=Host(`identity.paas.dev`)",
                "traefik.enable=true"
            ]
        }

        task "kratos" {
            driver = "docker"

            config {
                image = "oryd/kratos:v0.8.0-alpha.3"
                args = [
                    "serve", 
                    "-c", "/etc/kratos/kratos.json"
                ]

                volumes = [
                    "local/kratos.json:/etc/kratos/kratos.json",
                    "local/id.schema.json:/etc/kratos/id.schema.json"
                ]
            }

            template {
                destination = "local/kratos.json"
                data = <<EOF
${config_file}
EOF
            }

            template {
                destination = "local/id.schema.json"
                data = <<EOF
${id_schema}
EOF
            }            
        }
    }
}