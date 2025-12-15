// IInteractable.cs
public interface IInteractable
{
    string Prompt { get; }
    void Interact(PlayerProximityInteractor interactor);

}
