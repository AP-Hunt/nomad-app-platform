job "api" {
    datacenters = ["dc1"]
    type = "service"

    group "backing_services" {
        network {
            mode = "cni/calico"
            port "redis" { to = 6379 }
        }

        task "redis" {
            driver = "docker"

            config {
                image = "redis:6"
                ports = ["redis"]
            }
        }

        service {
            name = "redis-api"
            address_mode = "alloc"
            port = "redis"
            check {
                name        = "alive"
                type        = "tcp"
                port        = "redis"
                interval    = "10s"
                timeout     = "2s"
            }
        }
    }
    
    group "web" {
        network {
            mode = "cni/calico"
            port "http" { to = 80 }

            dns {
                servers = ["192.168.33.10"]
            }
        }
            
        service {
            name = "platform-api"
            address_mode = "alloc"
            port = "http"

            check {
                name        = "alive"
                type        = "tcp"
                port        = "http"
                interval    = "10s"
                timeout     = "2s"
            }

            tags = [
                "traefik.http.routers.platform-api.rule=Host(`api.paas.dev`)",
                "traefik.enable=true"
            ]
        }


        task "web" {
            driver = "docker" 

            config {
                image = "192.168.33.1:6000/nomad-app-platform/web:latest"
                force_pull = true

                ports = ["http"]
                volumes = [
                    "local/config.json:/app/config.json"
                ]
            }

            template {
                destination = "local/config.json"
                data = <<EOF
{
    "BlobStore": {
        "SourceBundleStoragePath": "/alloc/data/blob/source-bundles",
        "TerraformStatePath": "/alloc/data/blob/terraform-state"
    },
    "Database": {
        "ConnectionString": "Host=192.168.33.11;Port=5432;Database=api;Username=api;Password=nomad-api;SslMode=Disable;"
    },
    "DockerRegistry": {
        "RegistryAddress": "192.168.33.1:6000"
    },
    "Logging": {
        "LogPath": "/app/storage/logs",
        "LogToStdOut": true
    },
    "Nomad": {
        "ApiAddress": "http://192.168.33.10:4646",
        "DockerRegistry": {
            "RegistryAddress": "192.168.33.1:6000"
        }
    },
    "MessageQueue": {
        "RedisAddress": "redis-api.service.paas.dev:6379",
        "RetryCount": 2
    }
}
EOF
            }
        }

        task "worker" {
            driver = "docker" 

            config {
                image = "192.168.33.1:6000/nomad-app-platform/worker:latest"
                force_pull = true

                volumes = [
                    "local/config.json:/app/config.json",
                    "/var/run/docker.sock:/var/run/docker.sock"
                ]

                interactive = true
            }

            template {
                destination = "local/config.json"
                data = <<EOF
{
    "BlobStore": {
        "SourceBundleStoragePath": "/alloc/data/blob/source-bundles",
        "TerraformStatePath": "/alloc/data/blob/terraform-state"
    },
    "Database": {
        "ConnectionString": "Host=192.168.33.11;Port=5432;Database=api;Username=api;Password=nomad-api;SslMode=Disable;"
    },
    "DockerRegistry": {
        "RegistryAddress": "192.168.33.1:6000"
    },
    "Logging": {
        "LogPath": "/app/storage/logs",
        "LogToStdOut": true
    },
    "Nomad": {
        "ApiAddress": "http://192.168.33.10:4646",
        "DockerRegistry": {
            "RegistryAddress": "192.168.33.1:6000"
        }
    },
    "MessageQueue": {
        "RedisAddress": "redis-api.service.paas.dev:6379",
        "RetryCount": 2
    }
}
EOF
            }
        }
    }
}