datacenter = "dev"
log_level  = "INFO"
domain     = "apps.internal"
alt_domain = "paas.dev"
data_dir = "/opt/consul/data/"
rejoin_after_leave = true

ports {
    dns = 53
}

recursors = ["8.8.8.8", "8.8.4.4"]