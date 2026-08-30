namespace Raphael.Driver.DTOs
{
    // ⚠️ Hand-written mirror of Raphael.Shared/DTOs/Routing/RoutingDtos.cs. Only the parts the
    // driver app uses — it prices legs and nothing else. If the backend file changes, this one
    // changes in the same slice.

    public static class RoutingContract
    {
        public static class Statuses
        {
            public const string Ok = "Ok";

            /// <summary>
            /// No answer for this leg. ⚠️ Keep the ETA already on the driver's screen; writing a
            /// zero would tell them they arrive the instant they leave.
            /// </summary>
            public const string Unavailable = "Unavailable";
        }
    }

    public class RouteLegRequestItemDto
    {
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }
        public double DestLat { get; set; }
        public double DestLng { get; set; }

        /// <summary>Service date and departure hour, in business wall-clock time.</summary>
        public DateTime? Date { get; set; }

        public TimeSpan? DepartureTime { get; set; }
    }

    public class RouteLegsRequestDto
    {
        public List<RouteLegRequestItemDto> Legs { get; set; } = new List<RouteLegRequestItemDto>();
    }

    public class RouteLegResultDto
    {
        public int DurationSeconds { get; set; }

        public int? DurationInTrafficSeconds { get; set; }

        public int DistanceMeters { get; set; }

        public double DistanceMiles { get; set; }

        /// <summary>Cache or Google: was anybody billed for this.</summary>
        /// <remarks>
        /// It used to be able to say "Buffered" as well, which made it unusable: in MaxSavings
        /// mode every answer read Buffered whether it had been bought or cached. That question is
        /// now <see cref="Buffered"/>, and this field means only what its name says.
        /// </remarks>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// True when the planning duration is the server's own free-flow-plus-margin figure
        /// rather than a traffic estimate from Google. Independent of <see cref="Source"/>.
        /// </summary>
        public bool Buffered { get; set; }

        public string Status { get; set; } = RoutingContract.Statuses.Ok;

        public bool IsUsable => Status == RoutingContract.Statuses.Ok;

        /// <summary>The duration to plan against, falling back to free-flow.</summary>
        public TimeSpan TravelTime =>
            TimeSpan.FromSeconds(DurationInTrafficSeconds ?? DurationSeconds);
    }

    public class RouteLegsResponseDto
    {
        public string TrafficMode { get; set; } = string.Empty;

        public List<RouteLegResultDto> Legs { get; set; } = new List<RouteLegResultDto>();
    }
}
