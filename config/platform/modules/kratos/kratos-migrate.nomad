job "kratos_migrate" {
    datacenters = ["dc1"]
    type = "batch"

    group "kratos" {
        network {}

        task "kratos_migrate" {
            driver = "docker"

            config {
                image = "oryd/kratos:v0.8.0-alpha.3"
                args = [
                    "migrate", "sql", 
                    "--read-from-env",
                    "--config", "/etc/kratos/kratos.json",
                    "--yes"
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

