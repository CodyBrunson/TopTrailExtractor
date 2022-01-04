using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Syroot.Windows.IO;
using TopTrailExtractor.Models;

namespace TopTrailExtractor;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        //TODO: Set up longest ride by year.
        //TODO: Set up longest ride by horse.
        //TODO: Add more 'fun facts'.
        
        private string _csvFile = string.Empty;
        private ObservableCollection<string> _horseNames;
        private ObservableCollection<string> _rideYears;
        private ObservableCollection<Ride> _rideData;

        public MainWindow()
        {
            InitializeComponent();
            WindowGrid.ShowGridLines = false;
            ElementsGrid.ShowGridLines = false;
            DataElementsGrid.ShowGridLines = false;
        }

        private void HorseNameCBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HorseNameCBox.Items.Count != 0)
            {
                var selectedHorse = HorseNameCBox.SelectedItem.ToString();
                var selectedRideYear = RideYearCBox.SelectedItem.ToString();
                var totalMiles = _rideData.TakeWhile(ride => selectedHorse is not null && selectedRideYear is not null)
                    .Where(ride => selectedRideYear != null && ride.RideDate.Contains(selectedRideYear) && ride.Horse == selectedHorse)
                    .Sum(ride => ride.Distance);

                CurrentHorseTotalNumberLbl.Content = totalMiles.ToString("N");
            }
        }

        private void RideYearCBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRideYear = RideYearCBox.SelectedItem.ToString();
            _horseNames.Clear();
            foreach (var ride in _rideData)
            {
                if (selectedRideYear is null)
                {
                    break;
                }
                
                if (ride.RideDate.Contains(selectedRideYear))
                {
                    if (!_horseNames.Contains(ride.Horse))
                    {
                        _horseNames.Add(ride.Horse);
                    }
                }
            }

            UpdateYearTotal();
        }

        private void UpdateYearTotal()
        {
            var currentlySelectedYear = RideYearCBox.SelectedItem.ToString();
            var newYearTotal = _rideData.TakeWhile(ride => currentlySelectedYear is not null)
                .Where(ride => currentlySelectedYear != null && ride.RideDate.Contains(currentlySelectedYear))
                .Sum(ride => ride.Distance);

            RideYearTotalNumberLbl.Content = newYearTotal.ToString("N");
        }
        
        private void UploadData(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                InitialDirectory = new KnownFolder(KnownFolderType.Downloads).Path
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName.Contains(".csv"))
                {
                    FileNameTBox.Text = openFileDialog.FileName;
                    _csvFile = openFileDialog.FileName;
                    SetupData();
                }
            }
        }

        private void SetupFunFacts()
        {
            var longestRide = _rideData.Aggregate((ride1, ride2) => ride1.Distance > ride2.Distance ? ride1 : ride2);
            LongestRideMilesLbl.Content = longestRide.Distance.ToString("N");
            LongestRideDateLbl.Content = longestRide.RideDate.Replace("\"", "")[..10];
            LongestRideHorseNameLbl.Content = longestRide.Horse;
        }
        
        private void SetupData()
        {
            ExtractRideData();
            SetupFunFacts();
            Parallel.Invoke(ExtractHorseNames,ExtractRideYears);

            RideYearCBox.ItemsSource = _rideYears;
            RideYearCBox.SelectedIndex = 0;

            HorseNameCBox.ItemsSource = _horseNames;
            HorseNameCBox.SelectedIndex = 0;
        }

        private void ExtractRideYears()
        {
            var rideYears = new ObservableCollection<string>();
            foreach (var ride in _rideData)
            {
                var cleanRideYear = ride.RideDate
                    .Replace("\"", "")
                    .Trim()
                    .Substring(0,4);
                if (!rideYears.Contains(cleanRideYear))
                {
                    rideYears.Add(cleanRideYear);
                }
            }

            _rideYears = rideYears;
        }

        private void ExtractHorseNames()
        {
            var horseNames = new ObservableCollection<string>();
            foreach (var ride in _rideData)
            {
                var cleanHorseName = ride.Horse;
                if (!horseNames.Contains(cleanHorseName))
                {
                    horseNames.Add(cleanHorseName);
                }
            }
            
            _horseNames = horseNames;
        }
        private void ExtractRideData()
        {
            var data = File.ReadAllLines(_csvFile);
            var rides = new ObservableCollection<Ride>();

            foreach (var line in data)
            {
                if (!line.Contains("Ride ID"))
                {
                    List<string> lineData = line.Split(',').ToList();
                    var horse = new Ride()
                    {
                        RideId = Convert.ToInt32(lineData[0]),
                        UserId = Convert.ToInt32(lineData[1]),
                        RideDate = lineData[2],
                        UploadDate = lineData[3],
                        Distance = Convert.ToDouble(lineData[4]),
                        Unused = lineData[5],
                        Duration = lineData[6],
                        RideName = lineData[7],
                        Difficulty = lineData[8],
                        Privacy = lineData[9],
                        PublicAccess = lineData[10],
                        Journal = lineData[11],
                        Horse = lineData[12].Replace("\"", "").Trim(),
                    };
                    rides.Add(horse);
                }

                
            }

            _rideData = rides;
        }
    }