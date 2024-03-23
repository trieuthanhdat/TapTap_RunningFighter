using System.Drawing;

public interface ICharacter {
    void HandleAction();
    void SetPlayerColor(GameplayManagerSingleMode.PLAYER_COLOR color);
    float CurrentPercentageStamina();

}