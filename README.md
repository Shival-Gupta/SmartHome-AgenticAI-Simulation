# SmartHome-AgenticAI-Simulation

Welcome to the `SmartHome-AgenticAI-Simulation` repository! This project combines a Unity-based simulation environment for smart home automation with an AI-driven system for controlling and automating smart home devices. It serves as a comprehensive platform for developing, testing, and deploying smart home solutions.

## Repository Structure

- **`UnitySimulation/`**: Contains the Unity project for simulating a smart home environment. This includes assets, scripts, and configurations needed to run the simulation.
- **`AgenticAI/`**: Houses the AI agents and scripts responsible for automating and controlling smart home devices, built using the crewAI framework and custom tools for device interaction.

## Getting Started

### Unity Simulation
To set up and run the Unity-based smart home simulation:
- Refer to the **[UnitySimulation README](./UnitySimulation/README.md)** for detailed instructions on downloading, extracting, and launching the simulation, as well as interacting with the WebSocket API.

### AI Agents
To configure and run the AI-driven automation system:
- Check out the **[AgenticAI README](./AgenticAI/README.md)** for steps on installing dependencies, customizing agents and tasks, and executing the crewAI-based system.

## Integration
    
The AI agents in `AgenticAI/` communicate with the Unity simulation in `UnitySimulation/` through a WebSocket API. This allows for real-time control and automation of simulated smart home devices. For specifics on the API, including supported devices, actions, and message formats, see the **[IoT WebSocket API Documentation](./UnitySimulation/README.md#iot-websocket-api-documentation)** in the UnitySimulation README.

## Contributing

We welcome contributions to both the simulation and AI components! For guidelines on how to contribute, please review the **[CONTRIBUTING.md](./CONTRIBUTING.md)** file.

---

### Notes
- This `README.md` assumes the presence of `CONTRIBUTING.md` in the root directory, as indicated by your directory listing.
- If you wish to add a license section (e.g., MIT License), please specify the license type and include a `LICENSE` file in the repository root. You can then append a section like this:

  ```markdown
  ## License
  This project is licensed under the [MIT License](./LICENSE).
  ```

To use this, simply copy the content above into a file named `README.md` in the root of your `SmartHome-AgenticAI-Simulation` repository (e.g., `C:\Users\Shival Gupta\Desktop\Samsung GenAI\SmartHome-AgenticAI-Simulation\README.md`). Let me know if you'd like any modifications!
