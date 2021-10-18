job "netcat" {
    datacenters = ["dc1"]
    type = "service"

    group "processes" {
        network {
            mode = "cni/calico"
            port "http" { }
        }

        task "web" {
            driver = "docker"
            config {
                image = "alpine"
                args = [
                    "-p", "${NOMAD_PORT_http}",
                    "-lk",
                    "-e", "'echo hello world'",
                ]
                ports = ["http"]
                command = "nc"

                advertise_ipv6_address = false
            }
        }

        service {
            name         = "netcat"
            port         = "http"
            address_mode = "alloc"

            check {
                type     = "http"
                path     = "/"
                interval = "5s"
                timeout  = "5s"
            }
        }

        scaling {
            enabled = true
            min     = 3
            max     = 3
        }
    }
}
