"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/converFile").build();


connection.on("ReceiveConvertProgress", function (progressValue, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    document.getElementById("progressValueContainer").innerText = progressValue*100;
});


connection.on("ReceiveConvertError", function (errorMessage) {
    var errorTextContainer = document.getElementById("errorTextContainer");
    errorTextContainer.innerText = errorMessage;
});

connection.on("ReceiveConvertComplete", function (link) {

    //save link to cookie
    var l = document.getElementById("linkToFile");
    l.innerText="link";
    l.setAttribute("href", link);
    l.innerText = link;
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});