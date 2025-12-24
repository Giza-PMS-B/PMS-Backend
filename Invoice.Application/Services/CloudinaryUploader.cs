using System;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DotNetEnv;

namespace Invoice.Application.Services;

public class CloudinaryUploader
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryUploader()
    {
        var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

        var account = new Account(cloudName, apiKey, apiSecret);

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadHtmlAsync(string htmlContent, string fileName)
    {
        // Convert string to stream
        var bytes = Encoding.UTF8.GetBytes(htmlContent);
        using var stream = new MemoryStream(bytes);

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription($"{fileName}.html", stream),
            Folder = "html_docs",      // optional
            PublicId = fileName,      // optional
            Overwrite = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception(result.Error?.Message ?? "Upload failed");

        return result.SecureUrl.ToString();
    }
}
