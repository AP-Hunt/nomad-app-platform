.PHONY: services
services: redis

.PHONY: stop_services
stop_services: stop_redis

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