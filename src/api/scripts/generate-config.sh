#!/usr/bin/env bash

mkdir -p storage/blob/
mkdir -p storage/logs/

cat << EOF > storage/config.json
{
    "BlobStore": {
        "SourceBundleStoragePath": "$(pwd)/storage/blob/"
    },
    "DockerRegistry": {
        "RegistryAddress": "localhost:6000"
    },
    "Logging": {
        "LogPath": "$(pwd)/storage/logs"
    },
    "MessageQueue": {
        "RedisAddress": "localhost:6379",
        "RetryCount": 2
    }
}
EOF

echo "Written config to $(pwd)/storage/config.json"