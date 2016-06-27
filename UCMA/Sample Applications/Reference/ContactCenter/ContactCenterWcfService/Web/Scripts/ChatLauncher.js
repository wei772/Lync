
_ChatLauncher = function ()
{
}
_ChatLauncher.prototype =
{
	_window: null,
	_chatUrl: location.protocol + "//" + location.host + "/Chat.html",
	_windowName: "chatWindow",
	_width: 270,
	_height: 650,
	launch: function(context)
	{
		var options = [];
		options.push("width=" + this._width);
		options.push(",", "height=" + this._height);
		options.push(",", "resizable=yes");
		options.push(",", "scrollbars=no");
		options.push(",", "toolbar=no");
		options.push(",", "location=no");
		options.push(",", "directories=no");
		options.push(",", "status=no");
		options.push(",", "menubar=no");
		options.push(",", "copyhistory=no");
		this._window = window.open(this._chatUrl + "?" + context.toString(), this._windowName, options.join(), false); 
	},

	close: function()
	{
		if( this._window != null )
		{
			this._window.close();
		}
	}
}
var ChatLauncher = new _ChatLauncher();