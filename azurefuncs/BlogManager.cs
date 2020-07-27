using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using System.Net.Http;

namespace Rajirajcom.Api
{
    public class BlogManager
    {
        private static HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Get minstoread for passed in blog name + file content
        /// If metadata for blog doesn't exist in Azure blobs,
        /// compute and store info
        /// </summary>
        public async Task<string> GetMinsToRead(string name,
            string filePath,
            string storageConnectionString,
            ExecutionContext context)
        {
            BlogInfo info = new BlogInfo
            {
                Name = name,
                Path = filePath
            };
            return await AddMinsToRead(info,
                storageConnectionString,
                context);
        }

        private async Task<string> AddMinsToRead(BlogInfo blogInfo,
            string storageConnectionString,
            ExecutionContext context)
        {
            // Store all info in a single container
            BlobContainerClient container = new BlobContainerClient(
                storageConnectionString,
                "bloginfo");
            container.CreateIfNotExists();

            // create a new blob with the name and store
            // all metadata for the blob in that name
            BlobClient blob = container.GetBlobClient(blogInfo.Name);
            bool blobExists = blob.Exists();
            if (!blobExists)
            {
                await CreateNewBlog(blogInfo, context, blob);
            }

            // Return pre-computed content if info already
            // exists in the blog
            BlobProperties props = await blob.GetPropertiesAsync();
            if (props.Metadata.ContainsKey("minsToRead"))
            {
                return props.Metadata["minsToRead"];
            }

            // Open and read the file from its http location
            var response = await httpClient.GetAsync(blogInfo.Path);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException("Cannot read blog file");
            }

            // Store the metadata in the blob
            var responseContent = await response.Content.ReadAsStringAsync();
            var mins = ComputeMinsToRead(blob, blogInfo, responseContent);
            props.Metadata.Add("minsToRead", mins.ToString());
            blob.SetMetadata(props.Metadata);
            return mins.ToString();
        }

        private static async Task CreateNewBlog(BlogInfo blogInfo,
            ExecutionContext context, 
            BlobClient blob)
        {
            if (blogInfo.Path == null)
            {
                string pagesRootURL = ConfigReader.GetAppSettingOrDefault(context,
                    "contentfileroot",
                    "");
                if (pagesRootURL == "")
                {
                    throw new ApplicationException(
                        "Info with this blog name doesn't exist. " +
                        "Pass in a file path URL in the request to add it");
                }
                blogInfo.Path = ConfigReader.GetFileContentURL(
                   pagesRootURL, 
                   blogInfo.Name); 
            }

            // Store the file URL for blog in the blob content
            byte[] fileURL = Encoding.Unicode.GetBytes(blogInfo.Path);
            using (MemoryStream ms = new MemoryStream(fileURL))
            {
                await blob.UploadAsync(ms);
            }
        }

        private static int ComputeMinsToRead(BlobClient blob,
            BlogInfo blogInfo,
            string text)
        {
            string[] words = text.Split(
                new Char[] { ',', '\n', ' ', '\r' },
                StringSplitOptions.RemoveEmptyEntries
            );

            // average reading speed is 300 per min
            return (int)Math.Round(words.Length / 300.0);
        }
    }
}