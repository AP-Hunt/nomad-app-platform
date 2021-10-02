.PHONY: services
services: redis docker_registry

.PHONY: stop_services
stop_services: stop_redis stop_docker_registry

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
		-p 6000:5000 \
		--restart=always \
		registry:2
		
.PHONY: stop_docker_registry
stop_docker_registry:
	docker kill nomad-app-platform.api.docker-registry
	docker rm nomad-app-platform.api.docker-registry
	
.PHONY: config
config:
	./scripts/generate-config.sh