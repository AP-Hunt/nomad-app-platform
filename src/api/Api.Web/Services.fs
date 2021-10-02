namespace Api.Web.Services

open Api.Config
open Api.Domain.Stores
open ServiceStack.Messaging

type Services = {
    AppStore: IApplicationStore
    Configuration: Configuration
    Logger : Api.Config.Logging.Logger
    MessageQueue: IMessageService
}

