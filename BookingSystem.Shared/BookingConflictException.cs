using BookingSystem.Shared.Dtos;

namespace BookingSystem.Shared;

/// <summary>
/// Бросается, когда бронь пересекается по времени с уже существующей в той же комнате.
/// Контроллер превращает её в ответ 409 Conflict со списком пересечений.
/// </summary>
public class BookingConflictException : Exception
{
    public IReadOnlyList<BookingConflictDto> Conflicts { get; }

    public BookingConflictException(IReadOnlyList<BookingConflictDto> conflicts)
        : base("На выбранное время комната уже занята.")
        => Conflicts = conflicts;
}
