FROM ubuntu:focal

RUN apt-get update -q && \
    apt-get install -q -y \
        dnsutils zip wget

RUN wget -q -O nomad.zip "https://releases.hashicorp.com/nomad/1.2.0-beta1/nomad_1.2.0-beta1_linux_amd64.zip" && \
    unzip nomad.zip && mv ./nomad /usr/local/bin/nomad

RUN wget -q -O consul.zip "https://releases.hashicorp.com/consul/1.10.2/consul_1.10.2_linux_amd64.zip" && \
    unzip consul.zip && mv ./consul /usr/local/bin/consul

RUN wget -q -O kratos.tar.gz "https://github.com/ory/kratos/releases/download/v0.8.0-alpha.3/kratos_0.8.0-alpha.3_linux_64bit.tar.gz" && \
    tar xzf kratos.tar.gz && mv ./kratos /usr/local/bin/kratos

WORKDIR "/home/shell"

ENTRYPOINT ["bash"]