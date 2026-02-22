namespace RSVPApp.Models;

public record ApiUserRegisterRequest(
    string Name,
    string Email,
    string Password,
    string MobilePhone
);

public record ApiUserResponse(
    string Id,
    string Name,
    string Email,
    string MobilePhone
);

public record ApiEventCreateRequest(
    string HostUserId,
    string HostName,
    string EventName,
    string EventAddress,
    int MaxAllowedAttendees,
    DateTime EventDateTimeUtc,
    DateTime RsvpDeadlineUtc
);

public record ApiEventSummary(
    string Id,
    string HostUserId,
    string HostName,
    string EventName,
    string EventAddress,
    int MaxAllowedAttendees,
    int CurrentAttendeeCount,
    DateTime EventDateTimeUtc,
    DateTime RsvpDeadlineUtc
);

public record ApiEventDetails(
    string Id,
    string HostUserId,
    string HostName,
    string EventName,
    string EventAddress,
    int MaxAllowedAttendees,
    int CurrentAttendeeCount,
    DateTime EventDateTimeUtc,
    DateTime RsvpDeadlineUtc,
    List<string> AttendeeNames
);