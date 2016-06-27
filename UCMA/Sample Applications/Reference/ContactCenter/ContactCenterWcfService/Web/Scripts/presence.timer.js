Type.registerNamespace("Microsoft.Rtc.Collaboration.Sample.Controls.Presence");

Microsoft.Rtc.Collaboration.Sample.Controls.Presence.Timer = function (interval, callback, resetCallback) {
    this._interval = interval;
    this._timerID = null;
    this._userCallback = callback;
    this._resetCallback = resetCallback;

    // Define a delegate to invoke the user-defined callback
    this._callback = Function.createDelegate(this, this._callbackInternal);
}

Microsoft.Rtc.Collaboration.Sample.Controls.Presence.Timer.prototype = {

    start: function () {
        this._timerID = window.setTimeout(this._callback, this._interval)
    },

    stop: function () {
        window.clearTimeout(this._timerID);
        this._timerID = null;

        // Execute the user-defined clear function
        if (typeof (this._resetCallback) !== "undefined" && this._resetCallback !== null) {
            this._resetCallback();
        }
    },

    _callbackInternal: function () {
        // Invoke the user-defined callback
        if (this._userCallback !== null) {
            this._userCallback();
        }

        // Restart the timer if a reset-callback is provided
        if (typeof (this._resetCallback) !== "undefined" && this._resetCallback !== null) {
            this.start();
        }
    },

    initialize: function () {
        Microsoft.Rtc.Collaboration.Sample.Controls.Presence.Timer.callBaseMethod(this, "initialize", []);
    }
}


// Register the class with the Microsoft AJAX Library framework
Microsoft.Rtc.Collaboration.Sample.Controls.Presence.Timer.registerClass("Microsoft.Rtc.Collaboration.Sample.Controls.Presence.Timer");

// Notify ScriptManager that this is the end of the script.
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
