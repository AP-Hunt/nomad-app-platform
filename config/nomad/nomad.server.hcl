data_dir = "/opt/nomad/"

server {
    enabled = true
    bootstrap_expect = 1
}

ports {
    http = 4646
}