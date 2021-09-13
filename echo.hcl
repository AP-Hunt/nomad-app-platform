job "echo" {
    datacenters = ["dc1"]
    type = "service"

    group "processes" {
        network {
            port "http" { to = 80 }
        }

        task "web" {
            driver = "docker"
            config {
                image = "hashicorp/http-echo"
                args = [
                    "-listen", "[::]:${NOMAD_PORT_http}",
                    "-text", "hello world from index ${NOMAD_ALLOC_INDEX}"
                ]
                ports = ["http"]

                advertise_ipv6_address = true
            }

            service {
                name         = "echo"
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
            min     = 2
            max     = 2
        }
    }
}
