class DeviceManager {
    constructor() {
        this.devices = [];
        this.filteredDevices = [];
        this.rooms = new Set();
        this.deviceTypes = new Set();
    }

    updateDevices(devices) {
        if (!Array.isArray(devices)) {
            console.error("Invalid devices data:", devices);
            return;
        }
        this.devices = devices;
        console.log("Updated devices:", this.devices);
        this.updateFilters();
        this.applyFilters();
    }

    updateFilters() {
        // Update rooms and device types
        this.rooms.clear();
        this.deviceTypes.clear();
        
        this.devices.forEach(device => {
            if (device.location) this.rooms.add(device.location);
            if (device.deviceType) this.deviceTypes.add(device.deviceType);
        });

        // Update dropdowns
        this.updateRoomDropdown();
        this.updateDeviceTypeDropdown();
    }

    updateRoomDropdown() {
        const dropdown = document.getElementById('roomFilter');
        if (!dropdown) return;

        dropdown.innerHTML = '<option value="">All Rooms</option>';
        this.rooms.forEach(room => {
            dropdown.innerHTML += `<option value="${room}">${room}</option>`;
        });
    }

    updateDeviceTypeDropdown() {
        const dropdown = document.getElementById('deviceTypeFilter');
        if (!dropdown) return;

        dropdown.innerHTML = '<option value="">All Devices</option>';
        this.deviceTypes.forEach(type => {
            dropdown.innerHTML += `<option value="${type}">${type}</option>`;
        });
    }

    applyFilters() {
        const roomFilter = document.getElementById('roomFilter');
        const typeFilter = document.getElementById('deviceTypeFilter');
        const powerFilter = document.getElementById('powerFilter');
        
        if (!roomFilter || !typeFilter || !powerFilter) {
            console.warn("Filter elements not found");
            this.filteredDevices = [...this.devices];
            this.updateDeviceGrid();
            return;
        }
        
        const selectedRoom = roomFilter.value;
        const selectedType = typeFilter.value;
        const powerValue = powerFilter.value;

        this.filteredDevices = this.devices.filter(device => {
            const roomMatch = !selectedRoom || device.location === selectedRoom;
            const typeMatch = !selectedType || device.deviceType === selectedType;
            const powerMatch = powerValue === 'all' || 
                             (powerValue === 'on' && device.power) || 
                             (powerValue === 'off' && !device.power);
            
            return roomMatch && typeMatch && powerMatch;
        });

        console.log("Filtered devices:", this.filteredDevices);
        this.updateDeviceGrid();
    }

    updateDeviceGrid() {
        const grid = document.getElementById('deviceGrid');
        if (!grid) {
            console.error("Device grid element not found");
            return;
        }

        grid.innerHTML = '';
        
        if (this.filteredDevices.length === 0) {
            grid.innerHTML = '<div class="no-devices">No devices found</div>';
            return;
        }
        
        this.filteredDevices.forEach(device => {
            const card = this.createDeviceCard(device);
            grid.appendChild(card);
        });
    }

    createDeviceCard(device) {
        const card = document.createElement('div');
        card.className = `device-card ${device.power ? 'power-on' : 'power-off'}`;
        card.dataset.deviceId = device.deviceId;
        card.dataset.deviceType = device.deviceType;
        card.dataset.deviceIndex = device.deviceIndex;
        
        const statusClass = device.power ? 'status-on' : 'status-off';
        const deviceName = this.getDisplayName(device);
        
        card.innerHTML = `
            <div class="device-header">
                <h3>${deviceName}</h3>
                <span class="device-status ${statusClass}">${device.power ? 'ON' : 'OFF'}</span>
            </div>
            <div class="device-info">
                <p><strong>Location:</strong> ${device.location || 'Unknown'}</p>
                <p><strong>ID:</strong> ${device.deviceId || `${device.deviceType}_${device.deviceIndex}`}</p>
            </div>
            <div class="device-controls">
                ${this.createDeviceControls(device)}
            </div>
        `;

        return card;
    }

    getDisplayName(device) {
        // Create a user-friendly name for the device
        const type = device.deviceType || '';
        const location = device.location ? ` (${device.location})` : '';
        const index = device.deviceIndex !== undefined ? ` ${device.deviceIndex + 1}` : '';
        
        // Capitalize first letter of device type
        const capitalizedType = type.charAt(0).toUpperCase() + type.slice(1);
        
        return `${capitalizedType}${index}${location}`;
    }

