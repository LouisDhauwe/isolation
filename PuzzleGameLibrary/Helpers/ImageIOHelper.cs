using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PuzzleGameLibrary.Helpers {
    public static class ImageIOHelper {

        /// <summary>
        /// Get file extension of file path
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns></returns>
        public static string FileExtension(string filePath) {

            string extension = "";

            int fileExtPos = filePath.LastIndexOf(".");
            if (fileExtPos >= 0) {
                extension = filePath.Substring(fileExtPos + 1, filePath.Length - fileExtPos - 1);
            }

            extension.Replace("jpeg", "jpg");

            return extension;

        }

        /// <summary>
        /// Returns true if the given image file path's extension
        /// is supported for metadata reading/writing in this library
        /// (currently only png and jpg)
        /// </summary>
        /// <param name="imageFilePath">File path</param>
        /// <returns></returns>
        public static bool MetadataSupported(string imageFilePath) {
            string ext = FileExtension(imageFilePath);

            if (ext.ToLower() == "jpg" ||
                ext.ToLower() == "png") {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets comment of image metadata.
        /// Returns "" (empty string) if no metadata is found or if reading metadata
        /// for the given file path is not supported in this library.
        /// </summary>
        /// <param name="imageFilePath">File path</param>
        /// <returns></returns>
        public static string GetImageComment(string imageFilePath) {

            GC.Collect();
            GC.WaitForPendingFinalizers();

            string comment = "";

            string ext = FileExtension(imageFilePath);

            if (ext.ToLower() == "jpg") {
                comment = GetJPGImageComment(imageFilePath);
            } else if (ext.ToLower() == "png") {
                comment = GetPNGImageComment(imageFilePath);
            }

            return comment;
        }

        /// <summary>
        /// Set Image metadata comment
        /// If metadata reading isn't supported, or failed,
        /// nothing happens
        /// </summary>
        /// <param name="imageFilePath">File path</param>
        /// <param name="comments">Comments to set metadata field to</param>
        public static void AddImageComment(string imageFilePath, string comments) {

            GC.Collect();
            GC.WaitForPendingFinalizers();

            string ext = FileExtension(imageFilePath);

            if (ext.ToLower() == "jpg") {
                AddJPGImageComment(imageFilePath, comments);
            } else if (ext.ToLower() == "png") {
                AddPNGImageComment(imageFilePath, comments);
            }

        }

        /// <summary>
        /// Get comment from metadata for JPG file
        /// </summary>
        /// <param name="imageFilePath">JPG file path</param>
        /// <returns>Comment from metadata</returns>
        public static string GetJPGImageComment(string imageFilePath) {

            string comment = "";

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(imageFilePath);

            foreach (var item in bmp.PropertyItems) {

                if (item.Id == 40092) {

                    string result = System.Text.Encoding.UTF8.GetString(item.Value);
                    return result;

                }

            }

            return comment;

        }

        /// <summary>
        /// Get comment from metadata for PNG file
        /// </summary>
        /// <param name="imageFilePath">PNG file path</param>
        /// <returns>Comment from metadata</returns>
        public static string GetPNGImageComment(string imageFilePath) {

            string comment = "";

            BitmapDecoder decoder = null;
            BitmapFrame bitmapFrame = null;
            FileInfo originalImage = new FileInfo(imageFilePath);

            if (File.Exists(imageFilePath)) {
                using (Stream streamIn = File.Open(imageFilePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
                    decoder = new PngBitmapDecoder(streamIn, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                }

                bitmapFrame = decoder.Frames[0];
                BitmapMetadata metadata = (BitmapMetadata)bitmapFrame.Metadata;

                if (bitmapFrame != null) {
                    BitmapMetadata metaData = (BitmapMetadata)bitmapFrame.Metadata.Clone();

                    if (metaData != null) {

                        // http://msdn.microsoft.com/en-us/library/windows/desktop/ee719904(v=vs.85).aspx

                        string o = (string)metadata.GetQuery("/iTXt/TextEntry");

                        comment = o;

                    }
                }
            }

            return comment;

        }

        /// <summary>
        /// Set comment in metadata of JPG file
        /// </summary>
        /// <param name="imageFilePath">JPG file path</param>
        public static void AddJPGImageComment(string imageFilePath, string comments) {

            if (File.Exists(imageFilePath)) {

                BitmapDecoder decoder;

                using (Stream streamIn = File.Open(imageFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
                    decoder = new JpegBitmapDecoder(streamIn, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                }

                BitmapFrame bitmapFrame = decoder.Frames[0];

                if (bitmapFrame != null) {
                    BitmapMetadata metaData = (BitmapMetadata)bitmapFrame.Metadata.Clone();

                    if (metaData != null) {

                        // modify the metadata   
                        // http://msdn.microsoft.com/en-us/library/windows/desktop/ee719904(v=vs.85).aspx#_jpeg_metadata
                        metaData.SetQuery("/app1/ifd/exif:{uint=40092}", comments);

                        // get an encoder to create a new jpg file with the new metadata.      
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapFrame, bitmapFrame.Thumbnail, metaData, bitmapFrame.ColorContexts));

                        FileInfo originalImage = new FileInfo(imageFilePath);

                        // delete the original
                        originalImage.Delete();

                        // save the image with metadata added
                        using (Stream streamOut = File.Open(imageFilePath, FileMode.CreateNew, FileAccess.ReadWrite)) {
                            encoder.Save(streamOut);
                        }
                        
                    }
                }
            }
        }

        /// <summary>
        /// Set comment in metadata of PNG file
        /// </summary>
        /// <param name="imageFilePath">PNG file path</param>
        public static void AddPNGImageComment(string imageFilePath, string comments) {

            if (File.Exists(imageFilePath)) {

                BitmapDecoder decoder;

                using (Stream streamIn = File.Open(imageFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
                    decoder = new PngBitmapDecoder(streamIn, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                }

                BitmapFrame bitmapFrame = decoder.Frames[0];

                if (bitmapFrame != null) {
                    BitmapMetadata metaData = (BitmapMetadata)bitmapFrame.Metadata.Clone();

                    if (metaData != null) {

                        // modify the metadata   
                        // http://msdn.microsoft.com/en-us/library/windows/desktop/ee719904(v=vs.85).aspx#_png_metadata
                        metaData.SetQuery("/iTXt/TextEntry", comments);

                        // get an encoder to create a new png file with the new metadata.      
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        
                        encoder.Frames.Add(BitmapFrame.Create(bitmapFrame, bitmapFrame.Thumbnail, metaData, bitmapFrame.ColorContexts));

                        FileInfo originalImage = new FileInfo(imageFilePath);

                        // delete the original
                        originalImage.Delete();

                        // save the image with metadata added
                        using (Stream streamOut = File.Open(imageFilePath, FileMode.CreateNew, FileAccess.ReadWrite)) {
                            encoder.Save(streamOut);
                        }
                    }
                }
            }
        }

    }
}
