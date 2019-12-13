using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AutoSaveImgClipboard.Helper
{
    public static class ImageHelper
    {
        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                //Get Params for save jpg
                ImageCodecInfo myImageCodecInfo;
                System.Drawing.Imaging.Encoder myEncoder;
                EncoderParameter myEncoderParameter;
                EncoderParameters myEncoderParameters;
                // Get an ImageCodecInfo object that represents the JPEG codec.
                myImageCodecInfo = GetEncoderInfo("image/jpeg");
                // for the Quality parameter category.
                myEncoder = System.Drawing.Imaging.Encoder.Quality;
                // EncoderParameter object in the array.
                myEncoderParameters = new EncoderParameters(1);
                // Save the bitmap as a JPEG file with quality level 75.
                myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                imageIn.Save(ms, myImageCodecInfo, myEncoderParameters);
                return ms.ToArray();
            }
        }
        public static string GetSHA1(byte[] byteArray)
        {
            string hash=String.Empty;
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            hash = Convert.ToBase64String(sha1.ComputeHash(byteArray));
            return hash;
        }
        public static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
