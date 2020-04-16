/// <reference path="./libs/oidc-client.d.ts" />

var config = {
    authority: "https://localhost:5001/",
    
    client_id: "spa",
    redirect_uri: window.location.origin + "/callback.html",
    post_logout_redirect_uri: window.location.origin + "/index.html",

    // these two will be done dynamically from the buttons clicked, but are
    // needed if you want to use the silent_renew
    response_type: "code",
    scope: "openid profile api1",

    // silent renew will get a new access_token via an iframe 
    // just prior to the old access_token expiring (60 seconds prior)
    silent_redirect_uri: window.location.origin + "/silent.html",
};
Oidc.Log.logger = window.console;
Oidc.Log.level = Oidc.Log.INFO;

var mgr = new Oidc.UserManager(config);

mgr.events.addAccessTokenExpiring(function () {
    log("Access token expiring...");
});
mgr.events.addSilentRenewError(function (err) {
    log("Silent renew error: ", err.message);
});
mgr.events.addUserSignedOut(function () {
    log("User signed out of OP");
});

function login() {
    mgr.signinRedirect();
    //mgr.signinRedirect({ acr_values: "tenant:bar" });
    //mgr.signinRedirect({ acr_values: "idp:adfs-wsfed" });
}

function renew() {
    mgr.signinSilent().then(function (user) {
        log("Silent Renew Success");
        showTokens();
    }).catch(function (err) {
        log("Silent Renew Error", err);
    });
}

function logout() {
    mgr.signoutRedirect();
}

function revoke() {
    mgr.revokeAccessToken().then(function (user) {
        log("Revoke Success");
        showTokens();
    }).catch(function (err) {
        log("Revoke Error", err);
    });
}

function callApi() {
    mgr.getUser().then(function (user) {
        var xhr = new XMLHttpRequest();
        xhr.onload = function (e) {
            if (xhr.status >= 400) {
                display("#ajax-result", {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    wwwAuthenticate: xhr.getResponseHeader("WWW-Authenticate")
                });
            }
            else {
                display("#ajax-result", xhr.response);
            }
        };
        xhr.open("GET", "https://localhost:44350/test", true);
        xhr.setRequestHeader("Authorization", "Bearer " + user.access_token);
        xhr.send();
    });
}

if (window.location.hash) {
    handleCallback();
}

document.querySelector(".login").addEventListener("click", login, false);
document.querySelector(".renew").addEventListener("click", renew, false);
document.querySelector(".call").addEventListener("click", callApi, false);
document.querySelector(".revoke").addEventListener("click", revoke, false);
document.querySelector(".logout").addEventListener("click", logout, false);


function log(data) {
    document.getElementById('response').innerText = '';

    Array.prototype.forEach.call(arguments, function (msg) {
        if (msg instanceof Error) {
            msg = "Error: " + msg.message;
        }
        else if (typeof msg !== 'string') {
            msg = JSON.stringify(msg, null, 2);
        }
        document.getElementById('response').innerHTML += msg + '\r\n';
    });
}

function display(selector, data) {
    if (data && typeof data === 'string') {
        try {
            data = JSON.parse(data);
        }
        catch (e) { }
    }
    if (data && typeof data !== 'string') {
        data = JSON.stringify(data, null, 2);
    }
    document.querySelector(selector).textContent = data;
}

function showTokens() {
    mgr.getUser().then(function (user) {
        if (user) {
            display("#id-token", user);
        }
        else {
            log("Not logged in");
        }
    });
}
showTokens();

function handleCallback() {
    mgr.signinRedirectCallback().then(function (user) {
        var hash = window.location.hash.substr(1);
        var result = hash.split('&').reduce(function (result, item) {
            var parts = item.split('=');
            result[parts[0]] = parts[1];
            return result;
        }, {});

        log(result);
        showTokens();

        window.history.replaceState({},
            window.document.title,
            window.location.origin + window.location.pathname);

    }, function (error) {
        log(error);
    });
}