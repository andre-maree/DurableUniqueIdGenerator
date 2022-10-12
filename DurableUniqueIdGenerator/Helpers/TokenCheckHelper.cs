using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DurableUniqueIdGenerator
{
    public static class TokenCheckHelper
    {
        public static bool CheckGenerateIdsKey(this HttpRequestMessage req)
        {
            System.Net.Http.Headers.AuthenticationHeaderValue authorizationHeader = req.Headers.Authorization;

            // Check that the Authorization header is present in the HTTP request and that it is in the
            // format of "Authorization: Bearer <token>"
            if (authorizationHeader == null ||
                authorizationHeader.Scheme.CompareTo("Bearer") != 0 ||
                String.IsNullOrEmpty(authorizationHeader.Parameter) ||
                !authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("GenerateIdsKey")))
            {
                return false;
            }

            return true;
        }

        public static bool CheckMasterKey(this HttpRequestMessage req)
        {
            System.Net.Http.Headers.AuthenticationHeaderValue authorizationHeader = req.Headers.Authorization;

            // Check that the Authorization header is present in the HTTP request and that it is in the
            // format of "Authorization: Bearer <token>"
            if (authorizationHeader == null ||
                authorizationHeader.Scheme.CompareTo("Bearer") != 0 ||
                String.IsNullOrEmpty(authorizationHeader.Parameter) ||
                !authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("MasterKey")))
            {
                return false;
            }

            return true;
        }

        public static bool CheckEitherGenerateIdsKeyOrMasterKey(this HttpRequestMessage req)
        {
            System.Net.Http.Headers.AuthenticationHeaderValue authorizationHeader = req.Headers.Authorization;

            // Check that the Authorization header is present in the HTTP request and that it is in the
            // format of "Authorization: Bearer <token>"
            if (authorizationHeader == null ||
                authorizationHeader.Scheme.CompareTo("Bearer") != 0 ||
                String.IsNullOrEmpty(authorizationHeader.Parameter) ||
                (!authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("MasterKey")) && !authorizationHeader.Parameter.Equals(Environment.GetEnvironmentVariable("GenerateIdsKey"))))
            {
                return false;
            }

            return true;
        }
    }
}
