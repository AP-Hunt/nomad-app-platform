job "ingress" {
    datacenters = ["dc1"]
    type = "system"

    group "nginx" {
        network {
            port "http" {
                static = 80
                to = 80
            }
        }

        task "reverse-proxy" {
            driver = "docker"
            config {
                network_mode = "host"

                image = "nginx"
                args = []
                ports = ["http"]

                volumes = [
                    "local/default.conf:/etc/nginx/nginx.conf:ro"
                ]
            }

            resources {
                memory = 256
            }

            template {
                destination = "local/default.conf"
                data = <<EOF
events{}
http {
    map $http_host $consul_dns_name {
        ~(?<prefix>.+)\.paas\.dev$ "$prefix.service.paas.dev";
    }

    resolver 127.0.0.1 valid=10s ipv6=on;
    resolver_timeout 3s;

    server {
        listen 80;

        add_header "X-Upstream" $consul_dns_name always;
        set $backend "http://$consul_dns_name";


        location / {
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header Host $http_host;
            proxy_pass $backend;
        }
    }
}

EOF
            }
        }
    }
}
