using UnityEngine;

[CreateAssetMenu(fileName = "NewShipData", menuName = "AR Ships/Ship Data")]
public class ShipData : ScriptableObject
{
    [Header("Información Principal")]
    public string shipName;

    public AudioClip podcastClip;
    public int constructionYear;
    [TextArea(3, 10)]
    public string shipDescription;

    [Header("Recursos Visuales")]
    public Texture2D shipIcon;
    public GameObject shipPrefab; // El modelo 3D URP
    
    [Header("Configuración AR")]
    public float scaleMultiplier = 1.0f; // Por si algún barco es muy grande
    
}