using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Beazley.ServiceProxy.Attributes;
using Beazley.ServiceProxy.Client;
using Beazley.ServiceProxy.Exceptions;
using Beazley.ServiceProxy.Models;
using Newtonsoft.Json;
using RestSharp;
using Method = RestSharp.Method;

namespace Beazley.ServiceProxy
{
    /// <summary>
    /// The rest service proxy.
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class RestServiceProxy<T>
    {
        /// <summary>
        /// The rest client.
        /// </summary>
        private readonly IRestClient restClient;

        /// <summary>
        /// The data format.
        /// </summary>
        private readonly DataFormat dataFormat;

        /// <summary>
        /// The inline parameter.
        /// </summary>
        private readonly string inlineParameter;
        
        
        // todo comments must be added to all elements from this class 

        public RestServiceProxy(string baseUrl, string inlineParameter)
        {
            //const string serviceUrl = baseUrl;
            this.restClient = new RestClient(baseUrl);
            this.dataFormat = DataFormat.Json;
            this.inlineParameter = inlineParameter;

            this.restClient.Proxy = new WebProxy("127.0.0.1:8888");
            // todo this must be injected, not created a new instance as now
            //this.webServiceConfiguration = new WebServiceConfigurationManager();
        }

        public RestServiceProxy(string baseUrl) : this(baseUrl, null) { }

        public TResponse Post<TResponse>(Expression<Func<T, TResponse>> funcExpression, Action<IRestRequest> interceptor = null) where TResponse : new()
        {
            return Invoke(funcExpression, interceptor, Method.POST);
        }

        public void Post(Expression<Action<T>> funcExpression, Action<IRestRequest> interceptor = null)
        {
            Invoke(funcExpression, interceptor, Method.POST);
        }

        public TResponse Get<TResponse>(Expression<Func<T, TResponse>> funcExpression,
            Action<IRestRequest> interceptor = null) where TResponse : new()
        {
            return Invoke(funcExpression, interceptor, Method.GET);
        }

        public Task<TResponse> PostAsync<TResponse>(Expression<Func<T, TResponse>> funcExpression, Action<IRestRequest> interceptor = null) where TResponse : new()
        {
            return InvokeAsync<TResponse>(funcExpression, interceptor, Method.POST);
        }

        public Task PostAsync(Expression<Action<T>> funcExpression, Action<IRestRequest> interceptor = null)
        {
            return InvokeAsync(funcExpression, interceptor, Method.POST);
        }

        public Task<TResponse> GetAsync<TResponse>(Expression<Func<T, TResponse>> funcExpression,
            Action<IRestRequest> interceptor = null) where TResponse : new()
        {
            return InvokeAsync<TResponse>(funcExpression, interceptor, Method.GET);
        }

        private TResponse Invoke<TResponse>(Expression<Func<T, TResponse>> funcExpression, Action<IRestRequest> interceptor, Method verb) where TResponse : new()
        {
            var request = this.CreateRequest(funcExpression.Body, verb);

            interceptor?.Invoke(request);

            var response = this.restClient.Execute<TResponse>(request);

            VerifyResponse(response);

            return response.Data;
        }

        private void Invoke(Expression<Action<T>> funcExpression, Action<IRestRequest> interceptor, Method verb)
        {

            var request = this.CreateRequest(funcExpression.Body, verb);

            var response = this.restClient.Execute(request);

            this.VerifyResponse(response);
        }

        private Task<TResponse> InvokeAsync<TResponse>(Expression<Func<T, TResponse>> funcExpression, Action<IRestRequest> interceptor, Method verb) where TResponse : new()
        {
            var request = this.CreateRequest(funcExpression.Body, verb);
            interceptor?.Invoke(request);

            var result = new TaskCompletionSource<TResponse>();

            this.restClient.ExecuteAsync<TResponse>(request, response =>
            {
                try
                {

                    VerifyResponse(response);

                    result.TrySetResult(response.Data);
                }
                catch (Exception exception)
                {
                    result.TrySetException(exception);
                }
            });

            return result.Task;
        }

        private Task InvokeAsync(Expression<Action<T>> funcExpression, Action<IRestRequest> interceptor, Method verb)
        {
            var request = this.CreateRequest(funcExpression.Body, verb);

            interceptor?.Invoke(request);

            var result = new TaskCompletionSource<bool>();

            this.restClient.ExecuteAsync<bool>(request, response =>
            {
                try
                {
                    VerifyResponse(response, isAction: true);
                    result.TrySetResult(response.Data);
                }
                catch (Exception exception)
                {
                    result.TrySetException(exception);
                }
            });

            return result.Task;
        }

        private IRestRequest CreateRequest(Expression expression, Method? verb)
        {
            var methdCallExpression = (MethodCallExpression)expression;
            var methodAttribute = methdCallExpression.Method.GetCustomAttributes().OfType<RestApiService>().FirstOrDefault();
            var resource = !string.IsNullOrWhiteSpace(methodAttribute.Resource)
                ? methodAttribute.Resource
                : methdCallExpression.Method.Name;
            var resourceContainer = new StringBuilder();
            var method = Method.POST;

            if (methodAttribute != null)
            {
                method = methodAttribute.Method;

                if (!string.IsNullOrWhiteSpace(methodAttribute.Route))
                {
                    resourceContainer.Append(methodAttribute.Route).Append("/");
                }

                resourceContainer.Append(resource);

                if (!string.IsNullOrWhiteSpace(methodAttribute.Params))
                {
                    resourceContainer.Append(methodAttribute.Params);
                }

                resource = resourceContainer.ToString();
            }

            if (verb.HasValue)
            {
                method = verb.Value;
            }


            var request = new RestRequest(resource, method)
            {
                RequestFormat = this.dataFormat,
                JsonSerializer = new RestSharpJsonNetSerializer(),
                Timeout = 1000 * 300
            };

            var credentials = this.GetAuthentocationCredentials();

            if (!string.IsNullOrWhiteSpace(credentials))
            {
                request.AddHeader("Authorization", credentials);
            }

            foreach (var argument in methdCallExpression.Arguments)
            {
                var lambda = Expression.Lambda(argument).Compile();
                var value = lambda.DynamicInvoke();
                var memberExpression = (MemberExpression)argument;
                var field = (FieldInfo)memberExpression.Member;

                var methodParameter = (from parameter in methdCallExpression.Method.GetParameters()
                                       where parameter.Name == memberExpression.Member.Name && parameter.ParameterType == field.FieldType
                                       select parameter).Single();
                var apiParam = methodParameter.GetCustomAttributes<RestApiParam>().FirstOrDefault();

                if (argument.Type.IsClass && method != Method.GET && apiParam == null)
                {
                    request.AddBody(value);
                }
                else
                {
                    var paramType = apiParam?.ParameterType ?? ParameterType.QueryString;
                    request.AddParameter(memberExpression.Member.Name, value, paramType);
                }
            }

            request.UseDefaultCredentials = true;

            return request;
        }

        private string GetAuthentocationCredentials()
        {

            HttpClient client = new HttpClient();
            var response = client.PostAsync("http://beazley-rulebookendava-2-2-config-server.rulebookservices.com/token", new StringContent("grant_type=password&username=dobro&password=endava01")).Result;

            var result = response.Content.ReadAsStringAsync().Result;

            var credentials = JsonConvert.DeserializeObject<Credentials>(result);

            if (credentials != null)
            {
                return $"{credentials.TokenType} {credentials.AccessToken}";
            }

            return string.Empty;
            // var request = new RestRequest("token", Method.POST)
            // {
            //     RequestFormat = this.dataFormat,
            //     //JsonSerializer = new RestSharpJsonNetSerializer(),
            //     Timeout = 1000 * 300
            // };

            // //request.AddParameter("grant_type", "password", ParameterType.RequestBody);
            // //request.AddParameter("username", "dobro", ParameterType.RequestBody);
            // //request.AddParameter("password", "endava01", ParameterType.RequestBody);
            // request.AddBody("grant_type=password&username=dobro&password=endava01");
            // request.


            // var result = new TaskCompletionSource<string>();

            // this.restClient.Proxy = new WebProxy("127.0.0.1:8888");

            // this.restClient.ExecuteAsync<Credentials>(request, response => {
            //     try
            //     {
            //         if (response.Data != null)
            //         {
            //             result.TrySetResult( $" {response.Data.TokenType} {response.Data.AccessToken}");
            //         }
            //     }
            //     catch (Exception)
            //     {
            //         result.TrySetResult(string.Empty);
            //     }
            // });

            //return result.Task.Result;

        }

        /// <summary>
        /// The verify response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="isAction">The is action.</param>
        /// <exception cref="RestProxyException">Rest Proxy Exception</exception>
        private void VerifyResponse(IRestResponse response, bool isAction = false)
        {
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new RestProxyException($"Status={response.ResponseStatus}", response.ErrorException);
            }

            if (response.StatusCode == HttpStatusCode.InternalServerError && ((dynamic)response).Data != null)
            {
                var errorMessage = this.GetErrorMessage(response);
                var stackTrace = errorMessage;

                throw new RestProxyException(errorMessage, new Exception(stackTrace));
            }

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                return;
            }

