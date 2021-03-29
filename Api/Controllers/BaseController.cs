namespace Api.Controllers
{
    using System.Diagnostics;
    using Core.Entities;
    using Microsoft.AspNetCore.Mvc;

    public abstract class BaseController<T> : ControllerBase
        where T : class
    {
        protected QueueMessage<T> BuildQueueMessage(T request)
        {
            return new QueueMessage<T>
            {
                TraceId = Activity.Current.Id,
                Entity = request
            };
        }
    }
}