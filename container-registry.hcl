job "container-registry" {
    datacenters = ["dc1"]
    type = "service"

    group "container-registry" {
        network {
            mode = "cni/calico"
            port "registry" { to = 5000 }
        }

        task "container-registry" {
            driver = "docker"
            config {
                image = "registry:2"
                args = []
                ports = ["registry"]

                advertise_ipv6_address = true
            }

            service {
                name = "registry"
                port = "registry"
                address_mode = "driver"

                check {
                    type = "http"
                    path = "/v2"
                    interval = "5s"
                    timeout = "5s"
                }
            }
        }
    }
}
