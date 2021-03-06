﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;

namespace Roads
{
    class Program
    {
        private const int SHAPES = 3;
        private const int ARGS = 9;
        public const int LEN = 50;
        public static int size;
        private static int halfSize;
        public static int width;
        public static int height;
        public static string fileName;
        public static string dir;
        private static Random rand;
        private static Color borderColor = Color.Black;
        public static Color foreColor = Color.Red;
        public static Color backColor = Color.Snow;

        public static int pensize = 1;
        public static int halfpensize;

        private static Pen borderpen;
		private static int antialiasFactor = 1;
		
		/// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            //a holder for the result
            Bitmap result = new Bitmap(width, height);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }

		
        private static bool TryParseAguments(string[] args)
        {
            if (args.Length < ARGS)
            {
				foreach (string arg in args) {
					Console.WriteLine(arg);
				}
                Usage("size width height foreground background border_color border_size antialias_factor image_name");
                return false;
            }
            if (!int.TryParse(args[0], out size))
            {
                Usage("size need to be int");
                return false;
            }
            if (size <= 0)
            {
                Usage("size need to be greater than 0");
                return false;
            }
            if (!int.TryParse(args[1], out width))
            {
                Usage("width need to be int");
                return false;
            }
            if (width <= 0)
            {
                Usage("width need to be greater than 0");
                return false;
            }
            if (!int.TryParse(args[2], out height))
            {
                Usage("height need to be int");
                return false;
            }
            if (height <= 0)
            {
                Usage("height need to be greater than 0");
                return false;
            }

            //String foreColorS = args[3]; //#RRGGBB, hex
            if (!ColorFromString(args[3], out foreColor))
            {
                Usage("foreground need to be in format #rrggbb");
                return false;
            }

            if (!ColorFromString(args[4], out backColor))
            {
                Usage("background need to be in format #rrggbb");
                return false;
            }

            if (!ColorFromString(args[5], out borderColor))
            {
                Usage("border need to be in format #rrggbb");
                return false;
            }
			
			if (!int.TryParse(args[6], out pensize) && pensize<=0 )
			{
				Usage ("pensize need to be a positive integer");
			}
			
			if (!int.TryParse(args[7], out antialiasFactor) && !(antialiasFactor==0 || antialiasFactor==2 || antialiasFactor==4) )
			{
				Usage ("antialiasFactor has to be 1 2 or 4");
			}

            if (size % 2 == 0)
                size += 1;
			
			pensize *= antialiasFactor;
			size *= antialiasFactor;

            
            halfSize = size / 2;

            borderpen = new Pen(borderColor, pensize);
            halfpensize = (pensize / 2);

            fileName = args[8] + ".png";
            
            return true;
        }

        /// <summary>
        /// Creates color structure from a string.
        /// </summary>
        /// <param name="s">String in format 'RRGGBB', ie: 23FF43</param>
        /// <param name="color">Color made out the string</param>
        /// <returns></returns>
        private static bool ColorFromString(string s, out Color color)
        {
            color = Color.Black;
            if (s == null || s.Length == 0 || s.Length < 6) return false;

            string rcolor = s.Substring(0, 2);
            string gcolor = s.Substring(2, 2);
            string bcolor = s.Substring(4, 2);

            int rval = int.Parse(rcolor, System.Globalization.NumberStyles.HexNumber);
            int gval = int.Parse(gcolor, System.Globalization.NumberStyles.HexNumber);
            int bval = int.Parse(bcolor, System.Globalization.NumberStyles.HexNumber);

            color = Color.FromArgb(rval, gval, bval);
            return true;
        }

        private static void Usage(string message)
        {
            if (message != "")
            {
                Console.WriteLine("err: " + message);
            }
        }

        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            rand = new Random();
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            DirectoryInfo di2 = new DirectoryInfo(di.Parent.Parent.FullName);
            if (!Directory.Exists(di2.FullName + "\\data"))
                Directory.CreateDirectory(di2.FullName + "\\data");

