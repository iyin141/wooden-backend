namespace WoodLounge.Application.Visits;

public interface IVisitsService
{
    Task<List<VisitAvailabilityResponse>> GetAvailabilityAsync(int daysAhead = 14);
    Task<BookVisitResponse> BookVisitAsync(BookVisitRequest request);
}
