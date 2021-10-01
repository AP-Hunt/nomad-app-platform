#!/usr/bin/env bash

mkdir -p storage/blob/

cat << EOF > storage/config.json
{
    "DockerRegistry": {
        "RegistryAddress": "localhost:6000"
    },
    "MessageQueue": {
        "RedisAddress": "localhost:6379",
        "RetryCount": 2
    },
    "BlobStore": {
        "SourceBundleStoragePath": "$(pwd)/storage/blob/"
    }
}
EOF