            if (isAction && response.StatusCode == HttpStatusCode.NoContent)
            {
                return;
            }

            throw new RestProxyException($"{response.StatusDescription}\n{response.ResponseUri.AbsoluteUri}", response.ErrorException);
        }

        /// <summary>
        /// The get error message from response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetErrorMessage(IRestResponse response)
        {
            // todo: this needs to be removed and figure out how to get the right field from which the error message to be retrieved
            try
            {
                return ((dynamic)response).Data.DisplayMessage;
            }
            catch
            {
                // ignored
            }

            try
            {
                return ((dynamic)response).Data.Message;
            }
            catch
            {
                // ignored
            }

            try
            {
                return ((dynamic)response).Data.ExceptionMessage;
            }
            catch
            {
                // ignored
            }

            try
            {
                var data = ((dynamic)response).Data;

                if (data is IEnumerable<dynamic>)
                {
                    var errorMessage = "";
                    foreach (var error in (data as IEnumerable<dynamic>).FirstOrDefault()?.Errors)
                    {
                        errorMessage = errorMessage + error.Message + Environment.NewLine;
                    }

                    return errorMessage;
                }
            }
            catch
            {
                // ignored
            }

            throw new RestProxyException($"{response.StatusDescription}\n{response.ResponseUri.AbsoluteUri}", response.ErrorException);
        }
    }
}
