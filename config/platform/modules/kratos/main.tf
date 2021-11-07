data "local_file" "kratos_json" {
    filename = "${path.module}/kratos.json"
}

data "local_file" "id_schema_json" {
    filename = "${path.module}/id.schema.json"
}

resource "nomad_job" "kratos" {
  jobspec = templatefile("${path.module}/kratos.nomad", {
      config_file = data.local_file.kratos_json.content
      id_schema   = data.local_file.id_schema_json.content
  })
}

resource "nomad_job" "kratos_migrate" {
  jobspec = templatefile("${path.module}/kratos-migrate.nomad", {
      config_file = data.local_file.kratos_json.content
      id_schema   = data.local_file.id_schema_json.content
  })
}