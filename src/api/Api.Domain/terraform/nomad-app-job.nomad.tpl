job "${app_id}" {
  datacenters = ["dc1"]
  type = "service"
  
  spread {
    attribute = "$${node.datacenter}"
  }

  group "processes" {
    network {
      mode = "cni/calico"
      port "http" { to = 80 }
    }
    
    task "web" {
      driver = "docker"
      
      config {
        image = "${container_image}"
        args = []
        ports = ["http"]
      }
      
      env {
        PORT = "$${NOMAD_PORT_http}"
      }
    }

    service {
      name = "${app_name}"
      port = "http"
      address_mode = "alloc"

      check {
        type = "http"
        path = "/"
        interval = "5s"
        timeout = "5s"
      }

      tags = [
        "traefik.http.routers.${app_name}.rule=Host(`${app_name}.paas.dev`)",
        "traefik.enable=true"
      ]
    }
  }
}