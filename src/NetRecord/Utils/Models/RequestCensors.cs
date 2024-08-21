using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Web;
using NetRecord.Utils.Consts;
using NetRecord.Utils.Enums;
using NetRecord.Utils.Exceptions;
using NetRecord.Utils.Serialization;

namespace NetRecord.Utils.Models;

public class RequestCensors
{
    private readonly List<RequestCensorElement> _bodyElementsToCensor = new();
    private const string _censorText = "*****";
    private readonly List<RequestCensorElement> _headersToCensor = new();
    private readonly List<RequestCensorElement> _queryParamsToCensor = new();
    private readonly List<RegexRequestCensorElement> _pathElementsToCensor = new();

    public static RequestCensors None => new();
    
    public static RequestCensors DefaultSensitive
    {
        get
        {
            var censors = new RequestCensors();
            censors.CensorHeadersByKeys(StaticDefaults.CredentialHeadersToHide);
            censors.CensorQueryParametersByKeys(StaticDefaults.CredentialParametersToHide);
            censors.CensorBodyElementsByKeys(StaticDefaults.CredentialParametersToHide);

            return censors;
        }
    }

        /// <summary>
        ///     Add a rule to censor specified body elements.
        /// </summary>
        /// <param name="elements">List of body elements to censor.</param>
        /// <returns>The current Censor object.</returns>
        public RequestCensors CensorBodyElements(IEnumerable<RequestCensorElement> elements)
        {
            _bodyElementsToCensor.AddRange(elements);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified body elements by their keys.
        /// </summary>
        /// <param name="elementKeys">List of keys of body elements to censor.</param>
        /// <param name="caseSensitive">Whether to match case sensitively.</param>
        /// <returns></returns>
        public RequestCensors CensorBodyElementsByKeys(List<string> elementKeys, bool caseSensitive = false)
        {
            foreach (var key in elementKeys)
            {
                _bodyElementsToCensor.Add(new RequestCensorElement(key, caseSensitive));
            }

            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified headers.
        ///     Note: This will censor the header keys in both the request and response.
        /// </summary>
        /// <param name="headers">List of headers to censor.</param>
        /// <returns>The current Censor object.</returns>
        public RequestCensors CensorHeaders(IEnumerable<RequestCensorElement> headers)
        {
            _headersToCensor.AddRange(headers);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified headers by their keys.
        ///     Note: This will censor the header keys in both the request and response.
        /// </summary>
        /// <param name="headerKeys">List of keys of header to censor.</param>
        /// <param name="caseSensitive">Whether to match case sensitively.</param>
        /// <returns>The current Censor object.</returns>
        public RequestCensors CensorHeadersByKeys(List<string> headerKeys, bool caseSensitive = false)
        {
            foreach (var key in headerKeys)
            {
                _headersToCensor.Add(new RequestCensorElement(key, caseSensitive));
            }

            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified query parameters.
        /// </summary>
        /// <param name="elements">List of query parameters to censor.</param>
        /// <returns>The current Censor object.</returns>
        public RequestCensors CensorQueryParameters(IEnumerable<RequestCensorElement> elements)
        {
            _queryParamsToCensor.AddRange(elements);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified query parameters by their keys.
        /// </summary>
        /// <param name="parameterKeys">List of keys of query parameters to censor.</param>
        /// <param name="caseSensitive">Whether to match case sensitively.</param>
        /// <returns>The current Censor object.</returns>
        public RequestCensors CensorQueryParametersByKeys(List<string> parameterKeys, bool caseSensitive = false)
        {
            foreach (var key in parameterKeys)
            {
                _queryParamsToCensor.Add(new RequestCensorElement(key, caseSensitive));
            }

            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified path elements.
        /// </summary>
        /// <param name="elements">List of path elements to censor.</param>
        /// <returns>The current Censor object.</returns>
        public RequestCensors CensorPathElements(IEnumerable<RegexRequestCensorElement> elements)
        {
            _pathElementsToCensor.AddRange(elements);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified path elements by their patterns.
        /// </summary>
        /// <param name="patterns">List of patterns of path elements to censor.</param>
        /// <param name="caseSensitive">Whether to match case sensitively.</param>
        /// <returns>The current Censor object.</returns>
        public RequestCensors CensorPathElementsByPatterns(List<string> patterns, bool caseSensitive = false)
        {
            foreach (var pattern in patterns)
            {
                _pathElementsToCensor.Add(new RegexRequestCensorElement(pattern, caseSensitive));
            }

            return this;
        }

        /// <summary>
        ///     Censor the appropriate body parameters.
        /// </summary>
        /// <param name="body">String representation of request body to apply censors to.</param>
        /// <param name="contentType"> RequestBodyContentType enum indicating what type of content body is.</param>
        /// <returns>Censored string representation of request body.</returns>
        /// <exception cref="SerializeException">Could not serialize data to apply censors.</exception>
        internal string ApplyBodyParametersCensors(string body,  RequestBodyContentType? contentType, JsonSerializerOptions options)
        {
            if (contentType == null) throw new NetRecordException("Cannot determine content type of response body, unable to apply censors.");

            if (string.IsNullOrWhiteSpace(body))
            {
                // short circuit if body is null or empty
                return body;
            }

            if (_bodyElementsToCensor.Count == 0)
            {
                // short circuit if there are no censors to apply
                return body;
            }

            try
            {
                switch (contentType)
                {
                    case RequestBodyContentType.Text:
                    case  RequestBodyContentType.Html:
                        return body; // We can't censor plaintext bodies or HTML bodies.
                    case  RequestBodyContentType.Xml:
                        return body; // XML parsing is not supported yet, so we can't censor XML bodies.
                    case  RequestBodyContentType.Json:
                        return CensorJsonData(body, _censorText, _bodyElementsToCensor, options);
                    default:
                        throw new NetRecordException("Unrecognized content type: " + contentType);
                }
            }
            catch (SerializationException)
            {
                // short circuit if body is not a valid serializable type
                throw new NetRecordException("Body is not valid serializable type");
            }
        }

        /// <summary>
        ///     Censor the appropriate headers.
        /// </summary>
        /// <param name="headers">Dictionary of headers to apply censors to.</param>
        /// <returns>Censored IDictionary object.</returns>
        internal IDictionary<string, string> ApplyHeaderCensors(IDictionary<string, string> headers)
        {
            if (headers.Count == 0)
            {
                // short circuit if there are no headers to censor
                return headers;
            }

            return _headersToCensor.Count == 0 ? headers : headers.ToDictionary(header => header.Key, header => ElementShouldBeCensored(header.Key, _headersToCensor) ? _censorText : header.Value);
        }

        /// <summary>
        ///     Censor the appropriate path elements and query parameters.
        /// </summary>
        /// <param name="url">Full URL string to apply censors to.</param>
        /// <returns>Censored URL string.</returns>
        internal string? ApplyUrlCensors(string? url)
        {
            if (url == null)
            {
                // short circuit if url is null
                return url;
            }

            if (_queryParamsToCensor.Count == 0 && _pathElementsToCensor.Count == 0)
            {
                // short circuit if there are no censors to apply
                return url;
            }

            var uri = new Uri(url);

            var path = uri.GetLeftPart(UriPartial.Path); // bad function name, Microsoft. This gets the indicated portion of a URI (here, the full path minus query), not the left part of the path.
            if (path.EndsWith("?"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            var query = uri.Query;
            var queryParameters = HttpUtility.ParseQueryString(query);

            string censoredPath;
            string? censoredQueryString;

            if (_pathElementsToCensor.Count == 0)
            {
                // don't need to censor path elements
                censoredPath = path;
            }
            else
            {
                // censor path elements
                var tempPath = path;
                foreach (var pathCensor in _pathElementsToCensor)
                {
                    tempPath = pathCensor.MatchAndReplaceAsNeeded(tempPath, _censorText);
                }

                censoredPath = tempPath;
            }

            if (queryParameters.Count == 0)
            {
                // no query parameters to censor
                censoredQueryString = null;
            }
            else
            {
                if (_queryParamsToCensor.Count == 0)
                {
                    // don't need to censor query parameters
                    censoredQueryString = query;
                }
                else
                {
                    // censor query parameters
                    var censoredQueryParameters = new NameValueCollection();
                    foreach (var key in queryParameters.AllKeys)
                    {
                        if (key == null)
                        {
                            // short circuit if key is null
                            continue;
                        }
                        censoredQueryParameters.Add(key, ElementShouldBeCensored(key, _queryParamsToCensor) ? _censorText : queryParameters[key]);
                    }

                    censoredQueryString = ToQueryString(censoredQueryParameters);
                }
            }

            // build censored url
            var censoredUrl = censoredPath;
            if (censoredQueryString != null)
            {
                censoredUrl += $"?{censoredQueryString}";
            }

            return censoredUrl;
        }

        /// <summary>
        ///     Censor the appropriate path elements.
        /// </summary>
        /// <param name="url">Full URL string to apply censors to.</param>
        /// <returns>URL string with path elements censored. Query parameters will not be censored as part of this process.</returns>
        internal string? ApplyPathElementsCensors(string? url)
        {
            if (url == null)
            {
                // short circuit if url is null
                return url;
            }

            if (_pathElementsToCensor.Count == 0)
            {
                // short circuit if there are no censors to apply
                return url;
            }

            var uri = new Uri(url);
            var queryParameters = HttpUtility.ParseQueryString(uri.Query);

            var path = uri.GetLeftPart(UriPartial.Path); // bad function name, Microsoft. This gets the indicated portion of a URI (here, the full path minus query), not the left part of the path.

            foreach (var pathCensor in _pathElementsToCensor)
            {
                path = pathCensor.MatchAndReplaceAsNeeded(path, _censorText);
            }

            var censoredUrl = path;

            if (queryParameters.Count > 0)
            {
                censoredUrl = $"{censoredUrl}?{ToQueryString(queryParameters)}";
            }

            return censoredUrl;
        }

        /// <summary>
        ///     Apply censors to a JSON string.
        /// </summary>
        /// <param name="data">JSON string to apply censors to.</param>
        /// <param name="censorText">Test to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored JSON string.</returns>
        public static string CensorJsonData(string data, string censorText, IReadOnlyCollection<RequestCensorElement> elementsToCensors, JsonSerializerOptions options)
        {
            try
            {
                var jsonDictionary = JsonUtils.DeserializeJsonToObject<Dictionary<string, object>>(data, options);
                var censoredJsonDictionary = ApplyDataCensors(jsonDictionary, censorText, elementsToCensors);
                return JsonUtils.SerializeObjectToJson(censoredJsonDictionary, options);
            }
            catch (Exception)
            {
                // body is not a JSON dictionary
                try
                {
                    var jsonList = JsonUtils.DeserializeJsonToObject<List<object>>(data, options);
                    var censoredJsonList = ApplyDataCensors(jsonList, censorText, elementsToCensors);
                    return JsonUtils.SerializeObjectToJson(censoredJsonList, options);
                }
                catch
                {
                    // short circuit if body is not a JSON dictionary or JSON list
                    return data;
                }
            }
        }

        /// <summary>
        ///     Apply censors to an XML string.
        /// </summary>
        /// <param name="data">XML string to apply censors to.</param>
        /// <param name="censorText">Test to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored XML string.</returns>
        public static string CensorXmlData(string data, string censorText, IReadOnlyCollection<RequestCensorElement> elementsToCensors)
        {
            try
            {
                var xmlDictionary = XmlUtils.DeserializeXMLToObject<Dictionary<string, object>>(data);
                var censoredXmlDictionary = ApplyDataCensors(xmlDictionary, censorText, elementsToCensors);
                return XmlUtils.SerializeToXML(censoredXmlDictionary);
            }
            catch (Exception)
            {
                // body is not an XML dictionary
                try
                {
                    var xmlList = XmlUtils.DeserializeXMLToObject<List<object>>(data);
                    var censoredXmlList = ApplyDataCensors(xmlList, censorText, elementsToCensors);
                    return XmlUtils.SerializeToXML(censoredXmlList);
                }
                catch
                {
                    // short circuit if body is not a XML dictionary or XML list
                    return data;
                }
            }
        }

        /// <summary>
        ///     Apply censors to a list of elements.
        /// </summary>
        /// <param name="list">List of elements to apply censors to.</param>
        /// <param name="censorText">Test to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored list of elements.</returns>
        private static List<object> ApplyDataCensors(List<object> list, string censorText, IReadOnlyCollection<RequestCensorElement> elementsToCensors)
        {
            if (list.Count == 0)
                // short circuit if there are no body parameters
                return list;

            var censoredList = new List<object>();
            foreach (var entry in list)
            {
                if (entry is Dictionary<string, object> entryDict)
                {
                    if (entryDict == null)
                    {
                        // could not convert to dictionary, so skip (this should never happen)
                        censoredList.Add(entry);
                    }
                    else
                    {
                        var censoredEntryDict = ApplyDataCensors(entryDict, censorText, elementsToCensors);
                        censoredList.Add(censoredEntryDict);
                    }
                }
                else if (entry is List<object> entryList)
                {
                    if (entryList == null)
                    {
                        // could not convert to list, so skip (this should never happen)
                        censoredList.Add(entryList);
                    }
                    else
                    {
                        var censoredEntryList = ApplyDataCensors(entryList, censorText, elementsToCensors);
                        censoredList.Add(censoredEntryList);
                    }
                }
                else
                {
                    // either a primitive or null, no censoring needed
                    censoredList.Add(entry);
                }
            }

            return censoredList;
        }

        /// <summary>
        ///     Apply censors to a dictionary of elements.
        /// </summary>
        /// <param name="dictionary">Dictionary of elements to apply censors to.</param>
        /// <param name="censorText">Test to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored dictionary of elements.</returns>
        private static Dictionary<string, object> ApplyDataCensors(Dictionary<string, object> dictionary, string censorText, IReadOnlyCollection<RequestCensorElement> elementsToCensors)
        {
            if (dictionary.Count == 0)
                // short circuit if there are no body parameters
                return dictionary;

            var censoredBodyDictionary = new Dictionary<string, object>();
            foreach (var key in dictionary.Keys)
            {
                if (ElementShouldBeCensored(key, elementsToCensors))
                {
                    var value = dictionary[key];
                    if (value == null)
                    {
                        // don't need to worry about censoring something that's null (don't replace null with the censor string)
                        continue;
                    }

                    if (value is Dictionary<string, object>) 
                    {
                        // replace with empty dictionary
                        censoredBodyDictionary.Add(key, new Dictionary<string, object>());
                    }
                    else if (value is List<object>)
                    {
                        // replace with empty array
                        censoredBodyDictionary.Add(key, new List<object>());
                    }
                    else
                    {
                        // replace with censor text
                        censoredBodyDictionary.Add(key, censorText);
                    }
                }
                else
                {
                    var value = dictionary[key];

                    if (value is Dictionary<string, object> valueDict)
                    {
                        // recursively censor inner dictionaries
                        if (valueDict != null)
                        {
                            // change the value if can be parsed as a dictionary (otherwise, skip censoring)
                            value = ApplyDataCensors(valueDict, censorText, elementsToCensors);
                        }
                    }

                    else if (value is List<object> valueList)
                    {
                        // recursively censor list elements
                        if (valueList != null)
                        {
                            value = ApplyDataCensors(valueList, censorText, elementsToCensors);
                        }
                    }

                    censoredBodyDictionary.Add(key, value);
                }
            }

            return censoredBodyDictionary;
        }

        /// <summary>
        ///     Check if a JSON element should be censored.
        /// </summary>
        /// <param name="foundKey">The key of the JSON element to evaluate.</param>
        /// <param name="elementsToCensor">A list of elements to censor.</param>
        /// <returns>True if the JSON value should be censored, false otherwise.</returns>
        private static bool ElementShouldBeCensored(string foundKey, IReadOnlyCollection<RequestCensorElement> elementsToCensor)
        {
            return elementsToCensor.Count != 0 && elementsToCensor.Any(element => element.Matches(foundKey));
        }

        /// <summary>
        ///     Convert a collection of query parameter pairs to a query string.
        /// </summary>
        /// <param name="queryParamCollection">Collection of key-value pairs.</param>
        /// <returns>A formatted URL query string.</returns>
        private static string ToQueryString(NameValueCollection queryParamCollection)
        {
            return string.Join("&", queryParamCollection.AllKeys.Select(key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(queryParamCollection.Get(key))}").ToArray());
        }
}