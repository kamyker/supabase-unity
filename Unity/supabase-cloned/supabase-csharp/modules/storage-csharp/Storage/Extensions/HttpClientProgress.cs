using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Supabase.Storage.Extensions
{
    /// <summary>
    /// Adapted from: https://gist.github.com/dalexsoto/9fd3c5bdbe9f61a717d47c5843384d11
    /// </summary>
    internal static class HttpClientProgress
    {
        public static async Task<MemoryStream> DownloadDataAsync(this HttpClient client, Uri uri, Dictionary<string, string> headers = null, IProgress<float> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var destination = new MemoryStream();
            var message = new HttpRequestMessage(HttpMethod.Get, uri);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            using (var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead))
            {
                var contentLength = response.Content.Headers.ContentLength;
                using (var download = await response.Content.ReadAsStreamAsync())
                {
                    // no progress... no contentLength... very sad
                    if (progress is null || !contentLength.HasValue)
                    {
                        await download.CopyToAsync(destination);
                        return destination;
                    }
                    // Such progress and contentLength much reporting Wow!
                    var progressWrapper = new Progress<long>(totalBytes => progress.Report(GetProgressPercentage(totalBytes, contentLength.Value)));
                    await download.CopyToAsync(destination, 81920, progressWrapper, cancellationToken);
                }
            }

            float GetProgressPercentage(float totalBytes, float currentBytes) => (totalBytes / currentBytes) * 100f;

            return destination;
        }

        static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }

        public static Task<HttpResponseMessage> UploadFileAsync(this HttpClient client, Uri uri, string filePath, Dictionary<string, string> headers = null, Progress<float> progress = null)
        {
            var fileStream = new FileStream(filePath, mode: FileMode.Open, FileAccess.Read);
            return UploadAsync(client, uri, fileStream, headers, progress);
        }

        public static Task<HttpResponseMessage> UploadBytesAsync(this HttpClient client, Uri uri, byte[] data, Dictionary<string, string> headers = null, Progress<float> progress = null)
        {
            var stream = new MemoryStream(data);
            return UploadAsync(client, uri, stream, headers, progress);
        }

        public static async Task<HttpResponseMessage> UploadAsync(this HttpClient client, Uri uri, Stream stream, Dictionary<string, string> headers = null, Progress<float> progress = null)
        {
            var content = new ProgressableStreamContent(stream, 4096, progress);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (header.Key.Contains("content"))
                    {
                        content.Headers.Add(header.Key, header.Value);
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            }

            var response = await client.PostAsync(uri, content);

            return response;
        }
    }
}
