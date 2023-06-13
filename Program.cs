using System;
using System.Runtime.InteropServices;
using OpenCvSharp;

namespace SizeDetection
{
    class Program
    {
        private static Point? point1 = null;
        private static Point? point2 = null;

        static void Main(string[] args)
        {
            Mat image = Cv2.ImRead(@"C:\Users\Laptop\Desktop\ubytki-w-betonie.jpg");

            Point[][] contours = GetAllContours(image);
            Mat imageClone = image.Clone();
            Cv2.DrawContours(imageClone, contours, -1, new Scalar(250, 0, 0), thickness: 1);
            Cv2.ImShow("Contours", imageClone);
            Cv2.ImShow("Original", image);

            // Utwórz obiekt UserData i przypisz do niego referencję do obiektu Mat
            UserData userData = new UserData { ImageClone = imageClone };

            // Przekonwertuj obiekt UserData na wskaźnik IntPtr
            GCHandle gcHandle = GCHandle.Alloc(userData);
            IntPtr userdata = GCHandle.ToIntPtr(gcHandle);

            Cv2.SetMouseCallback("Original", OnMouseCallback, userdata);
            Cv2.WaitKey();

            // Obliczanie odległości między punktami
            if (point1 != null && point2 != null)
            {
                double distance = CalculateDistance(point1.Value, point2.Value);
                Cv2.Line(userData.ImageClone, point1.Value, point2.Value, new Scalar(0, 0, 255), thickness: 2);
                Cv2.PutText(userData.ImageClone, "Distance: " + distance.ToString("0.00") + " pixels", new Point(10, 30), HersheyFonts.HersheyPlain, 1.0, new Scalar(0, 0, 255), thickness: 2);
                Cv2.ImShow("Contours", userData.ImageClone);
                Cv2.WaitKey();
            }
            else
            {
                Console.WriteLine("Nie wybrano obu punktów.");
            }

            // Zwalnianie zasobów i zwolnienie wskaźnika IntPtr
            gcHandle.Free();

            string outputImagePath = @"C:\Users\Laptop\Desktop\krawedzie\2.jpg";
            Cv2.ImWrite(outputImagePath, imageClone);
        }

        static void OnMouseCallback(MouseEventTypes eventType, int x, int y, MouseEventFlags flags, IntPtr userdata)
        {
            // Przekonwertuj wskaźnik IntPtr na obiekt UserData
            GCHandle gcHandle = GCHandle.FromIntPtr(userdata);
            UserData data = (UserData)gcHandle.Target;

            if (eventType == MouseEventTypes.LButtonDown)
            {
                if (point1 == null)
                {
                    point1 = new Point(x, y);
                    Console.WriteLine("Kliknięto punkt 1: (" + x + ", " + y + ")");
                }
                else if (point2 == null)
                {
                    point2 = new Point(x, y);
                    Console.WriteLine("Kliknięto punkt 2: (" + x + ", " + y + ")");

                    // Obliczanie odległości między punktami
                    double distance = CalculateDistance(point1.Value, point2.Value);
                    Cv2.Line(data.ImageClone, point1.Value, point2.Value, new Scalar(0, 0, 255), thickness: 2);
                    Cv2.PutText(data.ImageClone, "Distance: " + distance.ToString("0.00") + " pixels", new Point(10, 30), HersheyFonts.HersheyPlain, 1.0, new Scalar(0, 0, 255), thickness: 2);
                    Cv2.ImShow("Contours", data.ImageClone);
                    Cv2.WaitKey();
                }
            }
        }

        static double CalculateDistance(Point p1, Point p2)
        {
            double distance = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            return distance;
        }

        static Point[][] GetAllContours(Mat img)
        {
            Mat refGray = new Mat();
            Cv2.CvtColor(img, refGray, ColorConversionCodes.BGR2GRAY);
            Mat thresh = new Mat();
            Cv2.Threshold(refGray, thresh, 127, 255, ThresholdTypes.Binary);
            Point[][] contours;
            HierarchyIndex[] hIndx;
            Cv2.FindContours(thresh, out contours, out hIndx, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            return contours;
        }
    }

    class UserData
    {
        public Mat ImageClone { get; set; }
    }
}