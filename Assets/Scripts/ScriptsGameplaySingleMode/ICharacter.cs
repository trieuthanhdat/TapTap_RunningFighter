using System.Drawing;

public interface ICharacter {
    void NormalRun();
    void HandleAction();
    void SetPlayerColor(GameSpawner.PLAYER_COLOR color);
    float CurrentPercentageStamina();

}