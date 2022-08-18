using System;
using System.Text;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class KX12To13ConversionRequest
    {

        public DateTime lastModified { get; set; }
        public Guid nodeGuid { get; set; }
        public string nodeAliasPath { get; set; }
        public string siteCodeName { get; set; }
        public string documentCulture { get; set; }
        public Guid documentGuid { get; set; }
        public string documentPageTemplateConfiguration { get; set; }
        public string documentPageBuilderWidgets { get; set; }
        public string noonce { get; set; }
        public static string GetNoonce(KX12To13ConversionRequest request, string privateToken)
        {
            string input = $"{request.lastModified}{request.nodeGuid}{request.nodeAliasPath}{request.siteCodeName}{request.documentCulture}{request.documentGuid}{request.documentPageTemplateConfiguration}{request.documentPageBuilderWidgets}{privateToken}";
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string prior to .NET 5
                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
