using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcGISAppTut2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constants for OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        private const string AppClientId = "rcZBgF9nL5gosV7C";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private const string OAuthRedirectUrl = "https://developers.arcgis.com";

        public MainWindow()
        {
            InitializeComponent();

            // Get the view model (defined as a resource in the XAML)
            _mapViewModel = this.FindResource("MapViewModel") as MapViewModel;

            // Define a selection handler on the basemap list
            BasemapListBox.SelectionChanged += OnBasemapsClicked;

            // Define a handler for the Save Map click
            SaveMapButton.Click += OnSaveMapClick;

            // Define a handler for the New Map click
            NewMapButton.Click += OnNewMapClicked;

            UpdateAuthenticationManager();
        }

        // Map initialization logic is contained in MapViewModel.cs
        private MapViewModel _mapViewModel;

        private void OnBasemapsClicked(object sender, SelectionChangedEventArgs e)
        {
            // Get the text (basemap name) selected in the list box
            var basemapName = e.AddedItems[0].ToString();

            // Pass the basemap name to the view model method to change the basemap
            _mapViewModel.ChangeBasemap(basemapName);

        }

        private async void OnSaveMapClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a challenge request for portal credentials (OAuth credential request for arcgis.com)
                CredentialRequestInfo challengeRequest = new CredentialRequestInfo();

                // Use the OAuth implicit grant flow
                challengeRequest.GenerateTokenOptions = new GenerateTokenOptions
                {
                    TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                };

                // Indicate the url (portal) to authenticate with (ArcGIS Online)
                challengeRequest.ServiceUri = new Uri("https://www.arcgis.com/sharing/rest");

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);

                // Get information for the new portal item
                var title = TitleTextBox.Text;
                var description = DescriptionTextBox.Text;
                var tags = TagsTextBox.Text.Split(',');

                // Throw an exception if the text is null or empty for title or description
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
                {
                    throw new Exception("Title and description are required");
                }

                // Get current map extent (viewpoint) for the map initial extent
                var currentViewpoint = MyMapView.GetCurrentViewpoint(Esri.ArcGISRuntime.Mapping.ViewpointType.BoundingGeometry);

                // See if the map has already been saved
                if (!_mapViewModel.MapIsSaved)
                {
                    // Call the SaveNewMapAsync method on the view model, pass in the required info
                    await _mapViewModel.SaveNewMapAsync(currentViewpoint, title, description, tags);

                    // Report success
                    MessageBox.Show("Map '" + title + "' was saved to your portal");
                }
                else
                {
                    // Map has previously been saved as a portal item, update it (title, description, and tags will remain the same)
                    _mapViewModel.UpdateMapItem();

                    // Report success
                    MessageBox.Show("Changes to '" + title + "' were updated to the portal.");
                }

            }
            catch (OperationCanceledException opCancelled)
            {
                MessageBox.Show("Map not saved");
            }
            catch (Exception ex)
            {
                // Report error
                MessageBox.Show("Error while saving: " + ex.Message);
            }
        }

        private void OnNewMapClicked(object sender, EventArgs e)
        {
            _mapViewModel.ResetMap();
        }

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception ex)
            {
                // Exception will be reported in calling function
                throw (ex);
            }

            return credential;
        }

        private void UpdateAuthenticationManager()
        {
            // Define the server information for ArcGIS Online
            ServerInfo portalServerInfo = new ServerInfo();
            // ArcGIS Online URI
            portalServerInfo.ServerUri = new Uri(ArcGISOnlineUrl);
            // Type of token authentication to use
            portalServerInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit;

            // Define the OAuth information
            OAuthClientInfo oAuthInfo = new OAuthClientInfo
            {
                ClientId = AppClientId,
                RedirectUri = new Uri(OAuthRedirectUrl)
            };
            portalServerInfo.OAuthClientInfo = oAuthInfo;

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the ArcGIS Online server information with the AuthenticationManager
            thisAuthenticationManager.RegisterServer(portalServerInfo);

            // Use the OAuthAuthorize class in this project to create a new web view to show the login UI
            thisAuthenticationManager.OAuthAuthorizeHandler = new OAuthAuthorize();
            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }
    }
}
