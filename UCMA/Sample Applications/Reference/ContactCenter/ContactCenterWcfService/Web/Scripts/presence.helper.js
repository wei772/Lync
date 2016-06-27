//Math.floor(Math.random() * 100000000 + 1)

var PresenceHelper = function () {
    return {
        refreshInterval: 5000,
        errorMessage: "Unable to load availability",
        webContext: { r: Math.random() },

        dataContext: Sys.create.adoNetDataContext({
            serviceUri: location.protocol + "//" + location.host + "/ContactCenterWcfService.svc/Presence"
        }),

        getPresenceAvailability: function () {
            var dv = $get("contacts");
            dv.control.set_fetchParameters({ r: Math.random() });
        },

        fetchFailed: function () {
            $get("presence-error").style.display = "block";
            $get("presence-error").innerHTML = PresenceHelper.errorMessage;
        },

        fetchSuccess: function () {
            $get("presence-error").style.display = "none";
        },

        presenceCallback: function () {
            //  
        },

        getTicTac: function (availability) {
            var presence = "availability unknown";
            if (availability < 3000) { presence = "availability unknown"; }
            else if (availability < 4500) { presence = "availability available"; }
            else if (availability < 6000) { presence = "availability away"; } //was idleOnline
            else if (availability < 7500) { presence = "availability busy"; }
            else if (availability < 9000) { presence = "availability away"; } //was idleBusy
            else if (availability < 12000) { presence = "availability dnd"; }
            else if (availability < 18000) { presence = "availability away"; }
            else { presence = "availability offline"; }
            return presence;
        },

        loadChatWindow: function (i) {
            var un = $get('ContentPlaceholder_txtUserName');
            var up = $get('ContentPlaceholder_txtUserPhone');
            ChatLauncher.launch('queueName=' + i.EntityName + '&un=' + un.value + '&up=' + up.value);
        }
    };
} ();

var availabilityTimer = new Microsoft.Rtc.Collaboration.Sample.Controls.Presence.Timer(PresenceHelper.refreshInterval, PresenceHelper.getPresenceAvailability, PresenceHelper.presenceCallback);
availabilityTimer.start();