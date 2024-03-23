using System.Drawing;

public interface ICharacter {
    void NormalRun();
    void HandleAction();
    void SetPlayerColor(GameplayManagerSingleMode.PLAYER_COLOR color);
    float CurrentPercentageStamina();

}