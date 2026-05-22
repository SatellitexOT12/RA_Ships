using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class ShipARController : MonoBehaviour
{
    public UIDocument uiDocument;

    private VisualElement _infoCard;
    private VisualElement _maskContainer; // Para la animación de escaneo
    private VisualElement _reticle;
    private Label _shipNameLabel;
    private Label _shipDescLabel;

    private ShipData _currentPendingData;
    private GameObject _currentPendingModel;
    private bool _isShipInView;
    private bool _isScanning;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        _infoCard = root.Q<VisualElement>("InfoCard");
        _maskContainer = root.Q<VisualElement>("MaskContainer");
        _reticle = root.Q<VisualElement>("Reticle");
        
        _shipNameLabel = root.Q<Label>("ShipName");
        _shipDescLabel = root.Q<Label>("ShipDescription");

		

        // Botón para cerrar la tarjeta si existe
        var btnClose = root.Q<Button>("BtnCloseCard");
        if (btnClose != null) btnClose.clicked += HideCard;

        HideCard();
        ShowReticle();
    }

     private void HideReticle()
    {
        if (_reticle != null)
            _reticle.style.display = DisplayStyle.None;
    }
    private void ShowReticle()
    {
        if (_reticle != null)
            _reticle.style.display = DisplayStyle.Flex;
    }

    public void PrepareShipData(ShipData data, GameObject model3d)
    {
        _currentPendingData = data;
        _currentPendingModel = model3d;
        _isShipInView = true;

        TryExecuteScan();
    }

    private void TryExecuteScan()
    {
        if (!_isShipInView || _isScanning || _currentPendingData == null) return;
        
        StartCoroutine(ScanRoutine());
    }

    private IEnumerator ScanRoutine()
    {
        _isScanning = true;

        if (_reticle != null) _reticle.AddToClassList("reticle-scanning");
        if (_maskContainer != null) _maskContainer.style.display = DisplayStyle.Flex;

        yield return new WaitForSeconds(0.8f);

        if (!_isShipInView) yield break;

        //ActualShowUI();
        
        // **OCULTAR RETÍCULA cuando el barco aparece**
        HideReticle();

        _isScanning = false;
        if (_reticle != null) _reticle.RemoveFromClassList("reticle-scanning");
        if (_maskContainer != null) _maskContainer.style.display = DisplayStyle.None;
    }



        public void ResetScanStatus()
    {
        _isShipInView = false;
        _isScanning = false;
        StopAllCoroutines();
        HideCard();
        
        // **VOLVER A MOSTRAR RETÍCULA al desaparecer el barco**
        ShowReticle();

        if (_reticle != null) _reticle.RemoveFromClassList("reticle-scanning");
        if (_maskContainer != null) _maskContainer.style.display = DisplayStyle.None;
        
        // Limpiar referencias del modelo
        _currentPendingModel = null;
        _currentPendingData = null;
    }

    public void HideCard()
    {
        if (_infoCard != null) _infoCard.style.display = DisplayStyle.None;
        // No ocultamos la retícula aquí porque la tarjeta y la retícula son independientes
    }
    public void OnShipTrackingLost()
    {
        ResetScanStatus(); // o al menos ShowReticle() y _isShipInView = false
    }

    // Por si necesitas reaccionar a cambios de pantalla (orientación)
    private void OnGeometryChange(GeometryChangedEvent evt)
    {
        // Lógica de redimensionamiento si es necesaria
    }
}