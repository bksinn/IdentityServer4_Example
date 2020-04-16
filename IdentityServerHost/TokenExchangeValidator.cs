using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerHost
{
    public class TokenExchangeValidator : IExtensionGrantValidator
    {
        public const string GrantName = "urn:ietf:params:oauth:grant-type:token-exchange";

        const string SubjectToken = "subject_token";
        const string SubjectTokenType = "subject_token_type";

        const string IdTokenType = "urn:ietf:params:oauth:token-type:id_token";
        const string AccessTokenType = "urn:ietf:params:oauth:token-type:access_token";
        const string RefreshTokenType = "urn:ietf:params:oauth:token-type:refresh_token";

        private static readonly string[] ClaimFilter = {
            JwtClaimTypes.AccessTokenHash,
            JwtClaimTypes.Audience,
            JwtClaimTypes.AuthorizationCodeHash,
            JwtClaimTypes.AuthorizedParty,
            JwtClaimTypes.ClientId,
            JwtClaimTypes.Events,
            JwtClaimTypes.Expiration,
            JwtClaimTypes.IssuedAt,
            JwtClaimTypes.Issuer,
            JwtClaimTypes.JwtId,
            JwtClaimTypes.Nonce,
            JwtClaimTypes.NotBefore,
            JwtClaimTypes.ReferenceTokenId,
            JwtClaimTypes.Scope,
            JwtClaimTypes.StateHash,
            JwtClaimTypes.Subject,
        };

        private readonly ITokenValidator _tokenValidator;
        private readonly ILogger<TokenExchangeValidator> _logger;

        public TokenExchangeValidator(ITokenValidator tokenValidator, ILogger<TokenExchangeValidator> logger)
        {
            _tokenValidator = tokenValidator;
            _logger = logger;
        }

        public string GrantType => GrantName;

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var subject_token = context.Request.Raw[SubjectToken];
            var subject_token_type = context.Request.Raw[SubjectTokenType];

            string errorDescription = null;
            if (subject_token == null) errorDescription = "Missing " + SubjectToken;
            else if (subject_token_type == null) errorDescription = "Missing " + SubjectTokenType;

            if (errorDescription != null)
            {
                _logger.LogError(errorDescription);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, errorDescription);
                return;
            }

            var subjectResult =
                subject_token_type == IdTokenType ?
                    await _tokenValidator.ValidateIdentityTokenAsync(subject_token, validateLifetime: false) :
                    subject_token_type == AccessTokenType ?
                        await _tokenValidator.ValidateAccessTokenAsync(subject_token) :
                        subject_token_type == RefreshTokenType ?
                            await _tokenValidator.ValidateRefreshTokenAsync(subject_token) :
                            null;

            if (subjectResult.IsError)
            {
                _logger.LogError("Subject token failed to validate. {error} {errorDescription}", subjectResult.Error, subjectResult.ErrorDescription);
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "subject token validation failure");
                return;
            }

            var sub = subjectResult.Claims.SingleOrDefault(x => x.Type == JwtClaimTypes.Subject)?.Value;
            if (sub == null)
            {
                _logger.LogError("No subject claim in validated token");
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "No sub claim in subject token");
                return;
            }

            var claims = subjectResult.Claims.Where(x => !ClaimFilter.Contains(x.Type));
            context.Result = new GrantValidationResult(sub, GrantType, claims: claims.ToArray());

            _logger.LogDebug("Token exchange validator successful for subject {subjectId}", sub);
        }
    }
}
