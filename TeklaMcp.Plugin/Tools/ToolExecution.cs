using System;
using System.Text.Json;

namespace TeklaMcp.Server.Tools
{
    internal static class ToolExecution
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static TArgs DeserializeRequired<TArgs>(JsonElement body, string invalidMessage)
        {
            var args = body.Deserialize<TArgs>(JsonOpts);
            if (args == null)
            {
                throw new ArgumentException(invalidMessage);
            }

            return args;
        }

        public static TArgs DeserializeOrDefault<TArgs>(JsonElement body)
            where TArgs : new()
        {
            return body.Deserialize<TArgs>(JsonOpts) ?? new TArgs();
        }

        public static TResult Run<TResult>(Func<TResult> action, Func<string, TResult> failureFactory)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                return failureFactory(ex.Message);
            }
        }
    }
}
