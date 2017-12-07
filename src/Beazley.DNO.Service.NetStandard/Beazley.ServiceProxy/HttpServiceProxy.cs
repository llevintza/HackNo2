using System;
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

namespace Beazley.ServiceProxy
{
    public class HttpServiceProxy<TProxyType>
    {
        private readonly HttpRestClient _restClient;

        public HttpServiceProxy(string baseUrl)
        {
            _restClient = new HttpRestClient(baseUrl);
        }

        public Task<TResponse> PostAsync<TResponse>(Expression<Func<TProxyType, TResponse>> funcExpression, Func<HttpRestResponse, TResponse> callback)
            where TResponse : new()
        {
            return InvokeAsync(funcExpression, Method.POST, callback);
        }

        public Task PostAsync(Expression<Action<TProxyType>> funcExpression, Action<HttpRestResponse> callback)
        {
            return InvokeAsync(funcExpression, Method.POST, callback);
        }

        public Task<TResponse> GetAsync<TResponse>(Expression<Func<TProxyType, TResponse>> funcExpression, Func<HttpRestResponse, TResponse> callback)
            where TResponse : new()
        {
            return InvokeAsync(funcExpression, Method.GET, callback);
        }

        private Task<TResponse> InvokeAsync<TResponse>(Expression<Func<TProxyType, TResponse>> funcExpression, Method verb, Func<HttpRestResponse, TResponse> callback) where TResponse : new()
        {
            var request = this.CreateRequest(funcExpression.Body, verb);

            var result = new TaskCompletionSource<TResponse>();

            this._restClient.ExecuteAsync(request, response =>
             {
                 try
                 {
                     VerifyResponse(response);

                     result.TrySetResult(callback(response));
                 }
                 catch (Exception exception)
                 {
                     result.TrySetException(exception);
                 }
             });

            return result.Task;
        }

        private Task InvokeAsync(Expression<Action<TProxyType>> funcExpression, Method verb, Action<HttpRestResponse> callback)
        {
            var request = this.CreateRequest(funcExpression.Body, verb);

            var result = new TaskCompletionSource<bool>();

            this._restClient.ExecuteAsync(request, response =>
             {
                 try
                 {
                     VerifyResponse(response, isAction: true);
                     callback(response);

                     result.TrySetResult(true);
                 }
                 catch (Exception exception)
                 {
                     result.TrySetException(exception);
                 }
             });

            return result.Task;
        }


        private HttpRestRequest CreateRequest(Expression expression, Method? verb)
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
                method = (Method)((int)methodAttribute.Method);

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

            var request = new HttpRestRequest(resource, method);

            foreach (var argument in methdCallExpression.Arguments)
            {
                var lambda = Expression.Lambda(argument).Compile();
                var value = lambda.DynamicInvoke();
                var memberExpression = (MemberExpression)argument;

                //todo: add support for multiple parameters like Body,QueryString,UrlParams
                //if (argument.Type.IsClass && method != Method.GET)
                //{
                //    request.AddBody(value);
                //}
                //else
                //{
                //    request.AddParameter(memberExpression.Member.Name, value);
                //}

                request.AddMultipartFormData(memberExpression.Member.Name, new StringContent(value.ToString()));
            }

            request.UseDefaultCredentials = true;

            return request;
        }

        private void VerifyResponse(HttpRestResponse response, bool isAction = false)
        {

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                return;
            }

            if (isAction && response.StatusCode == HttpStatusCode.NoContent)
            {
                return;
            }

            throw new RestProxyException($"Failed to call webservice at:\n{response.Url}");
        }
    }
}
