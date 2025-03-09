# Contributing to SmartHome-AgenticAI-Simulation

This repository combines `HomeAutomation-UnitySimulation` (in `/UnitySimulation`) and `HomeAutomation-AgenticAI` (in `/AgenticAI`). You can contribute directly here or via the original repositories.

## Prerequisites
- Git installed (`git --version` to check)
- Git LFS installed (`git lfs install`)
- Write access to `SmartHome-AgenticAI-Simulation` or the original repos

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
3. **Edit**: Modify files in `/UnitySimulation` or `/AgenticAI`.
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
For those with write access:
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
   - Use `--allow-unrelated-histories` if needed (first merge only).
4. **Push**:
   ```bash
   git push origin main
   ```

## Notes
- **Direct Changes**: Push to `SmartHome-AgenticAI-Simulation`.
- **Changes to Original Repos**: Push there, then sync here.
- **Git LFS**: Run `git lfs pull` after cloning/merging.
- **Conflicts**: Rare, but resolve manually if they occur.
