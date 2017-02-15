function InitialiseFacebook(appId,isLogon) {

    window.fbAsyncInit = function () {
        FB.init({
            appId: appId,
            status: true,
            cookie: true,
            xfbml: true
        });

        function SubmitLogin(credentials) {
            $.ajax({
                url: "/account/UserDetail",
                type: "POST",
                data: credentials,
                error: function () {
                    window.location.reload();
                },
                success: function () {
                    isLogon = true;
                    window.location.reload();
                }
            });
        }

        FB.Event.subscribe('auth.login', function (response) {
            var credentials = { uid: response.authResponse.userID, accessToken: response.authResponse.accessToken };
            SubmitLogin(credentials);
        });

        FB.getLoginStatus(function (response) {
            if (response.status === 'connected') {
                    var credentials = { uid: response.authResponse.userID, accessToken: response.authResponse.accessToken };
                    if (!isLogon) {
                        logout();
                }
                else {
                    SubmitLogin(credentials);
                }
            }
            //else if (response.status === 'not_authorized')
            //{ alert("user is not authorised"); }
            //else
            //{ alert("user is not conntected to facebook"); }

        });

        //function SubmitLogin(credentials) {
        //    $.ajax({
        //        url: "/account/FaceLogin",
        //        type: "POST",
        //        data: credentials,
        //        error: function () {
                  
        //        },
        //        success: function () {
        //            window.location.href = "/User/Index";
        //        }
        //    });
        //}
    };

    (function (d) {
        var js, id = 'facebook-jssdk', ref = d.getElementsByTagName('script')[0];
        if (d.getElementById(id)) {
            return;
        }
        js = d.createElement('script');
        js.id = id;
        js.async = true;
        js.src = "//connect.facebook.net/en_US/all.js";
        ref.parentNode.insertBefore(js, ref);
    }(document));
}

function logout() {
    FB.logout(function (response) {
        // user is now logged out
    });
}