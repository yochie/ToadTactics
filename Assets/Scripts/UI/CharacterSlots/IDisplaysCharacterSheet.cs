using UnityEngine.EventSystems;

public interface IDisplaysCharacterSheet : IPointerClickHandler
{
    public IntGameEventSO OnCharacterSheetDisplayedEvent { get; set; }
}
