.PHONY: services
services: redis docker_registry postgres config

.PHONY: stop_services
stop_services: stop_redis stop_docker_registry stop_postgres

.PHONY: redis
redis:
	docker run -d \
		--rm \
		--name nomad-app-platform.api.redis \
		-p 6379:6379 \
		redis:6
		
.PHONY: stop_redis
stop_redis:
	docker kill nomad-app-platform.api.redis

.PHONY: docker_registry	
docker_registry:
	docker run -d \
		--name nomad-app-platform.api.docker-registry \
		--rm \
		-p 6000:5000 \
		registry:2
		
.PHONY: stop_docker_registry
stop_docker_registry:
	docker kill nomad-app-platform.api.docker-registry
	
.PHONY: postgres
postgres:
	docker run -d \
		--rm \
		--name nomad-app-platform.api.postgres \
		-e POSTGRES_PASSWORD=nomad-app-platform \
		-e POSTGRES_DB=api \
		-e POSTGRES_USER=api \
		-p 5432:5432 \
		postgres:13
		
.PHONY: stop_postgres
stop_postgres:
	docker kill nomad-app-platform.api.postgres
	
.PHONY: config
config:
	./scripts/generate-config.sh

dev:
	$(eval export TARGET_REGISTRY ?= 192.168.33.1:6000)
	$(eval export IMAGE_TAG=$(shell date +%s))

web_container:
	@docker build \
	  -f ./Web.Dockerfile \
	  -t "${TARGET_REGISTRY}/nomad-app-platform/web:${IMAGE_TAG}" \
	  . > web.docker.build.log && \
	docker push "${TARGET_REGISTRY}/nomad-app-platform/web:${IMAGE_TAG}" > web.docker.push.log
	@echo "Published container ${TARGET_REGISTRY}/nomad-app-platform/web:${IMAGE_TAG}"

worker_container:
	@docker build \
	  -f ./Worker.Dockerfile \
	  -t "${TARGET_REGISTRY}/nomad-app-platform/worker:${IMAGE_TAG}" \
	  . > worker.docker.build.log && \
	docker push "${TARGET_REGISTRY}/nomad-app-platform/worker:${IMAGE_TAG}" > worker.docker.push.log
	@echo "Published container ${TARGET_REGISTRY}/nomad-app-platform/worker:${IMAGE_TAG}"