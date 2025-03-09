# Contributing to SmartHome-AgenticAI-Simulation

This repository combines `HomeAutomation-UnitySimulation` (in `/UnitySimulation`) and `HomeAutomation-AgenticAI` (in `/AgenticAI`), with a web interface in `/Website`. You can contribute directly here or via the original repositories. Contributions are welcome to enhance the smart home simulation, AI automation, or web controls, all integrated via WebSocket (`ws://localhost:8080/iot`).

## Getting Started
Before contributing:
1. Read the [root README](../README.md) for an overview and quick start.
2. Check component-specific guides:
   - [UnitySimulation README](../UnitySimulation/README.md) for simulation setup.
   - [AgenticAI README](../AgenticAI/README.md) for AI configuration.
   - [Website README](../Website/README.md) for web interface details.

## Prerequisites
- **Git**: Installed (`git --version` to check).
- **Git LFS**: Installed (`git lfs install`) for large files (e.g., Unity assets).
- **Write Access**: To this repo or the original repos (see below).

## Contributing Directly
1. **Clone**:
   ```bash
   git clone https://github.com/Shival-Gupta/SmartHome-AgenticAI-Simulation.git
   cd SmartHome-AgenticAI-Simulation
   ```
2. **Branch**:
   ```bash
   git checkout -b your-branch
   ```
3. **Edit**: Modify files in `/UnitySimulation`, `/AgenticAI`, or `/Website`.
4. **Track Large Files** (if needed):
   ```bash
   git lfs track "*.bin"
   git add .gitattributes
   ```
5. **Commit & Push**:
   ```bash
   git add .
   git commit -m "Your message"
   git push origin your-branch
   ```
6. **Submit PR**: Create a pull request on GitHub to `main`.

## Contributing via Original Repos
1. **Clone**:
   - For `HomeAutomation-UnitySimulation`:
     ```bash
     git clone https://github.com/Shival-Gupta/HomeAutomation-UnitySimulation.git
     ```
   - For `HomeAutomation-AgenticAI`:
     ```bash
     git clone https://github.com/Shival-Gupta/HomeAutomation-AgenticAI.git
     ```
2. **Branch, Edit, Commit, Push**:
   ```bash
   git checkout -b your-branch
   git add .
   git commit -m "Your message"
   git push origin your-branch
   ```
3. **Submit PR**: Create a pull request to the original repo’s `main`.
4. Maintainers will sync changes to this repo.

## Syncing Changes (Maintainers)
For those with write access to this repo:
1. **Add Remotes** (once):
   ```bash
   git remote add unitysim-origin https://github.com/Shival-Gupta/HomeAutomation-UnitySimulation.git
   git remote add agenticai-origin https://github.com/Shival-Gupta/HomeAutomation-AgenticAI.git
   ```
2. **Fetch**:
   ```bash
   git fetch unitysim-origin
   git fetch agenticai-origin
   ```
3. **Merge**:
   ```bash
   git merge unitysim-origin/main
   git merge agenticai-origin/main
   ```
   - Use `--allow-unrelated-histories` if merging repos with no shared history (typically only needed for the initial merge).
4. **Push**:
   ```bash
   git push origin main
   ```

## Testing Your Changes
Given the multi-component setup:
- Test Unity changes with `"UnitySimulation/Environment Samsung.exe"`.
- Verify AI functionality with `crewai run` in `/AgenticAI`.
- Check web interface updates by serving `/Website` (`python -m http.server 8000`).
- Ensure WebSocket communication (`ws://localhost:8080/iot`) works across components.

## Notes
- **Direct Changes**: Push to `SmartHome-AgenticAI-Simulation`.
- **Original Repos**: Push there, then sync here (maintainers only).
- **Git LFS**: Run `git lfs pull` after cloning/merging if large files are involved.
- **Conflicts**: Rare, but resolve manually if they occur.
- **API Details**: See [UnitySimulation API](../UnitySimulation/API.md) for WebSocket specs.
