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
    connection.on("ReceiveMessage", function (chatMessage) {
        console.log("Recieved message !");
        if (groupId !== chatMessage.groupId) {
            return;
        }

        const date = new Date(chatMessage.timestamp);
        const time = date.getHours().toString().padStart(2, '0') + ':' + date.getMinutes().toString().padStart(2, '0');

        // Create message element matching existing template
        const messageHtml = `
                <div class="mb-3 ${chatMessage.userId === currentUserId ? 'text-end' : ''}">
                    <div class="d-inline-block p-2 rounded ${chatMessage.userId === currentUserId ? 'bg-primary text-white' : 'bg-white border'}">
                        <div class="small text-${chatMessage.userId === currentUserId ? 'light' : 'muted'}">
                            ${chatMessage.userName} • ${time}
                        </div>
                        <div>${chatMessage.message}</div>
                    </div>
                </div>`;

        $('#messagesList').append(messageHtml);
    });
    connection.start().catch(function (err) {
        console.error(err.toString());
    });

});


