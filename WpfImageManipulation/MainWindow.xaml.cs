#define DEBUG   //This line sets DEBUG to true, comment out when not needed.

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
        private WriteableBitmap filteredImage;
        RadioButton blackWhite;
        RadioButton color;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load_Click_1(object sender, RoutedEventArgs e)
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
                Console.WriteLine("Combo option box selection: " + selected.Content.ToString());
            #endif
            
            switch(this.FilterSelect.SelectedIndex)
            {
                case 0:
                    //Black and White filter
                    Console.WriteLine("Case 0 entered.");
                    this.OptionsLabel.IsEnabled = false;
                    break;
                case 1:
                    //Sobel Edge Detection filter
                    Console.WriteLine("Case 1 entered.");
                    //Configure Sobel Options
                    this.OptionsLabel.IsEnabled = true;
                    blackWhite = new RadioButton();
                    color = new RadioButton();
                    blackWhite.GroupName = "Color Options";
                    blackWhite.Content = "Grayscale";
                    blackWhite.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    blackWhite.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    blackWhite.Margin = new Thickness(10, 180, 0, 0);

                    color.GroupName = "Color Options";
                    color.Content = "Color";
                    color.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    color.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    color.Margin = new Thickness(90, 180, 0, 0);

                    Grid.SetRow(blackWhite, 0);
                    Grid.SetColumn(blackWhite, 1);
                    Grid.SetRow(color, 0);
                    Grid.SetColumn(color, 1);
                    this.TopGrid.Children.Add(blackWhite);
                    this.TopGrid.Children.Add(color);
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
                    MessageBox.Show("This filter not supported.");
                    break;
            }

        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            #if DEBUG
                Console.WriteLine("Apply Filter Clicked with filter: " + this.FilterSelect.SelectedIndex);
            #endif

            if(this.FilterSelect.SelectedIndex < 0)
            {
                MessageBox.Show("You must first select a filter.");
            }
            else if(this.image == null)
            {
                MessageBox.Show("You must first select an image.");
            }
            else
            {
                //First create a writeable Bitmap from the image.
                //TODO: Maybe add this to background queue after image is initially loaded?
                this.filteredImage = new WriteableBitmap(image);

                switch (this.FilterSelect.SelectedIndex)
                {
                    case 0:
                        //Black and White filter
                        ApplyFilterBW();
                        break;
                    case 1:
                        //Sobel Edge Detection filter
                        if((bool)this.blackWhite.IsChecked || (bool)this.color.IsChecked)
                        {
                            ApplyFilerSobel();
                        }
                        else
                        {
                            MessageBox.Show("You must first configure the filter options.");
                        }
                        break;
                    case 2:
                        //Other Edge Detection filter
                        break;
                    case 3:
                        //Voroni Sections filter
                        break;
                    default:
                        break;
                }
            }
        }

        //Apply the Black and White Filter
        private void ApplyFilterBW()
        {
            int height = image.PixelHeight;
            int width = image.PixelWidth;

            int[] pixels = new int[width * height]; //To hold the new pixels

            this.filteredImage.CopyPixels(pixels, 4 * width, 0);

            //Perform filter on each pixel
            for (int i = 0; i < pixels.Length; i++)
            {
                //Calculate each of the color components
                int red = getRed(pixels[i]); // (pixels[i] & 0x00FF0000) >> 16; ;
                int green = getGreen(pixels[i]); // (pixels[i] & 0x0000FF00) >> 8;
                int blue = getBlue(pixels[i]); // pixels[i] & 0x000000FF;
                int intensity = (int)((0.3 * red) + (0.59 * green) + (0.11 * blue));

                pixels[i] = intensity << 16;    //Red
                pixels[i] |= intensity << 8;    //Green
                pixels[i] |= intensity;         //Blue
            }

            this.filteredImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, 4 * width, 0);
            imageDisplay.Source = this.filteredImage;
        }

        //Apply the Sobel Filter
        // http://homepages.inf.ed.ac.uk/rbf/HIPR2/sobel.htm and many other websites
        private void ApplyFilerSobel()
        {
            Console.WriteLine("Apply Sobel filter.");
            if((bool)this.blackWhite.IsChecked)
            {
                ApplyFilterBW();
            }

            int height = image.PixelHeight;
            int width = image.PixelWidth;

            // Convert the image pixel data into a 2d array for easy processing
            int[] pixelBytes = new int[width * height];
            int[,] pixels = new int[height, width];
            this.filteredImage.CopyPixels(pixelBytes, 4 * width, 0);
            for (int row = 0; row < height; row++)
            {
                for(int col = 0; col < width; col++)
                {
                    pixels[row, col] = pixelBytes[row * width + col];
                }
            }

            //Apply the filter to each pixel
            // [ 1, 2, 1      [-1, 0, 1       [ 0,  2, 2
            //   0, 0, 0   +   -2, 0, 2   =    -2,  0, 2
            //  -1,-2,-1]      -1, 0, 1]       -2, -2, 0]
            //Ignore each edge, since the pixels needed don't all exist.
            for (int row = 1; row < height - 1; row++)
            {
                for (int col = 1; col < width - 1; col++)
                {
                    int red = Math.Abs(getRed(pixels[row - 1, col]-1) + 2*getRed(pixels[row-1, col]) + getRed(pixels[row-1,col+1]) - 
                                       (getRed(pixels[row + 1, col-1]) + 2*getRed(pixels[row+1, col]) + getRed(pixels[row+1, col+1]))) 
                        + 
                        Math.Abs(-getRed(pixels[row - 1, col - 1]) + getRed(pixels[row - 1, col + 1]) +
                                 -2*getRed(pixels[row, col-1]) + 2*getRed(pixels[row, col+1]) +
                                 -getRed(pixels[row+1, col-1]) + getRed(pixels[row+1, col+1]));

                    int green = Math.Abs(getGreen(pixels[row - 1, col] - 1) + 2 * getGreen(pixels[row - 1, col]) + getGreen(pixels[row - 1, col + 1]) -
                                       (getGreen(pixels[row + 1, col - 1]) + 2 * getGreen(pixels[row + 1, col]) + getGreen(pixels[row + 1, col + 1])))
                        +
                        Math.Abs(-getGreen(pixels[row - 1, col - 1]) + getGreen(pixels[row - 1, col + 1]) +
                                 -2 * getGreen(pixels[row, col - 1]) + 2 * getGreen(pixels[row, col + 1]) +
                                 -getGreen(pixels[row + 1, col - 1]) + getGreen(pixels[row + 1, col + 1]));

                    int blue = Math.Abs(getBlue(pixels[row - 1, col] - 1) + 2 * getBlue(pixels[row - 1, col]) + getBlue(pixels[row - 1, col + 1]) -
                                       (getBlue(pixels[row + 1, col - 1]) + 2 * getBlue(pixels[row + 1, col]) + getBlue(pixels[row + 1, col + 1])))
                        +
                        Math.Abs(-getBlue(pixels[row - 1, col - 1]) + getBlue(pixels[row - 1, col + 1]) +
                                 -2 * getBlue(pixels[row, col - 1]) + 2 * getBlue(pixels[row, col + 1]) +
                                 -getBlue(pixels[row + 1, col - 1]) + getBlue(pixels[row + 1, col + 1]));


                    //int red = getRed(pixels[row - 1, col]) * 2 + getRed(pixels[row - 1, col + 1]) * 2 +
                    //    getRed(pixels[row, col - 1]) * (-2) + getRed(pixels[row, col + 1]) * 2 +
                    //    getRed(pixels[row + 1, col - 1]) * (-2) + getRed(pixels[row + 1, col]) * (-2);

                    //int green = getGreen(pixels[row - 1, col]) * 2 + getGreen(pixels[row - 1, col + 1]) * 2 +
                    //    getGreen(pixels[row, col - 1]) * (-2) + getGreen(pixels[row, col + 1]) * 2 +
                    //    getGreen(pixels[row + 1, col - 1]) * (-2) + getGreen(pixels[row + 1, col]) * (-2);

                    //int blue = getBlue(pixels[row - 1, col]) * 2 + getBlue(pixels[row - 1, col + 1]) * 2 +
                    //    getBlue(pixels[row, col - 1]) * (-2) + getBlue(pixels[row, col + 1]) * 2 +
                    //    getBlue(pixels[row + 1, col - 1]) * (-2) + getBlue(pixels[row + 1, col]) * (-2);

                    red = Math.Min(255, red);
                    green = Math.Min(255, green);
                    blue = Math.Min(255, blue);
                    pixels[row, col] = red << 16;
                    pixels[row, col] |= green << 8;
                    pixels[row, col] |= blue;
                    pixels[row, col] |= 0xFF << 24;
                }
            }
            this.filteredImage.WritePixels(new Int32Rect(0, 0, width, height), pixels, 4 * width, 0);
            imageDisplay.Source = this.filteredImage;
        }

        int getRed(int pixel)
        {
            return (pixel & 0x00FF0000) >> 16;
        }
        int getGreen(int pixel)
        {
            return (pixel & 0x0000FF00) >> 8;
        }
        int getBlue(int pixel)
        {
            return pixel & 0x000000FF;
        }
    }
}
