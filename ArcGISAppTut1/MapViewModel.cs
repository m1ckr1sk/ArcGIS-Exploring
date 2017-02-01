using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Portal;

namespace ArcGISAppTut1
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    public class MapViewModel : INotifyPropertyChanged
    {

        public MapViewModel()
        {

        }

        private string[] _basemapTypes = new string[]
        {
            "Topographic",
            "Topographic Vector",
            "Streets",
            "Streets Vector",
            "Imagery",
            "Oceans",
            "USGS National Map",
            "World Globe 1812"
        };

        // Read-only property to return the available basemap names
       

        private Map _map = new Map(Basemap.CreateImagery());

        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Map Map
        {
            get { return _map; }
            set { _map = value; OnPropertyChanged(); }
        }

        public string[] BasemapChoices
        {
            get { return _basemapTypes; }
        }

        /// <summary>
        /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var propertyChangedHandler = PropertyChanged;
            if (propertyChangedHandler != null)
                propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async void ChangeBasemap(string basemap)
        {
            // Apply the selected basemap to the map
            switch (basemap)
            {
                case "Topographic":
                    // Set the basemap to Topographic
                    _map.Basemap = Basemap.CreateTopographic();
                    break;
                case "Topographic Vector":
                    // Set the basemap to Topographic (vector)
                    _map.Basemap = Basemap.CreateTopographicVector();
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    _map.Basemap = Basemap.CreateStreets();
                    break;
                case "Streets Vector":
                    // Set the basemap to Streets (vector)
                    _map.Basemap = Basemap.CreateStreetsVector();
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    _map.Basemap = Basemap.CreateImagery();
                    break;
                case "Oceans":
                    // Set the basemap to Oceans
                    _map.Basemap = Basemap.CreateOceans();
                    break;
                case "USGS National Map":
                    // Set the basemap to USGS National Map by opening it from ArcGIS Online
                    var itemID = "809d37b42ca340a48def914df43e2c31";

                    // Connect to ArcGIS Online
                    ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync();

                    // Get the USGS webmap item
                    PortalItem usgsMapItem = await PortalItem.CreateAsync(agsOnline, itemID);

                    // Create a new basemap using the item
                    _map.Basemap = new Basemap(usgsMapItem);
                    break;
                case "World Globe 1812":
                    // Create a URI that points to a map service to use in the basemap
                    var uri = new Uri("https://tiles.arcgis.com/tiles/IEuSomXfi6iB7a25/arcgis/rest/services/World_Globe_1812/MapServer");

                    // Create an ArcGISTiledLayer from the URI
                    ArcGISTiledLayer baseLayer = new ArcGISTiledLayer(uri);

                    // Create a basemap from the layer and assign it to the map
                    _map.Basemap = new Basemap(baseLayer);
                    break;
            }
        }
    }
}