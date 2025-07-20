using System.Collections.Generic;

namespace diji_card_alt_full.Dtos;

public record UserProfileDto(string UserId, List<LinkDto> Links);
