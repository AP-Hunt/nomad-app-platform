module PathHelpers

open Suave.Filters
let trailingSlashPath str = pathRegex $"{str}/?" 
        

