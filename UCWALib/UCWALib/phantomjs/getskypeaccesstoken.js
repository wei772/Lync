var system = require('system');
var webPage = require('webpage');
var system = require('system');
var args = system.args;
var page = webPage.create();
var account = args[1];
var password = args[2];
//console.log("Account:" + account);
//console.log("Password:" + password);
if (!account || !password) {
  console.log("ERROR:Account or password is not specified.");
  phantom.exit();
}

var exit = function() {
    page.onLoadFinished = null;
    page.onUrlChanged = null;
    page.close();
    setTimeout(function(){
      phantom.exit();
    }, 100);
}

page.open('https://login.windows.net/common/oauth2/authorize?response_type=token&client_id=535f4d29-3bc7-4fe1-be19-d12a45004aca&redirect_uri=http://localhost/skypetest.html&resource=https://webdir0f.online.lync.com', function(status) {
  //console.log('Status: ' + status);
  if (status !== "success") {
    console.log("ERROR:Failed to open windows login page.");
    phantom.exit();
  }
  page.onLoadFinished = function(status) {
    if (status !== "success") {
      console.log("ERROR:Failed to open windows login page.");
      exit();
    }
    var isGrantPage = page.evaluate(function() {
        if ($("#cred_accept_button").length > 0) {
          $("#cred_accept_button").click();
          window.setInterval(function() {
            $("#cred_accept_button").click();
          }, 500);
          return true;
        }
        return false;
      });
      if (isGrantPage) {
        //console.log("Is Grant Page");
        return;
      }
      console.log("ERROR:Failed to login with current account.");
      exit();
  };
  page.onUrlChanged = function(targetUrl) {
    //console.log('New URL: ' + targetUrl);
    var accessTokenPos = targetUrl.indexOf('access_token=');
    if (accessTokenPos > -1) {
      accessTokenPos += 'access_token='.length;
      console.log("ACCESSTOKEN:" + targetUrl.substring(accessTokenPos, targetUrl.indexOf('&', accessTokenPos)));
      exit();
    }
  };
  page.evaluate(function(account, password) {
    $("#cred_userid_inputtext").val(account);
    $("#cred_password_inputtext").val(password);
    $("#cred_sign_in_button").click();
    window.setInterval(function() {
      $("#cred_sign_in_button").click();
    }, 500);
  }, account, password);
  //console.log("OK");
});
