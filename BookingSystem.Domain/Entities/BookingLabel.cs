namespace BookingSystem.Domain.Entities;

/// <summary>
/// Цветовая метка-статус записи (из диалога «Редактирование записи»).
/// </summary>
public enum BookingLabel
{
    /// <summary>Нет — без статуса (белый).</summary>
    None = 0,

    /// <summary>Подтверждено (зелёный).</summary>
    Confirmed = 1,

    /// <summary>Необходимо подтверждение (оранжевый).</summary>
    NeedsConfirmation = 2,

    /// <summary>Не явился (красный).</summary>
    NoShow = 3,

    /// <summary>Сотрудник (розовый).</summary>
    Staff = 4,

    /// <summary>Интернет (голубой).</summary>
    Internet = 5
}
