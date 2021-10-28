#!/usr/bin/env bash

mkdir -p storage/blob/source-bundles
mkdir -p storage/blob/terraform-state
mkdir -p storage/logs/

cat << EOF > storage/config.json
{
    "BlobStore": {
        "SourceBundleStoragePath": "$(pwd)/storage/blob/source-bundles",
        "TerraformStatePath": "$(pwd)/storage/blob/terraform-state"
    },
    "Database": {
        "ConnectionString": "Host=127.0.0.1;Port=5432;Database=api;Username=api;Password=nomad-app-platform;SslMode=Disable;"
    },
    "DockerRegistry": {
        "RegistryAddress": "localhost:6000"
    },
    "Logging": {
        "LogPath": "$(pwd)/storage/logs",
        "LogToStdOut": false
    },
    "Nomad": {
        "ApiAddress": "http://192.168.33.10:4646",
        "DockerRegistry": {
            "RegistryAddress": "192.168.33.1:6000"
        }
    },
    "MessageQueue": {
        "RedisAddress": "localhost:6379",
        "RetryCount": 2
    }
}
EOF

echo "Written config to $(pwd)/storage/config.json"
