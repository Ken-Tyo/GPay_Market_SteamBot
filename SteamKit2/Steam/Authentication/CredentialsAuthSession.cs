/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamKit2.Internal;

namespace SteamKit2.Authentication
{
    /// <summary>
    /// Credentials based authentication session.
    /// </summary>
    /// 

    public sealed class CredentialsAuthSession : AuthSession
    {
        /// <summary>
        /// SteamID of the account logging in, will only be included if the credentials were correct.
        /// </summary>
        public SteamID SteamID { get; }
        public CAuthentication_BeginAuthSessionViaCredentials_Response responsex;

        //internal CredentialsAuthSession( SteamAuthentication authentication, IAuthenticator? authenticator, CAuthentication_BeginAuthSessionViaCredentials_Response response )
        public CredentialsAuthSession( SteamAuthentication authentication, IAuthenticator? authenticator, CAuthentication_BeginAuthSessionViaCredentials_Response response )
                : base( authentication, authenticator, response.client_id, response.request_id, response.allowed_confirmations, response.interval )
        {
            SteamID = new SteamID( response.steamid );
            responsex = response;
        }

        /// <summary>
        /// Send Steam Guard code for this authentication session.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="codeType">Type of code.</param>
        /// <returns></returns>
        /// <exception cref="AuthenticationException"></exception>
        public async Task<EResult> SendSteamGuardCodeAsync( string code, EAuthSessionGuardType codeType )
        {
            try
            {


                var request = new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request
                {
                    client_id = ClientID,
                    steamid = SteamID,
                    code = code,
                    code_type = codeType,
                };

                var message = await Authentication.AuthenticationService. SendMessage( api => api.UpdateAuthSessionWithSteamGuardCode( request ) );

                if ( message.Result == EResult.OK )
                {
                    var response = message.GetDeserializedResponse<CAuthentication_UpdateAuthSessionWithSteamGuardCode_Response>();
                }

                return message.Result;
                // can be InvalidLoginAuthCode, TwoFactorCodeMismatch, Expired
                //if ( message.Result != EResult.OK )
                //{
                //    throw new AuthenticationException( "Failed to send steam guard code", message.Result );
                //}
            }
            catch ( Exception ex )
            {
                return EResult.BadResponse;
            }
            // response may contain agreement_session_url
        }
    }
}
