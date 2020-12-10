using GeodeticPDA.Model;
using GeodeticPDA.Presenter;
using System;
using System.Windows;

namespace GeodeticPDA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GeodeticPdaPresenter _presenter = new GeodeticPdaPresenter();
        private int _editedPropertyId = -1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _presenter.AddProperty(Id.Text, Number.Text, Description.Text,
                Gps1Latitude.Text, Gps1Longitude.Text, Gps2Latitude.Text, Gps2Longitude.Text);
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            Property p = _presenter.FindProperty(Id.Text);
            if (p != null)
            {
                _editedPropertyId = p.Id;

                Id.Text = p.Id.ToString();
                Number.Text = p.Number.ToString();
                Description.Text = p.Description;
                Gps1Latitude.Text = p.GpsCoordinates1.Latitude.ToString();
                Gps1Longitude.Text = p.GpsCoordinates1.Longitude.ToString();
                Gps2Latitude.Text = p.GpsCoordinates2.Latitude.ToString();
                Gps2Longitude.Text = p.GpsCoordinates2.Longitude.ToString();
            }
            else
            {
                _editedPropertyId = -1;
                ClearFields(clearId: false);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_presenter.RemoveProperty(Id.Text))
            {
                ClearFields();
            }
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = _presenter.SaveChanges(_editedPropertyId, Id.Text, Number.Text, Description.Text,
                Gps1Latitude.Text, Gps1Longitude.Text, Gps2Latitude.Text, Gps2Longitude.Text);
            if (success)
            {
                _editedPropertyId = int.Parse(Id.Text);
            }
        }

        private void GeneratePropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            _presenter.GenerateProperties(NumberOfProperties.Text);
        }

        private void ShowFilesContentButton_Click(object sender, RoutedEventArgs e)
        {
            MainFileListBox.ItemsSource = _presenter.MainFileItems();
            OverfillingFileListBox.ItemsSource = _presenter.OverfillingFileItems();
            ManagingData.Text = _presenter.GetManagingData();
        }

        private void ClearFields(bool clearId = true)
        {
            if (clearId) Id.Text = "";
            Number.Text = "";
            Description.Text = "";
            Gps1Latitude.Text = "";
            Gps1Longitude.Text = "";
            Gps2Latitude.Text = "";
            Gps2Longitude.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            _presenter.Dispose();
            base.OnClosed(e);
        }

    }
}
