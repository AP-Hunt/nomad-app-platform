data "template_file" "nomad_app_job" {
  template = "${file("nomad-app-job.nomad.tpl")}"
  vars = {
    app_name = var.app_name
    container_image = var.container_image
  }
}

resource "nomad_job" "app" {
  jobspec = data.template_file.nomad_app_job.rendered
}