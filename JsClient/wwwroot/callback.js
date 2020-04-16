var mgr = new Oidc.UserManager({response_mode:"query"});
mgr.signinRedirectCallback().then(function (user) {
    console.log(user);

    window.history.replaceState({},
        window.document.title,
        window.location.origin + window.location.pathname);

    window.location = "index.html";
}).catch(function (e) {
    if (e.message === "access_denied") {
        window.location = "index.html";
    }
    else {
        console.error(e);
    }
});
