using BookingSystem.Domain.Entities;

namespace BookingSystem.Shared;

/// <summary>
/// Единый справочник меток-статусов: русское название и цвет (hex).
/// Используется и сервером (заполнить LabelName/Color), и UI (легенда, дропдаун, раскраска).
/// </summary>
public static class BookingLabelInfo
{
    public record LabelMeta(BookingLabel Label, string Name, string Color);

    public static readonly IReadOnlyList<LabelMeta> All = new List<LabelMeta>
    {
        new(BookingLabel.None,              "Нет",                       "#E3E8EF"),
        new(BookingLabel.Confirmed,         "Подтверждено",              "#A8D5A2"),
        new(BookingLabel.NeedsConfirmation, "Необходимо подтверждение",  "#F2C078"),
        new(BookingLabel.NoShow,            "Не явился",                 "#E98A8A"),
        new(BookingLabel.Staff,             "Сотрудник",                 "#E3A0CF"),
        new(BookingLabel.Internet,          "Интернет",                  "#88CCD8"),
    };

    public static string Name(BookingLabel label) =>
        All.FirstOrDefault(x => x.Label == label)?.Name ?? "Нет";

    public static string Color(BookingLabel label) =>
        All.FirstOrDefault(x => x.Label == label)?.Color ?? "#E3E8EF";
}