    createDeviceControls(device) {
        let controls = '';
        
        // Common toggle button for all devices
        const toggleBtn = `<button onclick="toggleDevice('${device.deviceType}', ${device.deviceIndex})">Toggle Power</button>`;
        
        switch(String(device.deviceType).toLowerCase()) {
            case 'light':
                controls = `
                    ${toggleBtn}
                    <div class="control-group">
                        <label>Intensity: </label>
                        <input type="range" min="0" max="2" step="0.1" value="${device.intensity || 1}" 
                               onchange="setLightIntensity(${device.deviceIndex}, this.value)">
                    </div>
                    <div class="control-group">
                        <label>Color: </label>
                        <input type="color" value="#${device.color || 'FFFFFF'}" 
                               onchange="setLightColor(${device.deviceIndex}, this.value)">
                    </div>
                `;
                break;
            case 'tv':
                controls = `
                    ${toggleBtn}
                    <div class="control-group">
                        <label>Volume: </label>
                        <input type="range" min="0" max="100" value="${device.volume || 0}" 
                               onchange="setTVVolume(this.value)">
                    </div>
                    <div class="control-group">
                        <label>Channel: </label>
                        <input type="number" min="1" value="${device.channel || 1}" 
                               onchange="setTVChannel(this.value)">
                    </div>
                    <div class="control-group">
                        <label>Source: </label>
                        <select onchange="setTVSource(this.value)">
                            <option value="HDMI1" ${device.source === 'HDMI1' ? 'selected' : ''}>HDMI1</option>
                            <option value="HDMI2" ${device.source === 'HDMI2' ? 'selected' : ''}>HDMI2</option>
                        </select>
                    </div>
                `;
                break;
            case 'ac':
                controls = `
                    ${toggleBtn}
                    <div class="control-group">
                        <label>Temperature: </label>
                        <input type="number" min="16" max="30" value="${device.temperature || 24}" 
                               onchange="setACTemperature(this.value)">
                    </div>
                    <div class="control-group">
                        <label>Fan Speed: </label>
                        <select onchange="setACFanSpeed(this.value)">
                            <option value="1" ${device.fanSpeed === 1 ? 'selected' : ''}>Low</option>
                            <option value="2" ${device.fanSpeed === 2 ? 'selected' : ''}>Medium</option>
                            <option value="3" ${device.fanSpeed === 3 ? 'selected' : ''}>High</option>
                        </select>
                    </div>
                    <div class="control-group">
                        <label>Eco Mode: </label>
                        <button onclick="toggleACEcoMode(${!device.ecoMode})">${device.ecoMode ? 'ON' : 'OFF'}</button>
                    </div>
                `;
                break;
            case 'fan':
                controls = `
                    ${toggleBtn}
                    <div class="control-group">
                        <label>Speed: </label>
                        <input type="range" min="0" max="5" step="1" value="${device.rpm || 1}" 
                               onchange="setFanRPM(this.value)">
                    </div>
                `;
                break;
            // Add more device types as needed
            default:
                controls = toggleBtn;
        }
        
        return controls;
    }
}

// Initialize device manager
const deviceManager = new DeviceManager();

// Add event listeners for filters
document.addEventListener('DOMContentLoaded', () => {
    const roomFilter = document.getElementById('roomFilter');
    const deviceTypeFilter = document.getElementById('deviceTypeFilter');
    const powerFilter = document.getElementById('powerFilter');

    if (roomFilter) roomFilter.addEventListener('change', () => deviceManager.applyFilters());
    if (deviceTypeFilter) deviceTypeFilter.addEventListener('change', () => deviceManager.applyFilters());
    if (powerFilter) powerFilter.addEventListener('change', () => deviceManager.applyFilters());
});

// Device control functions
function toggleDevice(deviceType, deviceIndex) {
    sendCommand({
        device: deviceType,
        action: 'toggle',
        deviceIndex: deviceIndex,
        parameters: {}
    });
}

function setLightIntensity(deviceIndex, intensity) {
    sendCommand({
        device: 'light',
        action: 'setintensity',
        deviceIndex: deviceIndex,
        parameters: { intensity: parseFloat(intensity) }
    });
}

function setLightColor(deviceIndex, color) {
    // Remove # from color if present
    const colorValue = color.startsWith('#') ? color.substring(1) : color;
    sendCommand({
        device: 'light',
        action: 'setcolor',
        deviceIndex: deviceIndex,
        parameters: { color: colorValue }
    });
}

function setTVVolume(volume) {
    sendCommand({
        device: 'tv',
        action: 'setvolume',
        parameters: { volume: parseInt(volume) }
    });
}

function setTVChannel(channel) {
    sendCommand({
        device: 'tv',
        action: 'setchannel',
        parameters: { channel: parseInt(channel) }
    });
}

function setTVSource(source) {
    sendCommand({
        device: 'tv',
        action: 'setsource',
        parameters: { source: source }
    });
}

function setACTemperature(temperature) {
    sendCommand({
        device: 'ac',
        action: 'settemperature',
        parameters: { temperature: parseInt(temperature) }
    });
}

function setACFanSpeed(speed) {
    sendCommand({
        device: 'ac',
        action: 'setfanspeed',
        parameters: { speed: parseInt(speed) }
    });
}

function toggleACEcoMode(ecoMode) {
    sendCommand({
        device: 'ac',
        action: 'toggleeco',
        parameters: { eco: ecoMode }
    });
}

function setFanRPM(rpm) {
    sendCommand({
        device: 'fan',
        action: 'setrpm',
        parameters: { rpm: parseInt(rpm) }
    });
} 