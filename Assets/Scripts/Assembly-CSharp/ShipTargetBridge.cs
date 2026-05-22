using UnityEngine;

public class ShipTargetBridge : MonoBehaviour
{
    [Header("Datos del Barco a Mostrar")]
    public ShipData targetShipData;
    
    [Header("Referencia al Controlador UI")]
    public ShipARController arController;

    private GameObject _instantiatedModel;

    // Conecta este método al "On Target Found" del DefaultObserverEventHandler de Vuforia
    public void OnTargetDetected()
    {
        if (targetShipData == null || arController == null) return;

        // Si el modelo no está instanciado, lo creamos
        if (_instantiatedModel == null && targetShipData.shipPrefab != null)
        {
            _instantiatedModel = Instantiate(targetShipData.shipPrefab, transform);
            _instantiatedModel.transform.localPosition = Vector3.zero;
            _instantiatedModel.transform.localScale = Vector3.one * targetShipData.scaleMultiplier;
        }

        // Le pasamos los datos al controlador de UI para iniciar el escaneo
        arController.PrepareShipData(targetShipData, _instantiatedModel);
    }

    // Conecta este método al "On Target Lost" de Vuforia
    public void OnTargetLost()
    {
        if (arController != null)
        {
            arController.ResetScanStatus();
        }

        // Destruir el modelo 3D cuando desaparece el target
        if (_instantiatedModel != null)
        {
            Destroy(_instantiatedModel);
            _instantiatedModel = null;
        }
    }
}