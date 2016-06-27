var setProfileProps;

function pageLoad() {
    var userLoggedIn =
	    Sys.Services.AuthenticationService.get_isLoggedIn();
    // alert(userLoggedIn);

    profProperties = $get("setProfileProps");
    passwordEntry = $get("PwdId");

    if (userLoggedIn == true) {
        LoadProfile();
        GetElementById("setProfProps").style.visibility = "visible";
        GetElementById("logoutId").style.visibility = "visible";
    }
    else {
        DisplayInformation("User is not authenticated.");
    }

}


// The OnClickLogout function is called when 
// the user clicks the Logout button. 
// It logs out the current authenticated user.
function OnClickLogout() {
    Sys.Services.AuthenticationService.logout(
	    null, OnLogoutComplete, AuthenticationFailedCallback, null);
}


function OnLogoutComplete(result,
    userContext, methodName) {
    // Code that performs logout 
    // housekeeping goes here.			
}



// This is the callback function called 
// if the authentication failed.
function AuthenticationFailedCallback(error_object,
    userContext, methodName) {
    DisplayInformation("Authentication failed with this error: " +
	    error_object.get_message());
}



// Loads the profile of the current
// authenticated user.
function LoadProfile() {
    Sys.Services.ProfileService.load(null,
	    LoadCompletedCallback, ProfileFailedCallback, null);

}

// Saves the new profile
// information entered by the user.
function SaveProfile() {

    // Set background color.
    Sys.Services.ProfileService.properties.Backgroundcolor =
	    GetElementById("bgcolor").value;

    // Set foreground color.
    Sys.Services.ProfileService.properties.Foregroundcolor =
	    GetElementById("fgcolor").value;

    // Save profile information.
    Sys.Services.ProfileService.save(null,
	    SaveCompletedCallback, ProfileFailedCallback, null);

}

// Reads the profile information and displays it.
function LoadCompletedCallback(numProperties, userContext, methodName) {
    document.bgColor =
	    Sys.Services.ProfileService.properties.Backgroundcolor;

    document.fgColor =
	    Sys.Services.ProfileService.properties.Foregroundcolor;
}

// This is the callback function called 
// if the profile was saved successfully.
function SaveCompletedCallback(numProperties, userContext, methodName) {
    LoadProfile();
    // Hide the area that contains 
    // the controls to set the profile properties.
    SetProfileControlsVisibility("hidden");
}

// This is the callback function called 
// if the profile load or save operations failed.
function ProfileFailedCallback(error_object, userContext, methodName) {
    alert("Profile service failed with message: " +
	        error_object.get_message());
}


// Utility functions.

// This function sets the visibilty for the
// area containing the page elements for settings
// profiles.
function SetProfileControlsVisibility(currentVisibility) {
    profProperties.style.visibility = currentVisibility;
}

// Utility function to display user's information.
function DisplayInformation(text) {
    alert(text);
}


function GetElementById(elementId) {
    var element = document.getElementById(elementId);
    return element;
}

if (typeof (Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
