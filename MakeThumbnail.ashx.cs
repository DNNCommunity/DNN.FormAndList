using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.UserDefinedTable.Components;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Modules.UserDefinedTable
{
    /// <summary>
    ///   Summary description for MakeThumbnail
    /// </summary>
    public class MakeThumbnail : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var intMaxWidth = 0;
            var intMaxHeight = 0;
            Bitmap sourceImage = null;
            Bitmap newImage = null;
            var request = context.Request;
            Stream sourceImageStream = null;

            try
            {
                // Get max. width, if any
                if (request.Params["w"] != string.Empty)
                {
                    intMaxWidth = request.QueryString["w"].AsInt();
                }

                // Get max. height, if any
                if (request.Params["h"] != string.Empty)
                {
                    intMaxHeight = request.QueryString["h"].AsInt();
                }

                // Get source image path
                var strFilepath = request.Params["image"];
                //only virtual paths are valid!

                var ps = PortalController.Instance.GetCurrentPortalSettings();



                // Check cache for thumbnail
                //add prefix to identify cache item as belonging to UDT
                var cacheKey = "UDT_TN" + strFilepath + intMaxWidth + "x" + intMaxHeight;
                var image = DataCache.GetCache(cacheKey);
                ImageFormat iFormat;
                if (image == null)
                {
                    // Get source Image
                    var file = FileManager.Instance.GetFile(ps.PortalId, strFilepath);
                    sourceImageStream = FileManager.Instance.GetFileContent(file);
                    sourceImage = new Bitmap(sourceImageStream);

                    iFormat = ImageFormat.Jpeg;
                    var intSourceWidth = sourceImage.Width;
                    var intSourceHeight = sourceImage.Height;
                    if ((intMaxWidth > 0 && intMaxWidth < intSourceWidth) ||
                        (intMaxHeight > 0 && intMaxHeight < intSourceHeight))
                    {
                        // Resize image:
                        double aspect = sourceImage.PhysicalDimension.Width / sourceImage.PhysicalDimension.Height;

                        int newWidth;
                        int newHeight;
                        if (intMaxWidth == 0)
                        {
                            newWidth = (int)(intMaxHeight * aspect);
                            newHeight = intMaxHeight;
                        }
                        else if (intMaxHeight == 0)
                        {
                            newWidth = intMaxWidth;
                            newHeight = (int)(intMaxWidth / aspect);
                        }
                        else if ((intSourceWidth / intMaxWidth) >= (intSourceHeight / intMaxHeight))
                        {
                            newWidth = intMaxWidth;
                            newHeight = (int)(intMaxWidth / aspect);
                        }
                        else
                        {
                            newWidth = (int)(intMaxHeight * aspect);
                            newHeight = intMaxHeight;
                        }

                        newImage = new Bitmap(newWidth, newHeight);
                        using (var g = Graphics.FromImage(newImage))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                            g.DrawImage(sourceImage, 0, 0, newWidth, newHeight);
                        }

                        //Cache thumbnail clone (Disposing a cached image will destroy its cache too)
                        DataCache.SetCache(cacheKey, newImage.Clone());
                    }
                    else //use original width (no maxwidth given or image is narrow enough:
                    {
                        newImage = sourceImage;
                    }
                }
                else
                {
                    // Get (cloned) cached thumbnail
                    newImage = (Bitmap)((Bitmap)image).Clone();
                    iFormat = ImageFormat.Jpeg;
                }

                // Send image to the browser.
                context.Response.ContentType = GetContentType(iFormat);
                newImage.Save(context.Response.OutputStream, iFormat);
            }
            finally
            {
                // Clean up
                if (newImage != null) newImage.Dispose();
                if (sourceImage != null) sourceImage.Dispose();
                if (sourceImageStream != null) sourceImageStream.Dispose();
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        static string GetContentType(ImageFormat iFormat)
        {
            string contentType;

            if (iFormat.Guid.Equals(ImageFormat.Jpeg.Guid))
            {
                contentType = "image/jpeg";
            }
            else if (iFormat.Guid.Equals(ImageFormat.Gif.Guid))
            {
                contentType = "image/gif";
            }
            else if (iFormat.Guid.Equals(ImageFormat.Png.Guid))
            {
                contentType = "image/png";
            }
            else if (iFormat.Guid.Equals(ImageFormat.Tiff.Guid))
            {
                contentType = "image/tiff";
            }
            else if (iFormat.Guid.Equals(ImageFormat.Bmp.Guid))
            {
                contentType = "image/x-ms-bmp";
            }
            else
            {
                contentType = "image/jpeg";
            }
            return contentType;
        }
    }
}