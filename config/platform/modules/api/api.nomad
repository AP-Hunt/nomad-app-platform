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

    group "worker" {
        task "worker" {
            driver = "docker" 

            config {
                image = "192.168.33.1:6000/nomad-app-platform/worker:1635092060"

                volumes = [
                    "local/config.json:/app/config.json"
                ]
            }

            template {
                destination = "local/config.json"
                data = <<EOF
{
    "BlobStore": {
        "SourceBundleStoragePath": "/app/storage/blob/source-bundles",
        "TerraformStatePath": "/app/storage/blob/terraform-state"
    },
    "Database": {
        "ConnectionString": "Host=192.168.33.11;Port=5432;Database=api;Username=api;Password=nomad-api;SslMode=Disable;"
    },
    "DockerRegistry": {
        "RegistryAddress": "192.168.33.1:6000"
    },
    "Logging": {
        "LogPath": "/app/storage/logs"
    },
    "Nomad": {
        "ApiAddress": "http://192.168.33.10:4646",
        "DockerRegistry": {
            "RegistryAddress": "192.168.33.1:6000"
        }
    },
    "MessageQueue": {
        "RedisAddress": "redis-api.services.paas.dev:6379",
        "RetryCount": 2
    }
}
EOF
            }
        }
    }
  

    group "web" {
        network {
            mode = "cni/calico"
            port "http" { to = 80 }
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
                image = "192.168.33.1:6000/nomad-app-platform/web:1635092060"
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
        "SourceBundleStoragePath": "/app/storage/blob/source-bundles",
        "TerraformStatePath": "/app/storage/blob/terraform-state"
    },
    "Database": {
        "ConnectionString": "Host=192.168.33.11;Port=5432;Database=api;Username=api;Password=nomad-api;SslMode=Disable;"
    },
    "DockerRegistry": {
        "RegistryAddress": "192.168.33.1:6000"
    },
    "Logging": {
        "LogPath": "/app/storage/logs"
    },
    "Nomad": {
        "ApiAddress": "http://192.168.33.10:4646",
        "DockerRegistry": {
            "RegistryAddress": "192.168.33.1:6000"
        }
    },
    "MessageQueue": {
        "RedisAddress": "redis-api.services.paas.dev:6379",
        "RetryCount": 2
    }
}
EOF
            }
        }
    }
}