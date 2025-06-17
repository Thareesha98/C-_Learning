using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NSwag;
using NSwag.CodeGeneration.CSharp;

public class ClientGenerator
{
    public async Task GenerateClient()
    {
        var document = await OpenApiDocument.FromUrlAsync("http://localhost:5000/swagger/v1/swagger.json");

        var settings = new CSharpClientGeneratorSettings
        {
            ClassName = "CustomApiClient",
            CSharpGeneratorSettings =
            {
                Namespace = "CustomNamespace"
            }
        };

        var generator = new CSharpClientGenerator(document, settings);
        var code = generator.GenerateFile();

        File.WriteAllText("GeneratedApiClient.cs", code);
    }
}
