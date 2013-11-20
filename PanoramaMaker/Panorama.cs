using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Imaging;
using Accord.Math;
using System.Drawing;
using OpenSURFcs;
using AForge;
using Accord.Imaging.Filters;

namespace PanoramaMaker
{
    class Panorama
    {
        enum ImageSection { Left, Right };
        static private int cropWidthPercent = 100;
        static private int cropWidth;

        public Panorama() {}
        public Panorama(int cropWidthP)
        {
            cropWidthPercent = cropWidthP;
        }

        /// <summary>
        /// Crops image on specific side.
        /// </summary>
        /// <param name="image">Image to be cropped.</param>
        /// <param name="side">Which side is going to be cropped.</param>
        /// <returns>Cropped bitmap.</returns>
        static private Bitmap GetCroppedImage(Image image, ImageSection side)
        {
            cropWidth = image.Width * cropWidthPercent / 100;

            Rectangle cropSection;
            if (side == ImageSection.Left)
                cropSection = new Rectangle(0, 0, cropWidth, image.Height);
            else
                cropSection = new Rectangle(image.Width - cropWidth, 0, cropWidth, image.Height);

            Bitmap sourceImage = new Bitmap(image);
            
            return sourceImage.Clone(cropSection, sourceImage.PixelFormat);
        }

        /// <summary>
        /// Detect keypoints in images using OpenSURF library 
        /// http://www.chrisevansdev.com/computer-vision-opensurf.html
        /// </summary>
        /// <param name="leftImage">Left image.</param>
        /// <param name="rightImage">Right image.</param>
        /// <returns>Detected keypoints.</returns>
        static public List<List<IntPoint>> DetectKeypoints_SURF(Image leftImage, Image rightImage)
        {
            IntegralImage integralImage;
            List<List<IntPoint>> keypoints = new List<List<IntPoint>>();

            integralImage = IntegralImage.FromImage(GetCroppedImage(leftImage, ImageSection.Left));
            List<IPoint> surf_keypointsLeft = FastHessian.getIpoints(0.001f, 5, 2, integralImage);

            keypoints.Add(new List<IntPoint>());
            foreach (IPoint p in surf_keypointsLeft)
                keypoints.Last().Add(new IntPoint((int)p.x, (int)p.y));


            integralImage = IntegralImage.FromImage(GetCroppedImage(rightImage, ImageSection.Right));
            List<IPoint> surf_keypointsRight = FastHessian.getIpoints(0.0002f, 5, 2, integralImage);

            keypoints.Add(new List<IntPoint>());
            foreach (IPoint p in surf_keypointsRight)
                keypoints.Last().Add(new IntPoint((int)p.x, (int)p.y));

            return keypoints;
        }

        /// <summary>
        /// Detect keypoints in images using Harris corner detector from Accord.NET library
        /// http://accord-framework.net/docs/html/T_Accord_Imaging_HarrisCornersDetector.htm
        /// </summary>
        /// <param name="leftImage">Left image.</param>
        /// <param name="rightImage">Right image.</param>
        /// <returns>Detected keypoints.</returns>
        static public List<List<IntPoint>> DetectKeypoints_Harris(Image leftImage, Image rightImage) 
        {
            HarrisCornersDetector harris_detector = new HarrisCornersDetector(0.04f, 500f);
            List<List<IntPoint>> keypoints = new List<List<IntPoint>>();

            keypoints.Add(harris_detector.ProcessImage(GetCroppedImage(leftImage, ImageSection.Left)));    
            keypoints.Add(harris_detector.ProcessImage(GetCroppedImage(rightImage, ImageSection.Right)));

            return keypoints;           
        }

        
        /// <summary>
        /// Matches detected keypoints.
        /// </summary>
        /// <param name="leftImage">Left image.</param>
        /// <param name="rightImage">Right image.</param>
        /// <param name="keypoints">Detected keypoints.</param>
        /// <returns>Homography matrix.</returns>
        static public MatrixH MatchKeypoints(Image leftImage, Image rightImage, List<List<IntPoint>> keypoints)
        {
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            CorrelationMatching matcher = new CorrelationMatching(9);
            MatrixH homography;

            IntPoint[][] matches = matcher.Match(new Bitmap(leftImage), new Bitmap(rightImage), keypoints[0].ToArray(), keypoints[1].ToArray());
            IntPoint[] correlationPoints1 = matches[0];
            IntPoint[] correlationPoints2 = matches[1];

            homography = ransac.Estimate(correlationPoints1, correlationPoints2);
 
            return homography;
        }

        /// <summary>
        /// Blends two images with homography matrix.
        /// </summary>
        /// <param name="leftImage">Left image.</param>
        /// <param name="rightImage">Right image.</param>
        /// <param name="homography">homography matrix.</param>
        /// <returns>Panorama image.</returns>
        static public Bitmap BlendImages(Image leftImage, Image rightImage, MatrixH homography)
        {
            Blend blend = new Blend(homography, new Bitmap(leftImage));
            Bitmap panorama = blend.Apply(new Bitmap(rightImage));

            return panorama;
        }

    }
}
