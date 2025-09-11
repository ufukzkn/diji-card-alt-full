namespace diji_card_alt.Models;

// Safe DTO returned to clients (no password / private access password)
public record UserDto(
    string UserId,
    string FullName,
    string PhoneNumber,
    string Email,
    string JobTitle,
    string Company,
    bool IsPublic,
    string? ProfilePhotoUrl
);
