job "ingress_probe" {
    datacenters = ["dc1"]
    type = "service"

    spread {
        attribute = "${node.datacenter}"
    }

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
                    "-text", "{\"node\": \"${node.unique.name}\", \"index\": \"${NOMAD_ALLOC_INDEX}\"}"
                ]
                ports = ["http"]
            }
        }

        service {
            name         = "ingress-probe"
            port         = "http"
            address_mode = "alloc"

            check {
                type     = "http"
                path     = "/"
                interval = "5s"
                timeout  = "5s"
            }

            tags = [
                "traefik.http.routers.ingress-probe.rule=Host(`ingress-probe.paas.dev`)",
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
