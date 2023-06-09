﻿using Microsoft.AspNetCore.Http;
using Proj3.Contracts.NGO.Request;
using Proj3.Contracts.NGO.Response;

namespace Proj3.Application.Common.Interfaces.Services.NGO.Commands
{
    public interface IEventCommandService
    {
        Task<NewEventResponse> AddAsync(HttpContext httpContext, NewEventRequest newEventRequest);

        Task<UpdatedEventResponse> UpdateAsync(HttpContext httpContext, Guid eventId, UpdateEventRequest updateEventRequest);

        Task CancelAsync(HttpContext httpContext, Guid eventId);
    }
}
