
var PersistentNotifications = function () {
    var initialized = false;
    var updateNotificationCount = function () {
        $.get("/CoreHub/GetCount", function (data) {
            $('[data-notification-count]').html(data);
        });
    };
    var container = $('[data-notification-list]');
    var initializeNotifications = function () {
        $.get('/CoreHub/Get', function (notifications) {
            container.empty();
            if (notifications.length) {
                notifications.reverse();
                $.each(notifications, function (idx, notification) {
                    prependNotification(notification);
                });
                updateNotificationCount();
            } else {
                resetNotifications();
            }
            initialized = true;
        });
    };
    var displayNotification = function (notification) {

        return $('<li>').attr('data-notification', true).html('<a href="' + notification.ClickAction + '"><i class="fa fa-bell "></i> ' + notification.Message + '</a>');
    };
    var displayNoNotifications = function () {
        return $('<li>').html('No notifications');
    };
    var prependNotification = function (notification) {

        container.prepend(displayNotification(notification));
        if (initialized) {
            updateNotificationCount();
        }
    };
    var resetNotifications = function () {
        container.empty();
        container.append(displayNoNotifications());
        updateNotificationCount();
    };
    var markAllAsRead = function (event) {
        event.preventDefault();
        $.post($(event.target).attr('action'), function () {
            initialized = false;
            initializeNotifications();
        });
    };
    return {
        init: function () {
            initializeNotifications();
            $(document).on('submit', '[data-mark-all-as-read]', markAllAsRead);


        }
    };
};



