﻿#define DEBUG   //This line sets DEBUG to true, comment out when not needed.

using System;
using System.Drawing;
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
using Microsoft.Win32;

namespace WpfImageManipulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // This website proved helpful as it takes several steps to get the raw pixel data
        //http://social.msdn.microsoft.com/Forums/vstudio/en-US/9c08d28c-5e5d-4e0d-b4db-2f343fe4019c/getting-single-values-pixels-in-bitmapimage

        // I also learned a great deal about how to launch background tasks from the main UI thread in such a way that the system does not appear unresponsive.
        // There seem to be several ways to do this, but the clearest explanation I've found starts on page 287 of "WPF Control Development Unleashed" by Pavan Podila and Kevin Hoffman.

        private BitmapImage image;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openWindow = new OpenFileDialog();
            openWindow.Title = "Select an image to load.";
            openWindow.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png";
            if (openWindow.ShowDialog() == true)
            {
                this.image = new BitmapImage(new System.Uri(openWindow.FileName));
                imageDisplay.Source = this.image;
                pictureLabel.Content = "Image: \"" + openWindow.FileName + "\"";
                int picHeight = this.image.PixelHeight;
                int picWidth = this.image.PixelWidth;
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                //If the picture is too big to fit on the screen
                if (picWidth > (screenWidth-200) || picHeight > (screenHeight-200))
                {
                    this.WindowState = WindowState.Maximized;    //set window to full screen
                    //set imageDisplay dimensions appropriately, which requires first finding the ratio of the image
                    double ratio = (double)picHeight / (double)picWidth;
                    double screenRatio = (double)(screenHeight - 200) / (double)(screenWidth - 200);

                    if (ratio < screenRatio)    //This means the width is the limiting factor
                    {
                        imageDisplay.Width = screenWidth-200;
                        imageDisplay.Height = (screenWidth - 200) * ratio;
                    }
                    else    //This means the height is the limiting factor
                    {
                        imageDisplay.Height = screenHeight - 200;
                        imageDisplay.Width = (screenHeight - 200) / ratio;
                    }
                }
                else
                {
                    this.WindowState = WindowState.Normal;    //set window to normal state (not full screen)
                    imageDisplay.Height = picHeight;
                    imageDisplay.Width = picWidth;
                    this.Width = picWidth + 200;
                    this.Height = picHeight + 200;
                }
            }
            else
            {
                MessageBox.Show("Unable to open file.");
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #if DEBUG
                ComboBoxItem selected = (ComboBoxItem)e.AddedItems[0];
                MessageBox.Show("Combo option box selection: " + selected.Content.ToString());
            #endif
            
            switch(this.FilterSelect.SelectedIndex)
            {
                case 0:
                    //Black and White filter
                    Console.WriteLine("Case 0 entered.");
                    break;
                case 1:
                    //Sobel Edge Detection filter
                    Console.WriteLine("Case 1 entered.");
                    break;
                case 2:
                    //Other Edge Detection filter
                    Console.WriteLine("Case 2 entered.");
                    break;
                case 3:
                    //Voroni Sections filter
                    Console.WriteLine("Case 3 entered.");
                    break;
                default:
                    MessageBox.Show("This filter not supported. ");
                    break;
            }

        }
    }
}