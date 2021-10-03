job "${app_name}" {
  datacenters = ["dc1"]
  type = "service"
  
  group "processes" {
    network {
      port "http" { to = 80 }
    }
    
    task "web" {
      driver = "docker"
      
      config {
        image = "${container_image}"
        args = []
        ports = ["http"]
        
        advertise_ipv6_address = true
      }
      
      env {
        PORT = "$${NOMAD_PORT_http}"
      }
      
      service {
        name = "${app_name}"
        port = "http"
        address_mode = "driver"
        
        check {
          type = "http"
          path = "/"
          interval = "5s"
          timeout = "5s"
        }
      }
    }
  }
}