// Wrap everything in an IIFE (Immediately Invoked Function Expression)
// or use DOMContentLoaded to ensure elements exist before attaching listeners.
document.addEventListener('DOMContentLoaded', () => {

  // --- WebSocket Management ---
  let ws;
  const serverUrl = 'ws://localhost:8080/iot'; // Make sure this is correct
  let reconnectAttempts = 0;
  const maxReconnectAttempts = 10;
  const reconnectDelay = 3000;
  let reconnectTimer;

  const connectionStatus = document.getElementById('connectionStatus');

  function connectWebSocket() {
    ws = new WebSocket(serverUrl);

    ws.onopen = () => {
      console.log('WebSocket connected');
      reconnectAttempts = 0; // Reset reconnect attempts on successful connection
      if (connectionStatus) {
        connectionStatus.textContent = 'Connected';
        connectionStatus.className = 'status-connected';
      }
      // Request initial state - you can remove this if server sends it automatically
      requestDevices();
    };

    ws.onclose = (event) => {
      console.log('WebSocket disconnected', event);
      
      if (connectionStatus) {
        connectionStatus.textContent = 'Disconnected - Attempting to reconnect...';
        connectionStatus.className = 'status-disconnected';
      }
      
      // Try to reconnect unless the connection was closed cleanly
      if (reconnectAttempts < maxReconnectAttempts && !event.wasClean) {
        const delay = reconnectDelay * Math.pow(1.2, reconnectAttempts);
        console.log(`Attempting to reconnect in ${delay}ms (attempt ${reconnectAttempts + 1}/${maxReconnectAttempts})`);
        
        clearTimeout(reconnectTimer);
        reconnectTimer = setTimeout(() => {
          reconnectAttempts++;
          connectWebSocket();
        }, delay);
      } else if (reconnectAttempts >= maxReconnectAttempts) {
        if (connectionStatus) {
          connectionStatus.textContent = 'Connection failed - Please reload the page';
        }
        console.error(`Failed to reconnect after ${maxReconnectAttempts} attempts`);
      }
    };

    ws.onerror = (error) => {
      console.error('WebSocket error:', error);
      // Error handling is done in onclose
    };

    ws.onmessage = (event) => {
      try {
        const response = JSON.parse(event.data);
        console.log('Received:', response);

        // Handle any message with data property
        if (response.data) {
          if (response.messageType === 'initialState' && Array.isArray(response.data.devices)) {
            console.log('Received initial state with devices:', response.data.devices);
            deviceManager.updateDevices(response.data.devices);
          } else if (response.data.deviceId) {
            // Individual device update
            updateDeviceInManager(response.data);
          } else if (response.data.device) {
            // Legacy format - convert to new format and update
            const legacyDevice = response.data;
            const deviceData = {
              deviceId: legacyDevice.deviceIndex !== undefined 
                ? `${legacyDevice.device}_${legacyDevice.deviceIndex}` 
                : legacyDevice.device,
              deviceType: legacyDevice.device,
              power: legacyDevice.state,
              // Copy all other properties
              ...legacyDevice
            };
            updateDeviceInManager(deviceData);
          }
        }
      } catch (e) {
        console.error('Error parsing message:', e, event.data);
      }
    };
  }

  function updateDeviceInManager(deviceData) {
    // Find the device in the manager's devices array
    const deviceIndex = deviceManager.devices.findIndex(d => {
      // Handle different possible ID formats
      return d.deviceId === deviceData.deviceId || 
             (d.deviceType === deviceData.deviceType && d.deviceIndex === deviceData.deviceIndex);
    });

    if (deviceIndex !== -1) {
      // Update existing device
      deviceManager.devices[deviceIndex] = {
        ...deviceManager.devices[deviceIndex],
        ...deviceData
      };
    } else {
      // Add new device
      deviceManager.devices.push(deviceData);
    }
    
    // Refresh the UI
    deviceManager.updateFilters();
    deviceManager.applyFilters();
  }

  function requestDevices() {
    // Request the current device state from the server
    if (ws && ws.readyState === WebSocket.OPEN) {
      try {
        const request = {
          action: "getDevices"
        };
        console.log('Requesting devices:', request);
        ws.send(JSON.stringify(request));
      } catch (e) {
        console.error('Error requesting devices:', e);
      }
    }
  }

  // Make sendCommand available globally for deviceManager
  function sendCommand(command) {
    if (ws && ws.readyState === WebSocket.OPEN) {
      try {
        const commandString = JSON.stringify(command);
        console.log('Sending:', commandString);
        ws.send(commandString);
      } catch (e) {
        console.error('Error sending command:', command, e);
      }
    } else {
      console.error('WebSocket is not connected. Command not sent:', command);
      // Show a notification to user about connection issue
      showToast('Please wait for connection to be established before sending commands');
    }
  }

  // Show toast notification for user feedback
  function showToast(message, duration = 3000) {
    // Check if toast container exists, if not create it
    let toastContainer = document.getElementById('toast-container');
    
    if (!toastContainer) {
      toastContainer = document.createElement('div');
      toastContainer.id = 'toast-container';
      document.body.appendChild(toastContainer);
    }
    
    // Create new toast
    const toast = document.createElement('div');
    toast.className = 'toast';
    toast.innerText = message;
    
    // Add to container
    toastContainer.appendChild(toast);
    
    // Show with animation
    setTimeout(() => {
      toast.classList.add('show');
    }, 10);
    
    // Auto remove
    setTimeout(() => {
      toast.classList.remove('show');
      setTimeout(() => {
        toast.remove();
      }, 300);
    }, duration);
  }

  // --- Chat Input: Text & Voice Handling ---
  const chatText = document.getElementById('chatText');
  const sendText = document.getElementById('sendText');
  const startVoice = document.getElementById('startVoice');

  function sendChatCommand(inputText) {
    const text = inputText.trim();
    if (text) {
      console.log('Processing command:', text);
      sendCommand({
        device: "general", // Or specific AI endpoint
        action: "processCommand",
        parameters: { input: text }
      });
      if (chatText) chatText.value = ""; // Clear input field after sending
    }
  }

  if (sendText) {
    sendText.onclick = () => sendChatCommand(chatText.value);
  }

  // Allow sending with Enter key in text input
  if (chatText) {
    chatText.addEventListener('keypress', function (e) {
      if (e.key === 'Enter') {
        sendChatCommand(chatText.value);
      }
    });
  }


  // Voice Recognition Setup
  let recognition;
  const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

  if (SpeechRecognition) {
    recognition = new SpeechRecognition();
    recognition.continuous = false; // Process single utterance
    recognition.interimResults = false;
    recognition.lang = 'en-US'; // Set language

    recognition.onstart = () => {
      console.log('Voice recognition started. Speak now.');
      if (startVoice) startVoice.style.backgroundColor = '#dc3545'; // Indicate recording
    };

    recognition.onresult = (event) => {
      const transcript = event.results[0][0].transcript;
      console.log('Voice command received:', transcript);
      if (chatText) chatText.value = transcript; // Show transcript in input field
      sendChatCommand(transcript); // Send the transcript as a command
    };

    recognition.onerror = (event) => {
      console.error('Speech recognition error:', event.error);
      if (event.error === 'no-speech') {
        console.warn('No speech detected.');
      } else if (event.error === 'audio-capture') {
        console.error('Audio capture error. Ensure microphone is enabled and permitted.');
        alert('Could not start voice recognition. Please check microphone permissions.');
      } else if (event.error === 'not-allowed') {
        console.error('Microphone access denied.');
        alert('Microphone access was denied. Please allow access to use voice commands.');
      }
      // Handle other errors
    };

    recognition.onend = () => {
      console.log('Voice recognition ended.');
      if (startVoice) startVoice.style.backgroundColor = ''; // Reset button color
    };

  } else {
    console.warn('Speech recognition not supported in this browser.');
    if (startVoice) {
      startVoice.disabled = true; // Disable voice button if not supported
      startVoice.title = "Speech recognition not supported in this browser";
    }
  }

  if (startVoice && recognition) {
    startVoice.onclick = () => {
      try {
        recognition.start();
      } catch (e) {
        console.error("Could not start voice recognition (might already be active or error occurred):", e);
      }
    };
  }

  // --- Device Control Event Listeners ---

  // Helper function to update button state (ON/OFF text and class)
  function updateToggleButton(button, isOn) {
    if (!button) return;
    button.textContent = isOn ? 'ON' : 'OFF';
    if (isOn) {
      button.classList.remove('off');
    } else {
      button.classList.add('off');
    }
  }

  // Light Controls Setup (for each light card)
  function setupLightControl(index) {
    const toggle = document.getElementById(`light${index}Toggle`);
    const intensity = document.getElementById(`light${index}Intensity`);
    const intensityValue = document.getElementById(`light${index}IntensityValue`);
    const color = document.getElementById(`light${index}Color`);

    if (toggle) {
      toggle.onclick = function () {
        const willBeOn = this.classList.contains('off'); // If it's 'off' now, it will be 'on'
        updateToggleButton(this, willBeOn);
        sendCommand({
          device: 'light',
          action: 'toggle',
          deviceIndex: index,
          parameters: { state: willBeOn }
        });
      };
    }

    if (intensity) {
      // Update label on input change
      intensity.oninput = function () {
        if (intensityValue) intensityValue.textContent = parseFloat(this.value).toFixed(1);
      }
      // Send command only when interaction finishes (change event)
      intensity.onchange = function () {
        sendCommand({
          device: 'light',
          action: 'setintensity',
          deviceIndex: index,
          parameters: { intensity: parseFloat(this.value) }
        });
      };
    }

    if (color) {
      color.onchange = function () {
        // Send color without '#'
        sendCommand({
          device: 'light',
          action: 'setcolor',
          deviceIndex: index,
          parameters: { color: this.value.substring(1) }
        });
      };
    }
  }

  // Setup for 3 lights (indices 0, 1, 2)
  setupLightControl(0);
  setupLightControl(1);
  setupLightControl(2);

  // TV Controls
  const tvToggle = document.getElementById('tvToggle');
  const tvVolume = document.getElementById('tvVolume');
  const tvVolumeValue = document.getElementById('tvVolumeValue');
  const tvChannel = document.getElementById('tvChannel');
  const tvSource = document.getElementById('tvSource');

  if (tvToggle) {
    tvToggle.onclick = function () {
      const willBeOn = this.classList.contains('off');
      updateToggleButton(this, willBeOn);
      sendCommand({ device: 'tv', action: 'toggle', parameters: { state: willBeOn } });
    };
  }
  if (tvVolume) {
    tvVolume.oninput = () => { if (tvVolumeValue) tvVolumeValue.textContent = tvVolume.value; };
    tvVolume.onchange = function () {
      sendCommand({ device: 'tv', action: 'setvolume', parameters: { volume: parseInt(this.value) } });
    };
  }
  if (tvChannel) {
    tvChannel.onchange = function () {
      sendCommand({ device: 'tv', action: 'setchannel', parameters: { channel: parseInt(this.value) } });
    };
  }
  if (tvSource) {
    tvSource.onchange = function () {
      sendCommand({ device: 'tv', action: 'setsource', parameters: { source: this.value } });
    };
  }


  // AC Controls
  const acToggle = document.getElementById('acToggle');
  const acTemperature = document.getElementById('acTemperature');
  const acFanSpeed = document.getElementById('acFanSpeed');
  const acEcoMode = document.getElementById('acEcoMode');

  if (acToggle) {
    acToggle.onclick = function () {
      const willBeOn = this.classList.contains('off');
      updateToggleButton(this, willBeOn);
      sendCommand({ device: 'ac', action: 'toggle', parameters: { state: willBeOn } });
    };
  }
  if (acTemperature) {
    acTemperature.onchange = function () {
      sendCommand({ device: 'ac', action: 'settemperature', parameters: { temperature: parseInt(this.value) } });
    };
  }
  if (acFanSpeed) {
    acFanSpeed.onchange = function () {
      sendCommand({ device: 'ac', action: 'setfanspeed', parameters: { speed: parseInt(this.value) } });
    };
  }
  if (acEcoMode) {
    acEcoMode.onclick = function () {
      const willBeOn = this.classList.contains('off');
      updateToggleButton(this, willBeOn);
      sendCommand({ device: 'ac', action: 'toggleeco', parameters: { eco: willBeOn } });
    };
  }

  // Fan Controls
  const fanToggle = document.getElementById('fanToggle');
  const fanRPM = document.getElementById('fanRPM');

  if (fanToggle) {
    fanToggle.onclick = function () {
      const willBeOn = this.classList.contains('off');
      updateToggleButton(this, willBeOn);
      sendCommand({ device: 'fan', action: 'toggle', parameters: { state: willBeOn } });
    };
  }
  if (fanRPM) {
    fanRPM.onchange = function () {
      sendCommand({ device: 'fan', action: 'setrpm', parameters: { rpm: parseInt(this.value) } });
    };
  }


  // Fridge Controls
  const fridgeToggle = document.getElementById('fridgeToggle');
  const fridgeTemperature = document.getElementById('fridgeTemperature');
  const freezeTemperature = document.getElementById('freezeTemperature');
  const fridgeDoorToggle = document.getElementById('fridgeDoorToggle');
  const freezeDoorToggle = document.getElementById('freezeDoorToggle');

  if (fridgeToggle) {
    fridgeToggle.onclick = function () {
      const willBeOn = this.classList.contains('off');
      updateToggleButton(this, willBeOn);
      sendCommand({ device: 'fridge', action: 'toggle', parameters: { state: willBeOn } });
    };
  }
  if (fridgeTemperature) {
    fridgeTemperature.onchange = function () {
      sendCommand({ device: 'fridge', action: 'settemperature', parameters: { temperature: parseInt(this.value) } });
    };
  }
  if (freezeTemperature) {
    freezeTemperature.onchange = function () {
      sendCommand({ device: 'fridge', action: 'setfreezetemperature', parameters: { temperature: parseInt(this.value) } });
    };
  }

  // Helper for door buttons
  function updateDoorButton(button, isOpen) {
    if (!button) return;
    button.textContent = isOpen ? 'Open' : 'Closed';
    if (isOpen) {
      button.classList.remove('off'); // 'off' class might mean closed here, adjust if needed
    } else {
      button.classList.add('off');
    }
  }

  if (fridgeDoorToggle) {
    fridgeDoorToggle.onclick = function () {
      // Assume 'off' means closed. Clicking opens it.
      const willBeOpen = this.classList.contains('off');
      updateDoorButton(this, willBeOpen);
      sendCommand({
        device: 'fridge',
        action: 'setdoorstatus',
        // Send only the changing door state, keep other undefined if API supports partial updates
        parameters: { fridgeDoor: willBeOpen, freezeDoor: undefined }
      });
    };
  }
  if (freezeDoorToggle) {
    freezeDoorToggle.onclick = function () {
      const willBeOpen = this.classList.contains('off');
      updateDoorButton(this, willBeOpen);
      sendCommand({
        device: 'fridge',
        action: 'setdoorstatus',
        parameters: { fridgeDoor: undefined, freezeDoor: willBeOpen }
      });
    };
  }

  // Induction Controls
  const inductionHeat = document.getElementById('inductionHeat');
  const inductionHeatValue = document.getElementById('inductionHeatValue');

  if (inductionHeat) {
    inductionHeat.oninput = () => { if (inductionHeatValue) inductionHeatValue.textContent = inductionHeat.value; };
    inductionHeat.onchange = function () {
      sendCommand({ device: 'induction', action: 'setheat', parameters: { level: parseInt(this.value) } });
    };
  }

  // Washing Machine Controls
  const washingMachineToggle = document.getElementById('washingMachineToggle');

  if (washingMachineToggle) {
    washingMachineToggle.onclick = function () {
      const willBeOn = this.classList.contains('off');
      updateToggleButton(this, willBeOn);
      sendCommand({ device: 'washingmachine', action: 'toggle', parameters: { state: willBeOn } });
    };
  }

  // Function to update UI based on server response with new JSON structure
  function updateUI(data) {
    console.log('Updating UI with data:', data);
    
    // Handle the new JSON-based status structure
    // Check for the device type in the data
    const deviceType = data.deviceType ? data.deviceType.toLowerCase() : (data.device || '');
    
    switch (deviceType) {
      case 'light':
        const lightIndex = data.deviceIndex !== undefined ? data.deviceIndex : 0;
        updateLightUI(
          lightIndex, 
          data.power !== undefined ? data.power : data.state, 
          data.intensity !== undefined ? data.intensity : 1.0, 
          data.color || 'FFFFFF'
        );
        break;
      case 'tv':
        updateTVUI(
          data.power !== undefined ? data.power : data.state, 
          data.volume || 0, 
          data.channel || 1, 
          data.source || 'TV'
        );
        break;
      case 'ac':
        updateACUI(
          data.power !== undefined ? data.power : data.state, 
          data.temperature || 24, 
          data.fanSpeed || 1, 
          data.ecoMode || false
        );
        break;
      case 'fan':
        updateFanUI(
          data.power !== undefined ? data.power : data.state, 
          data.rpm || 400
        );
        break;
      case 'fridge':
        updateFridgeUI(
          data.power !== undefined ? data.power : data.state, 
          data.temperature || 4, 
          data.freezeTemperature || -18, 
          data.fridgeDoor || false, 
          data.freezeDoor || false
        );
        break;
      case 'induction':
        updateInductionUI(data.level || 0);
        break;
      case 'washingmachine':
      case 'washing machine':
      case 'washingMachine':
        updateWashingMachineUI(data.power !== undefined ? data.power : data.state);
        break;
      default:
        console.warn('Unknown device in response:', deviceType, data);
    }
  }

  // Function to update all UI elements based on initial server state
  function updateAllUI(data) {
    console.log('Updating all UI with data:', data);

    // Handle lights
    if (data.lights && Array.isArray(data.lights)) {
      data.lights.forEach(light => {
        const index = light.deviceIndex !== undefined ? light.deviceIndex : 0;
        updateLightUI(
          index,
          light.power !== undefined ? light.power : light.state,
          light.intensity !== undefined ? light.intensity : 1.0,
          light.color || 'FFFFFF'
        );
      });
    }

    // Handle TV
    if (data.tv) {
      updateTVUI(
        data.tv.power !== undefined ? data.tv.power : data.tv.state,
        data.tv.volume || 0,
        data.tv.channel || 1,
        data.tv.source || 'TV'
      );
    }

    // Handle AC
    if (data.ac) {
      updateACUI(
        data.ac.power !== undefined ? data.ac.power : data.ac.state,
        data.ac.temperature || 24,
        data.ac.fanSpeed || 1,
        data.ac.ecoMode || false
      );
    }

    // Handle Fan
    if (data.fan) {
      updateFanUI(
        data.fan.power !== undefined ? data.fan.power : data.fan.state,
        data.fan.rpm || 400
      );
    }

    // Handle Fridge
    if (data.fridge) {
      updateFridgeUI(
        data.fridge.power !== undefined ? data.fridge.power : data.fridge.state,
        data.fridge.temperature || 4,
        data.fridge.freezeTemperature || -18,
        data.fridge.fridgeDoor || false,
        data.fridge.freezeDoor || false
      );
    }

    // Handle Induction
    if (data.induction) {
      updateInductionUI(data.induction.level || 0);
    }

    // Handle Washing Machine
    if (data.washingMachine || data.washingmachine) {
      const wm = data.washingMachine || data.washingmachine;
      updateWashingMachineUI(wm.power !== undefined ? wm.power : wm.state);
    }
  }

  // Device-specific update functions
  function updateLightUI(index, state, intensity, color) {
    const toggleButton = document.getElementById(`light${index}Toggle`);
    const intensitySlider = document.getElementById(`light${index}Intensity`);
    const intensityValue = document.getElementById(`light${index}IntensityValue`);
    const colorPicker = document.getElementById(`light${index}Color`);
    if (toggleButton) updateToggleButton(toggleButton, state);
    if (intensitySlider && intensityValue) {
      intensitySlider.value = intensity;
      intensityValue.textContent = parseFloat(intensity).toFixed(1);
    }
    if (colorPicker) colorPicker.value = `#${color}`;
  }

  function updateTVUI(state, volume, channel, source) {
    const toggleButton = document.getElementById('tvToggle');
    const volumeSlider = document.getElementById('tvVolume');
    const volumeValue = document.getElementById('tvVolumeValue');
    const channelInput = document.getElementById('tvChannel');
    const sourceSelect = document.getElementById('tvSource');
    if (toggleButton) updateToggleButton(toggleButton, state);
    if (volumeSlider && volumeValue) {
      volumeSlider.value = volume;
      volumeValue.textContent = volume;
    }
    if (channelInput) channelInput.value = channel;
    if (sourceSelect) sourceSelect.value = source;
  }

  function updateACUI(state, temperature, fanSpeed, ecoMode) {
    const toggleButton = document.getElementById('acToggle');
    const tempInput = document.getElementById('acTemperature');
    const fanSpeedSelect = document.getElementById('acFanSpeed');
    const ecoButton = document.getElementById('acEcoMode');
    if (toggleButton) updateToggleButton(toggleButton, state);
    if (tempInput) tempInput.value = temperature;
    if (fanSpeedSelect) fanSpeedSelect.value = fanSpeed;
    if (ecoButton) updateToggleButton(ecoButton, ecoMode);
  }

  function updateFanUI(state, rpm) {
    const toggleButton = document.getElementById('fanToggle');
    const rpmInput = document.getElementById('fanRPM');
    if (toggleButton) updateToggleButton(toggleButton, state);
    if (rpmInput) rpmInput.value = rpm;
  }

  function updateFridgeUI(state, temperature, freezeTemperature, fridgeDoor, freezeDoor) {
    const toggleButton = document.getElementById('fridgeToggle');
    const tempInput = document.getElementById('fridgeTemperature');
    const freezeTempInput = document.getElementById('freezeTemperature');
    const fridgeDoorToggle = document.getElementById('fridgeDoorToggle');
    const freezeDoorToggle = document.getElementById('freezeDoorToggle');
    if (toggleButton) updateToggleButton(toggleButton, state);
    if (tempInput) tempInput.value = temperature;
    if (freezeTempInput) freezeTempInput.value = freezeTemperature;
    if (fridgeDoorToggle) updateDoorButton(fridgeDoorToggle, fridgeDoor);
    if (freezeDoorToggle) updateDoorButton(freezeDoorToggle, freezeDoor);
  }

  function updateInductionUI(level) {
    const heatSlider = document.getElementById('inductionHeat');
    const heatValue = document.getElementById('inductionHeatValue');
    if (heatSlider) heatSlider.value = level;
    if (heatValue) heatValue.textContent = level;
  }

  function updateWashingMachineUI(state) {
    const toggleButton = document.getElementById('washingMachineToggle');
    if (toggleButton) updateToggleButton(toggleButton, state);
  }


  // --- Keyboard Shortcuts ---
  document.addEventListener('keydown', (event) => {
    // Focus chat input: Ctrl + /
    if (event.ctrlKey && event.key === '/') {
      event.preventDefault();
      if (chatText) chatText.focus();
    }
    // Toggle voice input: Alt + V
    if (event.altKey && event.key === 'v') {
      event.preventDefault();
      if (startVoice && !startVoice.disabled) {
        startVoice.click(); // Trigger the voice recognition
      }
    }
    // Refresh connection: Ctrl + R (Prevent default browser refresh)
    // Note: Simple reconnect might be better than full page reload
    if (event.ctrlKey && event.key === 'r') {
      event.preventDefault();
      console.log('Attempting to manually reconnect WebSocket...');
      if (ws && ws.readyState !== WebSocket.OPEN) {
        connectWebSocket();
      } else if (ws) {
        ws.close(); // Will trigger reconnect via onclose handler
        console.log('Closing existing connection to trigger reconnect.');
      } else {
        connectWebSocket(); // If ws is null for some reason
      }
    }
  });

  // --- Initialisation ---
  connectWebSocket(); // Initial connection attempt when DOM is ready

  // Dark mode toggle
  const themeToggle = document.getElementById('themeToggle');
  const body = document.body;
  
  // Check for saved theme preference
  const savedTheme = localStorage.getItem('theme');
  if (savedTheme === 'dark') {
    body.classList.add('dark-mode');
    themeToggle.innerHTML = '<i class="fas fa-sun"></i>';
  }

  themeToggle.addEventListener('click', () => {
    body.classList.toggle('dark-mode');
    const isDarkMode = body.classList.contains('dark-mode');
    
    // Update icon
    themeToggle.innerHTML = isDarkMode ? '<i class="fas fa-sun"></i>' : '<i class="fas fa-moon"></i>';
    
    // Save preference
    localStorage.setItem('theme', isDarkMode ? 'dark' : 'light');
  });

}); // End DOMContentLoaded listener