using BookingSystem.Shared.Dtos;

namespace BookingSystem.Shared;

public class BookingConflictException : Exception
{
    public IReadOnlyList<BookingConflictDto> Conflicts { get; }

    public BookingConflictException(IReadOnlyList<BookingConflictDto> conflicts)
        : base("На выбранное время комната уже занята.")
        => Conflicts = conflicts;
}
