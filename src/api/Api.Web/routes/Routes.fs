module Routes

open Falco.Core

let all services : HttpEndpoint list = [] @ (AppRoutes.all services)

