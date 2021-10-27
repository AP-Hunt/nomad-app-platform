resource "nomad_job" "app" {
  jobspec = file("${path.module}/api.nomad")
}