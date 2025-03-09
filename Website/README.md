# SmartHome-AgenticAI-Simulation Website

This directory contains the website for interacting with the SmartHome-AgenticAI-Simulation project. The website provides user interfaces to control simulated smart home devices via a WebSocket connection, with options for basic manual control, basic AI integration, and advanced AI-enhanced automation.

## Overview

- **Landing Page**: `/Website/index.html` - Provides an introduction and a link to the control interfaces.
- **Control Interfaces**:
  - `/Website/remote/v1.html` - Basic control interface without AI integration (manual device control).
  - `/Website/remote/v2.html` - Basic control interface with AI integration (text/voice input for AI commands).
  - `/Website/remote/v3.html` - Advanced control interface with AI integration (comprehensive device control and AI automation).
- **Assets**: Styles and scripts shared across the interfaces, located in `/Website/remote/assets/`.

## Setup

1. **Run the Unity Simulation**:
   - Follow instructions in `../UnitySimulation/README.md` to launch the simulation.
   - Ensure the WebSocket server is running at `ws://localhost:8080/iot`.

2. **Serve the Website**:
   - Navigate to the `/Website` directory.
   - Use a local web server to serve the files:
     ```bash
     python -m http.server 8000
     ```
   - Open your browser to `http://localhost:8000/`.

3. **Using the Control Interfaces**:
   - From the landing page, click "Control the Simulation" to access the chooser page.
   - Select:
     - "Basic Control (No AI)" for manual device control (`v1.html`).
     - "Basic Control with AI" for AI-enhanced commands with limited devices (`v2.html`).
     - "Advanced Control (AI Integrated)" for comprehensive control and AI automation (`v3.html`).
   - For AI input (in `v2.html` and `v3.html`):
     - Type a command in the text input and click "Send".
     - Use the microphone button for voice input (supported in compatible browsers like Chrome, Edge).

4. **Troubleshooting**:
   - Ensure the simulation is running and the WebSocket server is accessible.
   - Check the connection status on the website.
   - For AI input, ensure AI agents are running as per `../AgenticAI/README.md`.

## Dependencies

- A modern web browser supporting HTML5, CSS3, and JavaScript.
- For voice input: Browser support for the Web Speech API (e.g., Chrome, Edge).

## Contributing

See the root `[CONTRIBUTING.md](../CONTRIBUTING.md)` for guidelines on contributing to this project, including website enhancements.
