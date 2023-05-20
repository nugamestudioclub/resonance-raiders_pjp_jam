using UnityEngine;

[CreateAssetMenu (fileName = "New PlayerValues", menuName = "PlayerValues")]
public class PlayerValues : ScriptableObject
{
    public int playerHealth;

    public float destructionWaveCooldown;

    public float disruptionWaveCooldown;

    public enum waveType { Destruction, Disruption };

    public waveType playerWaveType;

    public Color destructionColor;
    public Color distruptionColor;
    public float gridSize = 2f;

}
