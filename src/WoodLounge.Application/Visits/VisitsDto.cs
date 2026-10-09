namespace WoodLounge.Application.Visits;

public record VisitAvailabilityResponse(
    DateTime Date,
    List<TimeSlot> AvailableSlots
);

public record TimeSlot(
    string Time,
    bool Available
);

public record BookVisitRequest(
    DateTime Date,
    string Time,
    string CustomerName,
    string CustomerPhone,
    string CustomerEmail,
    string? Notes
);

public record BookVisitResponse(
    string Reference,
    DateTime Date,
    string Time
);
