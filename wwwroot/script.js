// Global variables
let connection;
let serviceStatuses = new Map();

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    initializeSignalR();
    loadServers();
    loadIndexingServers();
    loadThinClientServers();
});

// SignalR connection
async function initializeSignalR() {
    try {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/serviceHub")
            .build();

        connection.on("ServiceStatusUpdate", function(statuses) {
            updateServiceStatuses(statuses);
        });

        await connection.start();
        console.log("SignalR Connected");
    } catch (err) {
        console.error("SignalR Connection Error:", err);
    }
}

// Tab functionality
function showTab(tabName) {
    // Hide all tab contents
    const tabContents = document.querySelectorAll('.tab-content');
    tabContents.forEach(content => content.classList.remove('active'));

    // Remove active class from all tab buttons
    const tabButtons = document.querySelectorAll('.tab-button');
    tabButtons.forEach(button => button.classList.remove('active'));

    // Show selected tab content
    document.getElementById(tabName).classList.add('active');

    // Add active class to clicked button
    event.target.classList.add('active');
}

// Load servers
async function loadServers() {
    try {
        const response = await fetch('/api/service/servers');
        const servers = await response.json();
        renderServers(servers, 'servers-container');
    } catch (error) {
        showStatusMessage('Error loading servers: ' + error.message, 'error');
    }
}

// Load indexing servers
async function loadIndexingServers() {
    try {
        const response = await fetch('/api/service/indexing-servers');
        const servers = await response.json();
        renderServers(servers, 'indexing-servers-container');
    } catch (error) {
        showStatusMessage('Error loading indexing servers: ' + error.message, 'error');
    }
}

// Load thin client servers
async function loadThinClientServers() {
    try {
        const response = await fetch('/api/service/thinclient-servers');
        const servers = await response.json();
        renderThinClientServers(servers);
    } catch (error) {
        showStatusMessage('Error loading thin client servers: ' + error.message, 'error');
    }
}

// Render servers
function renderServers(servers, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    container.innerHTML = '';

    servers.forEach(server => {
        const serverCard = document.createElement('div');
        serverCard.className = 'server-card';
        serverCard.innerHTML = `
            <div class="server-header">
                <div>
                    <div class="server-name">${server.name}</div>
                    <div class="server-ip">${server.ipAddress}</div>
                </div>
            </div>
            <div class="services-list">
                ${server.services.map(service => `
                    <div class="service-item" data-server="${server.name}" data-service="${service}">
                        <div class="service-info">
                            <span class="service-name">${service}</span>
                            <span class="status-badge status-loading" id="status-${server.name}-${service}">
                                <span class="loading"></span> Loading...
                            </span>
                        </div>
                        <div class="service-actions">
                            <button class="btn btn-success" onclick="controlService('${server.name}', '${service}', 'start')">
                                <i class="fas fa-play"></i> Start
                            </button>
                            <button class="btn btn-danger" onclick="controlService('${server.name}', '${service}', 'stop')">
                                <i class="fas fa-stop"></i> Stop
                            </button>
                            <button class="btn btn-warning" onclick="controlService('${server.name}', '${service}', 'restart')">
                                <i class="fas fa-redo"></i> Restart
                            </button>
                        </div>
                    </div>
                `).join('')}
            </div>
        `;
        container.appendChild(serverCard);
    });
}

// Render thin client servers
function renderThinClientServers(servers) {
    const container = document.getElementById('thinclient-servers-container');
    if (!container) return;

    container.innerHTML = '';

    servers.forEach(server => {
        const serverCard = document.createElement('div');
        serverCard.className = 'server-card';
        serverCard.innerHTML = `
            <div class="server-header">
                <div>
                    <div class="server-name">${server.name}</div>
                    <div class="server-ip">${server.ipAddress}</div>
                </div>
            </div>
            <div class="file-operations">
                <div class="input-group">
                    <label>Folder Path:</label>
                    <input type="text" id="folder-${server.name}" placeholder="C:\inetpub\wwwroot" value="C:\inetpub\wwwroot">
                </div>
                <div class="bulk-actions">
                    <button class="btn btn-primary" onclick="performFileOperation('${server.name}', 'forward')">
                        <i class="fas fa-arrow-right"></i> Forward Operation
                    </button>
                    <button class="btn btn-secondary" onclick="performFileOperation('${server.name}', 'reverse')">
                        <i class="fas fa-arrow-left"></i> Reverse Operation
                    </button>
                </div>
            </div>
        `;
        container.appendChild(serverCard);
    });
}

// Update service statuses
function updateServiceStatuses(statuses) {
    statuses.forEach(status => {
        const statusElement = document.getElementById(`status-${status.serverName}-${status.serviceName}`);
        if (statusElement) {
            const statusClass = getStatusClass(status.status);
            statusElement.className = `status-badge ${statusClass}`;
            statusElement.textContent = status.status;
        }
    });
}

// Get status class
function getStatusClass(status) {
    if (status.toLowerCase().includes('running')) return 'status-running';
    if (status.toLowerCase().includes('stopped')) return 'status-stopped';
    if (status.toLowerCase().includes('error')) return 'status-error';
    return 'status-stopped';
}

// Control service
async function controlService(serverName, serviceName, operation) {
    try {
        const response = await fetch('/api/service/control-service', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                serverName: serverName,
                serviceName: serviceName,
                operation: operation
            })
        });

        const result = await response.json();
        showStatusMessage(`${operation} ${serviceName} on ${serverName}: ${result.result}`, 'success');
    } catch (error) {
        showStatusMessage(`Error controlling service: ${error.message}`, 'error');
    }
}

// Control all indexing services
async function controlAllIndexingServices(operation) {
    try {
        const response = await fetch('/api/service/control-all-indexing', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(operation)
        });

        const result = await response.json();
        showStatusMessage(`${operation} all indexing services: ${result.result}`, 'success');
    } catch (error) {
        showStatusMessage(`Error controlling indexing services: ${error.message}`, 'error');
    }
}

// Perform file operation
async function performFileOperation(serverName, operation) {
    const folderPath = document.getElementById(`folder-${serverName}`).value;
    
    try {
        const response = await fetch('/api/service/file-operation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                serverName: serverName,
                folderPath: folderPath,
                operation: operation
            })
        });

        const result = await response.json();
        showStatusMessage(`${operation} file operation on ${serverName}: ${result.result}`, 'success');
    } catch (error) {
        showStatusMessage(`Error performing file operation: ${error.message}`, 'error');
    }
}

// Perform file operation on all servers
async function performFileOperationOnAll(operation) {
    const folderPath = document.getElementById('folder-path').value;
    
    try {
        const response = await fetch('/api/service/file-operation-all', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                operation: operation,
                folderPath: folderPath
            })
        });

        const result = await response.json();
        showStatusMessage(`${operation} file operation on all servers: ${result.result}`, 'success');
    } catch (error) {
        showStatusMessage(`Error performing file operation: ${error.message}`, 'error');
    }
}

// Show status message
function showStatusMessage(message, type = 'info') {
    const container = document.getElementById('status-messages');
    const messageElement = document.createElement('div');
    messageElement.className = `status-message ${type}`;
    messageElement.innerHTML = `
        <div style="display: flex; justify-content: space-between; align-items: center;">
            <span>${message}</span>
            <button onclick="this.parentElement.parentElement.remove()" style="background: none; border: none; color: #666; cursor: pointer; font-size: 18px;">×</button>
        </div>
    `;
    
    container.appendChild(messageElement);

    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (messageElement.parentNode) {
            messageElement.remove();
        }
    }, 5000);
} 