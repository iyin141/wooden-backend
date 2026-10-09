namespace WoodLounge.Application.Visits;

public class VisitsService : IVisitsService
{
    public Task<List<VisitAvailabilityResponse>> GetAvailabilityAsync(int daysAhead = 14)
    {
        var availability = new List<VisitAvailabilityResponse>();
        var today = DateTime.UtcNow.Date;

        for (int i = 0; i < daysAhead; i++)
        {
            var date = today.AddDays(i);
            var slots = new List<TimeSlot>
            {
                new TimeSlot("10:00", Random.Shared.Next(0, 2) == 0),
                new TimeSlot("11:00", Random.Shared.Next(0, 2) == 0),
                new TimeSlot("12:00", Random.Shared.Next(0, 2) == 0),
                new TimeSlot("14:00", Random.Shared.Next(0, 2) == 0),
                new TimeSlot("15:00", Random.Shared.Next(0, 2) == 0),
                new TimeSlot("16:00", Random.Shared.Next(0, 2) == 0),
            };

            availability.Add(new VisitAvailabilityResponse(date, slots));
        }

        return Task.FromResult(availability);
    }

    public Task<BookVisitResponse> BookVisitAsync(BookVisitRequest request)
    {
        var reference = $"VISIT{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        return Task.FromResult(new BookVisitResponse(
            reference,
            request.Date,
            request.Time
        ));
    }
}
