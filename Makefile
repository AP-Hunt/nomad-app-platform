install: nomad consul etcd

nomad:
	wget -O nomad.zip "https://releases.hashicorp.com/nomad/1.1.2/nomad_1.1.2_linux_amd64.zip"
	unzip nomad.zip
	rm nomad.zip

consul:
	wget -O consul.zip "https://releases.hashicorp.com/consul/1.10.1/consul_1.10.1_linux_amd64.zip"
	unzip consul.zip
	rm consul.zip

etcd:
	wget -O etcd.tar.gz "https://github.com/etcd-io/etcd/releases/download/v3.5.0/etcd-v3.5.0-linux-amd64.tar.gz"
	tar -xzvf etcd.tar.gz --strip-components=1 \
	  etcd-v3.5.0-linux-amd64/etcdutl \
	  etcd-v3.5.0-linux-amd64/etcdctl \
	  etcd-v3.5.0-linux-amd64/etcd
	rm etcd.tar.gz

start: install
	vagrant up server-1 --provision
	vagrant up client-1 client-2 --provision
	vagrant ssh server-1 -c "/vagrant/nomad server members"

deploy: nomad registry ingress

ingress:
	@./nomad job run ./ingress.hcl

echo:
	@./nomad job run ./echo.hcl && \
	until [ "$$(dig -t ANY echo.service.apps.internal +short)" != "" ]; do echo "Waiting for DNS"; sleep 10; done;
	dig -t ANY echo.service.apps.internal +short

registry:
	@./nomad job run ./container-registry.hcl && \
	until [ "$$(dig -t ANY registry.service.apps.internal +short)" != "" ]; do echo "Waiting for DNS"; sleep 10; done;
	dig -t ANY registry.service.apps.internal +short

registry-dns:
	dig -t ANY registry.service.apps.internal +short

build-test-app:
	cd test-app && \
	pack build test-app \
		--builder paketobuildpacks/builder:full \
		--buildpack "gcr.io/paketo-buildpacks/nginx" \
		--path "/vagrant/test-app/" \
		--env BP_NGINX_VERSION="1.20.0" \
		--tag "registry.service.apps.internal:5000/containers/test-app" && \
	docker push registry.service.apps.internal:5000/containers/test-app:latest

build-php-app:
	cd php-app && \
	pack build php-app \
		--builder paketobuildpacks/builder:full \
		--buildpack "gcr.io/paketo-buildpacks/php" \
		--path "/vagrant/php-app/" \
		--tag "registry.service.apps.internal:5000/containers/php-app" && \
	docker push registry.service.apps.internal:5000/containers/php-app:latest

run-test-app:
	./nomad job run ./test-app.hcl

curl-test-app:
	curl -v test-app.service.apps.internal:8080

run-php-app:
	@./nomad job run ./php-app.hcl && \
	until [ "$$(dig -t ANY php-app.service.apps.internal +short)" != "" ]; do echo "Waiting for DNS"; sleep 10; done;
	@echo "Fetching DNS entries"
	@dig -t ANY php-app.service.apps.internal +short

php-app-dns:
	dig -t ANY php-app.service.apps.internal +short

curl-php-app:
	@curl php-app.service.apps.internal
	@curl php-app.service.apps.internal
	@curl php-app.service.apps.internal
	@curl php-app.service.apps.internal
	@curl php-app.service.apps.internal
