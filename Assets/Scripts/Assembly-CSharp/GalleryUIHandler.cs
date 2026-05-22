using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class GalleryUIHandler : MonoBehaviour
{
    public UIDocument uiDocument;
    public AudioSource audioSource;
    
    [Header("Base de Datos de Barcos")]
    public List<ShipData> shipDatabase;

    // Referencias de Contenedores Principales
    private VisualElement _galleryScreen;
    private ScrollView _galleryScrollView; // Lista de Barcos
    private ScrollView _podcastScroll;     // Lista de Podcasts
    private Button _btnGallery;
    private Button _btnCloseGallery;

    // Referencias de Pestañas (Tabs)
    private Button _tabShips;
    private Button _tabPodcasts;

    // Referencias de la Ventana de Detalles (Pop-up)
    private VisualElement _detailsView;
    private Label _detailsTitle;
    private Label _detailsText;
    private VisualElement _detailsImage;
    private Button _btnCloseDetails;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        // 1. Vincular Referencias de Galería
        _galleryScreen = root.Q<VisualElement>("GalleryScreen");
        _galleryScrollView = root.Q<ScrollView>("GalleryScroll");
        _podcastScroll = root.Q<ScrollView>("PodcastScroll");
        _btnGallery = root.Q<Button>("BtnGallery");
        _btnCloseGallery = root.Q<Button>("BtnCloseGallery");

        // 2. Vincular Pestañas
        _tabShips = root.Q<Button>("TabShips");
        _tabPodcasts = root.Q<Button>("TabPodcasts");

        // 3. Vincular Ventana de Detalles
        _detailsView = root.Q<VisualElement>("ShipDetailsView"); 
        _detailsTitle = root.Q<Label>("DetailsTitle");
        _detailsText = root.Q<Label>("DetailsFullText");
        _detailsImage = root.Q<VisualElement>("DetailsImage");
        _btnCloseDetails = root.Q<Button>("BtnCloseDetails");

        // --- EVENTOS ---

        // Abrir/Cerrar Galería
        if (_btnGallery != null) _btnGallery.clicked += ShowGallery;
        if (_btnCloseGallery != null) _btnCloseGallery.clicked += HideGallery;
        
        // Control de Pestañas
        if (_tabShips != null) _tabShips.clicked += () => SwitchTab(true);
        if (_tabPodcasts != null) _tabPodcasts.clicked += () => SwitchTab(false);

        // Cerrar detalles
        if (_btnCloseDetails != null) _btnCloseDetails.clicked += () => _detailsView.style.display = DisplayStyle.None;

        // Inicializar listas
        PopulateGallery();
        PopulatePodcasts();
    }

    // Llena la lista de Barcos (Flota)
    private void PopulateGallery()
    {
        if (_galleryScrollView == null || shipDatabase == null) return;
        _galleryScrollView.Clear();

        foreach (var ship in shipDatabase)
        {
            VisualElement card = CreateBaseCard(ship);
            // Al hacer clic, abrimos los detalles históricos
            card.RegisterCallback<ClickEvent>(ev => ShowShipFullDetails(ship));
            _galleryScrollView.Add(card);
        }
    }

    // Llena la lista de Podcasts (Relatos)
    private void PopulatePodcasts()
    {
        if (_podcastScroll == null || shipDatabase == null) return;
        _podcastScroll.Clear();

        foreach (var ship in shipDatabase)
        {
            VisualElement card = CreateBaseCard(ship);
            
            // Creamos un contenedor para los controles
            VisualElement controls = new VisualElement();
            controls.AddToClassList("podcast-controls");

            // Etiqueta de estado/botón
            Label statusLabel = new Label("▶ REPRODUCIR RELATO");
            statusLabel.name = "StatusLabel_" + ship.shipName; // Nombre único para buscarlo luego
            statusLabel.style.color = new Color(0.77f, 0.63f, 0.35f);
            statusLabel.AddToClassList("podcast-status-label");

            controls.Add(statusLabel);
            card.Add(controls);

            card.RegisterCallback<ClickEvent>(ev => {
                HandlePodcastPlayback(ship, statusLabel);
            });

            _podcastScroll.Add(card);
        }
    }

