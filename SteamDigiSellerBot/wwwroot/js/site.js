function sendRequest(url, type, data) {
    $.ajax({
        url: url,
        type: type,
        data: data,
    });
}

function enterSteamProfileUrl(gameName, steamProfileUrl, uniqueCode, activationCountry) {

    if (isEmptyOrSpaces(steamProfileUrl)) {
        Swal.fire({
            title: 'Enter Steam Profile Url',
            input: 'text',
            inputAttributes: {
                autocapitalize: 'off'
            },
            confirmButtonText: 'Continue',
            showLoaderOnConfirm: true,
            preConfirm: (inputSteamProfileUrl) => {
                steamProfileUrl = inputSteamProfileUrl
            },
        }).then((result) => {
            if (result.isConfirmed) {
                showWarnings(gameName, result.value, uniqueCode, activationCountry);
            }
        });
    }
    else {
        showWarnings(gameName, steamProfileUrl, uniqueCode, activationCountry);
    }

}

function showWarnings(gameName, steamProfileUrl, uniqueCode, activationCountry) {

    Swal.fire({
        title: 'Game - ' + gameName + '\r\n\r\n' + 'This your steam account? ' + steamProfileUrl,
        showDenyButton: true,
        denyButtonText: `Cancel deal`,
        confirmButtonText: `Continue`,
    }).then((dealResult) => {
        if (dealResult.isConfirmed) {
            if (activationCountry !== 'Global') {
                Swal.fire({
                    title: `Gift only for ` + activationCountry + ` steam accounts`,
                    showDenyButton: true,
                    denyButtonText: `Need help`,
                    confirmButtonText: `Continue`,
                }).then((countryHelpResult) => {
                    if (countryHelpResult.isConfirmed) {
                        sendFriendRequest(uniqueCode, steamProfileUrl);

                    } else if (countryHelpResult.isDenied) {
                        sendRequest(window.location.origin + '/Home/SetGameSessionStatus', 'POST', { 'uniqueCode': uniqueCode, 'gameSessionStatus': 5 });
                        setPanelStatus('<b style="color:black">You request help. Contact the seller in correspondence on the item.</b><br />');
                        Swal.fire('Contact the seller in correspondence on the item', '', 'error');
                        console.log('Need Help');
                    }
                });
            }
            else {
                sendFriendRequest(uniqueCode, steamProfileUrl);
            }
        } else if (dealResult.isDenied) {
            sendRequest(window.location.origin + '/Home/SetGameSessionStatus', 'POST', { 'uniqueCode': uniqueCode, 'gameSessionStatus': 4 });
            setPanelStatus('<b style="color:black">Cancelled. Contact the seller in correspondence on the item.</b><br />');
            Swal.fire('Contact the seller in correspondence on the item', '', 'error');
        }
    });
}

function setPanelStatus(statusHtml) {
    let panel = document.getElementById("panel");
    if (panel) {
        panel.innerHTML = statusHtml;
    }
}

function sendFriendRequest(uniqueCode, steamProfileUrl) {
    sendRequest(window.location.origin + '/Home/SetGameSessionStatus', 'POST', { 'uniqueCode': uniqueCode, 'steamProfileUrl': steamProfileUrl, 'gameSessionStatus': 2 });
    setPanelStatus('<b style="color:black">Accept friend request. Bot Steam id 777</b><br />');
    Swal.fire({
        icon: 'success',
        title: 'Accept friend request',
        text: 'Bot Steam id 777',
    })
    console.log('Send friend request');
}

function isEmptyOrSpaces(str) {
    return str === null || str.match(/^ *$/) !== null;
}