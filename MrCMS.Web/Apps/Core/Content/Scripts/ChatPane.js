$(document).ready(function () {
 
    //create chat database on the client side
    window.indexedDB = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB;

    window.IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.msIDBTransaction;
    window.IDBKeyRange = window.IDBKeyRange || window.webkitIDBKeyRange || window.msIDBKeyRange

    if (!window.indexedDB) {
        window.alert("Your browser doesn't support a stable version of IndexedDB.");
    }
  
 var dbName = "NovoHub";

   var request = window.indexedDB.open(dbName, 2);
    
    request.onerror = function(event) {
        // Handle errors.
        console.log("error with opening database.");
    };
    
    request.onsuccess = function (event) {
        var db = event.target.result;
        console.log("Open successfully");

    };
    request.onupgradeneeded = function(event) {
        var db = event.target.result;
        //create the object store and specify for it to generate an id
        var objectStore = db.createObjectStore("Chat", { autoIncrement: true });
        console.log('Chat table Created');
        // Create an index to search chat by from id. We may have duplicates
        // so we can't use a unique index.
      
        objectStore.createIndex("fromid", "fromid", { unique: false });
        objectStore.createIndex("toid", "toidid", { unique: false });
        
        // Use transaction oncomplete to make sure the objectStore creation is 
        // finished before adding data into it.
        objectStore.transaction.oncomplete = function(event) {
            console.log('Added the table successfully');
        };
    };
    function getAllItems(callback) {
        var requestnew = window.indexedDB.open(dbName, 2);

        requestnew.onerror = function (event) {
            // Handle errors.
            console.log("error with opening database.");
        };
        requestnew.onsuccess = function(event) {
            var db = event.target.result;
            var trans = db.transaction("Chat", IDBTransaction.READ_ONLY);
            var store = trans.objectStore("Chat");
            var items = [];

            trans.oncomplete = function(evt) {
                callback(items);
            };

            var cursorRequest = store.openCursor();

            cursorRequest.onerror = function(error) {
                console.log(error);
            };

            cursorRequest.onsuccess = function(evt) {
                var cursor = evt.target.result;
                if (cursor) {
                    items.push(cursor.value);
                    cursor.continue();
                }
            };
        };
        }
        
    function getAllMessages(fromid, toid) {

        getAllItems(function (items) {
            var len = items.length;
            $("#chatpanel").empty();
            for (var i = 0; i < len; i += 1) {
                var fromIdd = items[i].fromid;
                var toIdd=items[i].toid;
                var mesg=items[i].message;
                var msgdate=items[i].msgdate;
                    
                if ((fromid == fromIdd && toid == toIdd )|| (fromid == toIdd  && toid == fromIdd)) {
                    //append to div
                    $("#chatpanel").append(mesg);
                    //console.log(mesg);
                }
            }
            //scroll to end
            //$("#chatpanel").animate({ scrollTop: $("#chatpanel").height() }, "slow");
            $("#chatpanel").animate({ scrollTop: $("#chatpanel")[0].scrollHeight }, 1000);
        });

      
        };

   
    //End of database section
    
    //toggle the contact tab select the first tab

    $("#control-sidebar-home-tab").toggle();
    
    //Contact bar click
    $(".contactBar").click(function (event) {
        //check if there was a click on it.

        var contentPanelId = jQuery(this).attr("id");
        // Reference the auto-generated proxy for the hub.
        var nhub = $.connection.clientNotificationHub;
        nhub.server.returnUserOnlineStatus(contentPanelId);
        // Start the connection.
        $.connection.hub.start().done(function () {
           
        });
        //now set data id of the chat panel to this id
        $("#chatpanel").attr("data-id", contentPanelId);
        var usersfullname = $('#' + contentPanelId + '-name').attr("data-id");
        //now set user full name
        $("#chatfullname").html('<i class="fa fa-user useroffline" id="Chatuserstatus"></i> Chat with ' + usersfullname);
        //Check user status
        var userstatus = $('#' + contentPanelId + '-status').attr("data-id");

        //if theres something in the archeive then replace it.
        // var archeive = $('#' + contentPanelId + '-store').html();
        //get all chat from database
        getAllMessages(window.usernameGlobal, contentPanelId);

        //$("#chatpanel").html(tony);
        //done replace 
        if (userstatus == "1") {

            $("#Chatuserstatus").removeClass("useroffline").addClass("useronline");

        } else {
            $("#Chatuserstatus").removeClass("useronline").addClass("useroffline");

        }

        //Clear the message bar
        $('#message').val('');

        //load the hidden div content


    });

  
});