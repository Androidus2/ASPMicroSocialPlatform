// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

let searchTimeout;
const connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();


function performSearch(searchTerm = '') {
    fetch(`/Groups/Search?search=${searchTerm}`)
        .then(response => response.text())
        .then(html => {
            document.getElementById('searchResults').innerHTML = html;
        });
}

document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('searchInput');

    // Perform initial search

    searchTimeout = setTimeout(() => {
        performSearch();
    }, 300);

    // Setup search input handler
    searchInput.addEventListener('input', function (e) {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            performSearch(e.target.value);
        }, 300);
    });
});

function addUser(userId) {
    console.log('Adding user with ID: ' + userId);

    // Get all existing selected users (hidden inputs with name SelectedUserIds)
    var existingUserIds = document.querySelectorAll('input[name="SelectedUserIds"]');
    var existingUserIdsArray = Array.from(existingUserIds).map(input => input.value);

    console.log('Existing user IDs: ' + existingUserIdsArray.join(','))

    // If user is already selected, call removeUser instead
    if (existingUserIdsArray.includes(userId)) {
        console.log('User already selected, removing instead');
        removeUser(userId);
        return;
    }
    console.log('User does not exist in selected users, adding');

    const form = new FormData();
    form.append('userId', userId);

    // Add existing selected users
    document.querySelectorAll('input[name="SelectedUserIds"]').forEach(input => {
        form.append('existingUserIds', input.value);
    });

    fetch('/Groups/AddUser', {
        method: 'POST',
        body: form,
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
    .then(response => response.text())
    .then(html => {
        document.getElementById('selectedUsers').innerHTML = html;
    });
}

function removeUser(userId) {
    const form = new FormData();
    form.append('userId', userId);

    // Add existing selected users
    document.querySelectorAll('input[name="SelectedUserIds"]').forEach(input => {
        form.append('existingUserIds', input.value);
    });

    fetch('/Groups/RemoveUser', {
        method: 'POST',
        body: form,
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.text())
        .then(html => {
            document.getElementById('selectedUsers').innerHTML = html;
        });
}

document.addEventListener('DOMContentLoaded', function () {
    const groupContent = document.getElementById('groupContent');

    // Handle initial load if group ID in URL
    const urlParams = new URLSearchParams(window.location.search);
    const groupId = urlParams.get('id');
    if (groupId) {
        loadGroup(groupId);
    }

    // Handle group selection
    document.querySelectorAll('.group-item').forEach(item => {
        item.addEventListener('click', function (e) {
            e.preventDefault();
            const groupId = this.getAttribute('href').split('/').pop();
            loadGroup(groupId);

            // Update URL without refresh
            window.history.pushState({}, '', `/Groups/Show/${groupId}`);
        });
    });
});


document.addEventListener('DOMContentLoaded', function () {
    connection.off("ReceiveMessage");
    connection.off("ReceiveDelete");
    connection.off("ReceiveEdit");
    connection.on("ReceiveMessage", function (chatMessage) {
        console.log("Received message!");
        if (groupId !== chatMessage.groupId) {
            return;
        }

        // Format the timestamp
        const date = new Date(chatMessage.timestamp);
        const formattedTime = date.toLocaleString('en-GB', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        }).replace(',', '');

        // Create message element matching existing template
        const messageHtml = `
    <div class="mb-3 ${chatMessage.userId === currentUserId ? 'text-end' : ''}" data-message-id="${chatMessage.id}">
        <div class="d-flex ${chatMessage.userId === currentUserId ? 'flex-row-reverse' : 'flex-row'} align-items-start">
            <a href="/Users/Show/${chatMessage.userId}" class="avatar">
                <img src="${chatMessage.userProfilePicture ? chatMessage.userProfilePicture : '/images/MissingNo.png'}" alt="${chatMessage.userName}" class="w-100 h-100 rounded-circle">
            </a>
            <div class="d-inline-block p-2 rounded position-relative ${chatMessage.userId === currentUserId ? 'bg-primary text-white' : 'bg-white border'}">
                <div class="small text-${chatMessage.userId === currentUserId ? 'light' : 'muted'}">
                    <a href="/Users/Show/${chatMessage.userId}" class="${chatMessage.userId == currentUserId ? "self-" : ""}user-name text-decoration-none text-${chatMessage.userId === currentUserId ? 'light' : 'muted'}">
                        ${chatMessage.userName}
                    </a> • ${formattedTime}
                    ${chatMessage.userId === currentUserId || isAdmin ? `
                        <button class="btn btn-sm text-${chatMessage.userId === currentUserId ? 'light' : 'muted'} message-actions ms-2" data-bs-toggle="dropdown">
                            <i class="bi bi-three-dots-vertical"></i>
                        </button>
                        <ul class="dropdown-menu">
                            <li><button class="dropdown-item edit-button" data-message-id="${chatMessage.id}">Edit</button></li>
                            <li><button class="dropdown-item text-danger delete-button" data-message-id="${chatMessage.id}">Delete</button></li>
                        </ul>
                    ` : ''}
                </div>
                <div id="messageContent">${chatMessage.message}</div>
            </div>
        </div>
    </div>`;



        $('#messagesList').append(messageHtml);
    });


    connection.on("ReceiveDelete", function (messageId) {
        console.log("Received a package which told us to delete! " + messageId);

        // Find the message with the matching ID and remove it
        $('#messagesList .mb-3[data-message-id="' + messageId + '"]').remove();
    });

    connection.on("ReceiveEdit", function (chatMessage) {
        console.log("Received a package which told us to edit! " + chatMessage.id);
        console.log(chatMessage);

        // Find the message with the matching ID and update it
        const messageElement = $('#messagesList .mb-3[data-message-id="' + chatMessage.id + '"]');
        messageElement.find('#messageContent').text(chatMessage.message);
    });


    connection.start().catch(function (err) {
        console.error(err.toString());
    });

});


