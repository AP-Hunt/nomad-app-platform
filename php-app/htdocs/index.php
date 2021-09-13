<?php

echo "Hello from the PHP application";

$app_name = getenv("NOMAD_JOB_NAME");
echo "My application name is ".$app_name."<br />".PHP_EOL;

$port = getenv("NOMAD_PORT_http");
echo "I am listening on port ".$port."<br />".PHP_EOL;

$instance = getenv("NOMAD_ALLOC_INDEX");
echo "I am instance ".$instance."<br />".PHP_EOL;
