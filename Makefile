BIN_DIR		= bin
NOMAD_EXE 	?= $(BIN_DIR)/nomad
CONSUL_EXE 	= $(BIN_DIR)/consul
ETCD_EXE 	= $(BIN_DIR)/etcd

install: $(NOMAD_EXE) $(CONSUL_EXE) $(ETCD_EXE)

inside_vagrant_vm:
	# This isn't a perfect test, but it'll do
	[ -d "/home/vagrant" ] || (echo "Must be inside the Vagrant VM before running " && false)

$(NOMAD_EXE): inside_vagrant_vm
	[ -d $(BIN_DIR) ] || mkdir -p $(BIN_DIR)
	wget --quiet -O bin/nomad.zip "https://releases.hashicorp.com/nomad/1.1.2/nomad_1.1.2_linux_amd64.zip"
	unzip bin/nomad.zip -d bin/
	rm bin/nomad.zip

$(CONSUL_EXE): inside_vagrant_vm
	[ -d $(BIN_DIR) ] || mkdir -p $(BIN_DIR)
	wget --quiet -O bin/consul.zip "https://releases.hashicorp.com/consul/1.10.1/consul_1.10.1_linux_amd64.zip"
	unzip bin/consul.zip -d bin/
	rm bin/consul.zip

$(ETCD_EXE): inside_vagrant_vm
	[ -d $(BIN_DIR) ] || mkdir -p $(BIN_DIR)
	wget --quiet -O bin/etcd.tar.gz "https://github.com/etcd-io/etcd/releases/download/v3.5.0/etcd-v3.5.0-linux-amd64.tar.gz"
	tar -xzvf bin/etcd.tar.gz -C bin/ --strip-components=1 \
	  etcd-v3.5.0-linux-amd64/etcdutl \
	  etcd-v3.5.0-linux-amd64/etcdctl \
	  etcd-v3.5.0-linux-amd64/etcd
	rm bin/etcd.tar.gz

start: vagrant_up platform_up acceptance_test

vagrant_up:
	vagrant up --provision

platform_up:
	cd config/platform && \
	terraform init && \
	terraform apply -auto-approve -var nomad_addr=http://192.168.33.10:4646

acceptance_test:
	@ for node in "node-1" "node-2" "node-3"; do \
		vagrant ssh "$${node}" -c "cd /vagrant && make node_test"; \
	done; \
	cd tests/acceptance && \
	ginkgo -p -v platform_test/

node_test:
	@go install github.com/onsi/ginkgo/ginkgo@latest && \
	cd tests/acceptance && \
	echo "Running tests on $$(hostname)" && \
	CGO_ENABLED=0 ginkgo -p -v internal_test/

teardown:
	vagrant destroy -f

rebuild: teardown start

deploy: nomad registry ingress

ingress:
	@./bin/nomad job run ./ingress.hcl

echo:
	@./bin/nomad job run ./echo.hcl && \
	until [ "$$(dig -t ANY echo.service.apps.internal +short)" != "" ]; do echo "Waiting for DNS"; sleep 10; done;
	dig -t ANY echo.service.apps.internal +short

registry:
	@./bin/nomad job run ./container-registry.hcl && \
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
	./bin/nomad job run ./test-app.hcl

curl-test-app:
	curl -v test-app.service.apps.internal:8080

run-php-app:
	@./bin/nomad job run ./php-app.hcl && \
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
