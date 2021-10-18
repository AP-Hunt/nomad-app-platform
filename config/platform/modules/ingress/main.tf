data "template_file" "ingress_app_job_spec" {
    template = file("${path.module}/ingress.hcl.tpl")
    vars = {
        consul_address = var.consul_address
    }
}

resource "nomad_job" "app" {
  jobspec = data.template_file.ingress_app_job_spec.rendered
}