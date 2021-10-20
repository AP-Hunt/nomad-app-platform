namespace Api.Domain.Persistence

open System.Linq.Expressions
open Api.Domain.Applications
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Design
open Microsoft.FSharp.Linq.RuntimeHelpers
        
type Context (connectionString) =
    inherit DbContext()
    
    [<DefaultValue>]
    val mutable private _apps : DbSet<Application>
    
    let _connectionString : string = connectionString
    
    member this.Apps with get() = this._apps and set v = this._apps <- v
    
    member this.ApplyMigrations() =
        this.Database.Migrate()
    
    override this.OnConfiguring(optionsBuilder) =
        optionsBuilder.UseNpgsql(connectionString) |> ignore

    override this.OnModelCreating(modelBuilder) =
        let appEntity = modelBuilder.Entity<Application>()
        appEntity.HasKey(fun app -> (app.Id :> obj)) |> ignore
        appEntity.Property(fun app -> app.Name) |> ignore
        appEntity.Property(fun app -> app.Version) |> ignore
        
type ContextFactory () =
    
    interface IDesignTimeDbContextFactory<Context> with
        member this.CreateDbContext(args) =
            new Context("Server=127.0.0.1;Port=5432;Database=api;Userid=api;Password=password")