using RutaViva.Application.Abstractions.Messaging;

namespace RutaViva.Application.Commands.RunDailyReview;

public sealed record RunDailyReviewCommand(DateOnly? Date) : ICommand<IReadOnlyList<RoutineChange>>;
