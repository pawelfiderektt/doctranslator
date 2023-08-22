using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace DocTranslatorTTPSC
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public string GenerateBlobSasToken(string containerName, string blobName, BlobSasPermissions permissions)
        {
            var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b", // b for blob
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                Protocol = SasProtocol.Https
            };

            sasBuilder.SetPermissions(permissions);
            var sasToken = blobClient.GenerateSasUri(sasBuilder);

            return sasToken.ToString();
        }

        public string GenerateContainerSasToken(string containerName, BlobSasPermissions permissions)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Resource = "c", // c for container
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                Protocol = SasProtocol.Https,
            };

            sasBuilder.SetPermissions(permissions);
            var sasToken = containerClient.GenerateSasUri(sasBuilder);

            return sasToken.ToString();
        }
    }
}
