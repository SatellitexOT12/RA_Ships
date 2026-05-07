using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class PartSelector : MonoBehaviour
{
    [Header("Cámara AR")]
    public Camera arCamera;
    
    [Header("Capa de Partes Interactivas")]
    public LayerMask interactableLayer;

    [Header("Referencia a la UI")]
    public UIDocument uiDocument;

    [Header("Efecto Visual (Outline)")]
    public Material outlineMaterial; // Asigna aquí tu material brillante

    // Variables de control visual
    private Material _originalMaterial;
    private Renderer _lastRenderer;

    // Referencias a la UI
    private VisualElement _infoCard;
    private Label _titleLabel;
    private Label _descLabel;
    private Button _btnCloseCard;

    private void OnEnable()
    {
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            _infoCard = root.Q<VisualElement>("InfoCard");
            _titleLabel = root.Q<Label>("ShipName");
            _descLabel = root.Q<Label>("ShipDescription");
            _btnCloseCard = root.Q<Button>("BtnCloseCard");

            // Suscribir el botón de cerrar para limpiar el outline
            if (_btnCloseCard != null)
                _btnCloseCard.clicked += ClearSelection;
        }
    }

    void Update()
    {
        // Soporte para Mouse (Editor)
        if (Input.GetMouseButtonDown(0))
        {
            CheckTouch(Input.mousePosition);
        }
        
        // Soporte para Toque (Móvil)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            CheckTouch(Input.GetTouch(0).position);
        }
    }

    private void CheckTouch(Vector2 screenPosition)
    {
        // 1. Evitar que el clic pase si estamos tocando un botón de la UI
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        if (arCamera == null) arCamera = Camera.main;

        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, interactableLayer))
        {
            ShipPart part = hit.collider.GetComponent<ShipPart>();
            Renderer currentRenderer = hit.collider.GetComponent<Renderer>();

            if (part != null)
            {
                // Aplicar el resaltado visual
                if (currentRenderer != null) ApplyHighlight(currentRenderer);
                
                // Mostrar la información en la UI
                MostrarDetalleDeParte(part.partName, part.partDescription);
            }
        }
    }

    private void ApplyHighlight(Renderer newRenderer)
{
    if (_lastRenderer == newRenderer) return;

    ResetOutline();

    _lastRenderer = newRenderer;
    _originalMaterial = newRenderer.sharedMaterial;

    // Creamos una instancia del material de outline para este objeto específico
    Material highlightedMat = new Material(outlineMaterial);
    
    // IMPORTANTE: Le pasamos la textura original del barco al shader de outline
    if (_originalMaterial.HasProperty("_MainTex"))
    {
        highlightedMat.SetTexture("_MainTex", _originalMaterial.GetTexture("_MainTex"));
    }

    _lastRenderer.material = highlightedMat;
}

    private void MostrarDetalleDeParte(string nombre, string detalle)
    {
        if (_infoCard == null || _titleLabel == null || _descLabel == null) return;

        _titleLabel.text = nombre;
        _descLabel.text = detalle;
        _infoCard.style.display = DisplayStyle.Flex;
    }

    public void ClearSelection()
    {
        ResetOutline();
        if (_infoCard != null) _infoCard.style.display = DisplayStyle.None;
    }

    private void ResetOutline()
    {
        if (_lastRenderer != null && _originalMaterial != null)
        {
            _lastRenderer.material = _originalMaterial;
        }
        _lastRenderer = null;
        _originalMaterial = null;
    }
}