private void HandlePodcastPlayback(ShipData ship, Label currentLabel)
{
    if (ship.podcastClip == null || audioSource == null) return;

    // CASO 1: Es el mismo audio que ya está puesto
    if (audioSource.clip == ship.podcastClip)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            currentLabel.text = "▶ REANUDAR";
            currentLabel.style.color = new Color(0.77f, 0.63f, 0.35f);
        }
        else
        {
            audioSource.UnPause();
            currentLabel.text = "⏸ PAUSAR";
            currentLabel.style.color = Color.white;
        }
    }
    // CASO 2: Es un audio nuevo o no había nada sonando
    else
    {
        // Limpiamos visualmente todas las etiquetas antes de empezar el nuevo
        ResetAllPodcastLabels();

        audioSource.Stop();
        audioSource.clip = ship.podcastClip;
        audioSource.Play();
        
        currentLabel.text = "⏸ PAUSANDO..."; 
        currentLabel.text = "⏸ ESCUCHANDO";
        currentLabel.style.color = Color.white;
    }
}

// Método para que cuando des PLAY a uno, todos los demás vuelvan a decir "REPRODUCIR"
private void ResetAllPodcastLabels()
{
    _podcastScroll.Query<Label>().ForEach(lbl => {
        if (lbl.text.Contains("RELATO") || lbl.text.Contains("ESCUCHANDO") || lbl.text.Contains("REANUDAR"))
        {
            lbl.text = "▶ REPRODUCIR RELATO";
            lbl.style.color = new Color(0.77f, 0.63f, 0.35f);
        }
    });
}

    // Método auxiliar para crear la base de las tarjetas
    private VisualElement CreateBaseCard(ShipData ship)
    {
        VisualElement card = new VisualElement();
        card.AddToClassList("gallery-card");
        
        // Contenedor para icono y título (en fila)
        VisualElement headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.style.alignItems = Align.Center;

        if (ship.shipIcon != null)
        {
            VisualElement icon = new VisualElement();
            icon.style.backgroundImage = new StyleBackground(ship.shipIcon);
            icon.AddToClassList("gallery-icon");
            headerRow.Add(icon);
        }

        Label nameLabel = new Label(ship.shipName);
        nameLabel.AddToClassList("gallery-title");
        headerRow.Add(nameLabel);
        
        card.Add(headerRow);

        return card;
    }

    // Lógica para alternar entre las dos listas
    private void SwitchTab(bool isShipsTab)
    {
        _galleryScrollView.style.display = isShipsTab ? DisplayStyle.Flex : DisplayStyle.None;
        _podcastScroll.style.display = isShipsTab ? DisplayStyle.None : DisplayStyle.Flex;

        // Estilos visuales de los botones de pestaña
        if (isShipsTab)
        {
            _tabShips.AddToClassList("tab-active");
            _tabPodcasts.RemoveFromClassList("tab-active");
        }
        else
        {
            _tabPodcasts.AddToClassList("tab-active");
            _tabShips.RemoveFromClassList("tab-active");
        }
    }

    private void ShowShipFullDetails(ShipData ship)
    {
        if (_detailsView == null) return;

        _detailsTitle.text = ship.shipName;
        _detailsText.text = ship.shipDescription; 
        if (_detailsImage != null && ship.shipIcon != null) 
            _detailsImage.style.backgroundImage = new StyleBackground(ship.shipIcon);

        _detailsView.style.display = DisplayStyle.Flex;
    }

    private void ShowGallery()
    {
        if (_galleryScreen != null) _galleryScreen.style.display = DisplayStyle.Flex;
        if (_detailsView != null) _detailsView.style.display = DisplayStyle.None;
        SwitchTab(true); // Siempre abrir en la pestaña de barcos por defecto
    }

    private void HideGallery()
    {
        if (_galleryScreen != null) _galleryScreen.style.display = DisplayStyle.None;
    }

    
}