            dir = di2.FullName + "\\data";
            dir = Directory.GetCurrentDirectory();
            if (args.Length == 0)
            {
                Console.WriteLine("You can also use console interface\nRoads size width height image_name");
                Application.EnableVisualStyles();
                Application.Run(new GUI());
            }
            else
            {
                if (TryParseAguments(args))
                {
                    for (int i = 0; i < LEN; i++)
                    {
                        Console.Write("_");
                    }
                    Console.WriteLine("");
                    BackgroundWorker mybw = new BackgroundWorker();
                    mybw.ProgressChanged += new ProgressChangedEventHandler(mybw_ProgressChanged);
                    mybw.WorkerReportsProgress = true;
                    CreateImage(mybw);
                    Console.WriteLine("\nImage created sucesfully!\n\n");
                }
            }

        }

        static void mybw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            for (int i = 0; i < e.ProgressPercentage; i++)
                Console.Write("#");
        }

        public static void CreateImage(BackgroundWorker bg)
        {
            Bitmap bitmap = CreateBitmap();
            FloodColorBitmap(bitmap,bg);
            Directory.SetCurrentDirectory(dir);

            if (antialiasFactor != 1)
            {
                Bitmap smaller = ResizeImage(bitmap, bitmap.Width / antialiasFactor, bitmap.Height / antialiasFactor);
                bitmap = smaller;
            }

            bitmap.Save(fileName, ImageFormat.Png);
        }

        private static bool CompareColors(Color color1, Color color2)
        {
            return color1.ToArgb().Equals(color2.ToArgb());
        }

        private static void ColorBitmap(Bitmap bitmap, BackgroundWorker bw)
        {
            bool border = false;
            int counter = 0;
            for (int j = 1; j < height * size; j++)
            {
                if (CompareColors(bitmap.GetPixel(1, j), borderColor))
                {

                    if (!border)
                    {
                        SwitchColors();
                        border = true;
                    }
                }

                else
                {
                    border = false;
                    bitmap.SetPixel(1, j, foreColor);
                }
            }
            for (int i = 2; i < width * size; i++)
            {
                SetColor(bitmap, i, 1);
                counter = 0;
                for (int j = 1; j < height * size; j++)
                {
                    if (CompareColors(bitmap.GetPixel(i, j), borderColor))
                    {
                        counter++;
                    }

                    else
                    {
                        if (counter > 0)
                        {
                            if (counter > 1)
                            {
                                SetColor(bitmap, i, j);
                            }
                            else
                                SwitchColors();
                            counter = 0;
                        }
                        bitmap.SetPixel(i, j, foreColor);
                    }
                }
                if (i % (width * size / LEN) == 0)
                {
                    bw.ReportProgress(1);
                }
            }
        }
        private static void ColorBitmap(Bitmap bitmap)
        {
            bool border = false;
            int counter = 0;
            for (int i = 0; i < LEN; i++)
            {
                Console.Write("_");
            }
            Console.WriteLine();

            for (int j = 0; j < height * size; j++)
            {
                if (CompareColors(bitmap.GetPixel(0, j), borderColor))
                {
                 
                    if (!border)
                    {
                        SwitchColors();
                        border = true;
                    }
                }

                else
                {
                    border = false;
                    bitmap.SetPixel(0, j, foreColor);
                }
            }
            for (int i = 1; i < width * size; i++)
            {
                SetColor(bitmap, i, 1);
                counter = 0;
                bool lastpixel_border = false;
                for (int j = 0; j < height * size; j++)
                {
                    lastpixel_border = false;
                    if (CompareColors(bitmap.GetPixel(i, j), borderColor))
                    {
                        if (!lastpixel_border)
                        {
                            counter++;
                            lastpixel_border = true;
                        }
                    }

                    else
                    {
                        if (counter > 0)
                        {
                            
                            if (counter > 1)
                            {
                                SetColor(bitmap, i, j);
                            }
                            else
                                SwitchColors();
                            counter = 0;
                        }
                        bitmap.SetPixel(i, j, foreColor);
                    }
                }
                if (i % (width * size / LEN) == 0)
                {
                    Console.Write("#");
                }
            }
            Console.WriteLine();
        }

        private static void FloodColorBitmap(Bitmap bitmap, BackgroundWorker bw)
        {
            //kreslimy pierwszą linię, to jest w miare proste
            bool border = false;
            for (int j = 0; j < height * size; j++)
            {
                if (CompareColors(bitmap.GetPixel(0, j), borderColor))
                {

                    if (!border)
                    {
                        SwitchColors();
                        border = true;
                    }
                }

                else
                {
                    border = false;
                    //ale zamiast kreślić kreski to robimy floodfilla
                    FloodFill(bitmap, 0, j);
                }
            }

            for (int j = 0; j < height * size; j++)
            {
                int borderSize = 0;
                if (bitmap.GetPixel(0, j) == borderColor)
                {
                    continue;
                }
                else
                {
                    if (bitmap.GetPixel(0, j) != foreColor)
                    {
                        SwitchColors();
                    }
                }

                //probujemy kreślić w bok poczynając od tamtej kreski
                for (int i = 1; i < width * size; i++)
                {
                    if (bitmap.GetPixel(i, j) == borderColor)
                    {
                        borderSize++;
                        if (borderSize > pensize)
                        {
                            // jeśli napotkaliśmy na grubą kreskę to uciekamy
                            i = width * size;
                            continue;
                            //...i mamy nadzieję, że poozostałe floodfille to wypełnią
                        }
                    }
                    else
                    {
                        if (bitmap.GetPixel(i, j) != foreColor)
                        {
                            //była krawędź, ale mała. sprawdzamy czy musimy zmienić kolor
                            SwitchColors();
                        }
                        borderSize = 0;
                        FloodFill(bitmap, i, j);
                    }
                }

                if (j % (height * size / LEN) == 0)
                {
                    bw.ReportProgress(1);
                }
            }
        }

        //Wypełnianie rekurencyjne było brzydkie i nie działało (stackoverflow)
        private static Stack<KeyValuePair<int, int>> posStack = new Stack<KeyValuePair<int, int>>();
        private static int[,] floodMask = new int[4, 2] { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
        private static void FloodFill(Bitmap bitmap, int x, int y)
        {
            posStack.Clear();

            posStack.Push(new KeyValuePair<int, int>(x, y));

            while (posStack.Count != 0)
            {
                KeyValuePair<int, int> now = posStack.Pop();
                for (int i = 0; i < 4; i++)
                {
                    int targetX = now.Key + floodMask[i, 0];
                    int targetY = now.Value + floodMask[i, 1];
                    if (targetX < 0 || targetX >= bitmap.Width || targetY < 0 || targetY >= bitmap.Height) continue;
                    if (bitmap.GetPixel(targetX, targetY) != backColor && bitmap.GetPixel(targetX, targetY) != borderColor && bitmap.GetPixel(targetX, targetY) != foreColor)
                    {
                        bitmap.SetPixel(targetX, targetY, foreColor);
                        posStack.Push(new KeyValuePair<int, int>(targetX, targetY));
                    }
                }
            }
        }

        private static void SetColor(Bitmap bitmap, int i, int j)
        {
            int tmp;
            tmp = i - 1;
            int counter = 0;
            while (tmp > 0 && CompareColors(bitmap.GetPixel(tmp, j), borderColor))
            {
                tmp--;
                counter++;
            }
            if (counter > 0 && CompareColors(bitmap.GetPixel(tmp, j), foreColor))
            {
                SwitchColors();
            }
            else
            {
                if (counter == 0 && !CompareColors(bitmap.GetPixel(tmp, j), foreColor))
                {
                    SwitchColors();
                }
            }
        }
        private static void SwitchColors()
        {
            Color tmp;
            tmp = foreColor;
            foreColor = backColor;
            backColor = tmp;
        }
        private static Bitmap CreateBitmap()
        {
            Bitmap bitmap = new Bitmap(width * size, height * size);
            Graphics g = Graphics.FromImage(bitmap);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    switch (rand.Next() % SHAPES)
                    {
                        case 0:
                            DrawCross(g, i, j);
                            break;
                        case 1:
                            DrawCurveLeft(g, i, j);
                            break;
                        case 2:
                            DrawCurveRight(g, i, j);
                            break;
                    }
                }
            }
            return bitmap;
        }

        private static void DrawCurveRight(Graphics g, int i, int j)
        {

            g.DrawArc(borderpen, i * size - halfSize - halfpensize - 1, j * size - halfSize - halfpensize - 1, size + halfpensize + 1, size + halfpensize + 1, 0, 90);
            g.DrawArc(borderpen, (i + 1) * size - halfSize, (j + 1) * size - halfSize, size, size, 180, 90);
        }
        private static void DrawCurveLeft(Graphics g, int i, int j)
        {
            g.DrawArc(borderpen, i * size - halfSize - halfpensize - 1, (j + 1) * size - halfSize , size + halfpensize + 1, size, 270, 90);
            g.DrawArc(borderpen, (i + 1) * size - halfSize, j * size - halfSize - halfpensize - 1, size, size + halfpensize + 1, 90, 90);
        }
        private static void DrawCross(Graphics g, int i, int j)
        {
            g.DrawLine(borderpen, new Point(i * size, j * size + halfSize + 1), new Point((i + 1) * size, j * size + halfSize + 1));
            g.DrawLine(borderpen, new Point(i * size + halfSize + 1, j * size), new Point(i * size + halfSize + 1, (j + 1) * size + 1));
        }
    }
}