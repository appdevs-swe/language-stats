using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CollectStats_Functions.Services
{
    public static class BlobService
    {
        public static async Task StoreBlob(byte[] data, BlobContainerClient bcClient, string prefix)
        {
            var jsonStream = new MemoryStream(data);
            await bcClient.CreateIfNotExistsAsync();
            var blobClient = bcClient.GetBlobClient($"{prefix}-{DateTimeOffset.UtcNow.Date.ToShortDateString()}.json");
            await blobClient.UploadAsync(jsonStream);
        }
    }
}
