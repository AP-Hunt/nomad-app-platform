namespace Api.Web.Services

open Api.Config
open Api.Domain.Stores
open ServiceStack.Messaging

type Services = {
    AppStore: IApplicationStore
    Configuration: Configuration
    MessageQueue: IMessageService
}

