﻿using Microsoft.EntityFrameworkCore;
using Proj3.Application.Common.Interfaces.Persistence;
using Proj3.Application.Common.Interfaces.Persistence.NGO;
using Proj3.Application.Common.Interfaces.Services;
using Proj3.Domain.Entities.Common;
using Proj3.Domain.Entities.NGO;

namespace Proj3.Infrastructure.Persistence.Repositories.NGO
{
    public class EventRepository : IEventRepository
    {
        private readonly IRepositoryBase<Event> _repository;
        private readonly INgoRepository _ngoRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public EventRepository(IRepositoryBase<Event> repository, INgoRepository ngoRepository, IDateTimeProvider dateTimeProvider)
        {
            _repository = repository;
            _ngoRepository = ngoRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<List<Event>> GetAllFeedAsync(Guid userId, int daysToEvent, List<int> categories)
        {
            var ngos = await _ngoRepository.GetByCategoriesAsync(categories);
            List<Event> events = new(); 
            
            foreach(Ngo ngo in ngos)
            {
                var ngoEvents = await GetUpcomingEventsByNgoAsync(ngo.Id).ToListAsync();
                events.AddRange(ngoEvents);
            }

            return events;
        }

        public IAsyncEnumerable<Event> GetUpcomingEventsByNgoAsync(Guid ngoId)
        {
            return _repository.Entity.Where(
                e => e.NgoId == ngoId                
                && e.StartDate > DateTime.Now
                && e.Cancelled == false
            ).AsAsyncEnumerable();
        }

        public IAsyncEnumerable<Event> GetActiveEventsByNgoAsync(Guid ngoId)
        {
            return _repository.Entity.Where(
                e => e.NgoId == ngoId 
                && e.EndDate > DateTime.Now 
                && e.StartDate < DateTime.Now
                && e.Cancelled == false
            ).AsAsyncEnumerable();
        }

        public IAsyncEnumerable<Event> GetEndedEventsByNgoAsync(Guid ngoId)
        {
            return _repository.Entity.Where(
                e => e.NgoId == ngoId 
                && e.EndDate < DateTime.Now 
                && e.Cancelled == false
            ).AsAsyncEnumerable();            
        }

        public IAsyncEnumerable<Event> GetAllByNgoAsync(Guid ngoId)
        {
            return _repository.Entity.Where(e => e.NgoId == ngoId && e.Cancelled == false).AsAsyncEnumerable();
        }

        public async Task<List<Event>> GetUpcomingEventsByVolunteerAsync(Guid volunteerId)
        {
            List<Event> upcomingEvents = new();
            AppDbContext context = (AppDbContext)_repository.Context;            

            var events = await context.EventVolunteers!.Where(e => e.VolunteerId == volunteerId && e.Accepted == true).ToListAsync();            

            foreach(EventVolunteer @eventVolunteer in events)
            {
                Event @event = await context.Events!.Where(e => e.Id == @eventVolunteer.EventId).FirstAsync();

                if (@event.StartDate > DateTime.Now && @event.Cancelled == false)
                {
                    upcomingEvents.Add(@event);
                }                
            }

            return upcomingEvents;
        }

        public async Task<List<Event>> GetActiveEventsByVolunteerAsync(Guid volunteerId)
        {
            List<Event> activeEvents = new();
            AppDbContext context = (AppDbContext)_repository.Context;

            var events = await context.EventVolunteers!.Where(e => e.VolunteerId == volunteerId && e.Accepted == true).ToListAsync();

            foreach (EventVolunteer @eventVolunteer in events)
            {
                Event @event = await context.Events!.Where(e => e.Id == @eventVolunteer.EventId).FirstAsync();

                if (@event.StartDate < DateTime.Now && @event.EndDate > DateTime.Now && @event.Cancelled == false)
                {
                    activeEvents.Add(@event);
                }
            }

            return activeEvents;
        }

        public async Task<List<Event>> GetEndedEventsByVolunteerAsync(Guid volunteerId)
        {
            List<Event> endedEvents = new();
            AppDbContext context = (AppDbContext)_repository.Context;

            var events = await context.EventVolunteers!.Where(e => e.VolunteerId == volunteerId && e.Accepted == true).ToListAsync();

            foreach (EventVolunteer @eventVolunteer in events)
            {
                Event @event = await context.Events!.Where(e => e.Id == @eventVolunteer.EventId).FirstAsync();

                if (@event.EndDate < DateTime.Now && @event.Cancelled == false)
                {
                    endedEvents.Add(@event);
                }
            }

            return endedEvents;
        }

        public async Task<Event?> GetEventByIdAsync(Guid eventId)
        {
            return await _repository.GetByIdAsync(eventId);
        }

        public async Task<Event> AddAsync(Event @event)
        {
            return await _repository.AddAsync(@event);
        }

        public async Task<bool> UpdateAsync(Event @event)
        {
            return await _repository.UpdateAsync(@event);
        }

        public async Task CancelEventAsync(Guid eventId)
        {
            Event @event = (await _repository.GetByIdAsync(eventId))!;
            @event.Cancelled = true;
            await _repository.UpdateAsync(@event);
        }
    }
}
