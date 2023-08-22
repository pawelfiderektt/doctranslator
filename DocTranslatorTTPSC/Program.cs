using DocTranslatorTTPSC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions<DocumentTranslationConfig>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(nameof(DocumentTranslationConfig)).Bind(settings);
            });
        //string connectionString = "DefaultEndpointsProtocol=https;AccountName=sadoctranslatorttpsc;AccountKey=l1SQrAs1ZO39ZOX48GpPy8Y4/rTdxCAap9wJvyBjCyQwVjKsh1TWwJIIWRK492jc6A7ZO1VNDBwQ+AStQNWkYw==;EndpointSuffix=core.windows.net";
        services.AddSingleton<BlobService>(new BlobService(hostContext.Configuration["AzureWebJobsStorage"] ?? string.Empty));
    })
    .Build();

host.Run();
