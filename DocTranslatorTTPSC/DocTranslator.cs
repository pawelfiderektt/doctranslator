using Azure;
using Azure.AI.Translation.Document;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocTranslatorTTPSC
{
    public class DocTranslator
    {
        private readonly ILogger _logger;
        private readonly BlobService _blobService;
        private readonly DocumentTranslationConfig _documentTranslationConfig;

        public DocTranslator(ILoggerFactory loggerFactory, BlobService blobService, IOptions<DocumentTranslationConfig> documentTranslationConfig)
        {
            _blobService = blobService;
            _logger = loggerFactory.CreateLogger<DocTranslator>();
            _documentTranslationConfig = documentTranslationConfig.Value;
        }

        [Function("DocTranslator")]
        public async Task Run([BlobTrigger("documents/{name}", Connection = "AzureWebJobsStorage")] string myBlob, string name)
        {
            string documentTranslationEndpoint = _documentTranslationConfig.Endpoint; //"https://doctranslatorttpsc.cognitiveservices.azure.com/";
            string documentTranslationApiKey = _documentTranslationConfig.ApiKey; //"7fe01825f95544d098e06606d88e8827";

            var client = new DocumentTranslationClient(new Uri(documentTranslationEndpoint), new AzureKeyCredential(documentTranslationApiKey));

            //TODO: add privilages
            var permissions = BlobSasPermissions.All;
            var input = new DocumentTranslationInput(new TranslationSource(new Uri(_blobService.GenerateBlobSasToken("documentspl", name, permissions))), new TranslationTarget[]
            {
                new TranslationTarget(
                    new Uri(_blobService.GenerateContainerSasToken("translated-documentspl", permissions)), "eng")
            })
            {
                StorageUriKind = StorageInputUriKind.File
            };

            DocumentTranslationOperation operation = await client.StartTranslationAsync(input);

            await operation.WaitForCompletionAsync();

            await foreach (DocumentStatusResult document in operation.Value)
            {
                Console.WriteLine($"Document with Id: {document.Id}");
                Console.WriteLine($"  Status:{document.Status}");
                if (document.Status == DocumentTranslationStatus.Succeeded)
                {
                    Console.WriteLine($"  Translated Document Uri: {document.TranslatedDocumentUri}");
                    Console.WriteLine($"  Translated to language code: {document.TranslatedToLanguageCode}.");
                    Console.WriteLine($"  Document source Uri: {document.SourceDocumentUri}");
                }
                else
                {
                    Console.WriteLine($"  Error Code: {document.Error.Code}");
                    Console.WriteLine($"  Message: {document.Error.Message}");
                }
            }
        }


    }
}