$(function () {
    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-bottom-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "20000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
    
    new PersistentNotifications().init();
    window.usernameGlobal = "";
    // Reference the auto-generated proxy for the hub.
    var nhub = $.connection.clientNotificationHub;
   
  

    nhub.client.setUsername = function (username) {
        window.usernameGlobal = username;

    };
    // Create a function that the hub can call back to refresh notification.
    nhub.client.sendPersistentNotification = function (userId) {
        if (userId == window.usernameGlobal) {

            //refresh notification
            new PersistentNotifications().init();
        }


    };
    nhub.client.sendTransientNotification = function (username, title, message, type) {

        //Send TransientNotification

        //if the username changes then the stuff aint for you.

        if (username == window.usernameGlobal) {


            if (type == 0) {
                toastr["error"](message, title);
            }
            if (type == 1) {
                toastr["warning"](message, title);
            }
            if (type == 2) {
                toastr["info"](message, title);
            }
            if (type == 3) {
                toastr["success"](message, title);
            }
        }
    };
    nhub.client.sendAuthCodeToCallCenter = function (providerid,providerName, policynumber, fullname, diagnosis, reasonforcode) {
        //notifiy everyone that a code was requested for 

        var roleid = $("#___iscallcenter").val();
        //alert(roleid);
        
        if (roleid == 12 || roleid == 13) {
            toastr["info"](providerName + ' UPN:' + providerid + ' just requested for authorization code for ' + fullname + ' ' + policynumber + '  the diagnosis is ( ' + diagnosis + ' ) and the reason for the code is ( ' + reasonforcode + ' )', 'Provider Code Request');

        }
        
    
        //swal(
        //            'Provider Code Request!',
        //            providerName + ' UPN:' + providerid  + ' just requested for authorization code for ' + fullname + ' (' + policynumber + ' ) the diagnosis is ( ' + diagnosis + ' ) and the reason for the code is ( ' + reasonforcode + ' )',
        //            'success'
        //            );
    };

    //Chat side
    // Create a function that the hub can call back to refresh notification.
    nhub.client.showuserOnline = function (userId, status) {

        //get the user online object
        $('#' + userId + '-status').attr("data-id", status);


    };
    
    //Refresh the verification code page
    nhub.client.refreshVerificationPageList = function () {
        //if ($('#verificationlist').length) {
        //    //get the user online object
        //    $('#verificationlist').DataTable().ajax.reload();
        //}
    
        //alert('refreshed');


    };
    //Receive message
    nhub.client.receiveMsg = function (userId, toUserId, message, dateofmsg) {

        if (toUserId == window.usernameGlobal) {

            //Add to the hidden div of the user
            //var touserFullName = $('#' + toUserId + '-name').attr("data-id");
            var myFullName = $('#' + userId + '-name').attr("data-id");
            var large = '<div class="direct-chat-msg right"> <div class="direct-chat-info clearfix"> <span class="direct-chat-name pull-right">' + myFullName + '</span> <span class="direct-chat-timestamp pull-left">' + dateofmsg + '</span> </div> <!-- /.direct-chat-info --> <img alt="message user image" src="data:image/png;base64,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" class="direct-chat-img"><!-- /.direct-chat-img --> <div class="direct-chat-text leftchattext"> ' + message + ' </div> <!-- /.direct-chat-text --> </div>'
            //add to chat backup
            // $('#' + userId + '-store').append(large);



            //add to database newly added 
            addtochattable(userId, toUserId, large, dateofmsg);

            var id = $("#chatpanel").attr("data-id");

            if (id == userId) {
                //add to chat pane if the user corepondes with the present user
                $('#chatpanel').append(large);
                //scroll to end
                $("#chatpanel").animate({ scrollTop: $("#chatpanel")[0].scrollHeight }, 1000);
                //$("#chatpanel").animate({ scrollTop: $("#chatpanel").height() }, "slow");
            } else {
                //Do notification that a new message has been received.
                toastr["info"](message, 'New Message from ' + myFullName);

            }


        }


    };
    
    // Start the connection.
    $.connection.hub.start().done(function () {

    //Check if the textbox is sent
    $('#message').keyup(function (e) {
        if (e.keyCode == 13) {
            //Enter has been clicked
            nhub.server.send(window.usernameGlobal, $("#chatpanel").attr("data-id"), $('#message').val());
            //add to the userid store
            //get the status

            //var touserFullName = $('#' + $("#chatpanel").attr("data-id") + '-name').attr("data-id");
            var myFullName = $('#' + window.usernameGlobal + '-name').attr("data-id");
            var today = new Date();
            var dd = today.getDate();
            var mm = today.getMonth() + 1; //January is 0!
            var hH = today.getHours();
            var mM = today.getMinutes();

            var yyyy = today.getFullYear();
            if (dd < 10) {
                dd = '0' + dd;
            }
            if (mm < 10) {
                mm = '0' + mm;
            }
            today = dd + '/' + mm + '/' + yyyy + ' ' + hH + ':' + mM;
            var dateofmsg = today;
            var message = $('#message').val();
            var large = '<div class="direct-chat-msg"> <div class="direct-chat-info clearfix"> <span class="direct-chat-name pull-left">' + myFullName + '</span> <span class="direct-chat-timestamp pull-right">' + dateofmsg + '</span> </div> <!-- /.direct-chat-info --> <img alt="message user image" src="data:image/png;base64,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" class="direct-chat-img"><!-- /.direct-chat-img --> <div class="direct-chat-text rightchattext">' + message + '</div> <!-- /.direct-chat-text --> </div>';
            //add to chat backup
            // $('#' + $("#chatpanel").attr("data-id") + '-store').append(large);

            //add to database newly added 
            addtochattable(window.usernameGlobal, $("#chatpanel").attr("data-id"), large, dateofmsg);


            $('#chatpanel').append(large);
            //scroll to end
            $("#chatpanel").animate({ scrollTop: $("#chatpanel")[0].scrollHeight }, 1000);
            // $("#chatpanel").animate({ scrollTop: $("#chatpanel").height() }, "slow");


            $('#message').val('');

        }
    });
});

   

    

//add chat to database
function addtochattable(fromid, toid, message, msgdate) {

    var dbName = "NovoHub";
    var request = window.indexedDB.open(dbName, 2);

    request.onerror = function (event) {
        // Handle errors.
        console.log("error with opening database.");
    };

    request.onsuccess = function (event) {
        var db = event.target.result;

        var transaction = db.transaction("Chat", "readwrite");
        transaction.oncomplete = function (event) {
            console.log("Open Table successfully.");
        };

        transaction.onerror = function (event) {
            // Don't forget to handle errors!
            console.log("error opening table.");
        };

        //opem the chat object store
        var objectStore = transaction.objectStore("Chat");

        var requestnew = objectStore.add({ fromid: fromid, toid: toid, message: message, msgdate: msgdate });
        requestnew.onsuccess = function (event) {
            console.log('added chat message successfully');
        };

        requestnew.onerror = function(event) {
            console.log("Unable to add chat message to database");
        };

    };


}
  
var messages = function getAllMessages(fromid, toid) {

    var dbName = "NovoHub";
    var request = window.indexedDB.open(dbName, 2);

    request.onerror = function (event) {
        // Handle errors.
        console.log("error with opening database.");
    };

    request.onsuccess = function (event) {
        var db = event.target.result;

        var transaction = db.transaction("Chat", "readwrite");
        transaction.oncomplete = function (event) {
            console.log("Open Table successfully.");
        };

        transaction.onerror = function (event) {
            // Don't forget to handle errors!
            console.log("error opening table.");
        };

        //opem the chat object store
        var objectStore = transaction.objectStore("Chat");

        objectStore.getAll().onsuccess = function (event) {
           console.log("Got all customers: " + event.target.result);
        };


    };

    return "fff";

}



});