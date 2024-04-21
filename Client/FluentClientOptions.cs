namespace Pathoschild.Http.Client
{
    /// <summary>Options for the fluent client.</summary>
    public class FluentClientOptions
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to ignore null arguments when the request is dispatched. Default true if not specified.</summary>
        public bool? IgnoreNullArguments { get; set; }

        /// <summary>Whether HTTP error responses (e.g. HTTP 404) should be ignored (else raised as exceptions). Default false if not specified.</summary>
        public bool? IgnoreHttpErrors { get; set; }

        /// <summary>
        /// When <c>false</c>, indicates that the request should complete after reading the entire response including the content.
        /// When <c>true</c>, indicates that the request should complete as soon as a response is available and headers are read (the content is not read yet).
        /// Default <c>false</c> if not specified.
        /// </summary>
        public bool? StreamResponse { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the equivalent request options.</summary>
        internal RequestOptions ToRequestOptions()
        {
            return new RequestOptions
            {
                IgnoreHttpErrors = this.IgnoreHttpErrors,
                IgnoreNullArguments = this.IgnoreNullArguments,
                StreamResponse = this.StreamResponse,
            };
        }

        /// <summary>Copy the non-null values from the given options.</summary>
        /// <param name="options">The options to copy.</param>
        internal void MergeFrom(FluentClientOptions? options)
        {
            this.IgnoreNullArguments = options?.IgnoreNullArguments ?? this.IgnoreNullArguments;
            this.IgnoreHttpErrors = options?.IgnoreHttpErrors ?? this.IgnoreHttpErrors;
            this.StreamResponse = options?.StreamResponse ?? this.StreamResponse;
        }
    }
}
