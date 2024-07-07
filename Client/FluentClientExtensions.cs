using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pathoschild.Http.Client.Extensibility;
using Pathoschild.Http.Client.Internal;
using Pathoschild.Http.Client.Retry;

namespace Pathoschild.Http.Client
{
    /// <summary>Provides convenience methods for configuring the HTTP client.</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is a public API.")]
    public static class FluentClientExtensions
    {
        /*********
        ** Public methods
        *********/
        /****
        ** IClient
        ****/
        /// <summary>Remove all HTTP filters of the specified type.</summary>
        /// <typeparam name="TFilter">The filter type.</typeparam>
        /// <param name="filters">The filters to adjust.</param>
        /// <returns>Returns whether a filter was removed.</returns>
        public static bool Remove<TFilter>(this ICollection<IHttpFilter> filters)
            where TFilter : IHttpFilter
        {
            TFilter[] remove = filters.OfType<TFilter>().ToArray();
            foreach (TFilter filter in remove)
                filters.Remove(filter);
            return remove.Any();
        }

        /// <summary>Create an asynchronous HTTP DELETE request message (but don't dispatch it yet).</summary>
        /// <param name="client">The client.</param>
        /// <param name="resource">The URI to send the request to, or <c>null</c> to use the client's base URL (if set).</param>
        /// <returns>Returns a request builder.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        public static IRequest DeleteAsync(this IClient client, string? resource)
        {
            return client.SendAsync(HttpMethod.Delete, resource);
        }

        /// <summary>Create an asynchronous HTTP GET request message (but don't dispatch it yet).</summary>
        /// <param name="client">The client.</param>
        /// <param name="resource">The URI to send the request to, or <c>null</c> to use the client's base URL (if set).</param>
        /// <returns>Returns a request builder.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        public static IRequest GetAsync(this IClient client, string? resource)
        {
            return client.SendAsync(HttpMethod.Get, resource);
        }

        /// <summary>Create an asynchronous HTTP POST request message (but don't dispatch it yet).</summary>
        /// <param name="client">The client.</param>
        /// <param name="resource">The URI to send the request to, or <c>null</c> to use the client's base URL (if set).</param>
        /// <returns>Returns a request builder.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        public static IRequest PostAsync(this IClient client, string? resource)
        {
            return client.SendAsync(HttpMethod.Post, resource);
        }

        /// <summary>Create an asynchronous HTTP POST request message (but don't dispatch it yet).</summary>
        /// <param name="client">The client.</param>
        /// <typeparam name="TBody">The request body type.</typeparam>
        /// <param name="resource">The URI to send the request to, or <c>null</c> to use the client's base URL (if set).</param>
        /// <param name="body">The request body.</param>
        /// <returns>Returns a request builder.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        public static IRequest PostAsync<TBody>(this IClient client, string? resource, TBody body)
        {
            return client.PostAsync(resource).WithBody(body);
        }

        /// <summary>Create an asynchronous HTTP PUT request message (but don't dispatch it yet).</summary>
        /// <param name="client">The client.</param>
        /// <param name="resource">The URI to send the request to, or <c>null</c> to use the client's base URL (if set).</param>
        /// <returns>Returns a request builder.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        public static IRequest PutAsync(this IClient client, string? resource)
        {
            return client.SendAsync(HttpMethod.Put, resource);
        }

        /// <summary>Create an asynchronous HTTP PUT request message (but don't dispatch it yet).</summary>
        /// <param name="client">The client.</param>
        /// <typeparam name="TBody">The request body type.</typeparam>
        /// <param name="resource">The URI to send the request to, or <c>null</c> to use the client's base URL (if set).</param>
        /// <param name="body">The request body.</param>
        /// <returns>Returns a request builder.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        public static IRequest PutAsync<TBody>(this IClient client, string? resource, TBody body)
        {
            return client.PutAsync(resource).WithBody(body);
        }

        /// <summary>Create an asynchronous HTTP PATCH request message (but don't dispatch it yet).</summary>
        /// <param name="client">The client.</param>
        /// <param name="resource">The URI to send the request to, or <c>null</c> to use the client's base URL (if set).</param>
        /// <returns>Returns a request builder.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        public static IRequest PatchAsync(this IClient client, string? resource)
        {
            return client.SendAsync(new HttpMethod("PATCH"), resource);
        }

