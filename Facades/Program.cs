using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace Facades
{
    class Program
    {
        public static MatchCollection RegCollection(string fileName)                     //regex collecting numbers from our file name
        {
            return Regex.Matches(fileName, "[0-9]+");
        }

        public static List<string> NumbersFromFileName(string fileName)                 //convert regex (numbers from file name) into list for easier work
        {
            var numbers = RegCollection(fileName);

            return numbers.Cast<Match>().Select(match => match.Value).ToList();
        }

        public static int GetFloor(string fileName)                                     //returns number of floors
        {
            return Convert.ToInt32(NumbersFromFileName(fileName)[0]);
        }

        public static int GetTile(string fileName)                                      //returns number of tiles in each floor
        {
            return Convert.ToInt32(NumbersFromFileName(fileName)[1]);
        }

        public static double[] GetGridHorizontal(string fileName, Bitmap image)         //returns horizontal grid (location where the border is made)
        {
            double[] locations = new double[GetFloor(fileName)];
            for (int i = 1; i <= GetFloor(fileName); i++)
            {
                int locationValue = (image.Height / GetFloor(fileName)) * (i);
                locations[i - 1] = locationValue;
            }

            return locations;
        }

        public static double[] GetGridVertical(string fileName, Bitmap image)           //returns vertical grid (location where the "border" is made)
        {
            double[] locations = new double[GetTile(fileName)];
            for (int i = 1; i <= GetTile(fileName); i++)
            {
                int locationValue = (image.Width / GetTile(fileName)) * (i);
                locations[i - 1] = locationValue;
            }

            return locations;

        }

        static void Main(string[] args)
        {
            Random random = new Random();

            string resultDir = Directory.GetCurrentDirectory() + @"\Result";
            string windowPath = Directory.GetCurrentDirectory() + @"\windows";
            int numberOfFilesInWindows = 0;

            string[] windowFilesDirectories = Directory.GetFiles(windowPath, "*.*", SearchOption.AllDirectories); //all window files
            string[] subDirectory = Directory.GetDirectories(windowPath); //all folders containing windows

            string[] facadePaths = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\facades", "*.*", SearchOption.AllDirectories);   //array with all files in facade directory
            string[] wPaths = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\windows", "*.*", SearchOption.AllDirectories);   //array with all files in windows directory

            if (!Directory.Exists(resultDir))    //creates new folder called Result if it doesn't exist
            {
                Directory.CreateDirectory(resultDir);
            }

            Bitmap facadeImage = null;
            Bitmap windowImage = null;

            int imgCount;
            string facadeFileName;

            for (int i = 0; i < facadePaths.Length; i++) //first loop for all facade images
            {
                facadeFileName = Path.GetFileName(facadePaths[i]);
                imgCount = 1;
                

                for (int j = 0; j < subDirectory.Length; j++) //second loop for all window images pasted to facade images
                {
                    string currentWindowFolder = subDirectory[j];
                    numberOfFilesInWindows = Directory.GetFiles(subDirectory[j]).Length;
                    using (var image = new Bitmap(facadePaths[Convert.ToInt32(i)]))   //creates bitmap from facade image
                    {
                        facadeImage = new Bitmap(image);
                    }


                    double[] verticalGrid = GetGridHorizontal(facadeFileName, facadeImage);   //array containing all the vertical borders
                    double[] horizontalGrid = GetGridVertical(facadeFileName, facadeImage); //array containing all the horizontal borders

                    Graphics g = Graphics.FromImage(facadeImage);   //saving facadeimage to graphics object, program will draw window image on it

                    for (int k = 0; k < GetFloor(facadeFileName) ; k++) //loop that switches floors we are currently editing
                    {
                        for (int l = 0; l < GetTile(facadeFileName); l++) //loop that inserts windows into each tile
                        {
                            string[] currentWindowFilePaths = Directory.GetFiles(subDirectory[j], ".");
                            if (numberOfFilesInWindows == 1) //if there is only one file in window folder, 
                            {
                                using (var imageWindow = new Bitmap(currentWindowFilePaths[0]))   //creates bitmap from window image
                                {
                                    windowImage = new Bitmap(imageWindow);
                                    //resize
                                    double windowImageRatio = Convert.ToDouble(windowImage.Width) / Convert.ToDouble(windowImage.Height);      //original window image ratio
                                    int widthResized, heightResized;

                                    if (windowImageRatio > 0.5)
                                    {
                                        widthResized = Convert.ToInt32(facadeImage.Width / GetTile(facadeFileName) * 0.47);
                                        heightResized = Convert.ToInt32(widthResized / windowImageRatio);
                                    }
                                    else
                                    {
                                        heightResized = Convert.ToInt32(facadeImage.Height / GetFloor(facadeFileName) * 0.61);
                                        widthResized = Convert.ToInt32(heightResized * windowImageRatio);
                                    }

                                    Bitmap resized = new Bitmap(windowImage, new Size(widthResized, heightResized));
                                    windowImage = resized;
                                }
                            }

                            else  //if there is more than 1 picture in window folder, we need to create a list, which will contain only windows from same folder
                            {
                                List<Bitmap> windowImagesList = new List<Bitmap>();     //using list for creation of multiple window images from the same folder (they should exchange on the same picture)
                                for (int m = 0; m < numberOfFilesInWindows; m++)
                                {
                                    using (var imageWindow = new Bitmap(currentWindowFilePaths[Convert.ToInt32(m)]))   //creates bitmap from window image
                                    {
                                        windowImage = new Bitmap(imageWindow);
                                        //resize
                                        double windowImageRatio = Convert.ToDouble(windowImage.Width) / Convert.ToDouble(windowImage.Height);      //original window image ratio
                                        int widthResized, heightResized;

                                        if (windowImageRatio > 0.5)
                                        {
                                            widthResized = Convert.ToInt32(facadeImage.Width / GetTile(facadeFileName) * 0.47);
                                            heightResized = Convert.ToInt32(widthResized / windowImageRatio);
                                        }
                                        else
                                        {
                                            heightResized = Convert.ToInt32(facadeImage.Height / GetFloor(facadeFileName) * 0.61);
                                            widthResized = Convert.ToInt32(heightResized * windowImageRatio);
                                        }

                                        Bitmap resized = new Bitmap(windowImage, new Size(widthResized, heightResized));
                                        windowImage = resized;
                                    }
                                    windowImagesList.Add(windowImage);

                                }
                                int randomNumber = random.Next(0, windowImagesList.Count);
                                windowImage = windowImagesList[randomNumber];
                            }


                            //grid location
                            double verticalLine = verticalGrid[k];      
                            double horizontalLine = horizontalGrid[l];
                            
                            //one tile size, used for centering images in each tile
                            double tileSizeX = horizontalGrid[0];
                            double tileSizeY = verticalGrid[0];

                            //empty space left in a tile after picture is placed, used for centering images in each file
                            double emptySpaceHorizontal = tileSizeX - windowImage.Width;
                            double emptySpaceVertical = tileSizeY - windowImage.Height; 

                            //drawing a window image onto facade image
                            g.DrawImage(windowImage, new Point(Convert.ToInt32(horizontalLine - windowImage.Width - (emptySpaceHorizontal / 2)), 
                                Convert.ToInt32(verticalLine - windowImage.Height - (emptySpaceVertical / 2))));   
                        }

                    }

                    //naming a new image, each version has ID
                    int indexForID = RegCollection(facadeFileName)[2].Index; //regcollection collects all the numbers, returns last number (at index 2)
                    string fileId;

                    fileId = imgCount.ToString("D3");         //string in "000" format
                    facadeFileName = facadeFileName.Remove(indexForID, 3).Insert(indexForID, fileId); //replaces old ID number with new one

                    string[] fileNameWithNoExtension = facadeFileName.Split('.');

                    while (File.Exists(resultDir + @"\" + fileNameWithNoExtension[0] + ".jpg"))    //this loop makes sure that duplicates are not overwritten, but instead get different ID (+1 until it finds non-existent)
                    {
                        imgCount++;
                        fileId = imgCount.ToString("D3");
                        facadeFileName = facadeFileName.Remove(indexForID, 3).Insert(indexForID, fileId);
                        fileNameWithNoExtension = facadeFileName.Split('.');
                    }
                    

                    facadeImage.Save(resultDir + @"\" + fileNameWithNoExtension[0] + ".jpg", ImageFormat.Jpeg);   //saving newly created image

                    //disposing all the objects to improve performance
                    facadeImage.Dispose();
                    windowImage.Dispose();
                    g.Dispose();
                    //calling garbage collector
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    //increasing ID for next picture
                    imgCount++;

                }
            }
        }
    }
}
