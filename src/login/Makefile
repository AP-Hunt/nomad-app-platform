dev:
	$(eval export TARGET_REGISTRY ?= 192.168.33.1:6000)
	$(eval export IMAGE_TAG=$(shell date +%s))

container:
	@git clone -n https://github.com/ory/kratos-selfservice-ui-node.git \
	&& cd kratos-selfservice-ui-node \
	&& git checkout f93f8a6c1a101aaa441385eb7bb79c5dd28b611b \
	docker build \
	  -t "${TARGET_REGISTRY}/nomad-app-platform/login:${IMAGE_TAG}" \
	  -t "${TARGET_REGISTRY}/nomad-app-platform/login:latest" \
	  . > login.docker.build.log && \
	docker push "${TARGET_REGISTRY}/nomad-app-platform/login:${IMAGE_TAG}" > login.docker.push.log
	docker push "${TARGET_REGISTRY}/nomad-app-platform/login:latest" >> login.docker.push.log