        /// <summary>Create an asynchronous HTTP PATCH request message (but don't dispatch it yet).</summary>
        /// <param name="client">The client.</param>
        /// <typeparam name="TBody">The request body type.</typeparam>
        /// <param name="resource">The URI to send the request to, or <c>null</c> to use the client's base URL (if set).</param>
        /// <param name="body">The request body.</param>
        /// <returns>Returns a request builder.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        public static IRequest PatchAsync<TBody>(this IClient client, string? resource, TBody body)
        {
            return client.PatchAsync(resource).WithBody(body);
        }

        /// <summary>Create an asynchronous HTTP request message (but don't dispatch it yet).</summary>
        /// <param name="client">The client.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="resource">The URI to send the request to, or <c>null</c> to use the client's base URL (if set).</param>
        /// <returns>Returns a request builder.</returns>
        /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
        public static IRequest SendAsync(this IClient client, HttpMethod method, string? resource)
        {
            Uri uri = FluentClientExtensions.ResolveFinalUrl(client.BaseClient.BaseAddress, resource) ?? throw new InvalidOperationException("Can't send a request with a null URL.");
            HttpRequestMessage message = Factory.GetRequestMessage(method, uri, client.Formatters);
            return client.SendAsync(message);
        }

        /// <summary>Set the default authentication header using basic auth.</summary>
        /// <param name="client">The client.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public static IClient SetBasicAuthentication(this IClient client, string username, string password)
        {
            return client.SetAuthentication("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Concat(username, ":", password))));
        }

        /// <summary>Set the default authentication header using a bearer token.</summary>
        /// <param name="client">The client.</param>
        /// <param name="token">The bearer token (typically an API key).</param>
        public static IClient SetBearerAuthentication(this IClient client, string token)
        {
            return client.SetAuthentication("Bearer", token);
        }

        /// <summary>Set the default request coordinator.</summary>
        /// <param name="client">The client.</param>
        /// <param name="shouldRetry">A method which returns whether a request should be retried.</param>
        /// <param name="intervals">The intervals between each retry attempt.</param>
        public static IClient SetRequestCoordinator(this IClient client, Func<HttpResponseMessage, bool> shouldRetry, params TimeSpan[] intervals)
        {
            return client.SetRequestCoordinator(new RetryCoordinator(shouldRetry, intervals));
        }

        /// <summary>Set the default request coordinator.</summary>
        /// <param name="client">The client.</param>
        /// <param name="maxRetries">The maximum number of times to retry a request before failing.</param>
        /// <param name="shouldRetry">A method which returns whether a request should be retried.</param>
        /// <param name="getDelay">A method which returns the time to wait until the next retry. This is passed the retry index (starting at 1) and the last HTTP response received.</param>
        public static IClient SetRequestCoordinator(this IClient client, int maxRetries, Func<HttpResponseMessage, bool> shouldRetry, Func<int, HttpResponseMessage, TimeSpan> getDelay)
        {
            return client.SetRequestCoordinator(new RetryCoordinator(maxRetries, shouldRetry, getDelay));
        }

        /// <summary>Set the default request coordinator.</summary>
        /// <param name="client">The client.</param>
        /// <param name="config">The retry configuration to apply (or null for the default coordinator).</param>
        public static IClient SetRequestCoordinator(this IClient client, IRetryConfig? config)
        {
            return client.SetRequestCoordinator(new RetryCoordinator(config));
        }

        /// <summary>Set the default request coordinator.</summary>
        /// <param name="client">The client.</param>
        /// <param name="configs">The retry configurations to apply (or null for the default behavior). Each configuration will have the opportunity to retry a request.</param>
        public static IClient SetRequestCoordinator(this IClient client, IEnumerable<IRetryConfig?>? configs)
        {
            return client.SetRequestCoordinator(new RetryCoordinator(configs));
        }

        /// <summary>Set default options for all requests.</summary>
        /// <param name="client">The client.</param>
        /// <param name="ignoreHttpErrors">Whether HTTP error responses like HTTP 404 should be ignored (<c>true</c>) or raised as exceptions (<c>false</c>). Default <c>false</c>.</param>
        /// <param name="ignoreNullArguments">Whether null arguments in the request body and URL query string should be ignored (<c>true</c>) or sent as-is (<c>false</c>). Default <c>true</c>.</param>
        /// <param name="completeWhen">When we should stop waiting for the response. For example, setting this to <see cref="HttpCompletionOption.ResponseHeadersRead"/> will let you handle the response as soon as the headers are received, before the full response body has been fetched. This only affects getting the <see cref="IResponse"/>; reading the response body (e.g. using a method like <see cref="IResponse.As{T}"/>) will still wait for the request body to be fetched as usual.</param>
        public static IClient SetOptions(this IClient client, bool? ignoreHttpErrors = null, bool? ignoreNullArguments = null, HttpCompletionOption? completeWhen = null)
        {
            return client.SetOptions(new FluentClientOptions
            {
                IgnoreHttpErrors = ignoreHttpErrors,
                IgnoreNullArguments = ignoreNullArguments,
                CompleteWhen = completeWhen
            });
        }

        /****
        ** IRequest
        ****/
        /// <summary>Add an authentication header using basic auth.</summary>
        /// <param name="request">The request.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public static IRequest WithBasicAuthentication(this IRequest request, string username, string password)
        {
            return request.WithAuthentication("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Concat(username, ":", password))));
        }

        /// <summary>Add an authentication header using a bearer token.</summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The bearer token (typically an API key).</param>
        public static IRequest WithBearerAuthentication(this IRequest request, string token)
        {
            return request.WithAuthentication("Bearer", token);
        }

        /// <summary>Set the body content of the HTTP request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="body">The model to serialize into the HTTP body content, or an <c>HttpContent</c> instance.</param>
        /// <returns>Returns the request builder for chaining.</returns>
        /// <exception cref="InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
        public static IRequest WithBody<T>(this IRequest request, T body)
        {
            return request.WithBody(builder => body switch
            {
                null => null,
                HttpContent content => content,
                _ => builder.Model(body)
            });
        }

        /// <summary>Set the request coordinator for this request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="shouldRetry">A lambda which returns whether a request should be retried.</param>
        /// <param name="intervals">The intervals between each retry attempt.</param>
        public static IRequest WithRequestCoordinator(this IRequest request, Func<HttpResponseMessage, bool> shouldRetry, params TimeSpan[] intervals)
        {
            return request.WithRequestCoordinator(new RetryCoordinator(shouldRetry, intervals));
        }

        /// <summary>Set the request coordinator for this request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="maxRetries">The maximum number of times to retry a request before failing.</param>
        /// <param name="shouldRetry">A method which returns whether a request should be retried.</param>
        /// <param name="getDelay">A method which returns the time to wait until the next retry. This is passed the retry index (starting at 1) and the last HTTP response received.</param>
        public static IRequest WithRequestCoordinator(this IRequest request, int maxRetries, Func<HttpResponseMessage, bool> shouldRetry, Func<int, HttpResponseMessage, TimeSpan> getDelay)
        {
            return request.WithRequestCoordinator(new RetryCoordinator(maxRetries, shouldRetry, getDelay));
        }

        /// <summary>Set the request coordinator for this request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="config">The retry config (or null for the default behavior).</param>
        public static IRequest WithRequestCoordinator(this IRequest request, IRetryConfig? config)
        {
            return request.WithRequestCoordinator(new RetryCoordinator(config));
        }

        /// <summary>Set the request coordinator for this request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="configs">The retry configurations to apply (or null for the default behavior). Each configuration will have the opportunity to retry a request.</param>
        public static IRequest WithRequestCoordinator(this IRequest request, IEnumerable<IRetryConfig?>? configs)
        {
            return request.WithRequestCoordinator(new RetryCoordinator(configs));
        }

        /// <summary>Set options for this request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="ignoreHttpErrors">Whether HTTP error responses like HTTP 404 should be ignored (<c>true</c>) or raised as exceptions (<c>false</c>). Default <c>false</c>.</param>
        /// <param name="ignoreNullArguments">Whether null arguments in the request body and URL query string should be ignored (<c>true</c>) or sent as-is (<c>false</c>). Default <c>true</c>.</param>
        /// <param name="completeWhen">When we should stop waiting for the response. For example, setting this to <see cref="HttpCompletionOption.ResponseHeadersRead"/> will let you handle the response as soon as the headers are received, before the full response body has been fetched. This only affects getting the <see cref="IResponse"/>; reading the response body (e.g. using a method like <see cref="IResponse.As{T}"/>) will still wait for the request body to be fetched as usual.</param>
        public static IRequest WithOptions(this IRequest request, bool? ignoreHttpErrors = null, bool? ignoreNullArguments = null, HttpCompletionOption? completeWhen = null)
        {
            return request.WithOptions(new RequestOptions
            {
                IgnoreHttpErrors = ignoreHttpErrors,
                IgnoreNullArguments = ignoreNullArguments,
                CompleteWhen = completeWhen
            });
        }


        /*********
        ** Internal methods
        *********/
        /// <summary>Get a copy of the request.</summary>
        /// <param name="request">The request to copy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <remarks>Note that cloning a request isn't possible after it's dispatched, because the content stream is automatically disposed after the request.</remarks>
        internal static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            HttpRequestMessage clone = new(request.Method, request.RequestUri)
            {
                Content = await request.Content.CloneAsync(cancellationToken).ConfigureAwait(false),
                Version = request.Version
            };

#if NET5_0_OR_GREATER
            foreach ((string key, object? value) in request.Options)
                clone.Options.Set(new HttpRequestOptionsKey<object?>(key), value);
#else
            foreach (var prop in request.Properties)
                clone.Properties.Add(prop);
#endif
            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clone;
        }

        /// <summary>Get a copy of the request content.</summary>
        /// <param name="content">The content to copy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <remarks>Note that cloning content isn't possible after it's dispatched, because the stream is automatically disposed after the request.</remarks>
        internal static async Task<HttpContent?> CloneAsync(this HttpContent? content, CancellationToken cancellationToken = default)
        {
            if (content == null)
                return null;

            Stream stream = new MemoryStream();
            await content
                .CopyToAsync(stream
#if NET5_0_OR_GREATER
                    , cancellationToken
#endif
                )
                .ConfigureAwait(false);
            stream.Position = 0;

            StreamContent clone = new(stream);
            foreach (var header in content.Headers)
                clone.Headers.Add(header.Key, header.Value);

            return clone;
        }

        /// <summary>Resolve the final URL for a request.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="resource">The requested resource, or <c>null</c> to use the base URL (if set).</param>
        private static Uri? ResolveFinalUrl(Uri? baseUrl, string? resource)
        {
            // ignore if empty or already absolute
            if (string.IsNullOrWhiteSpace(resource))
                return baseUrl;
            if (Uri.TryCreate(resource, UriKind.Absolute, out Uri? absoluteUrl) && !absoluteUrl.IsFile)
                return absoluteUrl;

            // can't combine if no base URL
            if (baseUrl == null)
                throw new FormatException($"Can't use relative URL '{resource}' because no base URL was specified.");

            // parse URLs
            resource = resource!.Trim();
            UriBuilder builder = new(baseUrl);

            // special case: combine if either side is a fragment
            if (!string.IsNullOrWhiteSpace(builder.Fragment) || resource.StartsWith('#'))
                return new Uri(baseUrl + resource);

            // special case: if resource is a query string, validate and append it
            if (resource.StartsWith('?') || resource.StartsWith('&'))
            {
                bool baseHasQuery = !string.IsNullOrWhiteSpace(builder.Query);
                return baseHasQuery switch
                {
                    true when resource.StartsWith('?') => throw new FormatException($"Can't add resource name '{resource}' to base URL '{baseUrl}' because the latter already has a query string."),
                    false when resource.StartsWith('&') => throw new FormatException($"Can't add resource name '{resource}' to base URL '{baseUrl}' because the latter doesn't have a query string."),
                    _ => new Uri(baseUrl + resource)
                };
            }

            // else make absolute URL
            if (!builder.Path.EndsWith('/'))
            {
                builder.Path += "/";
                baseUrl = builder.Uri;
            }
            return new Uri(baseUrl, resource);
        }
    }
}
