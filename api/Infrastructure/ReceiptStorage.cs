using System.Net;
using Amazon.S3;
using Amazon.S3.Model;

namespace Api.Infrastructure;

public class ReceiptStorage(IAmazonS3 s3)
{
    private const string Bucket = "receipts";

    public async Task<string> UploadAsync(Stream content, string contentType, CancellationToken ct)
    {
        var key = $"{Guid.NewGuid()}.jpg";

        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = Bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType
        }, ct);

        return key;
    }

    public string GetUrl(string key) =>
        s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = Bucket,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        try
        {
            await s3.GetObjectMetadataAsync(Bucket, key, ct);
            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
