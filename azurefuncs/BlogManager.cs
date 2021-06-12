using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using System.Net.Http;

namespace Rajirajcom.Api
{
    public class BlogManager
    {
        private static HttpClient httpClient = new HttpClient();

        public async Task<List<BlogInfo>> GetMinsToRead(List<BlogInfo> blogInfos,
            string storageConnectionString,
            string contentFileRoot)
        {
            /// <summary>
            /// Get the list of minstoread for passed in list of
            /// blog name + file content.
            /// If metadata for blog doesn't exist in Azure blobs,
            /// compute and store info
            /// </summary>
            List<BlogInfo> toRet = new List<BlogInfo>();
            foreach (var item in blogInfos)
            {
                var mins = await GetMinsToRead(item,
                        storageConnectionString,
                        contentFileRoot);

                toRet.Add(new BlogInfo
                {
                    Name = item.Name,
                    MinsToRead = mins
                });
            }
            return toRet;
        }

        /// <summary>
        /// Get comments for passed in blog name 
        /// If metadata for blog doesn't exist in Azure blobs,
        /// compute and store info
        /// </summary>
        public async Task<string> GetComments(BlogInfo blogInfo,
            string storageConnectionString,
            string contentFileRoot)
        {
            BlobClient blob = await CreateBlogInfoIfNotExists(blogInfo, contentFileRoot, storageConnectionString);         

            string comments = blogInfo.Comments;
            // Return pre-computed content if info already
            // exists in the blog
            BlobProperties props = await blob.GetPropertiesAsync();
            if (props.Metadata.ContainsKey("comments"))
            {
                string oldComments = 
                    Encoding.UTF8.GetString(Convert.FromBase64String(props.Metadata["comments"]));
                comments += oldComments;
            }
            
            props.Metadata["comments"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(comments));;
            blob.SetMetadata(props.Metadata);
            return comments;
        }

        /// <summary>
        /// Add a like for passed in blog name 
        /// If metadata for blog doesn't exist in Azure blobs,
        /// compute and store info
        /// </summary>
        public async Task<int> AddLike(BlogInfo blogInfo,
            string storageConnectionString,
            string contentFileRoot)
        {
            BlobClient blob = await CreateBlogInfoIfNotExists(blogInfo, contentFileRoot, storageConnectionString);
            
            int currentLike = blogInfo.Likes;
            // Return pre-computed content if info already
            // exists in the blog
            BlobProperties props = await blob.GetPropertiesAsync();
            if (props.Metadata.ContainsKey("likes"))
            {
                var oldLikes = Int32.Parse(props.Metadata["likes"]);
                currentLike += oldLikes;
            }
            
            props.Metadata["likes"] = currentLike.ToString();
            blob.SetMetadata(props.Metadata);
            return currentLike;
        }

        private async Task<BlobClient> CreateBlogInfoIfNotExists(
            BlogInfo blogInfo, 
            string contentFileRoot,
            string storageConnectionString)
        {
            if (blogInfo.Name == null)
            {
                throw new ApplicationException(
                    "Pass in a unique blogname in the list of names.");
            }
            // Flesh out the blog content path 
            GetBlogContentPath(blogInfo, contentFileRoot);

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
                await CreateNewBlog(blogInfo, blob);
            }
            return blob;
        }

        /// Get Metadata for a blog 
        /// If metadata for blog doesn't exist in Azure blobs,
        /// compute and store info
        /// </summary>
        public async Task<BlogInfo> GetBlogMetadata(BlogInfo info,
            string storageConnectionString,
            string contentFileRoot)
        {
            BlobClient blob = await CreateBlogInfoIfNotExists(info, contentFileRoot, storageConnectionString);         

            // Return pre-computed content if info already
            // exists in the blog
            BlobProperties props = await blob.GetPropertiesAsync();
            if (props.Metadata.ContainsKey("comments")) {
                info.Comments = 
                    Encoding.UTF8.GetString(Convert.FromBase64String(props.Metadata["comments"]));
            }
            info.Likes = Int32.Parse(props.Metadata["likes"]);
            info.MinsToRead = props.Metadata["minsToRead"];
            return info;
        }

        /// <summary>
        /// Get minstoread for passed in blog name + file content
        /// If metadata for blog doesn't exist in Azure blobs,
        /// compute and store info
        /// </summary>
        public async Task<string> GetMinsToRead(BlogInfo blogInfo,
            string storageConnectionString,
            string contentFileRoot)
        {
            BlobClient blob = await CreateBlogInfoIfNotExists(blogInfo, contentFileRoot, storageConnectionString);         

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
            BlobClient blob)
        {
            // Store the file URL for blog in the blob content
            byte[] fileURL = Encoding.Unicode.GetBytes(blogInfo.Path);
            using (MemoryStream ms = new MemoryStream(fileURL))
            {
                await blob.UploadAsync(ms);
            }
        }

        private static void GetBlogContentPath(BlogInfo blogInfo,            
            string contentFileRoot)
        {
            if (blogInfo.Path == null)
            {
                if (contentFileRoot == "")
                {
                    throw new ApplicationException(
                        "Info with this blog name doesn't exist. " +
                        "Pass in a file path URL in the request to add it");
                }
                blogInfo.Path = ConfigReader.GetFileContentURL(
                   contentFileRoot,
                   blogInfo.Name);
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

            // average reading speed is 300 per min + 
            // a few more to account HTML tags
            return (int)Math.Round(words.Length / 400.0);
        }
    }
}