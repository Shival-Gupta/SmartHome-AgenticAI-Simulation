// Wrap everything in an IIFE (Immediately Invoked Function Expression)
// or use DOMContentLoaded to ensure elements exist before attaching listeners.
document.addEventListener('DOMContentLoaded', () => {

  // --- WebSocket Management ---
  let ws;
  const serverUrl = 'ws://localhost:8080/iot'; // Make sure this is correct
  // const serverUrl = 'ws://0.tcp.in.ngrok.io:11347/iot'; // Example ngrok URL

  const connectionStatus = document.getElementById('connectionStatus');

  function connectWebSocket() {
    ws = new WebSocket(serverUrl);

    ws.onopen = () => {
      console.log('WebSocket connected');
      if (connectionStatus) {
        connectionStatus.textContent = 'Connected';
        connectionStatus.classList.replace('disconnected', 'connected');
      }
    };

    ws.onclose = () => {
      console.log('WebSocket disconnected. Attempting to reconnect...');
      if (connectionStatus) {
        connectionStatus.textContent = 'Disconnected';
        connectionStatus.classList.replace('connected', 'disconnected');
      }
      // Implement exponential backoff or limit retries if needed
      setTimeout(connectWebSocket, 3000); // Reconnect after 3 seconds
    };

    ws.onerror = (error) => {
      console.error('WebSocket error:', error);
      // Optionally update UI to show error
    };

    ws.onmessage = (event) => {
      try {
        const response = JSON.parse(event.data);
        console.log('Received:', response);
        // TODO: Handle incoming messages (e.g., update UI based on device state changes)
        // updateUI(response); // Hypothetical function to update UI based on server message
      } catch (e) {
        console.error('Error parsing message:', event.data, e);
      }
    };
  }

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
      // Optionally alert the user or queue the command
    }
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
        if(startVoice) startVoice.style.backgroundColor = '#dc3545'; // Indicate recording
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
        if(startVoice) startVoice.style.backgroundColor = ''; // Reset button color
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
         if(intensityValue) intensityValue.textContent = parseFloat(this.value).toFixed(1);
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
      tvVolume.oninput = () => { if(tvVolumeValue) tvVolumeValue.textContent = tvVolume.value; };
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

  if(fanToggle) {
      fanToggle.onclick = function () {
          const willBeOn = this.classList.contains('off');
          updateToggleButton(this, willBeOn);
          sendCommand({ device: 'fan', action: 'toggle', parameters: { state: willBeOn } });
      };
  }
  if(fanRPM) {
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

  if(fridgeToggle) {
      fridgeToggle.onclick = function () {
          const willBeOn = this.classList.contains('off');
          updateToggleButton(this, willBeOn);
          sendCommand({ device: 'fridge', action: 'toggle', parameters: { state: willBeOn } });
      };
  }
  if(fridgeTemperature) {
      fridgeTemperature.onchange = function () {
          sendCommand({ device: 'fridge', action: 'settemperature', parameters: { temperature: parseInt(this.value) } });
      };
  }
  if(freezeTemperature) {
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

  if(fridgeDoorToggle) {
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
  if(freezeDoorToggle) {
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

  if(inductionHeat) {
      inductionHeat.oninput = () => { if(inductionHeatValue) inductionHeatValue.textContent = inductionHeat.value; };
      inductionHeat.onchange = function () {
          sendCommand({ device: 'induction', action: 'setheat', parameters: { level: parseInt(this.value) } });
      };
  }

  // Washing Machine Controls
  const washingMachineToggle = document.getElementById('washingMachineToggle');

  if(washingMachineToggle) {
      washingMachineToggle.onclick = function () {
          const willBeOn = this.classList.contains('off');
          updateToggleButton(this, willBeOn);
          sendCommand({ device: 'washingmachine', action: 'toggle', parameters: { state: willBeOn } });
      };
  }


  // --- Keyboard Shortcuts ---
  document.addEventListener('keydown', (event) => {
      // Focus chat input: Ctrl + /
      if (event.ctrlKey && event.key === '/') {
          event.preventDefault();
          if(chatText) chatText.focus();
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

}); // End DOMContentLoaded listener