// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServerHost
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };


        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("api1", "My API #1"),
                new ApiResource("api2", "My API #2"),
            };


        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // SPA client using code flow + pkce
                new Client
                {
                    ClientId = "spa",
                    ClientName = "SPA Client",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,

                    RedirectUris =
                    {
                        "http://localhost:5002/index.html",
                        "http://localhost:5002/callback.html",
                        "http://localhost:5002/silent.html",
                        "http://localhost:5002/popup.html",
                        "http://localhost:4200/",
                        "http://localhost:4200/silent",
                        "http://localhost:4200/auth-callback",
                    },

                    PostLogoutRedirectUris = 
                    { 
                        "http://localhost:5002/index.html", 
                        "http://localhost:4200/", 
                    },
                    AllowedCorsOrigins = { 
                        "http://localhost:5002", 
                        "http://localhost:4200" 
                    },

                    AllowedScopes = { "openid", "profile", "api1" }
                },
                new Client
                {
                    ClientId = "api1",
                    ClientSecrets = { new Secret("api1_secret".Sha256()) },
                    AllowedGrantTypes = { TokenExchangeValidator.GrantName },
                    AllowedScopes = { "api2" }
                }
            };
    }
}